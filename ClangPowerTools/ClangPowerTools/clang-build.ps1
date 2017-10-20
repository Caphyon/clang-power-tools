#Requires -Version 3
<#
.SYNOPSIS
    Compiles or tidies up code from Visual Studio .vcxproj project files.

.DESCRIPTION
    This PowerShell script scans for all .vcxproj Visual Studio projects inside a source directory.
    One or more of these projects will be compiled or tidied up (modernized), using Clang.

.PARAMETER aDirectory
    Alias 'dir'. Source directory to process.

.PARAMETER aVcxprojToCompile
    Alias 'proj'. Array of project(s) to compile. If empty, all projects are compiled.
    If the -literal switch is present, name is matched exactly. Otherwise, regex matching is used, 
    e.g. "msicomp" compiles all projects containing 'msicomp'.
    
    Can be passed as comma separated values.

.PARAMETER aVcxprojToIgnore
    Alias 'proj-ignore'. Array of project(s) to ignore, from the matched ones. 
    If empty, all already matched projects are compiled.
    If the -literal switch is present, name is matched exactly. Otherwise, regex matching is used, 
    e.g. "msicomp" ignores projects containing 'msicomp'.

    Can be passed as comma separated values.

.PARAMETER aCppToCompile
    Alias 'file'. What cpp(s) to compile from the found project(s). If empty, all CPPs are compiled.
    If the -literal switch is present, name is matched exactly. Otherwise, regex matching is used, 
    e.g. "table" compiles all CPPs containing 'table'.

.PARAMETER aCppToIgnore
    Alias 'file-ignore'. Array of file(s) to ignore, from the matched ones. 
    If empty, all already matched files are compiled.
    If the -literal switch is present, name is matched exactly. Otherwise, regex matching is used, 
    e.g. "table" ignores all CPPs containing 'table'.

    Can be passed as comma separated values.

.PARAMETER aIncludeDirectories
    Alias 'includeDirs'. Directories to be used for includes (libraries, helpers, etc).

.PARAMETER aUseParallelCompile
    Alias 'parallel'. Switch to run in parallel mode, on all logical CPU cores.

.PARAMETER aContinueOnError
     Alias 'continue'. Switch to continue project compilation even when errors occur.

.PARAMETER aClangCompileFlags
     Alias 'clang-flags'. Flags given to clang++ when compiling project, 
     alongside project-specific defines.

.PARAMETER aDisableNameRegexMatching
     Alias 'literal'. Switch to take project and cpp name filters literally, not by regex matching.

.PARAMETER aTidyFlags
      Alias 'tidy'. If not empty clang-tidy will be called with given flags, instead of clang++. 
      The tidy operation is applied to whole translation units, meaning all directory headers 
      included in the CPP will be tidied up too. Changes will not be applied, only simulated.

      If aTidyFixFlags is present, it takes precedence over this parameter.
      
.PARAMETER aTidyFixFlags
      Alias 'tidy-fix'. If not empty clang-tidy will be called with given flags, instead of clang++. 
      The tidy operation is applied to whole translation units, meaning all directory headers 
      included in the CPP will be tidied up too. Changes will be applied to the file(s).

      If present, this parameter takes precedence over aTidyFlags.

.PARAMETER aVisualStudioVersion
      Alias 'vs-ver'. Version of Visual Studio (VC++) installed and that'll be used for 
      standard library include directories. E.g. 2017.

.PARAMETER aVisualStudioSku
      Alias 'vs-sku'. Sku of Visual Studio (VC++) installed and that'll be used for 
      standard library include directories. E.g. Professional.

.PARAMETER aDefaultWinSdkVersion
      Alias 'win-sdk-ver'. Default version of Windows SDK to be used for default include directories.
      It is used only when the project does not explictly specify a Windows Target Platform Version.
      
      Only Windows 10 SDKs versions are supported.

      If not given as parameter and project doesn't specify it either , no WinSDK include paths are added. 
      E.g. 10.0.14393.0

.NOTES
    Author: Gabriel Diaconita
#>
param( [alias("dir")]          [Parameter(Mandatory=$true)] [string]   $aDirectory
     , [alias("proj")]         [Parameter(Mandatory=$false)][string[]] $aVcxprojToCompile
     , [alias("proj-ignore")]  [Parameter(Mandatory=$false)][string[]] $aVcxprojToIgnore
     , [alias("file")]         [Parameter(Mandatory=$false)][string]   $aCppToCompile
     , [alias("file-ignore")]  [Parameter(Mandatory=$false)][string[]] $aCppToIgnore
     , [alias("include-dirs")] [Parameter(Mandatory=$false)][string[]] $aIncludeDirectories
     , [alias("parallel")]     [Parameter(Mandatory=$false)][switch]   $aUseParallelCompile
     , [alias("continue")]     [Parameter(Mandatory=$false)][switch]   $aContinueOnError
     , [alias("clang-flags")]  [Parameter(Mandatory=$true)] [string[]] $aClangCompileFlags
     , [alias("literal")]      [Parameter(Mandatory=$false)][switch]   $aDisableNameRegexMatching
     , [alias("tidy")]         [Parameter(Mandatory=$false)][string]   $aTidyFlags
     , [alias("tidy-fix")]     [Parameter(Mandatory=$false)][string]   $aTidyFixFlags
     , [alias("vs-ver")]       [Parameter(Mandatory=$true)] [string]   $aVisualStudioVersion
     , [alias("vs-sku")]       [Parameter(Mandatory=$true)] [string]   $aVisualStudioSku
     , [alias("win-sdk-ver")]  [Parameter(Mandatory=$false)][string]   $aDefaultWinSdkVersion
     )

# System Architecture Constants
# ------------------------------------------------------------------------------------------------
       
Set-Variable -name kLogicalCoreCount -value (Get-WmiObject -class Win32_processor | `
                                             Select-Object -property NumberOfLogicalProcessors `
                                                           -ExpandProperty NumberOfLogicalProcessors) `
                                                                        -option Constant
# ------------------------------------------------------------------------------------------------
# Return Value Constants

Set-Variable -name kScriptFailsExitCode      -value  47                 -option Constant

# ------------------------------------------------------------------------------------------------
# File System Constants

Set-Variable -name kExtensionVcxproj         -value ".vcxproj"          -option Constant
Set-Variable -name kExtensionClangPch        -value ".clang.pch"        -option Constant

# ------------------------------------------------------------------------------------------------
# Vcxproj Related Constants

# filter used when looking for project additional includes and preprocessor definitions
Set-Variable -name kValidPlatformFilters -value @(
        '''$(Configuration)|$(Platform)''==''Debug|x64''',
        '''$(Configuration)|$(Platform)''==''Debug|Win32'''
    ) -option Constant

Set-Variable -name kVcxprojElemPreprocessorDefs  `
             -value                      "PreprocessorDefinitions"      -option Constant
Set-Variable -name kVcxprojElemAdditionalIncludes `
             -value                      "AdditionalIncludeDirectories" -option Constant
Set-Variable -name kVcxprojItemInheritedPreprocessorDefs `
             -value                      "%(PreprocessorDefinitions)"   -option Constant
Set-Variable -name kVcxprojItemInheritedAdditionalIncludes `
             -value "%(AdditionalIncludeDirectories)"                   -option Constant

Set-Variable -name kVStudioVarProjDir          -value '$(ProjectDir)'   -option Constant

# ------------------------------------------------------------------------------------------------
# Clang-Related Constants

Set-Variable -name kClangFlagSupressLINK    -value @("-fsyntax-only")   -option Constant
Set-Variable -name kClangFlagWarningIsError -value @("-Werror")         -option Constant
Set-Variable -name kClangFlagIncludePch     -value "-include-pch"       -option Constant
Set-Variable -name kClangFlagEmitPch        -value "-emit-pch"          -option Constant
Set-Variable -name kClangFlagMinusO         -value "-o"                 -option Constant

Set-Variable -name kClangDefinePrefix       -value "-D"                 -option Constant
Set-Variable -name kClangFlagNoUnusedArg    -value "-Wno-unused-command-line-argument" `
                                                                        -option Constant
Set-Variable -name kClangFlagNoMsInclude    -value "-Wno-microsoft-include" `
                                                                        -Option Constant
Set-Variable -name kClangFlagFileIsCPP      -value "-x c++"             -option Constant
Set-Variable -name kClangFlagForceInclude   -value "-include"           -option Constant

Set-Variable -name kClangCompiler             -value "clang++.exe"      -option Constant
Set-Variable -name kClangTidy                 -value "clang-tidy.exe"   -option Constant
Set-Variable -name kClangTidyFlags            -value @("-quiet"
                                                      ,"--")            -option Constant
Set-Variable -name kClangTidyFixFlags         -value @("-quiet"
                                                      ,"-fix-errors"
                                                      , "--")           -option Constant
Set-Variable -name kClangTidyFlagHeaderFilter -value "-header-filter="  -option Constant
Set-Variable -name kClangTidyFlagChecks       -value "-checks="         -option Constant

# ------------------------------------------------------------------------------------------------
# Default install locations of LLVM. If present there, we automatically use it

Set-Variable -name kLLVMInstallLocations    -value @("${Env:ProgramW6432}\LLVM\bin"
                                                    ,"${Env:ProgramFiles(x86)}\LLVM\bin"
                                                    )                   -option Constant

# ------------------------------------------------------------------------------------------------
# Helpers for locating Visual Studio on the computer

# VsWhere is available starting with Visual Studio 2017 version 15.2.
Set-Variable -name   kVsWhereLocation `
             -value  "${Env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" `
             -option Constant

# Default installation path of Visual Studio 2017. We'll use when VsWhere isn't available.
Set-Variable -name   kVs15DefaultLocation `
             -value  "${Env:ProgramFiles(x86)}\Microsoft Visual Studio\$aVisualStudioVersion\$aVisualStudioSku" `
             -option Constant

# Registry key containing information about Visual Studio 2015 installation path.
Set-Variable -name   kVs2015RegistryKey `
             -value  "HKLM:SOFTWARE\Wow6432Node\Microsoft\VisualStudio\14.0" `
             -option Constant

#-------------------------------------------------------------------------------------------------
# PlatformToolset-Related Constants

Set-Variable -name kDefinesUnicode   -value @("-DUNICODE"
                                             ,"-D_UNICODE"
                                             ) `
                                     -option Constant

Set-Variable -name kDefinesClangXpTargeting `
             -value @("-D_USING_V110_SDK71_") `
             -option Constant


Set-Variable -name kIncludePathsXPTargetingSDK  `
             -value "${Env:ProgramFiles(x86)}\Microsoft SDKs\Windows\v7.1A\Include"  `
             -option Constant

#-------------------------------------------------------------------------------------------------
# Custom Types

Add-Type -TypeDefinition @"
  public enum WorkloadType
  {
    Compile,
    Tidy,
    TidyFix
  }
"@

Set-Variable -name kVStudioDefaultPlatformToolset -Value "v141" -option Constant

#-------------------------------------------------------------------------------------------------
# Global functions

[System.Collections.ArrayList] $global:FilesToDeleteWhenScriptQuits = @()
[Boolean]                      $global:FoundErrors                  = $false

Function Exit-Script([Parameter(Mandatory=$false)][int] $code = 0)
{
  # Clean-up
  foreach ($file in $global:FilesToDeleteWhenScriptQuits)
  {
    Write-Verbose "Cleaning up $file"
    Remove-Item $file -ErrorAction SilentlyContinue | Out-Null
  }

  # Restore working directory
  Pop-Location

  exit $code
}

Function Fail-Script([parameter(Mandatory=$false)][string] $msg = "Got errors.")
{
  if (![string]::IsNullOrEmpty($msg))
  {
    Write-Err $msg
  }
  Exit-Script($kScriptFailsExitCode)
}

Function Write-Message([parameter(Mandatory=$true)][string] $msg
                      ,[Parameter(Mandatory=$true)][System.ConsoleColor] $color)
{
  $foregroundColor = $host.ui.RawUI.ForegroundColor
  $host.ui.RawUI.ForegroundColor = $color
  Write-Output $msg
  $host.ui.RawUI.ForegroundColor = $foregroundColor
}

# Writes an error without the verbose powershell extra-info (script line location, etc.)
Function Write-Err([parameter(ValueFromPipeline, Mandatory=$true)][string] $msg)
{
  Write-Message -msg $msg -color Red
}

Function Write-Success([parameter(ValueFromPipeline, Mandatory=$true)][string] $msg)
{
  Write-Message -msg $msg -color Green
}   

Function Exists-Command([Parameter(Mandatory=$true)][string] $command)
{
  try
  { 
    Get-Command -name $command -ErrorAction Stop
    return $true
  }
  catch
  {
    return $false
  }
}

Function Get-FileDirectory([Parameter(Mandatory=$true)][string] $filePath)
{
  return ([System.IO.Path]::GetDirectoryName($filePath))
}

Function Get-FileName( [Parameter(Mandatory=$true)][string] $path
                     , [Parameter(Mandatory=$false)][switch] $noext)
{
  if ($noext)
  {
    return ([System.IO.Path]::GetFileNameWithoutExtension($path))
  }
  else 
  {
    return ([System.IO.Path]::GetFileName($path))
  }
}

Function IsFileMatchingName( [Parameter(Mandatory=$true)][string] $filePath
                           , [Parameter(Mandatory=$true)][string] $matchName)
{
  [string] $fileName      = (Get-FileName -path $filePath)
  [string] $fileNameNoExt = (Get-FileName -path $filePath -noext) 
  if ($aDisableNameRegexMatching) 
  { 
    return (($fileName -eq $matchName) -or ($fileNameNoExt -eq $matchName))
  } 
  else
  { 
    return (($fileName -match $matchName) -or ($fileNameNoExt -match $matchName))
  }
}

Function Canonize-Path( [Parameter(Mandatory=$true)][string] $base
                      , [Parameter(Mandatory=$true)][string] $child
                      , [switch] $ignoreErrors)
{
  [string] $errorAction = If ($ignoreErrors) {"SilentlyContinue"} Else {"Stop"}
  [string] $path = $child
  if (![System.IO.Path]::IsPathRooted($path)) {
    [string] $path = Join-Path -Path "$base" -ChildPath "$child" -Resolve -ErrorAction $errorAction
  } 
  return $path
}

Function Get-MscVer()
{
  [string] $path = "${Env:ProgramFiles(x86)}\Microsoft Visual Studio\"
  $path         += "$aVisualStudioVersion\$aVisualStudioSku\VC\Tools\MSVC\"

  [System.IO.DirectoryInfo] $directory = (Get-Item $path)
  [System.IO.DirectoryInfo] $child = ($directory | Get-ChildItem)
  return $child.Name
}

Function Should-CompileProject([Parameter(Mandatory=$true)][string] $vcxprojPath)
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

Function Should-IgnoreProject([Parameter(Mandatory=$true)][string] $vcxprojPath)
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

Function Should-IgnoreFile([Parameter(Mandatory=$true)][string] $file)
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

Function Get-ProjectFilesToCompile([Parameter(Mandatory=$true)][string] $vcxprojPath,
                                   [Parameter(Mandatory=$false)][string] $pchCppName)
{
  [xml] $vcxproj = Get-Content $vcxprojPath
  [Boolean] $pchDisabled = [string]::IsNullOrEmpty($pchCppName)

  [string[]] $files = $vcxproj.Project.ItemGroup.ClCompile                    |
                     Where-Object { ($_.Include -ne $null) -and
                                    ($pchDisabled -or ($_.Include -notmatch $pchCppName))
                                  }                                           |
                     ForEach-Object { Canonize-Path -base (Get-FileDirectory($vcxprojPath)) `
                                                    -child $_.Include }
  if ($files.Count -gt 0)
  {
    $files = $files | Where-Object { ! (Should-IgnoreFile -file $_) }
  }

  return $files
}

Function Get-ProjectHeaders([Parameter(Mandatory=$true)][string] $vcxprojPath)
{
  [xml] $vcxproj = Get-Content $vcxprojPath
  [string[]] $headers = $vcxproj.Project.ItemGroup.ClInclude | ForEach-Object {$_.Include }

  return $headers
}

Function Get-Project-SDKVer([Parameter(Mandatory=$true)][string] $vcxprojPath)
{
  [xml] $vcxproj = Get-Content $vcxprojPath
  [string] $sdkVer = $vcxproj.Project.PropertyGroup.WindowsTargetPlatformVersion

  If ([string]::IsNullOrEmpty($sdkVer)) { "" } Else { $sdkVer.Trim() }
}

Function Is-ValidPlatform([string] $platformConfig)
{
  return $kValidPlatformFilters -contains $platformConfig
}

Function Is-Project-Unicode([Parameter(Mandatory=$true)][string] $vcxprojPath)
{
  [xml] $vcxproj = Get-Content $vcxprojPath
  $propGroup = $vcxproj.Project.PropertyGroup | `
  Where-Object { $_.GetAttribute -ne $null -and
                 (Is-ValidPlatform($_.GetAttribute("Condition"))) -and
                 $_.GetAttribute("Label") -eq "Configuration" }
  
  return ($propGroup.CharacterSet -eq "Unicode")
}

Function Get-ProjectPlatformToolset([Parameter(Mandatory=$true)][string] $vcxprojPath)
{
  [xml] $vcxproj = Get-Content $vcxprojPath
  $propGroup = $vcxproj.Project.PropertyGroup | `
               Where-Object { $_.GetAttribute -ne $null -and
                              (Is-ValidPlatform($_.GetAttribute("Condition"))) -and
                              $_.GetAttribute("Label") -eq "Configuration" }
  
  $toolset = $propGroup.PlatformToolset

  if ($toolset)
  {
    return $toolset
  }
  else
  {
    return $kVStudioDefaultPlatformToolset
  }
}

Function Get-VisualStudio-Includes([Parameter(Mandatory=$true)][string]  $vsPath,
                                   [Parameter(Mandatory=$false)][string] $mscVer)
{
  [string] $mscVerToken = ""
  If (![string]::IsNullOrEmpty($mscVer)) 
  { 
    $mscVerToken = "Tools\MSVC\$mscVer\" 
  }
  
  return @( "$vsPath\VC\$($mscVerToken)include"
          , "$vsPath\VC\$($mscVerToken)atlmfc\include"
          )
}

Function Get-VisualStudio-Path()
{
  if ($aVisualStudioVersion -eq "2015")
  {
    $installLocation = (Get-Item $kVs2015RegistryKey).GetValue("InstallDir")
    return Canonize-Path -base $installLocation -child "..\.."
  }
  else 
  {
    if (Test-Path $kVsWhereLocation)
    {
      return (& "$kVsWhereLocation" -nologo -property installationPath)
    }

    if (Test-Path -Path $kVs15DefaultLocation)
    {
      return $kVs15DefaultLocation
    }

    throw "Cannot locate Visual Studio location"
  }
}

Function Get-ProjectIncludeDirectories([Parameter(Mandatory=$true)][string] $vcxprojPath)
{
  [string[]] $returnArray = @()

  [string] $vsPath = Get-VisualStudio-Path
  Write-Verbose "Detected Visual Studio at $vsPath"

  if ($aVisualStudioVersion -eq "2015")
  {
    $returnArray += Get-VisualStudio-Includes -vsPath $vsPath
  }
  else
  {
    $returnArray += Get-VisualStudio-Includes -vsPath $vsPath `
                                              -mscVer (Get-MscVer -visualStudioPath $vsPath)
  }

  $sdkVer = (Get-Project-SDKVer -vcxprojPath $vcxprojPath)

  if ([string]::IsNullOrEmpty($sdkVer))
  {
    $sdkVer = $aDefaultWinSdkVersion
  }

  # ----------------------------------------------------------------------------------------------
  # Windows 10

  if ((![string]::IsNullOrEmpty($sdkVer)) -and ($sdkVer.StartsWith("10")))
  {
    $returnArray += @("${Env:ProgramFiles(x86)}\Windows Kits\10\Include\$sdkVer\ucrt")

    [string] $platformToolset = (Get-ProjectPlatformToolset -vcxprojPath $vcxprojPath)

    if ($platformToolset.EndsWith("xp"))
    {
      $returnArray += @($kIncludePathsXPTargetingSDK)
    }
    else
    {
      $returnArray += @( "${Env:ProgramFiles(x86)}\Windows Kits\10\Include\$sdkVer\um"
                       , "${Env:ProgramFiles(x86)}\Windows Kits\10\Include\$sdkVer\shared"
                       , "${Env:ProgramFiles(x86)}\Windows Kits\10\Include\$sdkVer\winrt"
                       )
    }
  }

  # ----------------------------------------------------------------------------------------------
  # Windows 8 / 8.1

  if ((![string]::IsNullOrEmpty($sdkVer)) -and ($sdkVer.StartsWith("8.")))
  {
    $returnArray += @("${Env:ProgramFiles(x86)}\Windows Kits\10\Include\10.0.10240.0\ucrt")

    [string] $platformToolset = (Get-ProjectPlatformToolset -vcxprojPath $vcxprojPath)

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

  $returnArray += $aIncludeDirectories

  return $returnArray
}

Function Get-Projects()
{
  $vcxprojs = Get-ChildItem -LiteralPath "$aDirectory" -recurse | 
              Where-Object { $_.Extension -eq $kExtensionVcxproj }

  return $vcxprojs;
}

Function Get-PchCppIncludeHeader([Parameter(Mandatory=$true)][string] $vcxprojPath
                                ,[Parameter(Mandatory=$true)][string] $pchCppFile)
{
  [string] $vcxprojDir = Get-FileDirectory -filePath $vcxprojPath
  [string] $cppPath = Canonize-Path -base $vcxprojDir -child $pchCppFile
  [string] $fileContent = Get-Content -path $cppPath

  return [regex]::match($fileContent,'#include "(\S+)"').Groups[1].Value
}

# Retrieve directory in which stdafx.h resides
Function Get-ProjectStdafxDir([Parameter(Mandatory=$true)][string] $vcxprojPath,
                              [Parameter(Mandatory=$true)][string] $pchHeaderName)
{
  [string[]] $projectHeaders = Get-ProjectHeaders($vcxprojPath)
  [string] $stdafxRelativePath = $projectHeaders | Where-Object { $_ -cmatch $pchHeaderName }
  if ([string]::IsNullOrEmpty($stdafxRelativePath))
  {
    return ""
  }

  [string] $stdafxAbsolutePath = Canonize-Path -base (Get-FileDirectory($vcxprojPath)) `
                                               -child $stdafxRelativePath;
  [string] $stdafxDir = Get-FileDirectory($stdafxAbsolutePath)

  return $stdafxDir
}

# Retrieve directory in which the PCH CPP resides (e.g. stdafx.cpp, stdafxA.cpp)
Function Get-Project-PchCpp([Parameter(Mandatory=$true)][string] $vcxprojPath)
{
  [xml] $vcxproj = Get-Content $vcxprojPath
  $pchCppRelativePath = $vcxproj.Project.ItemGroup.ClCompile.PrecompiledHeader | 
                        Where-Object {($_.InnerText -eq "Create")}             | 
                        Select-Object -ExpandProperty ParentNode               | 
                        Select-Object -first 1                                 |
                        Select-Object -ExpandProperty Include

  return $pchCppRelativePath
}

Function Set-ProjectIncludePaths([Parameter(Mandatory=$true)] $includeDirectories)
{
  [string] $includePathsString = $includeDirectories -join ";"
  Write-Verbose "Include directories:"
  foreach ($dir in $includeDirectories)
  {
    Write-Verbose $dir
  }
  $ENV:INCLUDE = $includePathsString;
}

Function Generate-Pch( [Parameter(Mandatory=$true)] [string]   $vcxprojPath
                     , [Parameter(Mandatory=$true)] [string]   $stdafxDir
                     , [Parameter(Mandatory=$true)] [string]   $stdafxHeaderName
                     , [Parameter(Mandatory=$false)][string[]] $preprocessorDefinitions)
{
  [string] $stdafx = (Canonize-Path -base $stdafxDir -child $stdafxHeaderName)
  [string] $vcxprojShortName = [System.IO.Path]::GetFileNameWithoutExtension($vcxprojPath);
  [string] $stdafxPch = (Join-Path -Path $stdafxDir `
                                   -ChildPath "$vcxprojShortName$kExtensionClangPch")
  Remove-Item -Path "$stdafxPch" -ErrorAction SilentlyContinue | Out-Null

  $global:FilesToDeleteWhenScriptQuits.Add($stdafxPch) | Out-Null

  # Supress -Werror for PCH generation as it throws warnings quite often in code we cannot control
  [string[]] $clangFlags = $aClangCompileFlags | Where-Object { $_ -ne $kClangFlagWarningIsError }

  [string[]] $compilationFlags = @("""$stdafx"""
                                  ,$kClangFlagEmitPch
                                  ,$kClangFlagMinusO
                                  ,"""$stdafxPch"""
                                  ,$clangFlags
                                  ,$kClangFlagNoUnusedArg
                                  ,$preprocessorDefinitions
                                  )

  [System.Diagnostics.Process] $processInfo = Start-Process -FilePath $kClangCompiler `
                                                            -ArgumentList $compilationFlags `
                                                            -WorkingDirectory "$aDirectory" `
                                                            -NoNewWindow `
                                                            -Wait `
                                                            -PassThru
  if ($processInfo.ExitCode -ne 0)
  {
    Fail-Script "Errors encountered during PCH creation"
  }

  return $stdafxPch
}

Function Get-PropertySheets([Parameter(Mandatory=$true)][string] $vcxprojPath)
{
  [xml] $vcxproj = Get-Content $vcxprojPath
  if (!$vcxproj.Project -or 
      !$vcxproj.Project.ImportGroup)
  {
    return $null
  }

  [string] $vcxprojDir = Get-FileDirectory($vcxprojPath)

  [System.Xml.XmlElement] $importGroup = $vcxproj.Project.ImportGroup               | 
                 Where-Object { $_.GetAttribute("Label")     -eq "PropertySheets" -and
                                (Is-ValidPlatform($_.GetAttribute("Condition")))  } |
                 Select-Object -first 1

  if (!$importGroup) {
      return $null
  }
  [string[]] $sheetAbsolutePaths = $importGroup.Import | 
                                   Where-Object `
                                   { 
                                     ![string]::IsNullOrEmpty((Canonize-Path -base $vcxprojDir `
                                                                             -child $_.GetAttribute("Project") `
                                                                             -ignoreErrors)) 
                                   }                   |
                                   ForEach-Object `
                                   {
                                     Canonize-Path -base $vcxprojDir -child $_.GetAttribute("Project")
                                   }
  return $sheetAbsolutePaths
}

Function Get-ProjectClCompileData( [Parameter(Mandatory=$true)][string]   $vcxprojOrPropSheetPath
                                 , [Parameter(Mandatory=$true)][string]   $clCompileChildItem
                                 , [Parameter(Mandatory=$true)][string[]] $valuesToIgnore
                                 , [switch] $isPropSheet)
{
  [xml] $vcxproj = Get-Content $vcxprojOrPropSheetPath
  $ns = New-Object System.Xml.XmlNamespaceManager($vcxproj.NameTable) 
  $ns.AddNamespace("ns", $vcxproj.DocumentElement.NamespaceURI)
           
  [string[]] $tokenData = @()

  [string] $xPathSelector = 'ns:Project/ns:ItemDefinitionGroup'
  if (!$isPropSheet)
  {
    $xPathSelector += '['
    [int] $count = 0
    foreach ($validPlatform in $kValidPlatformFilters) 
    {
      if ($count -ne 0) {
        $xPathSelector += ' or '
      }
      $xPathSelector += 'contains(@Condition, "' + $validPlatform + '")'
      $count++
    }
    $xPathSelector += ']'
  }
  $xPathSelector += "/ns:ClCompile/ns:$clCompileChildItem/text()"
  [System.Xml.XmlText[]] $definitions = $vcxproj.SelectNodes($xPathSelector, $ns)
  if ($definitions -ne $null -and $definitions.InnerText.Length -gt 0)
  {
    $tokenData = $definitions.InnerText.Split(";")        |
                 Where-Object { (![string]::IsNullOrEmpty($_)) -and 
                                ($valuesToIgnore -notcontains $_) }
  }

  if (!$isPropSheet)
  {
    [string[]] $propSheets = Get-PropertySheets($vcxprojOrPropSheetPath)

    foreach ($propSheet in $propSheets)
    {
       [string[]] $propSheetTokens = Get-ProjectClCompileData -vcxprojOrPropSheetPath $propSheet `
                                                              -clCompileChildItem $clCompileChildItem `
                                                              -valuesToIgnore $valuesToIgnore `
                                                              -isPropSheet
       if ($propSheetTokens)
       {
         $tokenData += $propSheetTokens
       }
    }
  }

  return $tokenData
}

# Retrieve array of preprocessor definitions for a given project, in Clang format (-DNAME )
Function Get-ProjectPreprocessorDefines([Parameter(Mandatory=$true)][string] $vcxprojPath)
{
  [string[]] $tokens = Get-ProjectClCompileData -vcxprojOrPropSheetPath $vcxprojPath `
                                                -clCompileChildItem     "PreprocessorDefinitions" `
                                                -valuesToIgnore         @($kVcxprojItemInheritedPreprocessorDefs)

  $defines = ($tokens | Where-Object { $_ } | ForEach-Object { $kClangDefinePrefix + $_ })

  if (Is-Project-Unicode -vcxprojPath $vcxprojPath)
  {
    $defines += $kDefinesUnicode
  }

  [string] $platformToolset = Get-ProjectPlatformToolset -vcxprojPath $vcxprojPath
  if ($platformToolset.EndsWith("xp"))
  {
    $defines += $kDefinesClangXpTargeting
  }

  return $defines
}

Function Get-ProjectAdditionalIncludes([Parameter(Mandatory=$true)][string] $vcxprojPath)
{
  [string[]] $tokens = Get-ProjectClCompileData -vcxprojOrPropSheetPath $vcxprojPath `
                                                -clCompileChildItem     $kVcxprojElemAdditionalIncludes `
                                                -valuesToIgnore         @($kVcxprojItemInheritedAdditionalIncludes)

  [string] $projDir = Get-FileDirectory($vcxprojPath)
  
  foreach ($token in $tokens)
  {
    if ($token -eq $kVStudioVarProjDir)
    {
      $projDir
    }
    Else
    {
      Canonize-Path -base $projDir -child $token -ignoreErrors
    }
  }
}

Function Get-ProjectForceIncludes([Parameter(Mandatory=$true)][string] $vcxprojPath)
{
  [xml] $vcxproj = Get-Content $vcxprojPath
  
  [System.Xml.XmlElement[]] $itemDefinitionGroup = $vcxproj.Project.ItemDefinitionGroup     | 
                            Where-Object { Is-ValidPlatform($_.GetAttribute("Condition")) }
  foreach ($group in $itemDefinitionGroup)
  {
    if (!$group.ClCompile -or !$group.ClCompile.ForcedIncludeFiles)
    {
      continue
    }
    return $group.ClCompile.ForcedIncludeFiles -split ";"
  }
  
  return $null
}

Function Get-ExeToCall([Parameter(Mandatory=$true)][WorkloadType] $workloadType)
{
  switch ($workloadType)
  {
     "Compile"  { return $kClangCompiler }
     "Tidy"     { return $kClangTidy     }
     "TidyFix"  { return $kClangTidy     }
  }
}

Function Get-CompileCallArguments( [Parameter(Mandatory=$false)][string[]] $preprocessorDefinitions
                                 , [Parameter(Mandatory=$false)][string[]] $forceIncludeFiles
                                 , [Parameter(Mandatory=$false)][string]   $pchFilePath
                                 , [Parameter(Mandatory=$true)][string]    $fileToCompile)
{
  [string[]] $projectCompileArgs = @()
  if (! [string]::IsNullOrEmpty($pchFilePath))
  {
    $projectCompileArgs += @($kClangFlagIncludePch , """$pchFilePath""")
  }
  
  $projectCompileArgs += @( $kClangFlagFileIsCPP
                          , """$fileToCompile"""
                          , $aClangCompileFlags
                          , $kClangFlagSupressLINK
                          , $preprocessorDefinitions
                          )
  if ($forceIncludeFiles)
  {
    $projectCompileArgs += $kClangFlagNoMsInclude;
    
    foreach ($file in $forceIncludeFiles)
    {
      $projectCompileArgs += "$kClangFlagForceInclude $file"
    }
  }

  return $projectCompileArgs
}

Function Get-TidyCallArguments( [Parameter(Mandatory=$false)][string[]] $preprocessorDefinitions
                              , [Parameter(Mandatory=$true)][string]   $fileToTidy
                              , [Parameter(Mandatory=$false)][switch]  $fix)
{
  [string[]] $tidyArgs = @("""$fileToTidy""")
  if ($fix)
  { 
    $tidyArgs += "$kClangTidyFlagChecks$aTidyFixFlags"
  } 
  else
  { 
    $tidyArgs += "$kClangTidyFlagChecks$aTidyFlags"
  }

  # The header-filter flag enables clang-tidy to run on headers too.
  # We want all headers from our directory to be tidied up.
  $tidyArgs += $kClangTidyFlagHeaderFilter + '"' + [regex]::Escape($aDirectory) + '"'

  if ($fix)
  {
    $tidyArgs += $kClangTidyFixFlags
  }
  else 
  {
    $tidyArgs += $kClangTidyFlags
  }
  
  # We reuse flags used for compilation and preprocessor definitions.
  $tidyArgs += $aClangCompileFlags
  $tidyArgs += $preprocessorDefinitions

  return $tidyArgs
}

Function Get-ExeCallArguments( [Parameter(Mandatory=$true) ][string]       $vcxprojPath
                             , [Parameter(Mandatory=$false)][string]       $pchFilePath
                             , [Parameter(Mandatory=$false)][string[]]     $preprocessorDefinitions
                             , [Parameter(Mandatory=$false)][string[]]     $forceIncludeFiles
                             , [Parameter(Mandatory=$true) ][string]       $currentFile
                             , [Parameter(Mandatory=$true) ][WorkloadType] $workloadType)
{
  switch ($workloadType)
  {
    Compile { return Get-CompileCallArguments -preprocessorDefinitions $preprocessorDefinitions `
                                              -forceIncludeFiles       $forceIncludeFiles `
                                              -pchFilePath             $pchFilePath `
                                              -fileToCompile           $currentFile }
    Tidy    { return Get-TidyCallArguments -preprocessorDefinitions $preprocessorDefinitions `
                                           -fileToTidy              $currentFile }
    TidyFix { return Get-TidyCallArguments -preprocessorDefinitions $preprocessorDefinitions `
                                           -fileToTidy              $currentFile `
                                           -fix}
  }
}

Function Process-ProjectResult($compileResult)
{
  if (!$compileResult.Success)
  {
    Write-Err ($compileResult.Output)

    if (!$aContinueOnError)
    {
      # Wait for other workers to finish. They have a lock on the PCH file
      Get-Job -state Running | Wait-Job | Remove-Job | Out-Null
      Fail-Script
    }

    $global:FoundErrors = $true
  }
  else 
  {
    if ( $compileResult.Output.Length -gt 0)
    {
      Write-Output $compileResult.Output
    }
  }
}

Function Wait-AndProcessBuildJobs([switch]$any)
{
  $runningJobs = @(Get-Job -state Running)

  if ($any)
  {
    $runningJobs | Wait-Job -Any | Out-Null
  }
  else
  {
    $runningJobs | Wait-Job | Out-Null
  }

  $jobData = Get-Job -State Completed
  foreach ($job in $jobData)
  {
    $buildResult = Receive-Job $job
    Process-ProjectResult -compileResult $buildResult
  }

  Remove-Job -State Completed
}

Function Wait-ForWorkerJobSlot()
{
  # We allow as many background workers as we have logical CPU cores
  $runningJobs = @(Get-Job -State Running)

  if ($runningJobs.Count -ge $kLogicalCoreCount) 
  {
    Wait-AndProcessBuildJobs -any
  }
}

Function Run-ClangJobs([Parameter(Mandatory=$true)] $clangJobs)
{
  # Script block (lambda) for logic to be used when running a clang job.
  $jobWorkToBeDone = `
  {
    param( $job )
    
    Push-Location $job.WorkingDirectory

    $callOutput = & $job.FilePath $job.ArgumentList.Split(' ') 2>&1 |`
                  ForEach-Object { $_.ToString() } |`
                  Out-String

    $callSuccess = $LASTEXITCODE -eq 0

    Pop-Location

    return New-Object PsObject -Prop @{ "File"    = $job.File;
                                        "Success" = $callSuccess;
                                        "Output"  = $callOutput } 
  }

  [int] $jobCount = $clangJobs.Count
  [int] $crtJobCount = $jobCount

  foreach ($job in $clangJobs)
  {
    # Check if we must wait for background jobs to complete
    Wait-ForWorkerJobSlot

    # Inform console what CPP we are processing next
    Write-Output "$($crtJobCount): $($job.File)"

    if ($aUseParallelCompile)
    {
      Start-Job -ScriptBlock  $jobWorkToBeDone `
                -ArgumentList $job `
                -ErrorAction Continue | Out-Null
    }
    else
    {
      $compileResult = Invoke-Command -ScriptBlock  $jobWorkToBeDone `
                                      -ArgumentList $job
      Process-ProjectResult -compileResult $compileResult
    }

    $crtJobCount -= 1
  }

  Wait-AndProcessBuildJobs
}

Function Process-Project( [Parameter(Mandatory=$true)][string]       $vcxprojPath
                        , [Parameter(Mandatory=$true)][WorkloadType] $workloadType)
{  
  #-----------------------------------------------------------------------------------------------
  # LOCATE STDAFX.H DIRECTORY

  [string] $stdafxDir = ""
  [string] $stdafxCpp = Get-Project-PchCpp -vcxprojPath $vcxprojPath
  
  if (![string]::IsNullOrEmpty($stdafxCpp))
  {
    Write-Verbose "PCH cpp name: $stdafxCpp"
    [string] $stdafxHeader = Get-PchCppIncludeHeader -vcxprojPath $vcxprojPath `
                                                     -pchCppFile $stdafxCpp
    Write-Verbose "PCH header name: $stdafxHeader"
  }
  
  if (![string]::IsNullOrEmpty($stdafxCpp))
  {
    $stdafxDir = Get-ProjectStdafxDir -vcxprojPath $vcxprojPath `
                                      -pchHeaderName $stdafxHeader
  }

  if ([string]::IsNullOrEmpty($stdafxDir))
  {
    Write-Verbose ("PCH not enabled for this project!")
  }
  else
  {
    Write-Verbose ("PCH directory: $stdafxDir")
  }
  #-----------------------------------------------------------------------------------------------
  # DETECT PROJECT PREPROCESSOR DEFINITIONS

  [string[]] $preprocessorDefinitions = Get-ProjectPreprocessorDefines($vcxprojPath)
  Write-Verbose "Preprocessor definitions: $preprocessorDefinitions"
  
  #-----------------------------------------------------------------------------------------------
  # DETECT PLATFORM TOOLSET

  [string] $platformToolset = Get-ProjectPlatformToolset($vcxprojPath)
  Write-Verbose "Platform toolset: $platformToolset"

  #-----------------------------------------------------------------------------------------------
  # DETECT PROJECT ADDITIONAL INCLUDE DIRECTORIES AND CONSTRUCT INCLUDE PATHS

  [string[]] $includeDirectories = Get-ProjectAdditionalIncludes($vcxprojPath)
  Write-Verbose "Additional includes:"
  foreach ($include in $includeDirectories)
  {
    Write-Verbose $include
  }
  
  $includeDirectories = (Get-ProjectIncludeDirectories -vcxprojPath $vcxprojPath) + $includeDirectories

  if (![string]::IsNullOrEmpty($stdafxDir))
  {
    $includeDirectories = @($stdafxDir) + $includeDirectories
  }
  $includeDirectories = @($aDirectory) + $includeDirectories

  Set-ProjectIncludePaths($includeDirectories)

  #-----------------------------------------------------------------------------------------------
  # FIND FORCE INCLUDES

  [string[]] $forceIncludeFiles = Get-ProjectForceIncludes -vcxprojPath $vcxprojPath
  Write-Verbose "Force includes: $forceIncludeFiles"

  #-----------------------------------------------------------------------------------------------
  # FIND LIST OF CPPs TO PROCESS

  [string[]] $projCpps = Get-ProjectFilesToCompile -vcxprojPath $vcxprojPath `
                                                   -pchCppName  $stdafxCpp

  if (![string]::IsNullOrEmpty($aCppToCompile))
  {
    $projCpps = ( $projCpps | 
                  Where-Object {  IsFileMatchingName -filePath $_ `
                                                     -matchName $aCppToCompile } )
  }
  Write-Verbose ("Processing " + $projCpps.Count + " cpps")
 
  #-----------------------------------------------------------------------------------------------
  # CREATE PCH IF NEED BE

  [string] $pchFilePath = ""
  if ($projCpps.Count -gt 0 -and 
      ![string]::IsNullOrEmpty($stdafxDir) -and
      $workloadType -eq [WorkloadType]::Compile)
  {
    # COMPILE PCH
    Write-Verbose "Generating PCH..."
    $pchFilePath = Generate-Pch -vcxprojPath      "$vcxprojPath"  `
                                -stdafxDir        "$stdafxDir"    `
                                -stdafxHeaderName "$stdafxHeader" `
                                -preprocessorDefinitions $preprocessorDefinitions
    Write-Verbose "Generated $pchFilePath"
  }
  
  #-----------------------------------------------------------------------------------------------
  # PROCESS CPP FILES. CONSTRUCT COMMAND LINE JOBS TO BE INVOKED

  $clangJobs = @()

  foreach ($cpp in $projCpps)
  {    
    [string] $exeToCall = Get-ExeToCall -workloadType $workloadType
                        
    [string] $exeArgs   = Get-ExeCallArguments -vcxprojPath             $vcxprojPath `
                                               -workloadType            $workloadType `
                                               -pchFilePath             $pchFilePath `
                                               -preprocessorDefinitions $preprocessorDefinitions `
                                               -forceIncludeFiles       $forceIncludeFiles `
                                               -currentFile             $cpp

    $newJob = New-Object PsObject -Prop @{ 'FilePath'        = $exeToCall;
                                           'WorkingDirectory'= $aDirectory;
                                           'ArgumentList'    = $exeArgs;
                                           'File'            = $cpp }
    $clangJobs += $newJob
  }  
  
  #-----------------------------------------------------------------------------------------------
  # RUN CLANG JOBS

  Run-ClangJobs -clangJobs $clangJobs
}
 
#-------------------------------------------------------------------------------------------------
# Script entry point

Clear-Host # clears console

# If LLVM is not in PATH try to detect it automatically
if (! (Exists-Command($kClangCompiler)) )
{
  foreach ($locationLLVM in $kLLVMInstallLocations)
  {
    if (Test-Path $locationLLVM)
    {
      Write-Verbose "Detected LLVM at $locationLLVM"
      $env:Path += ";$locationLLVM"
      break
    }
  }
}

Push-Location $aDirectory

# This powershell process may already have completed jobs. Discard them.
Remove-Job -State Completed

Write-Verbose "[INFO] Source directory: $aDirectory"
Write-Verbose "Scanning for .vcxproj files"

[System.IO.FileInfo[]] $projects = Get-Projects
Write-Verbose ("Found $($projects.Count) projects")

[System.IO.FileInfo[]] $projectsToProcess = @()

if ([string]::IsNullOrEmpty($aVcxprojToCompile) -and 
    [string]::IsNullOrEmpty($aVcxprojToIgnore))
{
  Write-Verbose "[ INFO ] PROCESSING ALL PROJECTS"
  $projectsToProcess = $projects
}
else
{
  $projectsToProcess = $projects |
                       Where-Object {       (Should-CompileProject -vcxprojPath $_.FullName) `
                                      -and !(Should-IgnoreProject  -vcxprojPath $_.FullName ) }
                                       
  if ($projectsToProcess -ne $null)
  {
    Write-Output ("[ INFO ] WILL PROCESS PROJECTS: `n`t" + ($projectsToProcess -join "`n`t"))
    $projectsToProcess = $projectsToProcess
  }
  else
  {
    Write-Err "Cannot find given project"
  }
}

$projectCounter = $projectsToProcess.Length;
foreach ($project in $projectsToProcess)
{ 
  [string] $vcxprojPath = $project.FullName;

  [WorkloadType] $workloadType = [WorkloadType]::Compile

  if (![string]::IsNullOrEmpty($aTidyFlags)) 
  {
     $workloadType = [WorkloadType]::Tidy    
  }
  
  if (![string]::IsNullOrEmpty($aTidyFixFlags))
  {
     $workloadType = [WorkloadType]::TidyFix
  }

  Write-Output ("`n[ INFO ] $projectCounter. PROCESSING PROJECT " + $vcxprojPath)
  Process-Project -vcxprojPath $vcxprojPath -workloadType $workloadType

  $projectCounter -= 1
}

if ($global:FoundErrors)
{
  Fail-Script
}
else
{
  Exit-Script
}