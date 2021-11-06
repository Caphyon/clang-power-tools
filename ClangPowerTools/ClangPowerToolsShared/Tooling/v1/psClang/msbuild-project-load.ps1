#-------------------------------------------------------------------------------------------------
# Global variables

# vcxproj and property sheet files declare MsBuild properties (e.g. $(MYPROP)).
# they are used in project xml nodes expressions. we have a
# translation engine (MSBUILD-POWERSHELL) for these. it relies on
# PowerShell to evaluate these expressions. We have to inject project
# properties in the PowerShell runtime context. We keep track of them in
# this list, so that each project can know to clean previous vars before loading begins.
if (! (Test-Path variable:global:ProjectSpecificVariables))
{
  [System.Collections.ArrayList] $global:ProjectSpecificVariables    = @()
}

if (! (Test-Path variable:global:ScriptParameterBackupValues))
{
  [System.Collections.Hashtable] $global:ScriptParameterBackupValues = @{}
}

# path of current project
[string] $global:vcxprojPath                     = "";


Set-Variable -name "kRedundantSeparatorsReplaceRules" -option Constant `
              -value @( <# handle multiple consecutive separators #>   `
                        (";+" , ";")                                   `
                        <# handle separator at end                #>   `
                      , (";$" , "")                                    `
                        <# handle separator at beginning          #>   `
                      , ("^;" , "")                                    `
                      )
Set-Variable -name "kCacheSyntaxVer" -Option Constant -value "1"

Add-Type -TypeDefinition @"
public class ProjectConfigurationNotFound : System.Exception
{
    public string ConfigPlatform;
    public string Project;

    public ProjectConfigurationNotFound(string proj, string configPlatform)
    {
        this.Project = proj;
        this.ConfigPlatform = configPlatform;
    }
}
"@

Function Set-Var([parameter(Mandatory = $false)][string] $name
                ,[parameter(Mandatory = $false)]         $value
                ,[parameter(Mandatory = $false)][switch] $asScriptParameter
                )
{
    if ($name -ieq "home")
    {
        Write-Verbose "Shimming HOME variable"
        # the HOME PowerShell variable is protected and we can't overwrite it
        $name = "CPT_SHIM_HOME"
    }

    if ($asScriptParameter)
    {
        if (Test-Path "variable:$name")
        {
          $oldVar = Get-Variable $name
          $oldValue = $oldVar.Value

          if ($oldValue           -and
              $oldValue.GetType() -and
              $oldValue.GetType().ToString() -eq "System.Management.Automation.SwitchParameter")
          {
            $oldValue = $oldValue.ToBool()
          }

          $global:ScriptParameterBackupValues[$name] = $oldValue
        }
        else
        {
          $global:ScriptParameterBackupValues[$name] = $null
        }
    }

    Write-Verbose "SET_VAR $($name): $value"
    if ($asScriptParameter)
    {
      Set-Variable -name $name -Value $value -Scope Script
    }
    else
    {
      Set-Variable -name $name -Value $value -Scope Global
    }

    if (!$asScriptParameter -and !$global:ProjectSpecificVariables.Contains($name))
    {
        $global:ProjectSpecificVariables.Add($name) > $null
    }
}

Function Add-Project-Item([parameter(Mandatory = $false)][string] $name
                         ,[parameter(Mandatory = $false)]         $value
                         ,[parameter(Mandatory = $false)]         $properties = $null)
{
    if (!$value)
    {
        return
    }

    $itemVarName = "CPT_PROJITEM_$name"
    if (!(Get-Variable $itemVarName -ErrorAction SilentlyContinue))
    {
        $itemList = New-Object System.Collections.ArrayList
        Set-Var -name $itemVarName -value $itemList
    }

    $itemList = (Get-Variable $itemVarName).Value
    if ($value -is [array])
    {
        foreach ($arrayValue in $value)
        {
            $itemList.Add( @($arrayValue, $properties) ) > $null
        }
    }
    else
    {
        $itemList.Add(@($value, $properties)) > $null
    }
}

Function Get-Project-Item([parameter(Mandatory = $true)][string] $name)
{
    $itemVarName = "CPT_PROJITEM_$name"

    $itemVar = Get-Variable $itemVarName -ErrorAction SilentlyContinue
    if ($itemVar)
    {
        $retStr = ""

        if ($itemVar.Value.GetType().Name -ieq "ArrayList")
        {
            foreach ($v in $itemVar.Value)
            {
                if ($retStr)
                {
                    $retStr += ";"
                }
                $retStr += $v[0] # index0 = item; index1 = properties
            }
        }
        else
        {
            $retStr = $itemVar.Value[0] # index0 = item; index1 = properties
        }

        return $retStr
    }

    return $null
}

Function Get-Project-ItemList([parameter(Mandatory = $true)][string] $name)
{
    $retList = New-Object System.Collections.ArrayList

    $itemVarName = "CPT_PROJITEM_$name"

    $itemVar = Get-Variable $itemVarName -ErrorAction SilentlyContinue
    if ($itemVar)
    {
        $retStr = ""

        if ($itemVar.Value.GetType().Name -ieq "ArrayList")
        {
            foreach ($v in $itemVar.Value)
            {
                if ($retStr)
                {
                    $retStr += ";"
                }
                $retList.Add($v) > $null # v is a pair. index0 = item; index1 = properties
            }
        }
        else
        {
            $retList.Add($itemVar.Value) > $null
        }
    }

    return $retList
}

Function Clear-Vars()
{
    Write-Verbose-Array -array $global:ProjectSpecificVariables `
        -name "Deleting variables initialized by previous project"

    foreach ($var in $global:ProjectSpecificVariables)
    {
        Remove-Variable -name $var -scope Global -ErrorAction SilentlyContinue
    }

    foreach ($varName in $global:ScriptParameterBackupValues.Keys)
    {
        Write-Verbose "Restoring $varName to old value $($ScriptParameterBackupValues[$varName])"
        Set-Variable -name $varName -value $ScriptParameterBackupValues[$varName]
    }

    $global:ScriptParameterBackupValues.Clear()

    $global:ProjectSpecificVariables.Clear()

    Reset-ProjectItemContext
}

Function Load-CacheRepositoryIndex()
{
  if (! (Test-Path $kCptCacheRepo))
  {
    New-Item $kCptCacheRepo -ItemType "Directory"
  }

  [string] $cptCacheRepoIndex = "$kCptCacheRepo\index.dat"
  [System.Collections.Hashtable] $cacheIndex = @{}
  if (Test-Path $cptCacheRepoIndex)
  {
    $cacheIndex = [System.Management.Automation.PSSerializer]::Deserialize((Get-Content $cptCacheRepoIndex))
  }
  return $cacheIndex
}

Function Save-CacheRepositoryIndex([System.Collections.Hashtable] $cacheIndex)
{
  if (! (Test-Path $kCptCacheRepo))
  {
    New-Item $kCptCacheRepo -ItemType "Directory"
  }

  [string] $cptCacheRepoIndex = "$kCptCacheRepo\index.dat"
  $serialized = [System.Management.Automation.PSSerializer]::Serialize($cacheIndex)
  $serialized > $cptCacheRepoIndex
}

Function Save-ProjectToCacheRepo()
{
  [System.Collections.Hashtable] $dataMap = @{}
  foreach ($varName in $global:ProjectSpecificVariables)
  {
    $dataMap[$varName] = Get-Variable -name $varName -ValueOnly
  }
  $dataMap['cptVisualStudioVersion']   = Get-Variable 'cptVisualStudioVersion'   -scope Global   -ValueOnly
  $dataMap['cptCurrentConfigPlatform'] = Get-Variable 'cptCurrentConfigPlatform' -scope Global   -ValueOnly
  
  [string] $pathToSave = "$kCptCacheRepo\$(Get-RandomString).dat"
  $serialized = [System.Management.Automation.PSSerializer]::Serialize($dataMap)
  $serialized > $pathToSave

  $projHash = (Get-FileHash $MSBuildProjectFullPath)
  
  [System.Collections.Hashtable] $cacheIndex = Load-CacheRepositoryIndex

  $cacheObject = New-Object PsObject -Prop @{ "ProjectFile"            = $MSBuildProjectFullPath
                                            ; "ProjectHash"            = $projHash.Hash
                                            ; "CachedDataPath"         = $pathToSave
                                            ; "ConfigurationPlatform"  = $aVcxprojConfigPlatform
                                            ; "CptCacheSyntaxVersion"  = $kCacheSyntaxVer
                                            }
  $cacheIndex[$MSBuildProjectFullPath] = $cacheObject
  
  Save-CacheRepositoryIndex $cacheIndex
}

Function Load-ProjectFromCache([string] $aVcxprojPath)
{
  [System.Collections.Hashtable] $cacheIndex = Load-CacheRepositoryIndex
  if ( ! $cacheIndex.ContainsKey($aVcxprojPath))
  {
    return $false
  }

  $projectCacheObject = $cacheIndex[$aVcxprojPath]

  if ( ! (Test-Path $projectCacheObject.CachedDataPath))
  {
    return $false
  }
  
  if ($projectCacheObject.CptCacheSyntaxVersion -ne $kCacheSyntaxVer)
  {
    # the cached version uses an outdated syntax, discard it
    Remove-Item $projectCacheObject.CachedDataPath
    return $false
  }
  
  $projHash = (Get-FileHash $aVcxprojPath)
  if ($projectCacheObject.ProjectHash -ne $projHash.Hash)
  {
    Remove-Item $projectCacheObject.CachedDataPath
    return $false
  }

  if ($projectCacheObject.ConfigurationPlatform -ne $aVcxprojConfigPlatform)
  {
    Remove-Item $projectCacheObject.CachedDataPath
    return $false
  }

  # Clean global variables that have been set by a previous project load
  Clear-Vars

  $global:vcxprojPath = $aVcxprojPath

  [System.Collections.Hashtable] $deserialized = @{}
  [string] $data = Get-Content $projectCacheObject.CachedDataPath
  $deserialized = [System.Management.Automation.PSSerializer]::Deserialize($data)

  foreach ($var in $deserialized.Keys)
  {
    Set-Var -name $var -value $deserialized[$var]
  }

  return $true
}

Function UpdateScriptParameter([Parameter(Mandatory = $true)] [string] $paramName
                              ,[Parameter(Mandatory = $false)][string] $paramValue)
{
  [bool]   $isSwitch  = $false
  $evalParamValue     = "" # no type specified because we don't know it yet

  if ($paramValue) # a parameter
  {
    $evalParamValue = Invoke-Expression $paramValue # evaluate expression to get actual value
  }
  else # a switch
  {
    $isSwitch = $true
  }

  # the parameter name we detected may be an alias => translate it into the real name
  [string] $realParamName = Get-CommandParameterName -command "$PSScriptRoot\..\clang-build.ps1" `
                                                     -nameOrAlias $paramName
  if (!$realParamName)
  {
    Write-Output "OVERVIEW: Clang Power Tools: compiles or tidies up code from Visual Studio .vcxproj project files`n"

    Write-Output "USAGE: clang-build.ps1 [options]`n"

    Write-Output "OPTIONS: "
    Print-CommandParameters "$PSScriptRoot\..\clang-build.ps1"

    Fail-Script "Unsupported option '$paramName'. Check cpt.config."
  }

  if ($isSwitch)
  {
    Set-Var -name $realParamName -value $true -asScriptParameter
  }
  else
  {
    Set-Var -name $realParamName -value $evalParamValue -asScriptParameter
  }
}

Function Get-ConfigFileParameters()
{
  [System.Collections.Hashtable] $retArgs = @{}

  [string] $startDir = If ( VariableExistsAndNotEmpty 'ProjectDir' )  { $ProjectDir } else { $aSolutionsPath }
  [string] $configFile = (cpt::GetDirNameOfFileAbove -startDir $startDir -targetFile "cpt.config") + "\cpt.config"
  if (!(Test-Path -LiteralPath $configFile))
  {
      return $retArgs
  }
  Write-Verbose "Found cpt.config in $configFile"

  [xml] $configXml = Get-Content $configFile
  $configXpathNS= New-Object System.Xml.XmlNamespaceManager($configXml.NameTable)
  $configXpathNS.AddNamespace("ns", $configXml.DocumentElement.NamespaceURI)

  [System.Xml.XmlElement[]] $argElems = $configXml.SelectNodes("/ns:cpt-config/*", $configXpathNS)

  foreach ($argEl in $argElems)
  {
    if ($argEl.Name.StartsWith("vsx-"))
    {
        continue # settings for the Visual Studio Extension
    }

    if ($argEl.HasAttribute("Condition"))
    {
      [bool] $isApplicable = Evaluate-MSBuildCondition -condition $argEl.GetAttribute("Condition")
      if (!$isApplicable)
      {
        continue
      }
    }
    $retArgs[$argEl.Name] = $argEl.InnerText
  }

  return $retArgs
}

Function Update-ParametersFromConfigFile()
{
  [System.Collections.Hashtable] $configParams = Get-ConfigFileParameters
  if (!$configParams)
  {
      return
  }

  foreach ($paramName in $configParams.Keys)
  {
    UpdateScriptParameter -paramName $paramName -paramValue $configParams[$paramName]
  }
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

    Set-Var -name "UserRootDir"              -value "$LocalAppData\Microsoft\MSBuild\v4.0"
    # These would enable full project platform references parsing, experimental right now
    if ($env:CPT_LOAD_ALL -eq '1')
    {
        Set-Var -name "ConfigurationType"        -value "Application"
        Set-Var -name "VCTargetsPath"            -value "$(Get-VisualStudio-Path)\Common7\IDE\VC\VCTargets\"
        Set-Var -name "VsInstallRoot"            -value (Get-VisualStudio-Path)
        Set-Var -name "MSBuildExtensionsPath"    -value "$(Get-VisualStudio-Path)\MSBuild"
        Set-Var -name "LocalAppData"             -value $env:LOCALAPPDATA
        Set-Var -name "UniversalCRT_IncludePath" -value "${Env:ProgramFiles(x86)}\Windows Kits\10\Include\10.0.10240.0\ucrt"
    }

    [string] $vsVer = (Get-VisualStudio-VersionNumber $global:cptVisualStudioVersion)
    Set-Var -name "VisualStudioVersion"    -value $vsVer
    Set-Var -name "MSBuildToolsVersion"    -value $vsVer

    [string] $projectSlnPath = Get-ProjectSolution
    [string] $projectSlnDir = Get-FileDirectory -filePath $projectSlnPath
    Set-Var -name "SolutionDir" -value $projectSlnDir
    [string] $projectSlnName = Get-FileName -path $projectSlnPath -noext
    Set-Var -name "SolutionName" -value $projectSlnName

    # pre-initialize Configuration and Platform properties
    if ( VariableExistsAndNotEmpty -name 'aVcxprojConfigPlatform')
    {
        Detect-ProjectDefaultConfigPlatform
    }

    Update-ParametersFromConfigFile
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
   Sets the Configuration and Platform project properties so that
   conditions can be properly evaluated.
#>
function Detect-ProjectDefaultConfigPlatform()
{
    [string] $configPlatformName = ""

    # detect the first platform/config pair from the project itemgroup
    $configItems = @(Get-Project-ItemList "ProjectConfiguration")

    if (![string]::IsNullOrWhiteSpace($global:cptCurrentConfigPlatform))
    {
        # we have script parameters we can use to set the platform/config
        $configPlatformName = $global:cptCurrentConfigPlatform
    }
    

    if ((!$configItems -or $configItems.Count -eq 0))
    {
        if ([string]::IsNullOrWhiteSpace($configPlatformName))
        {
            throw [ProjectConfigurationNotFound]::new($global:vcxprojPath, "");
        }
    }
    else 
    {
        $targetConfiguration = $null

        if ([string]::IsNullOrEmpty($configPlatformName))
        {
            $targetConfiguration = $configItems[0]
        }
        else 
        {
            foreach ($configItem in $configItems)
            {
                [string] $platformName = $configItem[0]
                if ($platformName -ieq $configPlatformName)
                {
                    $targetConfiguration = $configItem
                
                    break
                }
            }
            if ($null -eq $targetConfiguration)
            {
                throw [ProjectConfigurationNotFound]::new($global:vcxprojPath, $configPlatformName);
            }
        }
        
        $configPlatformName = $targetConfiguration[0]
    }

    $global:cptCurrentConfigPlatform = $configPlatformName

    [string[]] $configAndPlatform = $configPlatformName.Split('|')
    Set-Var -Name "Configuration" -Value $configAndPlatform[0]
    Set-Var -Name "Platform"      -Value $configAndPlatform[1]
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
        if (!$relPath)
        {
            return
        }
        [string[]] $paths = Canonize-Path -base (Get-Location) -child $relPath -ignoreErrors

        foreach ($path in $paths)
        {
            if (![string]::IsNullOrEmpty($path) -and (Test-Path -LiteralPath $path))
            {
                Write-Verbose "Property sheet: $path"
                ParseProjectFile($path)
            }
            else
            {
                Write-Verbose "Could not find property sheet $relPath"
                if ($relPath -like "\Microsoft.Cpp.props")
                {
                    # now we can begin to evaluate directory.build.props XML element conditions, load it
                    LoadDirectoryBuildPropSheetFile
                }
            }
        }
    }

    if ( ($node.Name -ieq "ClCompile" -or $node.Name -ieq "ClInclude") -and
        ![string]::IsNullOrEmpty($node.GetAttribute("Include")) )
    {
        [string] $expandedAttr = Evaluate-MSBuildExpression $node.GetAttribute("Include")
        $node.Attributes["Include"].Value = $expandedAttr
    }

    if ($node.Name -ieq "Otherwise")
    {
        [System.Xml.XmlElement[]] $siblings = @($node.ParentNode.ChildNodes | `
            Where-Object { $_.GetType().Name -ieq "XmlElement" -and $_ -ne $node })
        if ($siblings.Count -gt 0)
        {
            # means there's a <When> element that matched
            # <Otherwise> should not be evaluated, we could set unwated properties
            return
        }
    }

    if ($node.Name -ieq "ItemGroup")
    {
        [string] $oldItemContextName = Get-ProjectItemContext
        foreach ($child in $node.ChildNodes)
        {
            if ($child.GetType().Name -ine "XmlElement")
            {
                continue
            }

            [string] $childEvaluatedValue = Evaluate-MSBuildExpression $child.GetAttribute("Include")
            $itemProperties = @{}

            Set-ProjectItemContext $child.Name
            $contextProperties = Get-ProjectItemProperty
            if ($contextProperties -ne $null)
            {
                foreach ($k in $contextProperties.Keys)
                {
                    $itemProperties[$k] = $contextProperties[$k]
                }
            }

            foreach ($nodePropChild in $child.ChildNodes)
            {
                if ($nodePropChild.GetType().Name -ine "XmlElement")
                {
                    continue
                }                

                if ($nodePropChild.HasAttribute("Condition"))
                {
                    [string] $nodeCondition = $nodePropChild.GetAttribute("Condition")
                    [bool] $conditionSatisfied = ((Evaluate-MSBuildCondition($nodeCondition)) -eq $true)
                    if (!$conditionSatisfied)
                    {
                        continue
                    }
                }

                $itemProperties[$nodePropChild.Name] = Evaluate-MSBuildExpression $nodePropChild.InnerText
            }

            Add-Project-Item -name $child.Name -value $childEvaluatedValue -properties $itemProperties
        }

        Set-ProjectItemContext $oldItemContextName

        if ($node.GetAttribute("Label") -ieq "ProjectConfigurations")
        {
            Detect-ProjectDefaultConfigPlatform
        }
    }

    if ($node.Name -ieq "ItemDefinitionGroup")
    {
        foreach ($child in $node.ChildNodes)
        {
            if ($child.GetType().Name -ine "XmlElement")
            {
                continue
            }

            Push-ProjectItemContext $child.Name

            foreach ($propNode in $child.ChildNodes)
            {
                if ($propNode.GetType().Name -ine "XmlElement")
                {
                    continue
                }

                [string] $propVal = Evaluate-MSBuildExpression $propNode.InnerText
                Set-ProjectItemProperty $propNode.Name $propVal
            }

            Pop-ProjectItemContext
        }
    }

    if ($node.ParentNode -and $node.ParentNode.Name -ieq "PropertyGroup")
    {
        # set new property value
        [string] $propertyName = $node.Name
        [string] $propertyValue = Evaluate-MSBuildExpression $node.InnerText

        Set-Var -Name $propertyName -Value $propertyValue

        return
    }

    if ($node.ChildNodes.Count -eq 0)
    {
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
            $nodesToRemove.Add($child) > $null
            continue
        }
        else
        {
            SanitizeProjectNode($child)
        }
    }

    foreach ($nodeToRemove in $nodesToRemove)
    {
        $nodeToRemove.ParentNode.RemoveChild($nodeToRemove) > $null
    }
}

<#
.DESCRIPTION
   Parses a project file and loads data into corresponding data structures.
   Project elements that are conditioned will be evaluated and discarded if their
   condition is evaluted to False.
#>
function ParseProjectFile([string] $projectFilePath)
{
    # keep current file path, we'll need to restore it
    [string] $currentFile = ""
    if (VariableExistsAndNotEmpty 'MSBuildThisFileFullPath')
    {
        $currentFile = $MSBuildThisFileFullPath
    }

    Write-Verbose "`nSanitizing $projectFilePath"

    [xml] $fileXml = Get-Content -LiteralPath $projectFilePath

    Push-Location -LiteralPath (Get-FileDirectory -filePath $projectFilePath)

    InitializeMsBuildCurrentFileProperties -filePath $projectFilePath
    SanitizeProjectNode($fileXml.Project)

    Pop-Location
    
    # restore previous path
    if (![string]::IsNullOrWhiteSpace(($currentFile)))
    {
        Write-Verbose "[INFO] Restoring project file properties localstate to parent file"
        InitializeMsBuildCurrentFileProperties -filePath $currentFile
    }
}

function LoadDirectoryBuildPropSheetFile()
{
    if ($env:CPT_LOAD_ALL -ne "1")
    {
        # Tries to find a Directory.Build.props property sheet, starting from the
        # project directory, going up. When one is found, the search stops.
        # Multiple Directory.Build.props sheets are not supported.
        [string] $directoryBuildSheetPath = (cpt::GetDirNameOfFileAbove -startDir $ProjectDir `
                                             -targetFile "Directory.Build.props") + "\Directory.Build.props"
        if (Test-Path -LiteralPath $directoryBuildSheetPath)
        {
            ParseProjectFile($directoryBuildSheetPath)
        }

        [string] $vcpkgIncludePath = "$env:LOCALAPPDATA\vcpkg\vcpkg.user.targets"
        if (Test-Path -LiteralPath $vcpkgIncludePath)
        {
            ParseProjectFile($vcpkgIncludePath)
        }
    }
}

<#
.DESCRIPTION
Loads vcxproj and property sheets into memory. This needs to be called only once
when processing a project. Accessing project data can be done using ItemGroups and Properties
#>
function LoadProject([string] $vcxprojPath)
{
    # Clean global variables that have been set by a previous project load
    Clear-Vars

    $global:vcxprojPath = $vcxprojPath

    InitializeMsBuildProjectProperties

    ParseProjectFile -projectFilePath $global:vcxprojPath
}
