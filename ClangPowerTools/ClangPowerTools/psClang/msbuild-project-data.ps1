#-------------------------------------------------------------------------------------------------
# PlatformToolset constants

Set-Variable -name kDefinesUnicode   -value @('"-DUNICODE"'
                                             ,'"-D_UNICODE"'
                                             ) `
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
# Default platform sdks and standard

Set-Variable -name kVSDefaultWinSDK            -value '8.1'             -option Constant
Set-Variable -name kVSDefaultWinSDK_XP         -value '7.0'             -option Constant
Set-Variable -name kDefaultCppStd              -value "stdcpp14"        -option Constant

# ------------------------------------------------------------------------------------------------
Set-Variable -name kCProjectCompile         -value "CompileAsC" -option Constant

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

    foreach ($projIgnoreMatch in $global:cptIgnoredFilesPool)
    {
        if (IsFileMatchingName -filePath $file -matchName $projIgnoreMatch)
        {
            return $true
        }
    }

    return $false
}

Function Get-ProjectFilesToCompile()
{
    $projectCompileItems = @(Get-Project-ItemList "ClCompile")
    if (!$projectCompileItems)
    {
        Write-Verbose "Project does not have any items to compile"
        return @()
    }

    $files = @()
    foreach ($item in $projectCompileItems)
    {
        [System.Collections.Hashtable] $itemProps = $item[1];

        if ($itemProps -and $itemProps.ContainsKey('ExcludedFromBuild'))
        {
            if ($itemProps['ExcludedFromBuild'] -ieq 'true')
            {
                Write-Verbose "Skipping $($item[0]) because it is excluded from build"
                continue
            }
        }

        [string[]] $matchedFiles = @(Canonize-Path -base $ProjectDir -child $item[0] -ignoreErrors)
        if ($matchedFiles.Count -gt 0)
        {
            foreach ($file in $matchedFiles)
            {
                if ( (Should-IgnoreFile -file $file) )
                {
                    continue
                }

                $files += New-Object PsObject -Prop @{ "File"       = $file
                                                     ; "Properties" = $itemProps
                                                     }
            }
        }
    }

    return $files
}

Function Get-ProjectHeaders()
{
    $projectCompileItems = @(Get-Project-ItemList "ClInclude")

    [string[]] $headerPaths = @()

    foreach ($item in $projectCompileItems)
    {
        [string[]] $paths = @(Canonize-Path -base $ProjectDir -child $item[0] -ignoreErrors)
        if ($paths.Count -gt 0)
        {
            $headerPaths += $paths
        }
    }
    return $headerPaths
}

Function Get-Project-SDKVer()
{
    [string] $sdkVer = $WindowsTargetPlatformVersion

    If ([string]::IsNullOrEmpty($sdkVer)) { "" } Else { $sdkVer.Trim() }
}

Function Get-Project-MultiThreaded-Define()
{
    Set-ProjectItemContext "ClCompile"
    [string] $runtimeLibrary = Get-ProjectItemProperty "RuntimeLibrary"

    # /MT or /MTd
    if (@("MultiThreaded", "MultiThreadedDebug") -contains $runtimeLibrary)
    {
        return @('"-D_MT"')
    }

    # /MD or /MDd
    if (@("MultiThreadedDLL", "MultiThreadedDebugDLL") -contains $runtimeLibrary)
    {
        return @('"-D_MT"', '"-D_DLL"')
    }

    return @('"-D_MT"') # default value /MT
}

Function Is-Project-Unicode()
{
    return $CharacterSet -ieq "Unicode"
}

Function Get-Project-CppStandard()
{
    Set-ProjectItemContext "ClCompile"
    $cppStd = Get-ProjectItemProperty "LanguageStandard"
    if (!$cppStd)
    {
        $cppStd = $kDefaultCppStd
    }

    $cppStdMap = @{ 'stdcpplatest' = 'c++1z'
                  ; 'stdcpp14'     = 'c++14'
                  ; 'stdcpp17'     = 'c++17'
                  }

    [string] $cppStdClangValue = $cppStdMap[$cppStd]

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
    $toolset = $PlatformToolset

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
    [string[]] $returnArray = @()

    if ( (VariableExists 'IncludePath') )
    {
        $returnArray += ($IncludePath -split ";")                                                         | `
                        Where-Object { ![string]::IsNullOrWhiteSpace($_) }                                | `
                        ForEach-Object { Canonize-Path -base $ProjectDir -child $_.Trim() -ignoreErrors } | `
                        Where-Object { ![string]::IsNullOrEmpty($_) }                                     | `
                        ForEach-Object { $_ -replace '\\$', '' }
    }
    if ($env:CPT_LOAD_ALL -eq '1')
    {
        return $returnArray
    }

    [string] $vsPath = Get-VisualStudio-Path
    Write-Verbose "Visual Studio location: $vsPath"

    [string] $platformToolset = Get-ProjectPlatformToolset

    if (([int] $global:cptVisualStudioVersion) -lt 2017)
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
  Retrieve array of preprocessor definitions for a given project, in Clang format (-DNAME )
#>
Function Get-ProjectPreprocessorDefines()
{
    Set-ProjectItemContext "ClCompile"
    $preprocDefNodes = Get-ProjectItemProperty "PreprocessorDefinitions"
    if (!$preprocDefNodes)
    {
        return @()
    }

    [string[]] $tokens = @($preprocDefNodes -split ";")

    # make sure we add the required prefix and escape double quotes
    [string[]]$defines = @( $tokens | `
            ForEach-Object { $_.Trim() } | `
            Where-Object { $_ } | `
            ForEach-Object { '"' + $(($kClangDefinePrefix + $_) -replace '"', '\"') + '"' } )

    if (Is-Project-Unicode)
    {
        $defines += $kDefinesUnicode
    }

    $defines += @(Get-Project-MultiThreaded-Define)

    [string] $platformToolset = Get-ProjectPlatformToolset
    if ($platformToolset.EndsWith("xp"))
    {
        $defines += $kDefinesClangXpTargeting
    }

    return $defines
}

Function Get-ProjectAdditionalIncludes()
{
    Set-ProjectItemContext "ClCompile"
    $data = Get-ProjectItemProperty "AdditionalIncludeDirectories"

    [string[]] $tokens = @($data -split ";")

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
    Set-ProjectItemContext "ClCompile"
    $forceIncludes = Get-ProjectItemProperty "ForcedIncludeFiles"
    if ($forceIncludes)
    {
        return @($forceIncludes -split ";" | Where-Object { ![string]::IsNullOrWhiteSpace($_) })
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

    [string[]] $projectHeaders = @(Get-ProjectHeaders)
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

    [string[]] $fileLines = @(Get-Content -path $cppPath)
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
