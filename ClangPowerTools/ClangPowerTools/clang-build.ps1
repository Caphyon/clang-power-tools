#Requires -Version 3
<#
.SYNOPSIS
    Compiles or tidies up code from Visual Studio .vcxproj project files.

.DESCRIPTION
    This PowerShell script scans for all .vcxproj Visual Studio projects inside a source directory.
    One or more of these projects will be compiled or tidied up (modernized), using Clang.

.PARAMETER aSolutionsPath
    Alias 'dir'. Source directory to find sln files. 
                 Projects will be extracted from each sln.
    
    Important: You can pass an absolute path to a sln. This way, no file searching will be done, and
               only the projects from this solution file will be taken into acount.

.PARAMETER aVcxprojToCompile
    Alias 'proj'. Array of project(s) to compile. If empty, all projects found in solutions are compiled.
    If the -literal switch is present, name is matched exactly. Otherwise, regex matching is used, 
    e.g. "msicomp" compiles all projects containing 'msicomp'.

    Absolute disk paths to vcxproj files are accepted.
    
    Can be passed as comma separated values.

.PARAMETER aVcxprojToIgnore
    Alias 'proj-ignore'. Array of project(s) to ignore, from the matched ones. 
    If empty, all already matched projects are compiled.
    If the -literal switch is present, name is matched exactly. Otherwise, regex matching is used, 
    e.g. "msicomp" ignores projects containing 'msicomp'.

    Can be passed as comma separated values.

.PARAMETER aVcxprojConfigPlatform
    Alias 'active-config'. The configuration-platform pair, separated by |, 
    to be used when processing project files.

    E.g. 'Debug|Win32'. 
    If not specified, the first configuration-plaform found in the current project is used.

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

.PARAMETER aTreatAdditionalIncludesAsSystemIncludes
     Alias 'treat-sai'. Switch to treat project additional include directories as system includes.

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

      If '.clang-tidy' value is given, configuration will be read from .clang-tidy file 
      in the closest parent directory.
      
.PARAMETER aTidyFixFlags
      Alias 'tidy-fix'. If not empty clang-tidy will be called with given flags, instead of clang++. 
      The tidy operation is applied to whole translation units, meaning all directory headers 
      included in the CPP will be tidied up too. Changes will be applied to the file(s).

      If present, this parameter takes precedence over aTidyFlags.

      If '.clang-tidy' value is given, configuration will be read from .clang-tidy file 
      in the closest parent directory.
      
.PARAMETER aAfterTidyFixFormatStyle
      Alias 'format-style'. Used in combination with 'tidy-fix'. If present, clang-tidy will
      also format the fixed file(s), using the specified style.
      Possible values: - not present, no formatting will be done
                       - 'file'
                           Literally 'file', not a placeholder. 
                           Uses .clang-format file in the closest parent directory.
                       - 'llvm'
                       - 'google'
                       - 'webkit'
                       - 'mozilla'

.PARAMETER aVisualStudioVersion
      Alias 'vs-ver'. Version of Visual Studio (VC++) installed and that'll be used for 
      standard library include directories. E.g. 2017.

.PARAMETER aVisualStudioSku
      Alias 'vs-sku'. Sku of Visual Studio (VC++) installed and that'll be used for 
      standard library include directories. E.g. Professional.

.NOTES
    Author: Gabriel Diaconita
#>
param( [alias("dir")]          [Parameter(Mandatory=$true)] [string]   $aSolutionsPath
     , [alias("proj")]         [Parameter(Mandatory=$false)][string[]] $aVcxprojToCompile
     , [alias("proj-ignore")]  [Parameter(Mandatory=$false)][string[]] $aVcxprojToIgnore
     , [alias("active-config")][Parameter(Mandatory=$false)][string]   $aVcxprojConfigPlatform
     , [alias("file")]         [Parameter(Mandatory=$false)][string]   $aCppToCompile
     , [alias("file-ignore")]  [Parameter(Mandatory=$false)][string[]] $aCppToIgnore
     , [alias("parallel")]     [Parameter(Mandatory=$false)][switch]   $aUseParallelCompile
     , [alias("continue")]     [Parameter(Mandatory=$false)][switch]   $aContinueOnError
     , [alias("treat-sai")]    [Parameter(Mandatory=$false)][switch]   $aTreatAdditionalIncludesAsSystemIncludes
     , [alias("clang-flags")]  [Parameter(Mandatory=$true)] [string[]] $aClangCompileFlags
     , [alias("literal")]      [Parameter(Mandatory=$false)][switch]   $aDisableNameRegexMatching
     , [alias("tidy")]         [Parameter(Mandatory=$false)][string]   $aTidyFlags
     , [alias("tidy-fix")]     [Parameter(Mandatory=$false)][string]   $aTidyFixFlags
     , [alias("header-filter")][Parameter(Mandatory=$false)][string]   $aTidyHeaderFilter
     , [alias("format-style")] [Parameter(Mandatory=$false)][string]   $aAfterTidyFixFormatStyle
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
Set-Variable -name kExtensionSolution        -value ".sln"              -option Constant
Set-Variable -name kExtensionClangPch        -value ".clang.pch"        -option Constant

# ------------------------------------------------------------------------------------------------
# Vcxproj Related Constants

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
             -value "ns:Project/ns:ItemDefinitionGroup/ns:ClCompile/ns:ForcedIncludeFiles" `
             -option Constant 

Set-Variable -name kVcxprojXpathPCH `
             -value "ns:Project/ns:ItemGroup/ns:ClCompile/ns:PrecompiledHeader[text()='Create']" `
             -option Constant

Set-Variable -name kVcxprojXpathToolset `
             -value "ns:Project/ns:PropertyGroup[@Label='Configuration']/ns:PlatformToolset" `
             -option Constant

Set-Variable -name kVcxprojXpathDefaultConfigPlatform `
             -value "ns:Project/ns:ItemGroup[@Label='ProjectConfigurations']/ns:ProjectConfiguration[1]" `
             -option Constant

Set-Variable -name kVcxprojXpathConditionedElements `
             -value "//*[@Condition]" `
             -option Constant

Set-Variable -name kVcxprojXpathChooseElements `
             -value "ns:Project//ns:Choose" `
             -option Constant

Set-Variable -name kVcxprojXpathPropGroupElements `
             -value "ns:Project//ns:PropertyGroup/*" `
             -option Constant

Set-Variable -name kVcxprojXpathCppStandard `
             -value "ns:Project/ns:ItemDefinitionGroup/ns:ClCompile/ns:LanguageStandard" `
             -option Constant

Set-Variable -name kVSDefaultWinSDK            -value '8.1'             -option Constant
Set-Variable -name kVSDefaultWinSDK_XP         -value '7.0'             -option Constant

# ------------------------------------------------------------------------------------------------
# Clang-Related Constants

Set-Variable -name kDefaultCppStd           -value "stdcpp14"              -option Constant
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
Set-Variable -name kClangTidyUseFile          -value ".clang-tidy"      -option Constant
Set-Variable -name kClangTidyFormatStyle      -value "-format-style="   -option Constant

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

Set-Variable -name "kMsbuildExpressionToPsRules" -option Constant `
             -value    @(<# backticks are control characters in PS, replace them #>
                         ('`'                     , ''''                 )`
                         <# Temporarily replace     $( #>                 `
                       , ('\$\s*\('               , '!@#'                )`
                         <# Escape $                   #>                 `
                       , ('\$'                    , '`$'                 )`
                         <# Put back $(                #>                 `
                       , ('!@#'                   , '$('                 )`
                         <# Various operators          #>                 `
                       , ("([\s\)\'""])!="        , '$1 -ne '            )`
                       , ("([\s\)\'""])<="        , '$1 -le '            )`
                       , ("([\s\)\'""])>="        , '$1 -ge '            )`
                       , ("([\s\)\'""])=="        , '$1 -eq '            )`
                       , ("([\s\)\'""])<"         , '$1 -lt '            )`
                       , ("([\s\)\'""])>"         , '$1 -gt '            )`
                       , ("([\s\)\'""])or"        , '$1 -or '            )`
                       , ("([\s\)\'""])and"       , '$1 -and '           )`
                         <# Use only double quotes #>                     `
                       , ("\'"                    , '"'                  )`
      , ("Exists\((.*?)\)(\s|$)"           , '(Exists($1))$2'            )`
      , ("HasTrailingSlash\((.*?)\)(\s|$)" , '(HasTrailingSlash($1))$2'  )`
      , ("(\`$\()(Registry:)(.*?)(\))"     , '$$(GetRegValue("$3"))'     )`
                       )

Set-Variable -name "kMsbuildConditionToPsRules" -option Constant `
             -value   @(<# Use only double quotes #>                     `
                         ("\'"                    , '"'                  )`
                        <# We need to escape double quotes since we will eval() the condition #> `
                       , ('"'                     , '""'                 )`
                       )
  
Set-Variable -name "kRedundantSeparatorsReplaceRules" -option Constant `
              -value @( <# handle multiple consecutive separators #>    `
                        (";+" , ";")                                     `
                        <# handle separator at end                #>    `
                      , (";$" , "")                                      `
                        <# handle separator at beginning          #>    `
                      , ("^;" , "")                                      `
                      )

#-------------------------------------------------------------------------------------------------
# Global variables

# temporary files created during project processing (e.g. PCH files)
[System.Collections.ArrayList] $global:FilesToDeleteWhenScriptQuits = @()

# vcxproj and property sheet files declare MsBuild properties (e.g. $(MYPROP)).
# they are used in project xml nodes expressions. we have a 
# translation engine (MSBUILD-POWERSHELL) for these. it relies on
# PowerShell to evaluate these expressions. We have to inject project 
# properties in the Powershell runtime context. We keep track of them in
# this list, to be cleaned before the next project begins processing
[System.Collections.ArrayList] $global:ProjectSpecificVariables     = @()

# flag to signal when errors are encounteres during project processing
[Boolean]                      $global:FoundErrors                  = $false

# current vcxproj and property sheets
[xml[]]  $global:projectFiles                    = @();

# path of current project
[string] $global:vcxprojPath                     = "";

# namespace of current project vcxproj XML
[System.Xml.XmlNamespaceManager] $global:xpathNS = $null;

# filePath-fileData for SLN files located in source directory
[System.Collections.Generic.Dictionary[String,String]] $global:slnFiles = @{}

#-------------------------------------------------------------------------------------------------
# Global functions

Function Exit-Script([Parameter(Mandatory=$false)][int] $code = 0)
{
  Write-Verbose-Array -array $global:FilesToDeleteWhenScriptQuits `
                      -name "Cleaning up PCH temporaries"
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

Function Set-Var([parameter(Mandatory=$false)][string] $name,
                 [parameter(Mandatory=$false)][string] $value)
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

Function Write-Verbose-Array($array, $name)
{
  Write-Verbose "$($name):"
  $array | ForEach-Object { Write-Verbose "  $_" }
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
  return ([System.IO.Path]::GetDirectoryName($filePath) + "\")
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
  if ([System.IO.Path]::IsPathRooted($matchName))
  {
    return $filePath -ieq $matchName
  }

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

 <#
  .DESCRIPTION
  Merges an absolute and a relative file path.
  .EXAMPLE
  Havin base = C:\Windows\System32 and child = .. we get C:\Windows
  .EXAMPLE
  Havin base = C:\Windows\System32 and child = ..\..\..\.. we get C:\ (cannot go further up)
  .PARAMETER base
  The absolute path from which we start.
  .PARAMETER child
  The relative path to be merged into base. 
  .PARAMETER ignoreErrors
  If this switch is not present, an error will be triggered if the resulting path
  is not present on disk (e.g. c:\Windows\System33).

  If present and the resulting path does not exist, the function returns an empty string.
  #>
Function Canonize-Path( [Parameter(Mandatory=$true)][string] $base
                      , [Parameter(Mandatory=$true)][string] $child
                      , [switch] $ignoreErrors)
{
  [string] $errorAction = If ($ignoreErrors) {"SilentlyContinue"} Else {"Stop"}
  
  if ([System.IO.Path]::IsPathRooted($child)) 
  {
    if (!(Test-Path $child))
    {
      return ""
    }
    return $child
  }
  else
  {
    [string[]] $paths = Join-Path -Path "$base" -ChildPath "$child" -Resolve -ErrorAction $errorAction
    return $paths
  }
}

Function Get-SourceDirectory()
{
  [bool] $isDirectory = ($(Get-Item $aSolutionsPath) -is [System.IO.DirectoryInfo])
  if ($isDirectory)
  {
    return $aSolutionsPath
  }
  else 
  {
    return (Get-FileDirectory -filePath $aSolutionsPath)
  }
}

function Load-Solutions()
{
   Write-Verbose "Scanning for solution files"
   $slns = Get-ChildItem -recurse -LiteralPath "$aSolutionsPath" `
           | Where-Object { $_.Extension -eq $kExtensionSolution }
   foreach ($sln in $slns)
   {
     $slnPath = $sln.FullName
     $global:slnFiles[$slnPath] = (Get-Content $slnPath)
   }

   Write-Verbose-Array -array $global:slnFiles.Keys  -name "Solution file paths"
}

function Get-SolutionProjects([Parameter(Mandatory=$true)][string] $slnPath)
{
  [string] $slnDirectory = Get-FileDirectory -file $slnPath
  $matches = [regex]::Matches($global:slnFiles[$slnPath], 'Project\([{}\"A-Z0-9\-]+\) = \S+,\s(\S+),')
  $projectAbsolutePaths = $matches `
    | ForEach-Object { Canonize-Path -base $slnDirectory `
                                     -child $_.Groups[1].Value.Replace('"','') -ignoreErrors } `
    | Where-Object { ! [string]::IsNullOrEmpty($_) -and $_.EndsWith($kExtensionVcxproj) }
  return $projectAbsolutePaths
}

function Get-ProjectSolution()
{
  foreach ($slnPath in $global:slnFiles.Keys)
  {
    [string[]] $solutionProjectPaths = Get-SolutionProjects $slnPath
    if ($solutionProjectPaths -and $solutionProjectPaths -contains $global:vcxprojPath)
    {
      return $slnPath
    }
  }
  return ""
}

Function Get-MscVer()
{
  return ((Get-Item "$(Get-VisualStudio-Path)\VC\Tools\MSVC\" | Get-ChildItem) | select -last 1).Name
}

Function InitializeMsBuildCurrentFileProperties([Parameter(Mandatory=$true)][string] $filePath)
{
  Set-Var -name "MSBuildThisFileFullPath"  -value $filePath
  Set-Var -name "MSBuildThisFileExtension" -value ([IO.Path]::GetExtension($filePath))
  Set-Var -name "MSBuildThisFile"          -value (Get-FileName -path $filePath)
  Set-Var -name "MSBuildThisFileName"      -value (Get-FileName -path $filePath -noext)
  Set-Var -name "MSBuildThisFileDirectory" -value (Get-FileDirectory -filePath $filePath)
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
  if ($aVisualStudioVersion -eq "2015")
  {
    $vsVer = "14.0"
  }
  Set-Var -name "VisualStudioVersion"    -value $vsVer
  Set-Var -name "MSBuildToolsVersion"    -value $vsVer

  [string] $projectSlnPath = Get-ProjectSolution
  [string] $projectSlnDir = Get-FileDirectory -filePath $projectSlnPath
  Set-Var -name "SolutionDir" -value $projectSlnDir
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

Function Get-ProjectFilesToCompile([Parameter(Mandatory=$false)][string] $pchCppName)
{
  [Boolean] $pchDisabled = [string]::IsNullOrEmpty($pchCppName)

  [string[]] $projectEntries = Select-ProjectNodes($kVcxprojXpathCompileFiles) | `
                      Where-Object { ($_.Include -ne $null) -and
                                     ($pchDisabled -or ($_.Include -ne $pchCppName))
                                   }                                           | `
                      Select-Object -Property "Include" -ExpandProperty "Include"
  [string[]] $files = @()
  foreach ($entry in $projectEntries)
  {
    [string[]] $matchedFiles = Canonize-Path -base $ProjectDir -child $entry
    if ($matchedFiles.Count -gt 0)
    {
      $files += $matchedFiles
    }
  }

  if ($files.Count -gt 0)
  {
    $files = $files | Where-Object { ! (Should-IgnoreFile -file $_) }
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

Function Get-Project-SDKVer()
{
  [string] $sdkVer = (Select-ProjectNodes($kVcxprojXpathWinPlatformVer)).InnerText

  If ([string]::IsNullOrEmpty($sdkVer)) { "" } Else { $sdkVer.Trim() }
}

Function Is-Project-Unicode()
{
  $propGroup = Select-ProjectNodes("ns:Project/ns:PropertyGroup[@Label='Configuration']")
  
  return ($propGroup.CharacterSet -eq "Unicode")
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
                ;  'stdcpp14'    = 'c++14'
                ;  'stdcpp17'    = 'c++17'
                }

  [string] $cppStdClangValue = $cppStdMap[$cppStd]
  Set-Var -name $cachedValueVarName -value $cppStdClangValue

  return $cppStdClangValue
}

Function Get-ClangCompileFlags()
{
  [string[]] $flags = $aClangCompileFlags
  if (!($flags -match "-std=.*"))
  {
    [string] $cppStandard = Get-Project-CppStandard

    $flags = @("-std=$cppStandard") + $flags
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
      [string] $product = "Microsoft.VisualStudio.Product.$aVisualStudioSku"
      return (& "$kVsWhereLocation" -nologo `
                                    -property installationPath `
                                    -products $product `
                                    -prerelease)
    }

    if (Test-Path -Path $kVs15DefaultLocation)
    {
      return $kVs15DefaultLocation
    }

    throw "Cannot locate Visual Studio location"
  }
}

Function Get-ProjectIncludeDirectories([Parameter(Mandatory=$false)][string] $stdafxDir)
{
  [string[]] $returnArray = ($IncludePath -split ";")                                                  | `
                            Where-Object { ![string]::IsNullOrEmpty($_) }                              | `
                            ForEach-Object { Canonize-Path -base $ProjectDir -child $_ -ignoreErrors } | `
                            Where-Object { ![string]::IsNullOrEmpty($_) }                              | `
                            ForEach-Object { $_ -replace '\\$', '' }                                   
  if ($env:CPT_LOAD_ALL -eq '1')
  {
    return $returnArray
  }

  [string] $vsPath = Get-VisualStudio-Path
  Write-Verbose "Visual Studio location: $vsPath"
  
  [string] $platformToolset = Get-ProjectPlatformToolset

  if ($aVisualStudioVersion -eq "2015")
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

  if (![string]::IsNullOrEmpty($stdafxDir))
  {
    $returnArray = @($stdafxDir) + $returnArray
  }

  return ( $returnArray | ForEach-Object { $_ -replace '\\$', '' } )
}

Function Get-Projects()
{
  [string[]] $projects = @()

  foreach ($slnPath in $global:slnFiles.Keys)
  {
    [string[]] $solutionProjects = Get-SolutionProjects -slnPath $slnPath
    if ($solutionProjects.Count -gt 0)
    {
      $projects += $solutionProjects
    }
  }

  return ($projects | Select -Unique);
}

Function Get-PchCppIncludeHeader([Parameter(Mandatory=$true)][string] $pchCppFile)
{
  [string] $cppPath = Canonize-Path -base $ProjectDir -child $pchCppFile
  [string] $fileContent = Get-Content -path $cppPath

  return [regex]::match($fileContent,'#include "(\S+)"').Groups[1].Value
}

<#
.DESCRIPTION
  Retrieve directory in which stdafx.h resides
#>
Function Get-ProjectStdafxDir([Parameter(Mandatory=$true)][string] $pchHeaderName)
{
  [string[]] $projectHeaders = Get-ProjectHeaders
  [string] $stdafxPath = $projectHeaders | Where-Object { (Get-FileName -path $_) -cmatch $pchHeaderName }
  if ([string]::IsNullOrEmpty($stdafxPath))
  {
    $stdafxPath = Canonize-Path -base $ProjectDir `
                                -child $pchHeaderName
  }

  [string] $stdafxDir = Get-FileDirectory($stdafxPath)

  return $stdafxDir
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

Function Get-ClangIncludeDirectories( [Parameter(Mandatory=$false)][string[]] $includeDirectories
                                    , [Parameter(Mandatory=$false)][string[]] $additionalIncludeDirectories
                                    )
{
  [string[]] $returnDirs = @()

  foreach ($includeDir in $includeDirectories)
  {
    $returnDirs += "-isystem""$includeDir"""
  }
  foreach ($includeDir in $additionalIncludeDirectories)
  {
    if ($aTreatAdditionalIncludesAsSystemIncludes)
    {
      $returnDirs += "-isystem""$includeDir"""
    }
    else
    {
      $returnDirs += "-I""$includeDir"""
    }
  }

  return $returnDirs
}

Function Generate-Pch( [Parameter(Mandatory=$true)] [string]   $stdafxDir
                     , [Parameter(Mandatory=$false)][string[]] $includeDirectories
                     , [Parameter(Mandatory=$false)][string[]] $additionalIncludeDirectories
                     , [Parameter(Mandatory=$true)] [string]   $stdafxHeaderName
                     , [Parameter(Mandatory=$false)][string[]] $preprocessorDefinitions)
{
  [string] $stdafx = (Canonize-Path -base $stdafxDir -child $stdafxHeaderName)
  [string] $vcxprojShortName = [System.IO.Path]::GetFileNameWithoutExtension($global:vcxprojPath);
  [string] $stdafxPch = (Join-Path -path (Get-SourceDirectory) `
                                   -ChildPath "$vcxprojShortName$kExtensionClangPch")
  Remove-Item -Path "$stdafxPch" -ErrorAction SilentlyContinue | Out-Null

  $global:FilesToDeleteWhenScriptQuits.Add($stdafxPch) | Out-Null

  # Supress -Werror for PCH generation as it throws warnings quite often in code we cannot control
  [string[]] $clangFlags = Get-ClangCompileFlags | Where-Object { $_ -ne $kClangFlagWarningIsError }

  [string[]] $compilationFlags = @("""$stdafx"""
                                  ,$kClangFlagEmitPch
                                  ,$kClangFlagMinusO
                                  ,"""$stdafxPch"""
                                  ,$clangFlags
                                  ,$kClangFlagNoUnusedArg
                                  ,$preprocessorDefinitions
                                  )

  $compilationFlags += Get-ClangIncludeDirectories -includeDirectories           $includeDirectories `
                                                   -additionalIncludeDirectories $additionalIncludeDirectories

  Write-Verbose "INVOKE: ""$($global:llvmLocation)\$kClangCompiler"" $compilationFlags"

  [System.Diagnostics.Process] $processInfo = Start-Process -FilePath $kClangCompiler `
                                                            -ArgumentList $compilationFlags `
                                                            -WorkingDirectory "$(Get-SourceDirectory)" `
                                                            -NoNewWindow `
                                                            -Wait `
                                                            -PassThru
  if ($processInfo.ExitCode -ne 0)
  {
    Fail-Script "Errors encountered during PCH creation"
  }

  return $stdafxPch
}

function HasTrailingSlash([Parameter(Mandatory=$true)][string] $str)
{
  return $str.EndsWith('\') -or $str.EndsWith('/')
}

function Exists([Parameter(Mandatory=$false)][string] $path)
{
  if ([string]::IsNullOrEmpty($path))
  {
    return $false
  }
  
  return Test-Path $path
}

function GetRegValue([Parameter(Mandatory=$true)][string] $regPath)
{
  Write-Debug "REG_READ $regPath"

  [int] $separatorIndex = $regPath.IndexOf('@')
  [string] $valueName = ""
  if ($separatorIndex -gt 0)
  {
    [string] $valueName = $regPath.Substring($separatorIndex + 1)
    $regPath = $regPath.Substring(0, $separatorIndex)
  }
  if ([string]::IsNullOrEmpty($valueName))
  {
    throw "Cannot retrieve an empty registry value"
  }
  $regPath = $regPath -replace "HKEY_LOCAL_MACHINE\\", "HKLM:\"

  if (Test-Path $regPath)
  {
    return (Get-Item $regPath).GetValue($valueName)
  }
  else
  {
    return ""
  }
}

function Evaluate-MSBuildExpression([string] $expression, [switch] $isCondition)
{  
  Write-Debug "Start evaluate MSBuild expression $expression"

  foreach ($rule in $kMsbuildExpressionToPsRules)
  {
    $expression = $expression -replace $rule[0], $rule[1]
  }
  
  if ( !$isCondition -and ($expression.IndexOf('$') -lt 0))
  {
    # we can stop here, further processing is not required
    return $expression
  }
  
  [int] $expressionStartIndex = -1
  [int] $openParantheses = 0
  for ([int] $i = 0; $i -lt $expression.Length; $i += 1)
  {
    if ($expression.Substring($i, 1) -eq '(')
    {
      if ($i -gt 0 -and $expressionStartIndex -lt 0 -and $expression.Substring($i - 1, 1) -eq '$')
      {
        $expressionStartIndex = $i - 1
      }

      if ($expressionStartIndex -ge 0)
      {
        $openParantheses += 1
      }
    }

    if ($expression.Substring($i, 1) -eq ')'  -and $expressionStartIndex -ge 0)
    {
      $openParantheses -= 1
      if ($openParantheses -lt 0)
      {
        throw "Parse error"
      }
      if ($openParantheses -eq 0)
      {
        [string] $content = $expression.Substring($expressionStartIndex + 2, 
                                                  $i - $expressionStartIndex - 2)
        [int] $initialLength = $content.Length

        if ([regex]::Match($content, "[a-zA-Z_][a-zA-Z0-9_\-]+").Value -eq $content)
        {
          # we have a plain property retrieval
          $content = "`${$content}"
        }
        else
        {
          # dealing with a more complex expression
          $content = $content -replace '(^|\s+|\$\()([a-zA-Z_][a-zA-Z0-9_]+)(\.|\)|$)', '$1$$$2$3'
        }

        $newCond = $expression.Substring(0, $expressionStartIndex + 2) + 
                   $content + $expression.Substring($i)
        $expression = $newCond
        
        $i += ($content.Length - $initialLength)
        $expressionStartIndex = -1
      }
    }
  }

  Write-Debug "Intermediate PS expression: $expression"

  try
  {
    [string] $toInvoke = "(`$s = ""$expression"")"
    if ($isCondition)
    {
      $toInvoke = "(`$s = ""`$($expression)"")"
    }

    $res = Invoke-Expression $toInvoke
  }
  catch
  {
    write-debug $_.Exception.Message
  }

  Write-Debug "Evaluated expression to: $res"

  return $res
}
function Evaluate-MSBuildCondition([Parameter(Mandatory=$true)][string] $condition)
{
  Write-Debug "Evaluating condition $condition"
  foreach ($rule in $kMsbuildConditionToPsRules)
  {
    $condition = $condition -replace $rule[0], $rule[1]
  }
  $expression = Evaluate-MSBuildExpression -expression $condition -isCondition

  if ($expression -ieq "true")
  {
    return $true
  } 

  if ($expression -ieq "false")
  {
    return $false
  }

  [bool] $res = $false 
  try
  {
    $res = (Invoke-Expression $expression) -eq $true
  }
  catch
  {
    Write-Debug $_.Exception.Message
  }
  Write-Debug "Evaluated condition to $res" 

  return $res
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
                                  ,[System.Xml.XmlNode] $nodeToInheritFrom
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
  $replacementRules = @(,($whatToReplace, $replaceWith)) + $kRedundantSeparatorsReplaceRules
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
function Select-ProjectNodes([Parameter(Mandatory=$true)]  [string][string] $xpath
                            ,[Parameter(Mandatory=$false)] [int]            $fileIndex = 0)
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

  if ($nodes[$nodes.Count -1]."#text")
  {
    # we found textual settings that can be inherited. see if we should inherit
    
    [System.Xml.XmlNode] $nodeToReturn = $nodes[$nodes.Count -1]
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
    Detect-ProjectDefaultConfigPlatform $node.ChildNodes[0].GetAttribute("Include")
  }

  if ($node.ParentNode.Name -ieq "PropertyGroup")
  {
    # set new property value
    [string] $propertyName  = $node.Name
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
  $global:xpathNS     = New-Object System.Xml.XmlNamespaceManager($fileXml.NameTable) 
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
   
  # see if we can find a Directory.Build.props automatic prop sheet
  [string[]] $propSheetAbsolutePaths = @()
  [xml[]]    $autoPropSheetXmls      = $null
  $autoPropSheet = Get-AutoPropertySheet
  if (![string]::IsNullOrEmpty($autoPropSheet))
  {
    SanitizeProjectFile -projectFilePath $autoPropSheet
    $autoPropSheetXmls = $global:projectFiles
  }

  # the auto-prop sheet has to be last in the node retrieval sources
  # but properties defined in it have to be accessible in all project files :(
  $global:projectFiles = @()

  SanitizeProjectFile -projectFilePath $global:vcxprojPath

  if ($autoPropSheetXml)
  {
    $global:projectFiles += $autoPropSheetXmls
  }
}

<#
.DESCRIPTION
  Retrieve array of preprocessor definitions for a given project, in Clang format (-DNAME )
#>
Function Get-ProjectPreprocessorDefines()
{
  [string[]] $tokens = (Select-ProjectNodes $kVcxprojXpathPreprocessorDefs).InnerText -split ";"

  # make sure we add the required prefix and escape double quotes
  [string[]]$defines = ( $tokens             | `
                         Where-Object { $_ } | `
                         ForEach-Object { '"' + $(($kClangDefinePrefix + $_) -replace '"','\"') + '"' } )

  if (Is-Project-Unicode)
  {
    $defines += $kDefinesUnicode
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
    if ([string]::IsNullOrEmpty($token))
    {
      continue
    }
    
    [string] $includePath = Canonize-Path -base $ProjectDir -child $token -ignoreErrors
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
                                 , [Parameter(Mandatory=$false)][string[]] $includeDirectories
                                 , [Parameter(Mandatory=$false)][string[]] $additionalIncludeDirectories
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
                          , @(Get-ClangCompileFlags)
                          , $kClangFlagSupressLINK
                          , $preprocessorDefinitions
                          )

  $projectCompileArgs += Get-ClangIncludeDirectories -includeDirectories           $includeDirectories `
                                                     -additionalIncludeDirectories $additionalIncludeDirectories

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
                              , [Parameter(Mandatory=$false)][string[]] $includeDirectories
                              , [Parameter(Mandatory=$false)][string[]] $additionalIncludeDirectories
                              , [Parameter(Mandatory=$false)][string[]] $forceIncludeFiles
                              , [Parameter(Mandatory=$true)][string]   $fileToTidy
                              , [Parameter(Mandatory=$false)][string]  $pchFilePath
                              , [Parameter(Mandatory=$false)][switch]  $fix)
{
  [string[]] $tidyArgs = @("""$fileToTidy""")
  if ($fix -and $aTidyFixFlags -ne $kClangTidyUseFile)
  {
    $tidyArgs += "$kClangTidyFlagChecks$aTidyFixFlags"
  } 
  elseif ($aTidyFlags -ne $kClangTidyUseFile)
  {
    $tidyArgs += "$kClangTidyFlagChecks$aTidyFlags"
  }

  # The header-filter flag enables clang-tidy to run on headers too.
  if (![string]::IsNullOrEmpty($aTidyHeaderFilter))
  {
    if ($aTidyHeaderFilter -eq '_')
    {
      [string] $fileNameMatch = """$(Get-FileName -path $fileToTidy -noext).*"""
      $tidyArgs += "$kClangTidyFlagHeaderFilter$fileNameMatch"
    }
    else
    {
      $tidyArgs += "$kClangTidyFlagHeaderFilter""$aTidyHeaderFilter"""
    }
  }

  if ($fix)
  {
    if (![string]::IsNullOrEmpty($aAfterTidyFixFormatStyle))
    {
      $tidyArgs += "$kClangTidyFormatStyle$aAfterTidyFixFormatStyle"
    }

    $tidyArgs += $kClangTidyFixFlags
  }
  else 
  {
    $tidyArgs += $kClangTidyFlags
  }
  
  $tidyArgs += Get-ClangIncludeDirectories -includeDirectories           $includeDirectories `
                                           -additionalIncludeDirectories $additionalIncludeDirectories
  
  # We reuse flags used for compilation and preprocessor definitions.
  $tidyArgs += @(Get-ClangCompileFlags)
  $tidyArgs += $preprocessorDefinitions
  $tidyArgs += $kClangFlagFileIsCPP
  
  if (! [string]::IsNullOrEmpty($pchFilePath))
  {
    $tidyArgs += @($kClangFlagIncludePch , """$pchFilePath""")
  }

  if ($forceIncludeFiles)
  {
    $tidyArgs += $kClangFlagNoMsInclude;
    
    foreach ($file in $forceIncludeFiles)
    {
      $tidyArgs += "$kClangFlagForceInclude $file"
    }
  }

  return $tidyArgs
}

Function Get-ExeCallArguments( [Parameter(Mandatory=$false)][string]       $pchFilePath
                             , [Parameter(Mandatory=$false)][string[]]     $includeDirectories
                             , [Parameter(Mandatory=$false)][string[]]     $additionalIncludeDirectories
                             , [Parameter(Mandatory=$false)][string[]]     $preprocessorDefinitions
                             , [Parameter(Mandatory=$false)][string[]]     $forceIncludeFiles
                             , [Parameter(Mandatory=$true) ][string]       $currentFile
                             , [Parameter(Mandatory=$true) ][WorkloadType] $workloadType)
{
  switch ($workloadType)
  {
    Compile { return Get-CompileCallArguments -preprocessorDefinitions       $preprocessorDefinitions `
                                              -includeDirectories            $includeDirectories `
                                              -additionalIncludeDirectories  $additionalIncludeDirectories `
                                              -forceIncludeFiles             $forceIncludeFiles `
                                              -pchFilePath                   $pchFilePath `
                                              -fileToCompile                 $currentFile }
    Tidy    { return Get-TidyCallArguments -preprocessorDefinitions       $preprocessorDefinitions `
                                           -includeDirectories            $includeDirectories `
                                           -additionalIncludeDirectories  $additionalIncludeDirectories `
                                           -forceIncludeFiles             $forceIncludeFiles `
                                           -pchFilePath                   $pchFilePath `
                                           -fileToTidy                    $currentFile }
    TidyFix { return Get-TidyCallArguments -preprocessorDefinitions       $preprocessorDefinitions `
                                           -includeDirectories            $includeDirectories `
                                           -additionalIncludeDirectories  $additionalIncludeDirectories `
                                           -forceIncludeFiles             $forceIncludeFiles `
                                           -pchFilePath                   $pchFilePath `
                                           -fileToTidy                    $currentFile `
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
  # FIND FORCE INCLUDES

  [string[]] $forceIncludeFiles = Get-ProjectForceIncludes
  Write-Verbose "Force includes: $forceIncludeFiles"

  #-----------------------------------------------------------------------------------------------
  # LOCATE STDAFX.H DIRECTORY

  [string] $stdafxCpp    = Get-Project-PchCpp
  [string] $stdafxDir    = ""
  [string] $stdafxHeader = ""
  
  if (![string]::IsNullOrEmpty($stdafxCpp))
  {
    Write-Verbose "PCH cpp name: $stdafxCpp"

    if ($forceIncludeFiles.Count -gt 0)
    {
      $stdafxHeader = $forceIncludeFiles[0]
    }
    else
    {
      $stdafxHeader = Get-PchCppIncludeHeader -pchCppFile $stdafxCpp
    }

    Write-Verbose "PCH header name: $stdafxHeader"
    $stdafxDir = Get-ProjectStdafxDir -pchHeaderName $stdafxHeader
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

  [string[]] $preprocessorDefinitions = Get-ProjectPreprocessorDefines
  if ($aVisualStudioVersion -eq "2017")
  {
    # [HACK] pch generation crashes on VS 15.5 because of STL library, known bug.
    # Triggered by addition of line directives to improve std::function debugging.
    # There's a definition that supresses line directives.

    [string] $mscVer = Get-MscVer -visualStudioPath $vsPath
    if ($mscVer -eq "14.12.25827")
    {
      $preprocessorDefinitions += "-D_DEBUG_FUNCTIONAL_MACHINERY"
    }
  }
  
  Write-Verbose-Array -array $preprocessorDefinitions -name "Preprocessor definitions"
  
  #-----------------------------------------------------------------------------------------------
  # DETECT PLATFORM TOOLSET

  [string] $platformToolset = Get-ProjectPlatformToolset
  Write-Verbose "Platform toolset: $platformToolset"

  #-----------------------------------------------------------------------------------------------
  # DETECT PROJECT ADDITIONAL INCLUDE DIRECTORIES AND CONSTRUCT INCLUDE PATHS

  [string[]] $additionalIncludeDirectories = Get-ProjectAdditionalIncludes
  Write-Verbose-Array -array $additionalIncludeDirectories -name "Additional include directories"
  
  [string[]] $includeDirectories = Get-ProjectIncludeDirectories -stdafxDir $stdafxDir
  Write-Verbose-Array -array $includeDirectories -name "Include directories"

  #-----------------------------------------------------------------------------------------------
  # FIND LIST OF CPPs TO PROCESS

  [string[]] $projCpps = Get-ProjectFilesToCompile -pchCppName  $stdafxCpp

  if (![string]::IsNullOrEmpty($aCppToCompile))
  {
    $projCpps = ( $projCpps | 
                  Where-Object {  IsFileMatchingName -filePath $_ `
                                                     -matchName $aCppToCompile } )
  }
  Write-Verbose ("Processing " + $projCpps.Count + " cpps")
 
  #-----------------------------------------------------------------------------------------------
  # CREATE PCH IF NEED BE, ONLY FOR TWO CPPS OR MORE

  [string] $pchFilePath = ""
  if ($projCpps.Count -ge 2 -and 
      ![string]::IsNullOrEmpty($stdafxDir))
  {
    # COMPILE PCH
    Write-Verbose "Generating PCH..."
    $pchFilePath = Generate-Pch -stdafxDir        $stdafxDir    `
                                -stdafxHeaderName $stdafxHeader `
                                -preprocessorDefinitions $preprocessorDefinitions `
                                -includeDirectories $includeDirectories `
                                -additionalIncludeDirectories $additionalIncludeDirectories
    Write-Verbose "PCH: $pchFilePath"
  }
  
  #-----------------------------------------------------------------------------------------------
  # PROCESS CPP FILES. CONSTRUCT COMMAND LINE JOBS TO BE INVOKED

  $clangJobs = @()

  foreach ($cpp in $projCpps)
  {    
    [string] $exeToCall = Get-ExeToCall -workloadType $workloadType
                        
    [string] $exeArgs   = Get-ExeCallArguments -workloadType            $workloadType `
                                               -pchFilePath             $pchFilePath `
                                               -preprocessorDefinitions $preprocessorDefinitions `
                                               -forceIncludeFiles       $forceIncludeFiles `
                                               -currentFile             $cpp `
                                               -includeDirectories      $includeDirectories `
                                               -additionalIncludeDirectories $additionalIncludeDirectories

    $newJob = New-Object PsObject -Prop @{ 'FilePath'        = $exeToCall;
                                           'WorkingDirectory'= Get-SourceDirectory;
                                           'ArgumentList'    = $exeArgs;
                                           'File'            = $cpp }
    $clangJobs += $newJob
  }
   
  #-----------------------------------------------------------------------------------------------
  # PRINT DIAGNOSTICS

  if ($clangJobs.Count -ge 1)
  {
    Write-Verbose "INVOKE: ""$($global:llvmLocation)\$exeToCall"" $($clangJobs[0].ArgumentList)"
  }
  
  #-----------------------------------------------------------------------------------------------
  # RUN CLANG JOBS

  Run-ClangJobs -clangJobs $clangJobs

  #-----------------------------------------------------------------------------------------------
  # CLEAN GLOBAL VARIABLES SPECIFIC TO CURRENT PROJECT

  Clear-Vars
}
 
#-------------------------------------------------------------------------------------------------
# Script entry point

Clear-Host # clears console

#-------------------------------------------------------------------------------------------------
# Print script parameters

$bParams = $PSCmdlet.MyInvocation.BoundParameters
if ($bParams)
{
  [string] $paramStr = "clang-build.ps1 invocation args: `n"
  foreach ($key in $bParams.Keys)
  {
    $paramStr += "  $($key) = $($bParams[$key]) `n"
  }
  Write-Verbose $paramStr
}

#-------------------------------------------------------------------------------------------------
# Script entry point

Write-Verbose "CPU logical core count: $kLogicalCoreCount"

# If LLVM is not in PATH try to detect it automatically
if (! (Exists-Command($kClangCompiler)) )
{
  foreach ($locationLLVM in $kLLVMInstallLocations)
  {
    if (Test-Path $locationLLVM)
    {
      Write-Verbose "LLVM location: $locationLLVM"
      $env:Path += ";$locationLLVM"
      $global:llvmLocation = $locationLLVM
      break
    }
  }
}

Push-Location (Get-SourceDirectory)

# fetch .sln paths and data
Load-Solutions

# This powershell process may already have completed jobs. Discard them.
Remove-Job -State Completed

Write-Verbose "Source directory: $(Get-SourceDirectory)"
Write-Verbose "Scanning for project files"

[System.IO.FileInfo[]] $projects = Get-Projects
Write-Verbose ("Found $($projects.Count) projects")

[System.IO.FileInfo[]] $projectsToProcess = @()

if ([string]::IsNullOrEmpty($aVcxprojToCompile) -and 
    [string]::IsNullOrEmpty($aVcxprojToIgnore))
{
  Write-Verbose "PROCESSING ALL PROJECTS"
  $projectsToProcess = $projects
}
else
{
  $projectsToProcess = $projects |
                       Where-Object {       (Should-CompileProject -vcxprojPath $_.FullName) `
                                      -and !(Should-IgnoreProject  -vcxprojPath $_.FullName ) }

  if ($projectsToProcess.Count -gt 1)
  {
    Write-Output ("PROJECTS: `n`t" + ($projectsToProcess -join "`n`t"))
    $projectsToProcess = $projectsToProcess
  }
  
  if ($projectsToProcess.Count -eq 0)
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

  Write-Output ("`nPROJECT$(if ($projectCounter -gt 1) { " #$projectCounter" } else { } ): " + $vcxprojPath)
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