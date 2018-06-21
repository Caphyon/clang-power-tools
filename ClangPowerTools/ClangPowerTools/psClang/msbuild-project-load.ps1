#-------------------------------------------------------------------------------------------------
# Global variables

# vcxproj and property sheet files declare MsBuild properties (e.g. $(MYPROP)).
# they are used in project xml nodes expressions. we have a
# translation engine (MSBUILD-POWERSHELL) for these. it relies on
# PowerShell to evaluate these expressions. We have to inject project
# properties in the PowerShell runtime context. We keep track of them in
# this list, to be cleaned before the next project begins processing
[System.Collections.ArrayList] $global:ProjectSpecificVariables     = @()

# current vcxproj and property sheets
[xml[]]  $global:projectFiles                    = @();

# path of current project
[string] $global:vcxprojPath                     = "";

# namespace of current project vcxproj XML
[System.Xml.XmlNamespaceManager] $global:xpathNS = $null;


Set-Variable -name "kRedundantSeparatorsReplaceRules" -option Constant `
              -value @( <# handle multiple consecutive separators #>   `
                        (";+" , ";")                                   `
                        <# handle separator at end                #>   `
                      , (";$" , "")                                    `
                        <# handle separator at beginning          #>   `
                      , ("^;" , "")                                    `
                      )

Function Set-Var([parameter(Mandatory = $false)][string] $name,
    [parameter(Mandatory = $false)][string] $value)
{
    Write-Verbose "SET_VAR $($name): $value"
    Set-Variable -name $name -Value $value -Scope Global

    if (!$global:ProjectSpecificVariables.Contains($name))
    {
        $global:ProjectSpecificVariables.Add($name) | Out-Null
    }
}

Function Clear-Vars()
{
    Write-Verbose-Array -array $global:ProjectSpecificVariables `
        -name "Deleting project specific variables"

    foreach ($var in $global:ProjectSpecificVariables)
    {
        Remove-Variable -name $var -scope Global -ErrorAction SilentlyContinue
    }

    $global:ProjectSpecificVariables.Clear()
}

Function InitializeMsBuildProjectProperties()
{
    Write-Verbose "Importing environment variables into current scope"
    foreach ($var in (Get-ChildItem Env:))
    {
        Set-Var -name $var.Name -value $var.Value
    }

    Set-Var -name "MSBuildProjectFullPath"   -value $global:vcxprojPath
    Set-Var -name "ProjectDir"               -value (Get-FileDirectory -filePath $global:vcxprojPath)
    Set-Var -name "MSBuildProjectExtension"  -value ([IO.Path]::GetExtension($global:vcxprojPath))
    Set-Var -name "MSBuildProjectFile"       -value (Get-FileName -path $global:vcxprojPath)
    Set-Var -name "MSBuildProjectName"       -value (Get-FileName -path $global:vcxprojPath -noext)
    Set-Var -name "MSBuildProjectDirectory"  -value (Get-FileDirectory -filePath $global:vcxprojPath)
    Set-Var -name "MSBuildProgramFiles32"    -value "${Env:ProgramFiles(x86)}"
    # defaults for projectname and targetname, may be overriden by project settings
    Set-Var -name "ProjectName"              -value $MSBuildProjectName
    Set-Var -name "TargetName"               -value $MSBuildProjectName

    # These would enable full project platform references parsing, experimental right now
    if ($env:CPT_LOAD_ALL -eq '1')
    {
        Set-Var -name "ConfigurationType"        -value "Application"
        Set-Var -name "VCTargetsPath"            -value "$(Get-VisualStudio-Path)\Common7\IDE\VC\VCTargets\"
        Set-Var -name "VsInstallRoot"            -value (Get-VisualStudio-Path)
        Set-Var -name "MSBuildExtensionsPath"    -value "$(Get-VisualStudio-Path)\MSBuild"
        Set-Var -name "LocalAppData"             -value $env:LOCALAPPDATA
        Set-Var -name "UserRootDir"              -value "$LocalAppData\Microsoft\MSBuild\v4.0"
        Set-Var -name "UniversalCRT_IncludePath" -value "${Env:ProgramFiles(x86)}\Windows Kits\10\Include\10.0.10240.0\ucrt"
    }

    [string] $vsVer = "15.0"
    if ($global:cptVisualStudioVersion -eq "2015")
    {
        $vsVer = "14.0"
    }
    Set-Var -name "VisualStudioVersion"    -value $vsVer
    Set-Var -name "MSBuildToolsVersion"    -value $vsVer

    [string] $projectSlnPath = Get-ProjectSolution
    [string] $projectSlnDir = Get-FileDirectory -filePath $projectSlnPath
    Set-Var -name "SolutionDir" -value $projectSlnDir
    [string] $projectSlnName = Get-FileName -path $projectSlnPath -noext
    Set-Var -name "SolutionName" -value $projectSlnName
}

Function InitializeMsBuildCurrentFileProperties([Parameter(Mandatory = $true)][string] $filePath)
{
    Set-Var -name "MSBuildThisFileFullPath"  -value $filePath
    Set-Var -name "MSBuildThisFileExtension" -value ([IO.Path]::GetExtension($filePath))
    Set-Var -name "MSBuildThisFile"          -value (Get-FileName -path $filePath)
    Set-Var -name "MSBuildThisFileName"      -value (Get-FileName -path $filePath -noext)
    Set-Var -name "MSBuildThisFileDirectory" -value (Get-FileDirectory -filePath $filePath)
}

<#
.DESCRIPTION
A wrapper over the XmlDOcument.SelectNodes function. For convenience.
Not to be used directly. Please use Select-ProjectNodes instead.
#>
function Help:Get-ProjectFileNodes([xml] $projectFile, [string] $xpath)
{
    [System.Xml.XmlElement[]] $nodes = $projectFile.SelectNodes($xpath, $global:xpathNS)
    return $nodes
}

function  GetNodeInheritanceToken([System.Xml.XmlNode] $node)
{
    [string] $inheritanceToken = "%($($node.Name))";
    if ($node.InnerText.Contains($inheritanceToken))
    {
        return $inheritanceToken
    }

    return ""
}

function ReplaceInheritedNodeValue([System.Xml.XmlNode] $currentNode
    , [System.Xml.XmlNode] $nodeToInheritFrom
)
{
    [string] $inheritanceToken = GetNodeInheritanceToken($currentNode)
    if ([string]::IsNullOrEmpty($inheritanceToken))
    {
        # no need to inherit
        return $false
    }

    [string] $replaceWith = ""
    if ($nodeToInheritFrom)
    {
        $replaceWith = $nodeToInheritFrom.InnerText
    }

    [string] $whatToReplace = [regex]::Escape($inheritanceToken);
    if ([string]::IsNullOrEmpty($replaceWith))
    {
        # handle semicolon separators
        [string] $escTok = [regex]::Escape($inheritanceToken)
        $whatToReplace = "(;$escTok)|($escTok;)|($escTok)"
    }

    # replace inherited token and redundant separators
    $replacementRules = @(, ($whatToReplace, $replaceWith)) + $kRedundantSeparatorsReplaceRules
    foreach ($rule in $replacementRules)
    {
        $currentNode.InnerText = $currentNode.InnerText -replace $rule[0], $rule[1]
    }

    return $currentNode.InnerText.Contains($inheritanceToken)
}

<#
.SYNOPSIS
Selects one or more nodes from the project.
.DESCRIPTION
We often need to access data from the project, e.g. additional includes, Win SDK version.
A naive implementation would be to simply look inside the vcxproj, but that leaves out
property sheets.

This function takes care to retrieve the nodes we're searching by looking in both the .vcxproj
and property sheets, taking care to inherit values accordingly.
.EXAMPLE
Give an example of how to use it
.EXAMPLE
Give another example of how to use it.
.PARAMETER xpath
XPath we want to use for searching nodes.
.PARAMETER fileIndex
Optional. Index of the project xml file we want to start our search in.
0 = .vcxproj and then, recursively, all property sheets
1 = first property sheet and then, recursively, all other property sheets
etc.
#>
function Select-ProjectNodes([Parameter(Mandatory = $true)]  [string][string] $xpath
    , [Parameter(Mandatory = $false)] [int]            $fileIndex = 0)
{
    [System.Xml.XmlElement[]] $nodes = @()

    if ($fileIndex -ge $global:projectFiles.Count)
    {
        return $nodes
    }

    $nodes = Help:Get-ProjectFileNodes -projectFile $global:projectFiles[$fileIndex] `
        -xpath $xpath

    # nothing on this level or we're dealing with an ItemGroup, go above
    if ($nodes.Count -eq 0 -or $xpath.Contains("ItemGroup"))
    {
        [System.Xml.XmlElement[]] $upperNodes = Select-ProjectNodes -xpath $xpath -fileIndex ($fileIndex + 1)
        if ($upperNodes.Count -gt 0)
        {
            $nodes += $upperNodes
        }
        return $nodes
    }

    if ($nodes[$nodes.Count - 1]."#text")
    {
        # we found textual settings that can be inherited. see if we should inherit

        [System.Xml.XmlNode] $nodeToReturn = $nodes[$nodes.Count - 1]
        if ($nodeToReturn.Attributes.Count -gt 0)
        {
            throw "Did not expect node to have attributes"
        }

        [bool] $shouldInheritMore = ![string]::IsNullOrEmpty((GetNodeInheritanceToken -node $nodeToReturn))
        for ([int] $i = $nodes.Count - 2; ($i -ge 0) -and $shouldInheritMore; $i -= 1)
        {
            $shouldInheritMore = ReplaceInheritedNodeValue -currentNode $nodeToReturn -nodeToInheritFrom $nodes[$i]
        }

        if ($shouldInheritMore)
        {
            [System.Xml.XmlElement[]] $inheritedNodes = Select-ProjectNodes -xpath $xpath -fileIndex ($fileIndex + 1)
            if ($inheritedNodes.Count -gt 1)
            {
                throw "Did not expect to inherit more than one node"
            }
            if ($inheritedNodes.Count -eq 1)
            {
                $shouldInheritMore = ReplaceInheritedNodeValue -currentNode $nodeToReturn -nodeToInheritFrom $inheritedNodes[0]
            }
        }

        # we still could have to inherit from parents but when not loading
        # all MS prop sheets we have nothing to inherit from, delete inheritance token
        ReplaceInheritedNodeValue -currentNode $nodeToReturn -nodeToInheritFrom $null | Out-Null

        return @($nodeToReturn)
    }
    else
    {
        # return what we found
        return $nodes
    }
}

<#
.DESCRIPTION
   Finds the first config-platform pair in the vcxproj.
   We'll use it for all project data retrievals.

   Items for other config-platform pairs will be removed from the DOM.
   This is needed so that our XPath selectors don't get confused when looking for data.
#>
function Detect-ProjectDefaultConfigPlatform([string] $projectValue)
{
    [string]$configPlatformName = ""

    if (![string]::IsNullOrEmpty($aVcxprojConfigPlatform))
    {
        $configPlatformName = $aVcxprojConfigPlatform
    }
    else
    {
        $configPlatformName = $projectValue
    }

    if ([string]::IsNullOrEmpty($configPlatformName))
    {
        throw "Could not automatically detect a configuration platform"
    }

    [string[]] $configAndPlatform = $configPlatformName.Split('|')
    Set-Var -Name "Configuration" -Value $configAndPlatform[0]
    Set-Var -Name "Platform"      -Value $configAndPlatform[1]
}

function HandleChooseNode([System.Xml.XmlNode] $aChooseNode)
{
    SanitizeProjectNode $aChooseNode
    if ($aChooseNode.ChildNodes.Count -eq 0)
    {
        return
    }

    [System.Xml.XmlElement] $selectedChild = $aChooseNode.ChildNodes | `
        Where-Object { $_.GetType().Name -eq "XmlElement" } | `
        Select -first 1

    foreach ($selectedGrandchild in $selectedChild.ChildNodes)
    {
        $aChooseNode.ParentNode.AppendChild($selectedGrandchild.Clone()) | Out-Null
    }

    $aChooseNode.ParentNode.RemoveChild($aChooseNode) | Out-Null
}

function SanitizeProjectNode([System.Xml.XmlNode] $node)
{
    if ($node.Name -ieq "#comment")
    {
        return
    }

    [System.Collections.ArrayList] $nodesToRemove = @()

    if ($node.Name -ieq "#text" -and $node.InnerText.Length -gt 0)
    {
        # evaluate node content
        $node.InnerText = Evaluate-MSBuildExpression $node.InnerText
    }

    if ($node.Name -ieq "Import")
    {
        [string] $relPath = Evaluate-MSBuildExpression $node.GetAttribute("Project")
        [string[]] $paths = Canonize-Path -base (Get-Location) -child $relPath -ignoreErrors

        foreach ($path in $paths)
        {
            if (![string]::IsNullOrEmpty($path) -and (Test-Path $path))
            {
                Write-Verbose "Property sheet: $path"
                SanitizeProjectFile($path)
            }
            else
            {
                Write-Verbose "Could not find property sheet $relPath"
            }
        }
    }

    if ( ($node.Name -ieq "ClCompile" -or $node.Name -ieq "ClInclude") -and
        ![string]::IsNullOrEmpty($node.GetAttribute("Include")) )
    {
        [string] $expandedAttr = Evaluate-MSBuildExpression $node.GetAttribute("Include")
        $node.Attributes["Include"].Value = $expandedAttr
    }

    if ($node.Name -ieq "Choose")
    {
        HandleChooseNode $chooseChild
    }

    if ($node.Name -ieq "Otherwise")
    {
        [System.Xml.XmlElement[]] $siblings = $node.ParentNode.ChildNodes | `
            Where-Object { $_.GetType().Name -ieq "XmlElement" -and $_ -ne $node }
        if ($siblings.Count -gt 0)
        {
            # means there's a <When> element that matched
            # <Otherwise> should not be evaluated, we could set unwated properties
            return
        }
    }

    if ($node.Name -ieq "ItemGroup" -and $node.GetAttribute("Label") -ieq "ProjectConfigurations")
    {
        [System.Xml.XmlElement] $firstChild = $node.ChildNodes                                     | `
            Where-Object { $_.GetType().Name -ieq "XmlElement" } | `
            Select-Object -First 1
        Detect-ProjectDefaultConfigPlatform $firstChild.GetAttribute("Include")
    }

    if ($node.ParentNode.Name -ieq "PropertyGroup")
    {
        # set new property value
        [string] $propertyName = $node.Name
        [string] $propertyValue = Evaluate-MSBuildExpression $node.InnerText

        Set-Var -Name $propertyName -Value $propertyValue

        return
    }

    foreach ($child in $node.ChildNodes)
    {
        [bool] $validChild = $true
        if ($child.GetType().Name -ieq "XmlElement")
        {
            if ($child.HasAttribute("Condition"))
            {
                # process node condition
                [string] $nodeCondition = $child.GetAttribute("Condition")
                $validChild = ((Evaluate-MSBuildCondition($nodeCondition)) -eq $true)
                if ($validChild)
                {
                    $child.RemoveAttribute("Condition")
                }
            }
        }
        if (!$validChild)
        {
            $nodesToRemove.Add($child) | out-null
            continue
        }
        else
        {
            SanitizeProjectNode($child)
        }
    }

    foreach ($nodeToRemove in $nodesToRemove)
    {
        $nodeToRemove.ParentNode.RemoveChild($nodeToRemove) | out-null
    }
}

<#
.DESCRIPTION
   Sanitizes a project xml file, by removing config-platform pairs different from the
   one we selected.
   This is needed so that our XPath selectors don't get confused when looking for data.
#>
function SanitizeProjectFile([string] $projectFilePath)
{
    Write-Verbose "`nSanitizing $projectFilePath"

    [xml] $fileXml = Get-Content $projectFilePath
    $global:projectFiles += @($fileXml)
    $global:xpathNS = New-Object System.Xml.XmlNamespaceManager($fileXml.NameTable)
    $global:xpathNS.AddNamespace("ns", $fileXml.DocumentElement.NamespaceURI)

    Push-Location (Get-FileDirectory -filePath $projectFilePath)

    InitializeMsBuildCurrentFileProperties -filePath $projectFilePath
    SanitizeProjectNode($fileXml.Project)

    Pop-Location
}

<#
.DESCRIPTION
  Tries to find a Directory.Build.props property sheet, starting from the
  project directories, going up. When one is found, the search stops.

  Multiple Directory.Build.props sheets are not supported.
#>
function Get-AutoPropertySheet()
{
    $startPath = $global:vcxprojPath
    while ($true)
    {
        $propSheetPath = Canonize-Path -base $startPath `
            -child "Directory.Build.props" `
            -ignoreErrors
        if (![string]::IsNullOrEmpty($propSheetPath))
        {
            return $propSheetPath
        }

        $newPath = Canonize-Path -base $startPath -child ".."
        if ($newPath -eq $startPath)
        {
            return ""
        }
        $startPath = $newPath
    }
}

<#
.DESCRIPTION
Loads vcxproj and property sheets into memory. This needs to be called only once
when processing a project. Accessing project nodes can be done using Select-ProjectNodes.
#>
function LoadProject([string] $vcxprojPath)
{
    $global:vcxprojPath = $vcxprojPath

    InitializeMsBuildProjectProperties

    $global:projectFiles = @()

    SanitizeProjectFile -projectFilePath $global:vcxprojPath
}
