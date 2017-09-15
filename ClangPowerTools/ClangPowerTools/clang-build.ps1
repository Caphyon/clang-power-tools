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
    e.g. "msicomp" compiles all msicomp projects.
    
    Can be passed as comma separated values.

.PARAMETER aVcxprojToIgnore
    Alias 'proj-ignore'. Array of project(s) to ignore, from the matched ones. 
    If empty, all already matched projects are compiled.
    If the -literal switch is present, name is matched exactly. Otherwise, regex matching is used, 
    e.g. "msicomp" compiles all msicomp projects.

    Can be passed as comma separated values.

.PARAMETER aCppToCompile
    Alias 'file'. What cpp(s) to compile from the found project(s). If empty, all CPPs are compiled.
    If the -literal switch is present, name is matched exactly. Otherwise, regex matching is used, 
    e.g. "table" compiles all CPPs containing 'table'.

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
      included in the CPP will be tidied up too.

.PARAMETER aVisualStudioVersion
      Alias 'vs-ver'. Version of Visual Studio (VC++) installed and that'll be used for 
      standard library include directories. E.g. 2017.

.PARAMETER aVisualStudioSku
      Alias 'vs-sku'. Sku of Visual Studio (VC++) installed and that'll be used for 
      standard library include directories. E.g. Professional.

.EXAMPLE
    PS .\clang-build.ps1 -dir C:\Proj -proj foo,bar -file meow -tidy "-*,modernize-*"
    <Description of example>
    Runs clang-tidy, using "-*,modernize-*", on all CPPs containing 'meow' in their name from 
    the projects containing 'foo' or 'bar' in their names.

.EXAMPLE
    PS .\clang-build.ps1 -dir C:\Proj -proj foo -proj-ignore foobar
    <Description of example>
    Runs clang++ on all CPPs in foo... projects, except foobar
  
.OUTPUTS
    Will output Clang warnings and errors to screen. The return code will be 0 for success, >0 for failure.

.NOTES
    Author: Gabriel Diaconita
#>
param( [alias("dir")]          [Parameter(Mandatory=$true)] [string]   $aDirectory,
       [alias("proj")]         [Parameter(Mandatory=$false)][string[]] $aVcxprojToCompile,
       [alias("proj-ignore")]  [Parameter(Mandatory=$false)][string[]] $aVcxprojToIgnore,
       [alias("file")]         [Parameter(Mandatory=$false)][string]   $aCppToCompile,
       [alias("include-dirs")] [Parameter(Mandatory=$false)][string[]] $aIncludeDirectories,
       [alias("parallel")]     [Parameter(Mandatory=$false)][switch]   $aUseParallelCompile,
       [alias("continue")]     [Parameter(Mandatory=$false)][switch]   $aContinueOnError,
       [alias("clang-flags")]  [Parameter(Mandatory=$true)] [string[]] $aClangCompileFlags,
       [alias("literal")]      [Parameter(Mandatory=$false)][switch]   $aDisableNameRegexMatching,
       [alias("tidy")]         [Parameter(Mandatory=$false)][string]   $aTidyFlags,
       [alias("vs-ver")]       [Parameter(Mandatory=$true)] [string]   $aVisualStudioVersion,
       [alias("vs-sku")]       [Parameter(Mandatory=$true)] [string]   $aVisualStudioSku
     )
       
# System Architecture Constants
# ------------------------------------------------------------------------------------------------
       
Set-Variable -name kLogicalCoreCount -value (Get-WmiObject -class Win32_processor | `
                                             Select-Object -property NumberOfLogicalProcessors `
                                                           -ExpandProperty NumberOfLogicalProcessors) `
                                                                        -Option Constant
# ------------------------------------------------------------------------------------------------
# Return Value Constants

Set-Variable -name kScriptFailsExitCode      -value  47                 -Option Constant
Set-Variable -name kScriptSuccessExitCode    -value  0                  -Option Constant

# ------------------------------------------------------------------------------------------------
# File System Constants

Set-Variable -name kExtensionCpp             -value ".cpp"              -Option Constant
Set-Variable -name kExtensionVcxproj         -value ".vcxproj"          -Option Constant
Set-Variable -name kExtensionClangPch        -value ".clang.pch"        -Option Constant
Set-Variable -name kNameStdAfxCpp            -value "stdafx.cpp"        -Option Constant
Set-Variable -name kNameStdAfxH              -value "stdafx.h"          -Option Constant

# ------------------------------------------------------------------------------------------------
# Vcxproj Related Constants

# filter used when looking for project additional includes and preprocessor definitions
Set-Variable -name kPlatformFilter    `
             -value '''$(Configuration)|$(Platform)''==''Debug|Win32''' -Option Constant
             
Set-Variable -name kVcxprojElemPreprocessorDefs  `
             -value                      "PreprocessorDefinitions"      -Option Constant
Set-Variable -name kVcxprojElemAdditionalIncludes `
             -value                      "AdditionalIncludeDirectories" -Option Constant
Set-Variable -name kVcxprojItemInheritedPreprocessorDefs `
             -value                      "%(PreprocessorDefinitions)"   -Option Constant
Set-Variable -name kVcxprojItemInheritedAdditionalIncludes `
             -value "%(AdditionalIncludeDirectories)"                   -Option Constant

Set-Variable -name kVStudioVarProjDir          -value '$(ProjectDir)'   -Option Constant

# ------------------------------------------------------------------------------------------------
# Clang-Related Constants

Set-Variable -name kClangFlagSupressLINK    -value @("-fsyntax-only")   -Option Constant
Set-Variable -name kClangFlagWarningIsError -value @("-Werror")         -Option Constant
Set-Variable -name kClangFlagIncludePch     -value "-include-pch"       -Option Constant
Set-Variable -name kClangFlagEmitPch        -value "-emit-pch"          -Option Constant
Set-Variable -name kClangFlagMinusO         -value "-o"                 -Option Constant

Set-Variable -name kClangDefinePrefix       -value "-D"                 -Option Constant

Set-Variable -name kClangCompiler             -value "clang++"          -Option Constant
Set-Variable -name kClangTidy                 -value "clang-tidy"       -Option Constant
Set-Variable -name kClangTidyFlags            -value @("-fix", "--")    -Option Constant
Set-Variable -name kClangTidyFlagHeaderFilter -value "-header-filter="  -Option Constant
Set-Variable -name kClangTidyFlagChecks       -value "-checks="         -Option Constant

#-------------------------------------------------------------------------------------------------
# PlatformToolset-Related Constants
#
# *Important* When updating constants for a new platform toolset version, pleased update the
# 'PlatformToolset' custom enum type defined below, in the 'Custom Types' section.

Set-Variable -name kClangFlagsClCompat_v141   -value @("-DUNICODE",
                                                       "-D_UNICODE")    -Option Constant

Set-Variable -name KClangFlagsClCompat_v141_xp                          -Option Constant `
                                              -value @("-DUNICODE",
                                                       "-D_UNICODE", 
                                                       "-D_USING_V110_SDK71_")

Set-Variable -name kIncludePaths_v141                                   -Option Constant `
             -value (@("${Env:ProgramFiles(x86)}\Microsoft Visual Studio\$aVisualStudioVersion\$aVisualStudioSku\VC\Tools\MSVC\14.11.25503\include",
                      "${Env:ProgramFiles(x86)}\Microsoft Visual Studio\$aVisualStudioVersion\$aVisualStudioSku\VC\Tools\MSVC\14.11.25503\atlmfc\include",
                      "${Env:ProgramFiles(x86)}\Windows Kits\10\Include\10.0.14393.0\ucrt",
                      "${Env:ProgramFiles(x86)}\Windows Kits\10\Include\10.0.14393.0\um",
                      "${Env:ProgramFiles(x86)}\Windows Kits\10\Include\10.0.14393.0\shared",
                      "${Env:ProgramFiles(x86)}\Windows Kits\10\Include\10.0.14393.0\winrt") + $aIncludeDirectories)

Set-Variable -name kIncludePaths_v141_xp                                -Option Constant `
             -value (@("${Env:ProgramFiles(x86)}\Microsoft Visual Studio\$aVisualStudioVersion\$aVisualStudioSku\VC\Tools\MSVC\14.11.25503\include",
                      "${Env:ProgramFiles(x86)}\Microsoft Visual Studio\$aVisualStudioVersion\$aVisualStudioSku\VC\Tools\MSVC\14.11.25503\atlmfc\include",
                      "${Env:ProgramFiles(x86)}\Windows Kits\10\Include\10.0.14393.0\ucrt",
                      "${Env:ProgramFiles(x86)}\Microsoft SDKs\Windows\v7.1A\Include") + $aIncludeDirectories)

#-------------------------------------------------------------------------------------------------
# Custom Types

Add-Type -TypeDefinition @"
  public enum WorkloadType
  {
    Compile,
    Tidy
  }
"@

# *IMPORTANT* When adding a new platform toolset enum VAL, corresponding kIncludePaths_VAL and
# kClangFlagsClCompat_VAL have to be created. This is all that is required. 
# The code automatically uses those variables. Just be sure that the enum value matches the value 
# used by Visual Studio in the .vcxproj file.

Add-Type -TypeDefinition @"
  public enum PlatformToolset
  {
    v141,        //Visual Studio 2017
    v141_xp      //Visual Studio 2017 - Windows XP
  }
"@

Set-Variable -name kVStudioDefaultPlatformToolset -Value ([PlatformToolset]::v141) -Option Constant

#-------------------------------------------------------------------------------------------------
# Global functions

[System.Collections.ArrayList] $global:FilesToDeleteWhenScriptQuits = @()
[Boolean]                      $global:FoundErrors                  = $false

Function Exit-Script([Parameter(Mandatory=$true)][int] $code)
{
  # Clean-up
  foreach ($file in $global:FilesToDeleteWhenScriptQuits)
  {
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

Function Write-Message([parameter(Mandatory=$true)][string] $msg,
                       [Parameter(Mandatory=$true)][System.ConsoleColor] $color)
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

Function Get-FileDirectory([Parameter(Mandatory=$true)][string] $filePath)
{
  return ([System.IO.Path]::GetDirectoryName($filePath))
}

Function Get-FileName([Parameter(Mandatory=$true)][string] $path,
                      [Parameter(Mandatory=$false)][switch] $noext)
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

Function IsFileMatchingName([Parameter(Mandatory=$true)][string] $filePath,
                            [Parameter(Mandatory=$true)][string] $matchName)
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

Function Canonize-Path([Parameter(Mandatory=$true)][string] $base, 
                       [Parameter(Mandatory=$true)][string] $child, 
                       [switch] $ignoreErrors)
{
  [string] $errorAction = If ($ignoreErrors) {"SilentlyContinue"} Else {"Stop"}
  [string] $path = Join-Path -Path "$base" -ChildPath "$child" -Resolve -ErrorAction $errorAction

  return $path
}

Function Detect-MscVer([Parameter(Mandatory=$true)][string] $vsVer,
                       [Parameter(Mandatory=$true)][string] $vsSku)
{
  [string] $path = "${Env:ProgramFiles(x86)}\Microsoft Visual Studio\"
  $path         += "$vsVer\$vsSku\VC\Tools\MSVC\"

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

Function Get-ProjectCpps([Parameter(Mandatory=$true)][string] $vcxprojPath)
{
  [xml] $vcxproj = Get-Content $vcxprojPath

  [string[]] $cpps = $vcxproj.Project.ItemGroup.ClCompile                     | 
                     Where-Object { ($_.Include -ne $null)     -and 
                                    ($_.Include -match $kExtensionCpp) -and 
                                    ($_.Include -notmatch $kNameStdAfxCpp) }  | 
                     ForEach-Object { Canonize-Path -base (Get-FileDirectory($vcxprojPath)) `
                                                    -child $_.Include }

  return $cpps
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

  return $sdkVer
}

Function Is-Project-Unicode([Parameter(Mandatory=$true)][string] $vcxprojPath)
{
  [xml] $vcxproj = Get-Content $vcxprojPath
  $propGroup = $vcxproj.Project.PropertyGroup | `
               Where-Object { $_.GetAttribute("Condition") -eq $kPlatformFilter -and 
                              $_.GetAttribute("Label") -eq "Configuration" }
  
  return ($propGroup.CharacterSet -eq "Unicode")
}

Function Get-Projects()
{
  $vcxprojs = Get-ChildItem -LiteralPath "$aDirectory" -recurse | 
              Where-Object { $_.Extension -eq $kExtensionVcxproj }

  return $vcxprojs;
}

# Retrieve directory in which stdafx.h resides
Function Get-ProjectStdafxDir([Parameter(Mandatory=$true)][string] $vcxprojPath)
{
  [string[]] $projectHeaders = Get-ProjectHeaders($vcxprojPath)
  [string] $stdafxRelativePath = $projectHeaders | Where-Object { $_ -cmatch $kNameStdAfxH }
  if ($stdafxRelativePath -eq $null)
  {
    return ""
  }

  [string] $stdafxAbsolutePath = Canonize-Path -base (Get-FileDirectory($vcxprojPath)) `
                                               -child $stdafxRelativePath;
  [string] $stdafxDir = Get-FileDirectory($stdafxAbsolutePath)

  return $stdafxDir
}

# Retrieve directory in which the PCH CPP resides (e.g. stdafx.cpp, stdafxA.cpp)
Function Project-HasPch([Parameter(Mandatory=$true)][string] $vcxprojPath)
{
  [xml] $vcxproj = Get-Content $vcxprojPath
  $pchCppRelativePath = $vcxproj.Project.ItemGroup.ClCompile.PrecompiledHeader | 
                        Where-Object {($_.InnerText -eq "Create")}             | 
                        Select-Object -ExpandProperty ParentNode               | 
                        Select-Object -first 1                                 |
                        Select-Object -ExpandProperty Include

  return $pchCppRelativePath -ne $null
}

Function Set-ProjectIncludePaths([Parameter(Mandatory=$true)] $includeDirectories)
{
  [string] $includePathsString = $includeDirectories -join ";"
  Write-Output "  --> include directories: $includePathsString"
  $ENV:INCLUDE = $includePathsString;
}

Function Generate-Pch([Parameter(Mandatory=$true)] [string]   $stdafxDir,
                      [Parameter(Mandatory=$false)][string[]] $preprocessorDefinitions)
{
  [string] $stdafx = (Canonize-Path -base $stdafxDir -child $kNameStdAfxH)
  [string] $vcxprojShortName = [System.IO.Path]::GetFileNameWithoutExtension($vcxprojPath);
  [string] $stdafxPch = (Join-Path -Path $stdafxDir `
                                   -ChildPath "$vcxprojShortName$kExtensionClangPch")
  Remove-Item -Path "$stdafxPch" -ErrorAction SilentlyContinue | Out-Null

  $global:FilesToDeleteWhenScriptQuits.Add($stdafxPch) | Out-Null

  [string[]] $compilationFlags = @("""$stdafx""",
                                   $kClangFlagEmitPch,
                                   $kClangFlagMinusO,
                                   """$stdafxPch""",
                                   $aClangCompileFlags,
                                   (Get-Variable -name ("kClangFlagsClCompat_" + $platformToolset) -ValueOnly),
                                   $preprocessorDefinitions)

  [System.Diagnostics.Process] $processInfo = Start-Process -FilePath "clang++" `
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

  [System.Xml.XmlElement] $importGroup = $vcxproj.Project.ImportGroup | 
                 Where-Object { $_.GetAttribute("Label")     -eq "PropertySheets" -and
                                $_.GetAttribute("Condition") -eq $kPlatformFilter }

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

Function Get-ProjectClCompileData([Parameter(Mandatory=$true)][string]   $vcxprojOrPropSheetPath,
                                  [Parameter(Mandatory=$true)][string]   $clCompileChildItem,
                                  [Parameter(Mandatory=$true)][string[]] $valuesToIgnore,
                                  [switch] $isPropSheet)
{
  [string] $itemDefinitionFilter = if ($isPropSheet) { "" } else { $kPlatformFilter }

  [xml] $vcxproj = Get-Content $vcxprojOrPropSheetPath
  $ns = New-Object System.Xml.XmlNamespaceManager($vcxproj.NameTable) 
  $ns.AddNamespace("ns", $vcxproj.DocumentElement.NamespaceURI)
           
  [string[]] $tokenData = @()

  [string] $xPathSelector = 'ns:Project/ns:ItemDefinitionGroup'
  if (![string]::IsNullOrEmpty($itemDefinitionFilter))
  {
    $xPathSelector += '[@Condition="' + $itemDefinitionFilter + '"]'
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
  return ($tokens | ForEach-Object { $kClangDefinePrefix + $_ })
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

Function Get-ProjectPlatformToolset([Parameter(Mandatory=$true)][string] $vcxprojPath)
{
  [xml] $vcxproj = Get-Content $vcxprojPath
  $propGroup = $vcxproj.Project.PropertyGroup | `
               Where-Object { $_.GetAttribute("Condition") -eq $kPlatformFilter -and 
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

Function Get-ExeToCall([Parameter(Mandatory=$true)][WorkloadType] $workloadType)
{
  switch ($workloadType)
  {
     "Compile"  { return $kClangCompiler }
     "Tidy"     { return $kClangTidy     }
  }
}

Function Get-CompileCallArguments([Parameter(Mandatory=$true)][string[]]        $preprocessorDefinitions,
                                  [Parameter(Mandatory=$true)][PlatformToolset] $platformToolset,
                                  [Parameter(Mandatory=$true)][string]          $pchFilePath,
                                  [Parameter(Mandatory=$true)][string]          $fileToCompile)
{
  [string[]] $projectCompileArgs = @($kClangFlagIncludePch,
                                     """$pchFilePath""",
                                     """$fileToCompile""",
                                     $aClangCompileFlags,
                                     (Get-Variable -name ("kClangFlagsClCompat_" + $platformToolset) -ValueOnly),
                                     $kClangFlagSupressLINK,
                                     $kClangFlagWarningIsError,
                                     $preprocessorDefinitions)

  return $projectCompileArgs
}

Function Get-TidyCallArguments([Parameter(Mandatory=$true)][string[]]        $preprocessorDefinitions,
                               [Parameter(Mandatory=$true)][PlatformToolset] $platformToolset,
                               [Parameter(Mandatory=$true)][string]          $fileToTidy)
{
  [string[]] $tidyArgs = @("""$fileToTidy""")
  $tidyArgs += $kClangTidyFlagChecks + $aTidyFlags

  # The header-filter flag enables clang-tidy to run on headers too.
  # We want all headers from our directory to be tidied up.
  $tidyArgs += $kClangTidyFlagHeaderFilter + '"' + [regex]::Escape($aDirectory) + '"'

  $tidyArgs += $kClangTidyFlags
  
  # We reuse flags used for compilation and preprocessor definitions.
  $tidyArgs += $aClangCompileFlags
  $tidyArgs += (Get-Variable -name ("kClangFlagsClCompat_" + $platformToolset) -ValueOnly)
  $tidyArgs += $preprocessorDefinitions

  return $tidyArgs
}

Function Get-ExeCallArguments([Parameter(Mandatory=$true) ][string]         $vcxprojPath,
                              [Parameter(Mandatory=$true)][PlatformToolset] $platformToolset,
                              [Parameter(Mandatory=$false)][string]         $pchFilePath,
                              [Parameter(Mandatory=$true) ][string[]]       $preprocessorDefinitions,
                              [Parameter(Mandatory=$true) ][string]         $currentFile,
                              [Parameter(Mandatory=$true) ][WorkloadType]   $workloadType)
{
  switch ($workloadType)
  {
    Compile { return Get-CompileCallArguments -preprocessorDefinitions $preprocessorDefinitions `
                                              -platformToolset         $platformToolset `
                                              -pchFilePath             $pchFilePath `
                                              -fileToCompile           $currentFile }
    Tidy    { return Get-TidyCallArguments -preprocessorDefinitions $preprocessorDefinitions `
                                           -platformToolset         $platformToolset `
                                           -fileToTidy              $currentFile }
  }
}

Function Process-ProjectResult($compileResult)
{
  if (!$compileResult.Success)
  {
    Write-Err ("Error: " + $compileResult.Output)

    if (!$aContinueOnError)
    {
      # Wait for other workers to finish. They have a lock on the PCH file
      Get-Job -state Running | Wait-Job | Remove-Job | Out-Null
      Fail-Script
    }

    $global:FoundErrors = $true
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
                  ForEach-Object { $_.ToString() }                  |`
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

Function Process-Project([Parameter(Mandatory=$true)][string]       $vcxprojPath, 
                         [Parameter(Mandatory=$true)][WorkloadType] $workloadType)
{  
  #-----------------------------------------------------------------------------------------------
  # LOCATE STDAFX.H DIRECTORY

  [string] $stdafxDir = Get-ProjectStdafxDir($vcxprojPath)
  if ([string]::IsNullOrEmpty($stdafxDir))
  {
    if (Project-HasPch($vcxprojPath))
    {
      Fail-Script "Project has a pch cpp, but not a $kNameStdAfxH!"
    }

    Write-Output ("  --> $kNameStdAfxH doesn't exist, skipping project")
    Return
  }
  else
  {
    Write-Output ("  --> $kNameStdAfxH located in $stdafxDir")
  }
  #-----------------------------------------------------------------------------------------------
  # DETECT PROJECT PREPROCESSOR DEFINITIONS

  [string[]] $preprocessorDefinitions = Get-ProjectPreprocessorDefines($vcxprojPath)
  Write-Output "  --> preprocessor definitions: $preprocessorDefinitions"
  
  #-----------------------------------------------------------------------------------------------
  # DETECT PLATFORM TOOLSET

  [PlatformToolset] $platformToolset = Get-ProjectPlatformToolset($vcxprojPath)
  Write-Output "  --> platform toolset: $platformToolset"

  #-----------------------------------------------------------------------------------------------
  # DETECT PROJECT ADDITIONAL INCLUDE DIRECTORIES AND CONSTRUCT INCLUDE PATHS

  [string[]] $includeDirectories = Get-ProjectAdditionalIncludes($vcxprojPath)
  Write-Output "  --> additional includes: $includeDirectories"
  
  $defaultIncludePaths = (Get-Variable -Name ("kIncludePaths_" + $platformToolset) -ValueOnly)
  $includeDirectories = $defaultIncludePaths + $includeDirectories

  if (![string]::IsNullOrEmpty($stdafxDir))
  {
    $includeDirectories = @($stdafxDir) + $includeDirectories
  }
  $includeDirectories = @($aDirectory) + $includeDirectories

  Set-ProjectIncludePaths($includeDirectories)

  #-----------------------------------------------------------------------------------------------
  # FIND LIST OF CPPs TO PROCESS

  [string[]] $projCpps = Get-ProjectCpps($vcxprojPath)

  if (![string]::IsNullOrEmpty($aCppToCompile))
  {
    $projCpps = ( $projCpps | 
                  Where-Object {  IsFileMatchingName -filePath $_ `
                                                     -matchName $aCppToCompile } )
  }
  Write-Output ("  --> processing " + $projCpps.Count + " cpps")
 
  #-----------------------------------------------------------------------------------------------
  # CREATE PCH IF NEED BE

  [string] $pchFilePath = ""
  if ($projCpps.Count -gt 0 -and 
      $workloadType -eq [WorkloadType]::Compile)
  {
    # COMPILE PCH
    Write-Output "  --> generating PCH..."
    $pchFilePath = Generate-Pch -stdafxDir "$stdafxDir" `
                                -preprocessorDefinitions $preprocessorDefinitions
    Write-Output "  --> generated $pchFilePath"
  }
  
  #-----------------------------------------------------------------------------------------------
  # PROCESS CPP FILES. CONSTRUCT COMMAND LINE JOBS TO BE INVOKED

  $clangJobs = @()

  foreach ($cpp in $projCpps)
  {    
    [string] $exeToCall = Get-ExeToCall -workloadType $workloadType
                        
    [string] $exeArgs   = Get-ExeCallArguments -vcxprojPath             $vcxprojPath `
                                               -platformToolset         $platformToolset `
                                               -workloadType            $workloadType `
                                               -pchFilePath             $pchFilePath `
                                               -preprocessorDefinitions $preprocessorDefinitions `
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

Push-Location $aDirectory

# This powershell process may already have completed jobs. Discard them.
Remove-Job -State Completed

Write-Output "[INFO] Source directory: $aDirectory"
Write-Output "  --> scanning for .vcxproj files"

[System.IO.FileInfo[]] $projects = Get-Projects
Write-Output ("  --> found " + $projects.Count + " projects")

[System.IO.FileInfo[]] $projectsToProcess = @()

if ([string]::IsNullOrEmpty($aVcxprojToCompile) -and 
    [string]::IsNullOrEmpty($aVcxprojToIgnore))
{
  Write-Output "[ INFO ] PROCESSING ALL PROJECTS"
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

  [WorkloadType] $workloadType = `
         if ([string]::IsNullOrEmpty($aTidyFlags)) { [WorkloadType]::Compile    } `
         else                                      { [WorkloadType]::Tidy }

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
  Exit-Script($kScriptSuccessExitCode)
}