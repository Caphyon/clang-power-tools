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

.NOTES
    Author: Gabriel Diaconita
#>
param( [alias("dir")]          [Parameter(Mandatory=$true)] [string]   $aDirectory
     , [alias("proj")]         [Parameter(Mandatory=$false)][string[]] $aVcxprojToCompile
     , [alias("proj-ignore")]  [Parameter(Mandatory=$false)][string[]] $aVcxprojToIgnore
     , [alias("file")]         [Parameter(Mandatory=$false)][string]   $aCppToCompile
     , [alias("file-ignore")]  [Parameter(Mandatory=$false)][string[]] $aCppToIgnore
     , [alias("parallel")]     [Parameter(Mandatory=$false)][switch]   $aUseParallelCompile
     , [alias("continue")]     [Parameter(Mandatory=$false)][switch]   $aContinueOnError
     , [alias("clang-flags")]  [Parameter(Mandatory=$true)] [string[]] $aClangCompileFlags
     , [alias("literal")]      [Parameter(Mandatory=$false)][switch]   $aDisableNameRegexMatching
     , [alias("tidy")]         [Parameter(Mandatory=$false)][string]   $aTidyFlags
     , [alias("tidy-fix")]     [Parameter(Mandatory=$false)][string]   $aTidyFixFlags
     , [alias("vs-ver")]       [Parameter(Mandatory=$true)] [string]   $aVisualStudioVersion
     , [alias("vs-sku")]       [Parameter(Mandatory=$true)] [string]   $aVisualStudioSku
     )

# System Architecture Constants
# ------------------------------------------------------------------------------------------------
       
Set-Variable -name kLogicalCoreCount -value                                                                 `
  (@(Get-WmiObject -class Win32_processor)  |                                                               `
   ForEach-Object -Begin   { $coreCount = 0 }                                                               `
                  -Process { $coreCount += ($_ | Select-Object -property       NumberOfLogicalProcessors    `
                                                               -ExpandProperty NumberOfLogicalProcessors) } `
                  -End     { $coreCount })                              -option Constant
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
    )        -option Constant

Set-Variable -name kVcxprojXpathPreprocessorDefs  `
             -value "ns:Project/ns:ItemDefinitionGroup/ns:ClCompile/ns:PreprocessorDefinitions" `
             -option Constant

Set-Variable -name kVcxprojXpathAdditionalIncludes `
             -value "ns:Project/ns:ItemDefinitionGroup/ns:ClCompile/ns:AdditionalIncludeDirectories" `
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
             -value "ns:Project/ns:ItemDefinitionGroup/ns:ClCompile/ns:ForceIncludeFiles" `
             -option Constant 

Set-Variable -name kVcxprojXpathPCH `
             -value "ns:Project/ns:ItemGroup/ns:ClCompile/ns:PrecompiledHeader[text()='Create']" `
             -option Constant 

Set-Variable -name kVcxprojXpathPropSheets `
             -value "ns:Project/ns:ImportGroup[@Label='PropertySheets']/ns:Import" `
             -option Constant

Set-Variable -name kVcxprojXpathToolset `
             -value "ns:Project/ns:PropertyGroup[@Label='Configuration']/ns:PlatformToolset" `
             -option Constant

Set-Variable -name kVcxprojXpathDefaultConfigPlatform `
             -value "ns:Project/ns:ItemGroup[@Label='ProjectConfigurations']/ns:ProjectConfiguration[1]" `
             -option Constant

Set-Variable -name kVcxprojXpathConfigPlatformSpecificElements `
             -value "//*[starts-with(@Condition, ""'`$(Configuration)|`$(Platform)'"")]" `
             -option Constant

Set-Variable -name kVStudioVarProjDir          -value '$(ProjectDir)'   -option Constant
Set-Variable -name kVSDefaultWinSDK            -value '8.1'             -option Constant
Set-Variable -name kVSDefaultWinSDK_XP         -value '7.0'             -option Constant

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
# Global variables

[System.Collections.ArrayList] $global:FilesToDeleteWhenScriptQuits = @()
[Boolean]                      $global:FoundErrors                  = $false

# current vcxproj and property sheets
[xml[]]  $global:projectFiles                    = @();

# path of current project
[string] $global:vcxprojPath                     = "";

# namespace of current project vcxproj XML
[System.Xml.XmlNamespaceManager] $global:xpathNS = $null;

#-------------------------------------------------------------------------------------------------
# Global functions

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
  return (Get-Item "$(Get-VisualStudio-Path)\VC\Tools\MSVC\" | Get-ChildItem).Name
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
  [Boolean] $pchDisabled = [string]::IsNullOrEmpty($pchCppName)

  [string[]] $files = Select-ProjectNodes($kVcxprojXpathCompileFiles) |
                      Where-Object { ($_.Include -ne $null) -and
                                     ($pchDisabled -or ($_.Include -notmatch $pchCppName))
                                   }                                 |
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
  [string[]] $headers = Select-ProjectNodes($kVcxprojXpathHeaders) | ForEach-Object {$_.Include }

  return $headers
}

Function Get-Project-SDKVer([Parameter(Mandatory=$true)][string] $vcxprojPath)
{
  [string] $sdkVer = (Select-ProjectNodes($kVcxprojXpathWinPlatformVer)).InnerText

  If ([string]::IsNullOrEmpty($sdkVer)) { "" } Else { $sdkVer.Trim() }
}

Function Is-Project-Unicode([Parameter(Mandatory=$true)][string] $vcxprojPath)
{
  $propGroup = Select-ProjectNodes("ns:Project/ns:PropertyGroup[@Label='Configuration']")
  
  return ($propGroup.CharacterSet -eq "Unicode")
}

Function Get-ProjectPlatformToolset([Parameter(Mandatory=$true)][string] $vcxprojPath)
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
  
  [string] $platformToolset = (Get-ProjectPlatformToolset -vcxprojPath $vcxprojPath)

  if ($aVisualStudioVersion -eq "2015")
  {
    $returnArray += Get-VisualStudio-Includes -vsPath $vsPath
  }
  else
  {
    $mscVer = Get-MscVer -visualStudioPath $vsPath
    Write-Verbose "MSCVER : $mscVer"

    $returnArray += Get-VisualStudio-Includes -vsPath $vsPath -mscVer $mscVer
  }

  $sdkVer = (Get-Project-SDKVer -vcxprojPath $vcxprojPath)

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

  Write-Verbose "WinSDK version : $sdkVer"

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
  $pchCppRelativePath = Select-ProjectNodes($kVcxprojXpathPCH)   |
                        Select-Object -ExpandProperty ParentNode | 
                        Select-Object -first 1                   |
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

function Help:Get-ProjectFileNodes([xml] $projectFile, [string] $xpath)
{
  [System.Xml.XmlElement[]] $nodes = $projectFile.SelectNodes($xpath, $global:xpathNS)
  return $nodes
}

function Select-ProjectNodes([string] $xpath, $fileIndex = 0)
{
  [System.Xml.XmlElement[]] $returnNodes = @() 
  if ($fileIndex -ge $global:projectFiles.Count)
  {
    return $returnNodes
  }

  [System.Xml.XmlElement[]] $nodes = Help:Get-ProjectFileNodes -projectFile $global:projectFiles[$fileIndex] `
                                                                -xpath $xpath

  # nothing on this level, go above
  if ($nodes.Count -eq 0)
  {
    $nodes = Select-ProjectNodes -xpath $xpath -fileIndex ($fileIndex + 1)
  }

  # we found something. see if we should inherit values from above
  if ($nodes.Count -eq 1)
  {
    [string] $nodeName = $nodes[0].Name
    [string] $inheritanceToken = "%($nodeName)";
    if ($nodes[0].InnerText.Contains($inheritanceToken))
    {
      [System.Xml.XmlElement[]] $inheritedNodes = Select-ProjectNodes -xpath $xpath -fileIndex ($fileIndex + 1)
      [string] $replaceWith = ""

      if ($inheritedNodes.Count -gt 1)
      {
        throw "Did not expect that"
      }

      if ($inheritedNodes.Count -eq 1)
      {
        $replaceWith = $inheritedNodes[0].InnerText
      }
      
      [string] $whatToReplace = [regex]::Escape($inheritanceToken);
      if ([string]::IsNullOrEmpty($replaceWith))
      {
        # handle case when a semicolon was before and there's nothing to be inserted
        $whatToReplace = ";?\s*" + [regex]::Escape($inheritanceToken)
      }
      # handle case when string ends with semicolon separator
      $whatToReplace += "(\s*;\s*$)?"

      $nodes[0].InnerText = ($nodes[0].InnerText -replace $whatToReplace, $replaceWith)
    } 
    return $nodes
  }

  # return what we found
  return $nodes
}

function SanitizeProject([xml] $vcxproj)
{
  [string]$configPlatformCondition = ""
  [System.Xml.XmlElement[]] $configNodes = Select-ProjectNodes -xpath $kVcxprojXpathDefaultConfigPlatform
  if ($configNodes)
  {
    $configPlatformName = $configNodes.GetAttribute("Include")
    Write-Verbose "Configuration platform: $configPlatformName"

    $configPlatformCondition = "'`$(Configuration)|`$(Platform)'=='$configPlatformName'"
  }

  if ([string]::IsNullOrEmpty($configPlatformCondition))
  {
    throw "Could not detect a configuration platform"
  }


  [System.Xml.XmlElement[]] $configNodes = Select-ProjectNodes -xpath $kVcxprojXpathConfigPlatformSpecificElements

  foreach ($node in $configNodes)
  {
    [string] $nodeConfigPlatform = $node.GetAttribute("Condition")

    if ($nodeConfigPlatform -eq $configPlatformCondition)
    {
      $node.RemoveAttribute("Condition")
    }
    else
    {
      $node.ParentNode.RemoveChild($node) | out-null
    }
  }
}

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
  
function Get-ProjectPropertySheets([string] $filePath, [xml] $fileXml)
{
  [string] $vcxprojDir = Get-FileDirectory($filePath)

  [System.Xml.XmlElement[]] $importGroup = $fileXml.SelectNodes($kVcxprojXpathPropSheets, $global:xpathNS)
  if (!$importGroup) 
  {
      return @()
  }

  [string[]] $sheetAbsolutePaths = $importGroup | 
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
  
  # a property sheet may have references to other property sheets
  [string[]] $returnPaths = @()
  if ($sheetAbsolutePaths)
  {
    foreach ($path in $sheetAbsolutePaths)
    {
      [string[]] $childrenPaths = Get-ProjectPropertySheets -filePath $path `
                                                            -fileXml ([xml](Get-Content $path))
      if ($childrenPaths.Length -gt 0)
      {
        $returnPaths += $childrenPaths
      }
      
      $returnPaths += $path
    }
  }

  return $returnPaths
}

function LoadProject([string] $vcxprojPath)
{
  $global:projectFiles = @([xml] (Get-Content $vcxprojPath))

  $global:vcxprojPath = $vcxprojPath
  $global:xpathNS     = New-Object System.Xml.XmlNamespaceManager($global:projectFiles[0].NameTable) 
  $global:xpathNS.AddNamespace("ns", $global:projectFiles[0].DocumentElement.NamespaceURI)
  
  SanitizeProject($global:projectFiles[0])
   
  # see if we can find a Directory.Build.props automatic prop sheet
  [string[]] $propSheetAbsolutePaths = @()
  $autoPropSheet = Get-AutoPropertySheet
  if (![string]::IsNullOrEmpty($autoPropSheet))
  {
    $propSheetAbsolutePaths += $autoPropSheet
  }

  # see if project has manually specified property sheets
  $propSheetAbsolutePaths += Get-ProjectPropertySheets -filePath $global:vcxprojPath `
                                                       -fileXml  $global:projectFiles[0]

  if (!$propSheetAbsolutePaths)
  {
    return
  }

  Write-Verbose "Property sheets: $($propSheetAbsolutePaths -join '; ')"

  [array]::Reverse($propSheetAbsolutePaths)
  foreach ($propSheetPath in $propSheetAbsolutePaths)
  {
    [xml] $propSheetXml = Get-Content $propSheetPath

    $global:projectFiles += $propSheetXml
  }
}

# Retrieve array of preprocessor definitions for a given project, in Clang format (-DNAME )
Function Get-ProjectPreprocessorDefines([Parameter(Mandatory=$true)][string] $vcxprojPath)
{
  [string[]] $tokens = (Select-ProjectNodes $kVcxprojXpathPreprocessorDefs).InnerText -split ";"

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
  [string[]] $tokens = (Select-ProjectNodes $kVcxprojXpathAdditionalIncludes).InnerText -split ";"
  if (!$tokens)
  {
    return
  }

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

Function Get-ProjectForceIncludes()
{ 
  [System.Xml.XmlElement] $forceIncludes = Select-ProjectNodes $kVcxprojXpathForceIncludes
  if ($forceIncludes)
  {
    return $forceIncludes.InnerText -split ";"
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
  # Load data
  LoadProject($vcxprojPath)

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

  [string[]] $forceIncludeFiles = Get-ProjectForceIncludes
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
Write-Verbose "CPU logical core count = $kLogicalCoreCount"

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