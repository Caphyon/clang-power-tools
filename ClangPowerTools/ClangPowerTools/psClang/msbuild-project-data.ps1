#-------------------------------------------------------------------------------------------------
# PlatformToolset constants

Set-Variable -name kDefinesUnicode   -value @("-DUNICODE"
                                             ,"-D_UNICODE"
                                             ) `
                                     -option Constant

Set-Variable -name kDefinesMultiThreaded -value @("-D_MT") `
                                     -option Constant

Set-Variable -name kDefinesClangXpTargeting `
             -value @("-D_USING_V110_SDK71_") `
             -option Constant

Set-Variable -name kIncludePathsXPTargetingSDK  `
             -value "${Env:ProgramFiles(x86)}\Microsoft SDKs\Windows\v7.1A\Include"  `
             -option Constant

Set-Variable -name kVStudioDefaultPlatformToolset -Value "v141" -option Constant

Set-Variable -name kClangFlag32BitPlatform        -value "-m32" -option Constant

# ------------------------------------------------------------------------------------------------
# Xpath selectors

Set-Variable -name kVcxprojXpathPreprocessorDefs  `
             -value "ns:Project/ns:ItemDefinitionGroup/ns:ClCompile/ns:PreprocessorDefinitions" `
             -option Constant

Set-Variable -name kVcxprojXpathAdditionalIncludes `
             -value "ns:Project/ns:ItemDefinitionGroup/ns:ClCompile/ns:AdditionalIncludeDirectories" `
             -option Constant

Set-Variable -name kVcxprojXpathRuntimeLibrary `
             -value "ns:Project/ns:ItemDefinitionGroup/ns:ClCompile/ns:RuntimeLibrary" `
             -option Constant

Set-Variable -name kVcxprojXpathHeaders `
             -value "ns:Project/ns:ItemGroup/ns:ClInclude" `
             -option Constant

Set-Variable -name kVcxprojXpathCompileFiles `
             -value "ns:Project/ns:ItemGroup/ns:ClCompile" `
             -option Constant

Set-Variable -name kVcxprojXpathWinPlatformVer `
             -value "ns:Project/ns:PropertyGroup/ns:WindowsTargetPlatformVersion" `
             -option Constant

Set-Variable -name kVcxprojXpathForceIncludes `
             -value "ns:Project/ns:ItemDefinitionGroup/ns:ClCompile/ns:ForcedIncludeFiles" `
             -option Constant

Set-Variable -name kVcxprojXpathPCH `
             -value "ns:Project/ns:ItemGroup/ns:ClCompile/ns:PrecompiledHeader[text()='Create']" `
             -option Constant

Set-Variable -name kVcxprojXpathToolset `
             -value "ns:Project/ns:PropertyGroup[@Label='Configuration']/ns:PlatformToolset" `
             -option Constant

Set-Variable -name kVcxprojXpathCppStandard `
             -value "ns:Project/ns:ItemDefinitionGroup/ns:ClCompile/ns:LanguageStandard" `
             -option Constant


Set-Variable -name kVcxprojXpathProjectCompileAs `
             -value "ns:Project/ns:ItemDefinitionGroup/ns:ClCompile/ns:CompileAs" `
             -option Constant

# ------------------------------------------------------------------------------------------------
# Default platform sdks and standard

Set-Variable -name kVSDefaultWinSDK            -value '8.1'             -option Constant
Set-Variable -name kVSDefaultWinSDK_XP         -value '7.0'             -option Constant
Set-Variable -name kDefaultCppStd              -value "stdcpp14"        -option Constant

# ------------------------------------------------------------------------------------------------
Set-Variable -name kCProjectCompile         -value "CompileAsC" -option Constant

Add-Type -TypeDefinition @"
  public enum UsePch
  {
    Use,
    NotUsing,
    Create
  }
"@

Function Should-CompileProject([Parameter(Mandatory = $true)][string] $vcxprojPath)
{
    if ($aVcxprojToCompile -eq $null)
    {
        return $true
    }

    foreach ($projMatch in $aVcxprojToCompile)
    {
        if (IsFileMatchingName -filePath $vcxprojPath -matchName $projMatch)
        {
            return $true
        }
    }

    return $false
}

Function Should-IgnoreProject([Parameter(Mandatory = $true)][string] $vcxprojPath)
{
    if ($aVcxprojToIgnore -eq $null)
    {
        return $false
    }

    foreach ($projIgnoreMatch in $aVcxprojToIgnore)
    {
        if (IsFileMatchingName -filePath $vcxprojPath -matchName $projIgnoreMatch)
        {
            return $true
        }
    }

    return $false
}

Function Should-CompileFile([Parameter(Mandatory = $false)][System.Xml.XmlNode] $fileNode
    , [Parameter(Mandatory = $false)][string] $pchCppName
)
{
    if ($fileNode -eq $null)
    {
        return $false
    }

    [string] $file = $fileNode.Include

    if (($file -eq $null) -or (![string]::IsNullOrEmpty($pchCppName) -and ($file -eq $pchCppName)))
    {
        return $false
    }

    [System.Xml.XmlNode] $excluded = $fileNode.SelectSingleNode("ns:ExcludedFromBuild", $global:xpathNS)

    if (($excluded -ne $null) -and ($excluded.InnerText -ne $null) -and ($excluded.InnerText -ieq "true"))
    {
        return $false
    }

    return $true
}

Function Should-IgnoreFile([Parameter(Mandatory = $true)][string] $file)
{
    if ($aCppToIgnore -eq $null)
    {
        return $false
    }

    foreach ($projIgnoreMatch in $aCppToIgnore)
    {
        if (IsFileMatchingName -filePath $file -matchName $projIgnoreMatch)
        {
            return $true
        }
    }

    return $false
}

Function Get-ProjectFilesToCompile([Parameter(Mandatory = $false)][string] $pchCppName)
{
    [System.Xml.XmlElement[]] $projectEntries = Select-ProjectNodes($kVcxprojXpathCompileFiles) | `
        Where-Object { Should-CompileFile -fileNode $_ -pchCppName $pchCppName }

    [System.Collections.ArrayList] $files = @()
    foreach ($entry in $projectEntries)
    {
        [string[]] $matchedFiles = Canonize-Path -base $ProjectDir -child $entry.GetAttribute("Include")
        [UsePch] $usePch = [UsePch]::Use

        $nodePch = $entry.SelectSingleNode('ns:PrecompiledHeader', $global:xpathNS)
        if ($nodePch -and ![string]::IsNullOrEmpty($nodePch.'#text'))
        {
            switch ($nodePch.'#text')
            {
                'NotUsing' { $usePch = [UsePch]::NotUsing }
                'Create'   { $usePch = [UsePch]::Create   }
            }
        }

        if ($matchedFiles.Count -gt 0)
        {
            foreach ($file in $matchedFiles)
            {
                $files += New-Object PsObject -Prop @{ "File"= $file;
                                                       "Pch" = $usePch; }
            }
        }
    }

    if ($files.Count -gt 0)
    {
        $files = @($files | Where-Object { ! (Should-IgnoreFile -file $_.File) })
    }

    return $files
}

Function Get-ProjectHeaders()
{
    [string[]] $headers = Select-ProjectNodes($kVcxprojXpathHeaders) | ForEach-Object {$_.Include }

    [string[]] $headerPaths = @()

    foreach ($headerEntry in $headers)
    {
        [string[]] $paths = Canonize-Path -base $ProjectDir -child $headerEntry -ignoreErrors
        if ($paths.Count -gt 0)
        {
            $headerPaths += $paths
        }
    }
    return $headerPaths
}

Function Is-CProject()
{
    [string] $compileAs = (Select-ProjectNodes($kVcxprojXpathProjectCompileAs)).InnerText
    return $compileAs -eq $kCProjectCompile
}

Function Get-Project-SDKVer()
{
    [string] $sdkVer = (Select-ProjectNodes($kVcxprojXpathWinPlatformVer)).InnerText

    If ([string]::IsNullOrEmpty($sdkVer)) { "" } Else { $sdkVer.Trim() }
}

Function Is-Project-MultiThreaded()
{
    $propGroup = Select-ProjectNodes($kVcxprojXpathRuntimeLibrary)

    $runtimeLibrary = $propGroup.InnerText

    return ![string]::IsNullOrEmpty($runtimeLibrary)
}

Function Is-Project-Unicode()
{
    $propGroup = Select-ProjectNodes("ns:Project/ns:PropertyGroup[@Label='Configuration']/ns:CharacterSet")
    if (! $propGroup)
    {
        return $false
    }
    return ($propGroup.InnerText -ieq "Unicode")
}

Function Get-Project-CppStandard()
{
    [string] $cachedValueVarName = "ClangPowerTools:CppStd"

    [string] $cachedVar = (Get-Variable $cachedValueVarName -ErrorAction SilentlyContinue -ValueOnly)
    if (![string]::IsNullOrEmpty($cachedVar))
    {
        return $cachedVar
    }

    [string] $cppStd = ""

    $cppStdNode = Select-ProjectNodes($kVcxprojXpathCppStandard)
    if ($cppStdNode)
    {
        $cppStd = $cppStdNode.InnerText
    }
    else
    {
        $cppStd = $kDefaultCppStd
    }

    $cppStdMap = @{ 'stdcpplatest' = 'c++1z'
        ; 'stdcpp14'               = 'c++14'
        ; 'stdcpp17'               = 'c++17'
    }

    [string] $cppStdClangValue = $cppStdMap[$cppStd]
    Set-Var -name $cachedValueVarName -value $cppStdClangValue

    return $cppStdClangValue
}

Function Get-ClangCompileFlags([Parameter(Mandatory = $false)][bool] $isCpp = $true)
{
    [string[]] $flags = $aClangCompileFlags
    if ($isCpp -and !($flags -match "-std=.*"))
    {
        [string] $cppStandard = Get-Project-CppStandard

        $flags = @("-std=$cppStandard") + $flags
    }

    if ($Platform -ieq "x86" -or $Platform -ieq "Win32")
    {
        $flags += @($kClangFlag32BitPlatform)
    }

    return $flags
}

Function Get-ProjectPlatformToolset()
{
    $propGroup = Select-ProjectNodes($kVcxprojXpathToolset)

    $toolset = $propGroup.InnerText

    if ($toolset)
    {
        return $toolset
    }
    else
    {
        return $kVStudioDefaultPlatformToolset
    }
}

Function Get-ProjectIncludeDirectories()
{
    [string[]] $returnArray = ($IncludePath -split ";")                                   | `
        Where-Object { ![string]::IsNullOrWhiteSpace($_) }                                | `
        ForEach-Object { Canonize-Path -base $ProjectDir -child $_.Trim() -ignoreErrors } | `
        Where-Object { ![string]::IsNullOrEmpty($_) }                                     | `
        ForEach-Object { $_ -replace '\\$', '' }
    if ($env:CPT_LOAD_ALL -eq '1')
    {
        return $returnArray
    }

    [string] $vsPath = Get-VisualStudio-Path
    Write-Verbose "Visual Studio location: $vsPath"

    [string] $platformToolset = Get-ProjectPlatformToolset

    if ($global:cptVisualStudioVersion -eq "2015")
    {
        $returnArray += Get-VisualStudio-Includes -vsPath $vsPath
    }
    else
    {
        $mscVer = Get-MscVer -visualStudioPath $vsPath
        Write-Verbose "MSCVER: $mscVer"

        $returnArray += Get-VisualStudio-Includes -vsPath $vsPath -mscVer $mscVer
    }

    $sdkVer = Get-Project-SDKVer

    # We did not find a WinSDK version in the vcxproj. We use Visual Studio's defaults
    if ([string]::IsNullOrEmpty($sdkVer))
    {
        if ($platformToolset.EndsWith("xp"))
        {
            $sdkVer = $kVSDefaultWinSDK_XP
        }
        else
        {
            $sdkVer = $kVSDefaultWinSDK
        }
    }

    Write-Verbose "WinSDK version: $sdkVer"

    # ----------------------------------------------------------------------------------------------
    # Windows 10

    if ((![string]::IsNullOrEmpty($sdkVer)) -and ($sdkVer.StartsWith("10")))
    {
        $returnArray += @("${Env:ProgramFiles(x86)}\Windows Kits\10\Include\$sdkVer\ucrt")

        if ($platformToolset.EndsWith("xp"))
        {
            $returnArray += @($kIncludePathsXPTargetingSDK)
        }
        else
        {
            $returnArray += @( "${Env:ProgramFiles(x86)}\Windows Kits\10\Include\$sdkVer\um"
                , "${Env:ProgramFiles(x86)}\Windows Kits\10\Include\$sdkVer\shared"
                , "${Env:ProgramFiles(x86)}\Windows Kits\10\Include\$sdkVer\winrt"
                , "${Env:ProgramFiles(x86)}\Windows Kits\10\Include\$sdkVer\cppwinrt"
            )
        }
    }

    # ----------------------------------------------------------------------------------------------
    # Windows 8 / 8.1

    if ((![string]::IsNullOrEmpty($sdkVer)) -and ($sdkVer.StartsWith("8.")))
    {
        $returnArray += @("${Env:ProgramFiles(x86)}\Windows Kits\10\Include\10.0.10240.0\ucrt")

        if ($platformToolset.EndsWith("xp"))
        {
            $returnArray += @($kIncludePathsXPTargetingSDK)
        }
        else
        {
            $returnArray += @( "${Env:ProgramFiles(x86)}\Windows Kits\$sdkVer\Include\um"
                , "${Env:ProgramFiles(x86)}\Windows Kits\$sdkVer\Include\shared"
                , "${Env:ProgramFiles(x86)}\Windows Kits\$sdkVer\Include\winrt"
            )
        }
    }

    # ----------------------------------------------------------------------------------------------
    # Windows 7

    if ((![string]::IsNullOrEmpty($sdkVer)) -and ($sdkVer.StartsWith("7.0")))
    {
        $returnArray += @("$vsPath\VC\Auxiliary\VS\include")

        if ($platformToolset.EndsWith("xp"))
        {
            $returnArray += @( "${Env:ProgramFiles(x86)}\Windows Kits\10\Include\10.0.10240.0\ucrt"
                , $kIncludePathsXPTargetingSDK
            )
        }
        else
        {
            $returnArray += @( "${Env:ProgramFiles(x86)}\Windows Kits\10\Include\7.0\ucrt")
        }
    }

    return ( $returnArray | ForEach-Object { Remove-PathTrailingSlash -path $_ } )
}

<#
.DESCRIPTION
  Retrieve directory in which the PCH CPP resides (e.g. stdafx.cpp, stdafxA.cpp)
#>
Function Get-Project-PchCpp()
{
    $pchCppRelativePath = Select-ProjectNodes($kVcxprojXpathPCH)   |
        Select-Object -ExpandProperty ParentNode |
        Select-Object -first 1                   |
        Select-Object -ExpandProperty Include

    return $pchCppRelativePath
}


<#
.DESCRIPTION
  Retrieve array of preprocessor definitions for a given project, in Clang format (-DNAME )
#>
Function Get-ProjectPreprocessorDefines()
{
    [string[]] $tokens = (Select-ProjectNodes $kVcxprojXpathPreprocessorDefs).InnerText -split ";"

    # make sure we add the required prefix and escape double quotes
    [string[]]$defines = ( $tokens | `
            ForEach-Object { $_.Trim() } | `
            Where-Object { $_ } | `
            ForEach-Object { '"' + $(($kClangDefinePrefix + $_) -replace '"', '\"') + '"' } )

    if (Is-Project-Unicode)
    {
        $defines += $kDefinesUnicode
    }

    if (Is-Project-MultiThreaded)
    {
        $defines += $kDefinesMultiThreaded
    }

    [string] $platformToolset = Get-ProjectPlatformToolset
    if ($platformToolset.EndsWith("xp"))
    {
        $defines += $kDefinesClangXpTargeting
    }

    return $defines
}

Function Get-ProjectAdditionalIncludes()
{
    [string[]] $tokens = @()

    $data = Select-ProjectNodes $kVcxprojXpathAdditionalIncludes
    $tokens += ($data).InnerText -split ";"

    foreach ($token in $tokens)
    {
        if ([string]::IsNullOrWhiteSpace($token))
        {
            continue
        }

        [string] $includePath = Canonize-Path -base $ProjectDir -child $token.Trim() -ignoreErrors
        if (![string]::IsNullOrEmpty($includePath))
        {
            $includePath -replace '\\$', ''
        }
    }
}

Function Get-ProjectForceIncludes()
{
    [System.Xml.XmlElement] $forceIncludes = Select-ProjectNodes $kVcxprojXpathForceIncludes
    if ($forceIncludes)
    {
        return $forceIncludes.InnerText -split ";"
    }

    return $null
}

<#
.DESCRIPTION
  Retrieve directory in which stdafx.h resides
#>
Function Get-ProjectStdafxDir( [Parameter(Mandatory = $true)]  [string]   $pchHeaderName
    , [Parameter(Mandatory = $false)] [string[]] $includeDirectories
    , [Parameter(Mandatory = $false)] [string[]] $additionalIncludeDirectories
)
{
    [string] $stdafxPath = ""

    [string[]] $projectHeaders = Get-ProjectHeaders
    if ($projectHeaders.Count -gt 0)
    {
        # we need to use only backslashes so that we can match against file header paths
        $pchHeaderName = $pchHeaderName.Replace("/", "\")

        $stdafxPath = $projectHeaders | Where-Object { (Get-FileName -path $_) -eq $pchHeaderName }
    }

    if ([string]::IsNullOrEmpty($stdafxPath))
    {
        [string[]] $searchPool = @($ProjectDir);
        if ($includeDirectories.Count -gt 0)
        {
            $searchPool += $includeDirectories
        }
        if ($additionalIncludeDirectories.Count -gt 0)
        {
            $searchPool += $additionalIncludeDirectories
        }

        foreach ($dir in $searchPool)
        {
            [string] $stdafxPath = Canonize-Path -base $dir -child $pchHeaderName -ignoreErrors
            if (![string]::IsNullOrEmpty($stdafxPath))
            {
                break
            }
        }
    }

    if ([string]::IsNullOrEmpty($stdafxPath))
    {
        return ""
    }
    else
    {
        [string] $stdafxDir = Get-FileDirectory($stdafxPath)
        return $stdafxDir
    }
}

Function Get-PchCppIncludeHeader([Parameter(Mandatory = $true)][string] $pchCppFile)
{
    [string] $cppPath = Canonize-Path -base $ProjectDir -child $pchCppFile

    [string[]] $fileLines = Get-Content -path $cppPath
    foreach ($line in $fileLines)
    {
        $regexMatch = [regex]::match($line, '^\s*#include\s+"(\S+)"')
        if ($regexMatch.Success)
        {
            return $regexMatch.Groups[1].Value
        }
    }
    return ""
}
