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
               only the projects from this solution file will be taken into account.

.PARAMETER aVcxprojToCompile
    Alias 'proj'. Array of project(s) to compile. If empty, all projects found in solutions are compiled.
    Regex matching is supported, using the [regex] prefix (e.g. [regex]'.*components').

    Absolute disk paths to vcxproj files are accepted.

    Can be passed as comma separated values.

.PARAMETER aVcxprojToIgnore
    Alias 'proj-ignore'. Array of project(s) to ignore, from the matched ones.
    If empty, all already matched projects are compiled.
    Regex matching is supported, using the [regex] prefix (e.g. [regex]'.*components').

    Can be passed as comma separated values.

.PARAMETER aVcxprojConfigPlatform
    Alias 'active-config'. Array of configuration-platform pairs, each pair being separated by |,
    to be used when processing project files.

    E.g. "Debug|Win32", "Debug|x64".
    If not specified, the first configuration-platform found in the current project is used.
    Projects will be processed for each configuration-platform specified, the ones that are missing will be skipped.

.PARAMETER aCppToCompile
    Alias 'file'. What cpp(s) to compile from the found project(s). If empty, all CPPs are compiled.
    Regex matching is supported, using the [regex] prefix (e.g. [regex]'.*table').

    Note: If any headers are given then all translation units that include them will be processed.

.PARAMETER aCppToIgnore
    Alias 'file-ignore'. Array of file(s) to ignore, from the matched ones.
    Regex matching is supported, using the [regex] prefix (e.g. [regex]'.*table').

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

      Optional. If not given, it will be inferred based on the project toolset version.

.PARAMETER aVisualStudioSku
      Alias 'vs-sku'. Sku of Visual Studio (VC++) installed and that'll be used for
      standard library include directories. E.g. Professional.

      If not given, the first detected Visual Studio SKU will be used.

.NOTES
    Author: Gabriel Diaconita
#>
#Requires -Version 3
param( [alias("proj")]
       [Parameter(Mandatory=$false, HelpMessage="Filter project(s) to compile/tidy")]
       [System.Object[]] $aVcxprojToCompile = @()

     , [alias("dir")]
       [Parameter(Mandatory=$false, HelpMessage="Source directory for finding solutions; projects will be found from each sln")]
       [string] $aSolutionsPath

     , [alias("proj-ignore")]
       [Parameter(Mandatory=$false, HelpMessage="Specify projects to ignore")]
       [System.Object[]] $aVcxprojToIgnore = @()

     , [alias("active-config")]
       [Parameter(Mandatory=$false, HelpMessage="Config/platform to be used, e.g. Debug|Win32")]
       [string[]] $aVcxprojConfigPlatform = @()

     , [alias("file")]
       [Parameter(Mandatory=$false, HelpMessage="Filter file(s) to compile/tidy")]
       [System.Object[]] $aCppToCompile = @()

     , [alias("file-ignore")]
       [Parameter(Mandatory=$false, HelpMessage="Specify file(s) to ignore")]
       [System.Object[]] $aCppToIgnore = @()

     , [alias("parallel")]
       [Parameter(Mandatory=$false, HelpMessage="Compile/tidy projects in parallel")]
       [switch]   $aUseParallelCompile

     , [alias("continue")]
       [Parameter(Mandatory=$false, HelpMessage="Allow CPT to continue immediately after encountering an error")]
       [switch]   $aContinueOnError

     , [alias("resume")]
       [Parameter(Mandatory=$false, HelpMessage="Allow CPT to resume the last session (start from last active project / file number).")]
       [switch]   $aResumeAfterError

     , [alias("treat-sai")]
       [Parameter(Mandatory=$false, HelpMessage="Treat project additional include directories as system includes")]
       [switch]   $aTreatAdditionalIncludesAsSystemIncludes

     , [alias("clang-flags")]
       [Parameter(Mandatory=$false, HelpMessage="Specify compilation flags to CLANG")]
       [string[]] $aClangCompileFlags = @()

     , [alias("tidy")]
       [Parameter(Mandatory=$false, HelpMessage="Specify flags to CLANG TIDY")]
       [string]   $aTidyFlags

     , [alias("tidy-fix")]
       [Parameter(Mandatory=$false, HelpMessage="Specify flags to CLANG TIDY & FIX")]
       [string]   $aTidyFixFlags

     , [alias("header-filter")]
       [Parameter(Mandatory=$false, HelpMessage="Enable Clang-Tidy to run on header files")]
       [string]   $aTidyHeaderFilter

     , [alias("format-style")]
       [Parameter(Mandatory=$false, HelpMessage="Used with 'tidy-fix'; tells CLANG TIDY-FIX to also format the fixed file(s)")]
       [string]   $aAfterTidyFixFormatStyle

     , [alias("vs-ver")]
       [Parameter(Mandatory=$false, HelpMessage="Version of Visual Studio toolset to use for loading project")]
       [string]   $aVisualStudioVersion

     , [alias("vs-sku")]
       [Parameter(Mandatory=$false, HelpMessage="Edition of Visual Studio toolset to use for loading project")]
       [string]   $aVisualStudioSku
       
     , [alias("export-jsondb")]
       [Parameter(Mandatory=$false, HelpMessage="Switch to generate a JSON compilation database file, in the current working directory")]
       [switch]   $aExportJsonDB
     )

Set-StrictMode -version latest
$ErrorActionPreference = 'Continue'

# System Architecture Constants
# ------------------------------------------------------------------------------------------------

Set-Variable -name kLogicalCoreCount -value $Env:number_of_processors   -option Constant

Set-Variable -name kCptGithubRepoBase -value `
"https://raw.githubusercontent.com/Caphyon/clang-power-tools/master/ClangPowerTools/ClangPowerToolsShared/Tooling/v1/" `
                                      -option Constant
Set-Variable -name kCptGithubLlvm -value "https://github.com/Caphyon/clang-power-tools/releases/download/v8.2.0" `
                                      -option Constant

Set-Variable -name kPsMajorVersion    -value (Get-Host).Version.Major   -Option Constant 
# ------------------------------------------------------------------------------------------------
# Return Value Constants

Set-Variable -name kScriptFailsExitCode      -value  47                 -option Constant

# ------------------------------------------------------------------------------------------------
# File System Constants

Set-Variable -name kExtensionVcxproj         -value ".vcxproj"          -option Constant
Set-Variable -name kExtensionSolution        -value ".sln"              -option Constant
Set-Variable -name kExtensionClangPch        -value ".clang.pch"        -option Constant
Set-Variable -name kExtensionC               -value ".c"                -option Constant


# ------------------------------------------------------------------------------------------------
# Envinroment Variables for controlling logic

Set-Variable -name kVarEnvClangTidyPath     -value "CLANG_TIDY_PATH"-option Constant

# ------------------------------------------------------------------------------------------------
# Clang-Related Constants

Set-Variable -name kClangFlagSupressLINK    -value @("-fsyntax-only")   -option Constant
Set-Variable -name kClangFlagIncludePch     -value "-include-pch"       -option Constant
Set-Variable -name kClangFlagFasterPch      -value "-fpch-instantiate-templates" -option Constant
Set-Variable -name kClangFlagMinusO         -value "-o"                 -option Constant

Set-Variable -name kClangDefinePrefix       -value "-D"                 -option Constant
Set-Variable -name kClangFlagNoUnusedArg    -value "-Wno-unused-command-line-argument" `
                                                                        -option Constant
Set-Variable -name kClangFlagNoMsInclude    -value "-Wno-microsoft-include" `
                                                                        -Option Constant
Set-Variable -name kClangFlagFileIsCPP      -value "-x c++"             -option Constant
Set-Variable -name kClangFlagFileIsC        -value "-x c"               -option Constant
Set-Variable -name kClangFlagForceInclude   -value "-include"           -option Constant

Set-Variable -name kClangCompiler           -value "clang++.exe"        -option Constant

Set-Variable -name kClangTidyFlags            -value @("-quiet"
                                                      ,"--")            -option Constant
Set-Variable -name kClangTidyFixFlags         -value @("-quiet"
                                                      ,"-fix-errors"
                                                      , "--")           -option Constant
Set-Variable -name kClangTidyFlagHeaderFilter -value "-header-filter="  -option Constant
Set-Variable -name kClangTidyFlagChecks       -value "-checks="         -option Constant
Set-Variable -name kClangTidyUseFile          -value ".clang-tidy"      -option Constant
Set-Variable -name kClangTidyFormatStyle      -value "-format-style="   -option Constant

Set-Variable -name kClangTidyFlagTempFile     -value ""

Set-Variable -name kCptRegHiveSettings -value "HKCU:SOFTWARE\Caphyon\Clang Power Tools"      -option Constant
Set-Variable -name kCptVsixSettings    -value "${env:APPDATA}\ClangPowerTools\settings.json" -Option Constant

# ------------------------------------------------------------------------------------------------
# Default install locations of LLVM. If present there, we automatically use it

Set-Variable -name kLLVMInstallLocations    -value @("${Env:ProgramW6432}\LLVM\bin"
                                                    ,"${Env:ProgramFiles(x86)}\LLVM\bin"
                                                    )                   -option Constant

# ------------------------------------------------------------------------------------------------

Function cpt:getSetting([string] $name)
{
  if ((Test-Path $kCptVsixSettings))
  {
    $settingsJson = ( (Get-Content -Raw -Path $kCptVsixSettings) | ConvertFrom-Json)
    $settingField = $settingsJson.Where{ $_ | Get-Member $name }
    if (!$settingField)
    {
      return $null
    }
    
    return $settingField.$name
  }
  return $null
}

# Include required scripts, or download them from Github, if necessary

Function cpt:ensureScriptExists( [Parameter(Mandatory=$true)] [string] $scriptName
                               , [Parameter(Mandatory=$false)][bool]   $forceRedownload
                               )
{
  [string] $scriptFilePath = "$PSScriptRoot/psClang/$scriptName"
  if ( $forceRedownload -or (! (Test-Path $scriptFilePath)) )
  {
    Write-Verbose "Download required script $scriptName ..."
    [string] $request = "$kCptGithubRepoBase/psClang/$scriptName"

    if ( ! (Test-Path "$PSScriptRoot/psClang"))
    {
      New-Item "$PSScriptRoot/psClang" -ItemType Directory > $null
    }

    # Invoke-WebRequest has an issue when displaying progress on PowerShell 7, it won't go away
    # and will keep nagging the user, and on PS5 it causes slow transfers  => we supress it.
    
    $prevPreference = $progressPreference
    $ProgressPreference = "SilentlyContinue"
    
    Invoke-WebRequest -Uri $request -OutFile $scriptFilePath
    (Get-Content $scriptFilePath -Raw).Replace("`n","`r`n") | Set-Content $scriptFilePath -Force -NoNewline

    $ProgressPreference = $prevPreference
    
    if (! (Test-Path $scriptFilePath))
    {
      Write-Error "Could not download required script file ($scriptName). Aborting..."
      exit 1
    }
  }

  return $scriptFilePath
}

Function Has-InternetConnectivity
{  
  $resp = Get-WmiObject -Class Win32_PingStatus -Filter 'Address="github.com" and Timeout=100' | Select-Object ResponseTime
  [bool] $hasInternetConnectivity = ($resp.ResponseTime -and $resp.ResponseTime -gt 0)
  return $hasInternetConnectivity
}

[bool] $shouldRedownloadForcefully = $false
[Version] $cptVsixVersion = cpt:getSetting "Version"
Write-Verbose "Current Clang Power Tools VSIX version: $cptVsixVersion"

# If the main script has been updated meanwhile, we invalidate all other scripts, and force
# them to update from github. We need to watch for this because older CPT VS Extensions (before v7.9)
# did not updated all helper scripts, but a list of predefined ones; we need to update the new ones as well.
if ( ( ![string]::IsNullOrWhiteSpace($cptVsixVersion) -and 
        [Version]::new($cptVsixVersion) -lt [Version]::new(7, 9, 0) ) -and
     (Test-Path $kCptRegHiveSettings) )
{
  Write-Verbose "Checking to see if we should redownload script helpers..."
  $regHive = Get-Item -LiteralPath $kCptRegHiveSettings
  $currentHash = (Get-FileHash $PSCommandPath -Algorithm "SHA1").Hash
  $savedHash = $regHive.GetValue('ScriptContentHash');

  # we used to rely on timestamps but it's unreliable so make sure to clean up the reg value
  if ($regHive.GetValue('ScriptTimestamp'))
  {
    Remove-ItemProperty -path $kCptRegHiveSettings -name 'ScriptTimestamp'
  }

  if (! (Has-InternetConnectivity) )
  {
    Write-Verbose "No internet connectivity. Postponing helper scripts update from github..."
  }
  
  [string] $featureDisableValue = '42'
  if ( $hasInternetConnectivity -and 
      ($savedHash -ne $currentHash) -and
      ($savedHash -ne $featureDisableValue) )
  {
    Write-Verbose "Detected changes in main script. Will redownload helper scripts from Github..."
    Write-Verbose "Current main script SHA1: $currentHash. Saved SHA1: $savedHash"
    Set-ItemProperty -path $kCptRegHiveSettings -name 'ScriptContentHash' -value $currentHash
    $shouldRedownloadForcefully = $true
  }
}

@( "io.ps1"
 , "visualstudio-detection.ps1"
 , "msbuild-expression-eval.ps1"
 , "msbuild-project-data.ps1"
 , "msbuild-project-load.ps1"
 , "msbuild-project-cache-repository.ps1"
 , "get-header-references.ps1"
 , "itemdefinition-context.ps1"
 , "jsondb-export.ps1"
)                                                                        |
ForEach-Object { cpt:ensureScriptExists $_ $shouldRedownloadForcefully } |
ForEach-Object { . $_ }

Write-InformationTimed "Imported scripts"

#-------------------------------------------------------------------------------------------------
# we may have a custom path for Clang-Tidy. Use it if that's the case.

[string] $customTidyPath = (Get-QuotedPath -path ([Environment]::GetEnvironmentVariable($kVarEnvClangTidyPath)))
if (![string]::IsNullOrWhiteSpace($customTidyPath))
{
  Set-Variable -name kClangTidy             -value $customTidyPath      -option Constant
}
else
{
  Set-Variable -name kClangTidy             -value "clang-tidy.exe"     -option Constant
}

Set-Variable -name kCacheRepositorySaveIsNeeded -value $false 

#-------------------------------------------------------------------------------------------------
# Custom Types


Write-InformationTimed "Before .NET enum types"

if ($kPsMajorVersion -lt 5)
{
Add-Type -TypeDefinition @"
  public enum WorkloadType
  {
    Compile,
    Tidy,
    TidyFix
  }
"@

Add-Type -TypeDefinition @"
  public enum StopReason
  {
    Unknown,
    ConfigurationNotFound
  }
"@
}
else 
{
  # this is much faster if PowerShell supports enums, we will save some time
  Invoke-Expression @"
  enum WorkloadType 
  {
    Compile
    Tidy
    TidyFix
  }

  enum StopReason 
  {
    Unknown
    ConfigurationNotFound
  }
"@
}

Write-InformationTimed "Created .NET enum types"
#-------------------------------------------------------------------------------------------------
# Global variables

# temporary files created during project processing (e.g. PCH files)
[System.Collections.ArrayList] $global:FilesToDeleteWhenScriptQuits = @()

# filePath-fileData for SLN files located in source directory
[System.Collections.Generic.Dictionary[String,String]] $global:slnFiles = @{}

# flag to signal when errors are encounteres during project processing
[Boolean]                      $global:FoundErrors                  = $false

# default ClangPowerTools version of visual studio to use
[string] $global:cptDefaultVisualStudioVersion = "2017"

[string[]] $global:cptIgnoredFilesPool = @()

# holds file items to process and their contextual properties (inherited + locally defined)
[System.Collections.Hashtable] $global:cptFilesToProcess = @{}

#-------------------------------------------------------------------------------------------------
# Global functions

Function Exit-Script([Parameter(Mandatory=$false)][int] $code = 0)
{
  Write-Verbose-Array -array $global:FilesToDeleteWhenScriptQuits `
                      -name "Cleaning up PCH temporaries"
  # Clean-up
  foreach ($file in $global:FilesToDeleteWhenScriptQuits)
  {
    Remove-Item -LiteralPath $file -ErrorAction SilentlyContinue > $null
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

Function Get-SourceDirectory()
{
  [bool] $isDirectory = ($(Get-Item -LiteralPath $aSolutionsPath) -is [System.IO.DirectoryInfo])
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
   [string] $pathToCheck = $aSolutionsPath
   if ($kPsMajorVersion -lt 6)
   {
     # we need not bother with long path support in PowerShell 6+
     # only in Windows PowerShell
     $pathToCheck = "\\?\$aSolutionsPath"
   }

   $slns = Get-ChildItem -recurse -LiteralPath $pathToCheck -Filter "*$kExtensionSolution"
   foreach ($sln in $slns)
   {
     Write-Verbose "Caching solution file $sln"
     $slnPath = $sln.FullName

     # remove the UNC long path prefix
     $slnPath = $slnPath.Replace('\\?\', '')
     $global:slnFiles[$slnPath] = (Get-Content -LiteralPath $slnPath)
     
     Write-Verbose "Solution full path: $slnPath"
     Write-Verbose "Solution data length: $($global:slnFiles[$slnPath].Length)"
   }

   Write-Verbose-Array -array $global:slnFiles.Keys  -name "Solution file paths"
}

function Get-SolutionProjects([Parameter(Mandatory=$true)][string] $slnPath)
{
  Write-Verbose "Retrieving project list for solution $slnPath"
  [string] $slnDirectory = Get-FileDirectory -file $slnPath

  Write-Verbose "Solution directory: $slnDirectory"
  $matches = [regex]::Matches($global:slnFiles[$slnPath], 'Project\([{}\"A-Z0-9\-]+\) = \".*?\",\s\"(.*?)\"')

  Write-Verbose "Intermediate solution project matches count: $($matches.Count)"
  foreach ($match in $matches)
  {
    Write-Verbose $match.Groups[1].Value
  }

  [string[]] $projectAbsolutePaths = @()
  foreach ($projPathMatch in $matches)
  {
    [string] $matchValue = $projPathMatch.Groups[1].Value.Replace('"','')
    if ([string]::IsNullOrWhiteSpace($matchValue))
    {
      continue
    }

    $projExpandedPath = [Environment]::ExpandEnvironmentVariables($matchValue)
    if ( ! $projExpandedPath.EndsWith($kExtensionVcxproj))
    {
      continue
    }

    # canonize-path is smart enough to figure out if this is a relative path or not
    $projExpandedPath = Canonize-Path -base $slnDirectory -child $projExpandedPath -ignoreErrors
  
    $projectAbsolutePaths += @($projExpandedPath)
  }
  Write-Verbose-Array -array $projectAbsolutePaths -name "Resolved project paths for solution $slnPath"
  return $projectAbsolutePaths
}

function Get-ProjectSolution()
{
  foreach ($slnPath in $global:slnFiles.Keys)
  {
    [string[]] $solutionProjectPaths = @(Get-SolutionProjects $slnPath)
    if ($solutionProjectPaths -and $solutionProjectPaths -contains $global:vcxprojPath)
    {
      return $slnPath
    }
  }
  return ""
}

Function Get-Projects()
{
  [string[]] $projects = @()

  foreach ($slnPath in $global:slnFiles.Keys)
  {
    [string[]] $solutionProjects = @(Get-SolutionProjects -slnPath $slnPath)
    if ($solutionProjects -and $solutionProjects.Count -gt 0)
    {
      $projects += $solutionProjects
    }
  }

  return ($projects | Select -Unique);
}

Function Get-ClangIncludeDirectories( [Parameter(Mandatory=$false)][string[]] $includeDirectories
                                    , [Parameter(Mandatory=$false)][string[]] $additionalIncludeDirectories
                                    )
{
  [string[]] $returnDirs = @()

  foreach ($includeDir in $includeDirectories)
  {
    $returnDirs += ("-isystem" + (Get-QuotedPath $includeDir))
  }
  foreach ($includeDir in $additionalIncludeDirectories)
  {
    if ($aTreatAdditionalIncludesAsSystemIncludes)
    {
      $returnDirs += ("-isystem" + (Get-QuotedPath $includeDir))
    }
    else
    {
      $returnDirs += ("-I"+ (Get-QuotedPath $includeDir))
    }
  }

  return $returnDirs
}

Function Get-ProjectFileLanguageFlag([Parameter(Mandatory=$true)] [string]   $fileFullName)
{
  [bool] $isCpp = $true
  if ($fileFullName.EndsWith($kExtensionC))
  {
    $isCpp = $false
  }

  try
  {
    [string] $compileAsVal = (Get-ProjectFileSetting -fileFullName $fileFullName -propertyName "CompileAs")
    [bool] $isDefault = [string]::IsNullOrWhiteSpace($compileAsVal) -or $compileAsVal -ieq "Default"
    if ($isDefault)
    {
      $isCpp = ! $fileFullName.EndsWith($kExtensionC)
    }
    else 
    {
      $isCpp = $compileAsVal -ine $kCProjectCompile
    }
  }  
  catch {}

  [string] $languageFlag = If ($isCpp) { $kClangFlagFileIsCPP } else { $kClangFlagFileIsC }

  return $languageFlag
}

Function Generate-Pch( [Parameter(Mandatory=$true)] [string]   $stdafxDir
                     , [Parameter(Mandatory=$true)] [string]   $stdafxCpp
                     , [Parameter(Mandatory=$false)][string[]] $includeDirectories
                     , [Parameter(Mandatory=$false)][string[]] $additionalIncludeDirectories
                     , [Parameter(Mandatory=$true)] [string]   $stdafxHeaderName
                     , [Parameter(Mandatory=$false)][string[]] $preprocessorDefinitions)
{
  [string] $stdafxSource = (Canonize-Path -base $stdafxDir -child $stdafxHeaderName)
  [string] $stdafx = $stdafxSource + ".hpp"

  # Clients using Perforce will have their source checked-out as readonly files, so the 
  # PCH copy would be, by-default, readonly as well, which would present problems. Make sure to remove the RO attribute.
  Copy-Item -LiteralPath $stdafxSource -Destination $stdafx -PassThru | Set-ItemProperty -name isreadonly -Value $false

  $global:FilesToDeleteWhenScriptQuits.Add($stdafx) > $null

  [string] $vcxprojShortName = [System.IO.Path]::GetFileNameWithoutExtension($global:vcxprojPath);
  [string] $stdafxPch = (Join-Path -path (Get-SourceDirectory) `
                                   -ChildPath "$vcxprojShortName$kExtensionClangPch")
  Remove-Item -LiteralPath "$stdafxPch" -ErrorAction SilentlyContinue > $null

  $global:FilesToDeleteWhenScriptQuits.Add($stdafxPch) > $null

  [string] $languageFlag = (Get-ProjectFileLanguageFlag -fileFullName $stdafxCpp)

  [string[]] $compilationFlags = @((Get-QuotedPath $stdafx)
                                  ,$kClangFlagMinusO
                                  ,(Get-QuotedPath $stdafxPch)
                                  ,$languageFlag
                                  ,(Get-ClangCompileFlags -isCpp ($languageFlag -ieq $kClangFlagFileIsCPP))
                                  ,$kClangFlagNoUnusedArg
                                  ,$preprocessorDefinitions
                                  )
  [int] $clangVer = Get-ClangVersion 
  if ($clangVer -ge 11)
  {
    # this flag gets around 15% faster PCH compilation times
    # https://www.phoronix.com/scan.php?page=news_item&px=LLVM-Clang-11-PCH-Instant-Temp
    $compilationFlags += $kClangFlagFasterPch
  }

  $compilationFlags += Get-ClangIncludeDirectories -includeDirectories           $includeDirectories `
                                                   -additionalIncludeDirectories $additionalIncludeDirectories

  # Remove empty arguments from the list because Start-Process will complain
  $compilationFlags = $compilationFlags | Where-Object { $_ } | Select -Unique

  [string] $exeToCallVerbosePath  = $kClangCompiler
  if (![string]::IsNullOrWhiteSpace($global:llvmLocation))
  {
    $exeToCallVerbosePath = "$($global:llvmLocation)\$exeToCallVerbosePath"
  }
  Write-Verbose "INVOKE: $exeToCallVerbosePath $compilationFlags"

  $kClangWorkingDir = "$(Get-SourceDirectory)" -replace '\[', '`[' -replace ']', '`]'
  # We could skip the WorkingDir parameter as all paths are absolute but 
  # Powershell 3-5 has a bug when calling Start-Process from a directory containing square brackets
  # in its path. This can be overcome by providing escaped brackets in the WorkingDirectory arg.
  # Powershell 7 does not have this limitation.
  [System.Diagnostics.Process] $processInfo = Start-Process -FilePath $kClangCompiler `
                                                            -ArgumentList $compilationFlags `
                                                            -WorkingDirectory $kClangWorkingDir `
                                                            -NoNewWindow `
                                                            -Wait `
                                                            -PassThru
  if (($processInfo.ExitCode -ne 0) -and (!$aContinueOnError))
  {
    Fail-Script "Errors encountered during PCH creation"
  }

  if (Test-Path -LiteralPath $stdafxPch)
  {
    return $stdafxPch
  }
  else
  {
    return ""
  }
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
  if (! [string]::IsNullOrEmpty($pchFilePath) -and ! $fileToCompile.EndsWith($kExtensionC))
  {
    $projectCompileArgs += @($kClangFlagIncludePch , (Get-QuotedPath $pchFilePath))
  }

  [string] $languageFlag = (Get-ProjectFileLanguageFlag -fileFullName $fileToCompile)

  $projectCompileArgs += @( $languageFlag
                          , (Get-QuotedPath $fileToCompile)
                          , @(Get-ClangCompileFlags -isCpp ($languageFlag -ieq $kClangFlagFileIsCPP))
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
  [string[]] $tidyArgs = @((Get-QuotedPath $fileToTidy))
  if ($fix -and $aTidyFixFlags -ne $kClangTidyUseFile)
  {
    $tidyArgs += "$kClangTidyFlagChecks`"$aTidyFixFlags`""
  }
  elseif ($aTidyFlags -ne $kClangTidyUseFile)
  {
    $tidyArgs += "$kClangTidyFlagChecks`"$aTidyFlags`""
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

  [string] $languageFlag = (Get-ProjectFileLanguageFlag -fileFullName $fileToTidy)

  # We reuse flags used for compilation and preprocessor definitions.
  $tidyArgs += @(Get-ClangCompileFlags -isCpp ($languageFlag -ieq $kClangFlagFileIsCPP))
  $tidyArgs += $preprocessorDefinitions
  $tidyArgs += $languageFlag

  if (! [string]::IsNullOrEmpty($pchFilePath) -and ! $fileToTidy.EndsWith($kExtensionC))
  {
    $tidyArgs += @($kClangFlagIncludePch , (Get-QuotedPath $pchFilePath))
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
  $global:cptCurrentClangJobCounter = $compileResult.JobCounter
  Write-Debug "Receiving results for clang job $($global:cptCurrentClangJobCounter)"

  if (!$compileResult.Success)
  {
    Write-Err ($compileResult.Output)

    if (!$aContinueOnError)
    {
      # Wait for other workers to finish. They have a lock on the PCH file
      Get-Job -state Running | Wait-Job | Remove-Job > $null
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
    $runningJobs | Wait-Job -Any > $null
  }
  else
  {
    $runningJobs | Wait-Job > $null
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

Function Run-ClangJobs( [Parameter(Mandatory=$true)] $clangJobs
                      , [Parameter(Mandatory=$true)][WorkloadType] $workloadType
                      )
{
  # Script block (lambda) for logic to be used when running a clang job.
  $jobWorkToBeDone = `
  {
    param( $job )

    Push-Location -LiteralPath $job.WorkingDirectory

    [string] $clangConfigFile = [System.IO.Path]::GetTempFileName()
    [string] $cppDirectory = (Get-ChildItem -LiteralPath $job.File).DirectoryName
    [string] $clangConfigContent = ""
    [string] $clangTidyFile      = ""
    [string] $clangTidyBackupFile = ""
    if ($job.FilePath -like '*tidy*')
    {
      # if we need to place a .clang-tidy file make sure we don't override
      # an existing one
      if (![string]::IsNullOrWhiteSpace($job.TidyFlagsTempFile) -and (Test-Path -LiteralPath $job.TidyFlagsTempFile))
      {
        $clangTidyFile       = "$cppDirectory\.clang-tidy"
        $clangTidyBackupFile = "$cppDirectory\.clang-tidy.cpt_backup"
        if (Test-Path -LiteralPath $clangTidyFile)
        {
          # file already exists, temporarily rename it
          Rename-Item -Path $clangTidyFile -NewName $clangTidyBackupFile
        }
      }

      # We have to separate Clang args from Tidy args
      $splitparams = $job.ArgumentList -split "--"
      $clangConfigContent = $splitparams[1]

      $job.ArgumentList = "$($splitparams[0]) -- --config ""$clangConfigFile"""
    }
    else
    {
      # Tell Clang to take its args from a config file
      $clangConfigContent = $job.ArgumentList
      $job.ArgumentList = "--config ""$clangConfigFile"""
    }

    # escape slashes for file paths
    # make sure escaped double quotes are not messed up
    $clangConfigContent = $clangConfigContent -replace '\\([^"])', '\\$1'

    # save arguments to clang config file
    $clangConfigContent > $clangConfigFile

    if (![string]::IsNullOrWhiteSpace($clangTidyFile))
    {
      Copy-Item -Path $job.TidyFlagsTempFile -Destination $clangTidyFile
    }

    # When PowerShell encounters errors, the first one is handled differently from consecutive ones
    # To circumvent this, do not execute the job directly, but execute it via cmd.exe
    # See also https://stackoverflow.com/a/35980675
    
    $callOutput = cmd /c "$($job.FilePath) $($job.ArgumentList) 2>&1" | Out-String

    $callSuccess = $LASTEXITCODE -eq 0

    Remove-Item $clangConfigFile
    if (![string]::IsNullOrWhiteSpace($clangTidyFile))
    {
      Remove-Item $clangTidyFile

      # make sure to restore previous file, if any
      if (Test-Path -LiteralPath $clangTidyBackupFile)
      {
        Rename-Item -Path $clangTidyBackupFile -NewName $clangTidyFile
      }
    }

    Pop-Location

    return New-Object PsObject -Prop @{ "File"    = $job.File
                                      ; "Success" = $callSuccess
                                      ; "Output"  = $callOutput
                                      ; "JobCounter" = $job.JobCounter
                                      }
  }

  if (!$aResumeAfterError)
  {
    $global:cptCurrentClangJobCounter = $clangJobs.Count
  }
  else
  {
    if (!(VariableExists 'cptCurrentClangJobCounter'))
    {
      Write-Warning "Can't resume. Previous state is unreliable. Processing all files..."
      $global:cptCurrentClangJobCounter = $clangJobs.Count
    }
    else
    {
      if ($global:cptCurrentClangJobCounter -gt 0)
      {
        Write-Output "Resuming from file #$($global:cptCurrentClangJobCounter)"
      }
    }
  }

  [int] $crtJobCount = $clangJobs.Count

  foreach ($job in $clangJobs)
  {
    if ($global:cptCurrentClangJobCounter -ge 0 -and $crtJobCount -gt $global:cptCurrentClangJobCounter)
    {
      $crtJobCount--
      continue
    }

    $job.JobCounter = $crtJobCount

    # Check if we must wait for background jobs to complete
    Wait-ForWorkerJobSlot

    # Inform console what CPP we are processing next
    Write-Output "$($crtJobCount): $($job.File)"

    # Tidy-fix can cause header corruption when run in parallel
    # because multiple workers modify shared headers concurrently. Do not allow.
    if ($aUseParallelCompile -and $workloadType -ne [WorkloadType]::TidyFix)
    {
      Start-Job -ScriptBlock  $jobWorkToBeDone `
                -ArgumentList $job `
                -ErrorAction Continue > $null
    }
    else
    {
      $compileResult = Invoke-Command -ScriptBlock  $jobWorkToBeDone `
                                      -ArgumentList $job
      Process-ProjectResult -compileResult $compileResult
      $global:cptCurrentClangJobCounter = $compileResult.JobCounter
    }

    $crtJobCount -= 1
  }

  Wait-AndProcessBuildJobs

  $global:cptCurrentClangJobCounter = -1 # stop the mechanism after one project
}

Function Get-ProjectFileSetting( [Parameter(Mandatory=$true)] [string] $fileFullName
                               , [Parameter(Mandatory=$true)] [string] $propertyName
                               , [Parameter(Mandatory=$false)][string] $defaultValue)
{
  if (!$global:cptFilesToProcess.ContainsKey($fileFullName))
  {
    throw "File $aFileFullName is not in processing queue."
  }

  if ($global:cptFilesToProcess[$fileFullName].Properties -and
      $global:cptFilesToProcess[$fileFullName].Properties.ContainsKey($propertyName))
  {
    return $global:cptFilesToProcess[$fileFullName].Properties[$propertyName]
  }

  if ($defaultValue -ne $null)
  {
    return $defaultValue
  }

  throw "Could not find $propertyName for $fileFullName. No default value specified."
}

Function Process-Project( [Parameter(Mandatory=$true)] [string]       $vcxprojPath
                        , [Parameter(Mandatory=$true)] [WorkloadType] $workloadType
                        , [Parameter(Mandatory=$false)][string]       $platformConfig
                        )
{
  #-----------------------------------------------------------------------------------------------
  $global:cptCurrentConfigPlatform = $platformConfig

  $projCounter = $global:cptProjectCounter
  [string] $projectOutputString = ("PROJECT$(if ($projCounter -gt 1) { " #$projCounter" } else { } ): " + $vcxprojPath)
  
  [bool] $loadedFromCache = $false
  try
  { 
    Set-Variable 'kCacheRepositorySaveIsNeeded' -value $false
    Write-InformationTimed "Before project load"
    
    if (Is-CacheLoadingEnabled)
    {
      Write-InformationTimed "Trying to load project from cache"
      $loadedFromCache = Load-ProjectFromCache $vcxprojPath
      
      if (!$loadedFromCache)
      {
        LoadProject $vcxprojPath
        Set-Variable 'kCacheRepositorySaveIsNeeded' -value $true
      }
    }
    else 
    {
      LoadProject $vcxprojPath
    }
    
    Write-InformationTimed "After project load"
    Write-Output "$projectOutputString [$($global:cptCurrentConfigPlatform)]"
  }
  catch [ProjectConfigurationNotFound]
  {
    [string] $configPlatform = ([ProjectConfigurationNotFound]$_.Exception).ConfigPlatform
    
    Write-Output "$projectOutputString [$($global:cptCurrentConfigPlatform)]"
    Write-Output ("Skipped. Configuration not present: " + $configPlatform);
    
    Pop-Location
    return
  }
  
  Write-InformationTimed "Detecting toolset"

  #-----------------------------------------------------------------------------------------------
  # DETECT PLATFORM TOOLSET

  if (! $loadedFromCache)
  {
    [string] $global:platformToolset = Get-ProjectPlatformToolset
    Write-Verbose "Platform toolset: $platformToolset"
    Add-ToProjectSpecificVariables 'platformToolset'

    if ( $platformToolset -match "^v\d+(_xp)?$" )
    {
      [int] $toolsetVersion = [int]$platformToolset.Remove(0, 1).Replace("_xp", "")

      [string] $desiredVisualStudioVer = ""

      # toolsets attached to specific Visual Studio versions
      if ($toolsetVersion -le 120)
      {
        $desiredVisualStudioVer = "2013"
      }
      elseif ($toolsetVersion -eq 140)
      {
        $desiredVisualStudioVer = "2015"
      }
      elseif ($toolsetVersion -eq 141)
      {
        $desiredVisualStudioVer = "2017"
      }
      elseif ($toolsetVersion -eq 142)
      {
        $desiredVisualStudioVer = "2019";
      }
      elseif ($toolsetVersion -eq 143)
      {
        $desiredVisualStudioVer = "2022";
      }

      [string] $desiredVisualStudioVerNumber = (Get-VisualStudio-VersionNumber $desiredVisualStudioVer)
      if ($VisualStudioVersion -ne $desiredVisualStudioVerNumber)
      {
        [bool] $shouldReload = $false

        if ([double]::Parse($VisualStudioVersion) -gt [double]::Parse($desiredVisualStudioVerNumber))
        {
          # in this case we may have a newer Visual Studio with older toolsets installed
          [string[]] $supportedVsToolsets = Get-VisualStudioToolsets
    
          if ($supportedVsToolsets -notcontains $toolsetVersion)
          {
              $shouldReload = $true
          }
          else 
          {
            Write-Verbose "[ INFO ] Detected project using older toolset ($toolsetVersion)"
            Write-Verbose "Loading using Visual Studio $VisualStudioVersion with toolset $toolsetVersion"
          }
        }
        else 
        {
          # project uses a newer VS version, clearly we should reload using the newer version
          $shouldReload = $true
        }

        if ($shouldReload)
        {
          # We need to reload everything and use the VS version we decided upon above
          Write-Verbose "[ RELOAD ] Project will reload because of toolset requirements change..."
          Write-Verbose "Current = $VisualStudioVersion. Required = $desiredVisualStudioVerNumber."

          $global:cptVisualStudioVersion = $desiredVisualStudioVer
          LoadProject($vcxprojPath)
          
          Write-InformationTimed "Project reloaded"
        }
      }
    }
  
    Write-InformationTimed "Detected toolset"

    #-----------------------------------------------------------------------------------------------
    # FIND FORCE INCLUDES

    [string[]] $global:forceIncludeFiles = @(Get-ProjectForceIncludes)
    Write-Verbose-Array -array $forceIncludeFiles -name "Force includes"
    Add-ToProjectSpecificVariables 'forceIncludeFiles'

    #-----------------------------------------------------------------------------------------------
    # DETECT PROJECT PREPROCESSOR DEFINITIONS

    [string[]] $global:preprocessorDefinitions = @(Get-ProjectPreprocessorDefines)
    if ([int]$global:cptVisualStudioVersion -ge 2017)
    {
      # [HACK] pch generation crashes on VS 15.5 because of STL library, known bug.
      # Triggered by addition of line directives to improve std::function debugging.
      # There's a definition that supresses line directives.

      $preprocessorDefinitions += @('"-D_DEBUG_FUNCTIONAL_MACHINERY"')
    }
    Add-ToProjectSpecificVariables 'preprocessorDefinitions'
    
    Write-InformationTimed "Detected preprocessor definitions"

    Write-Verbose-Array -array $preprocessorDefinitions -name "Preprocessor definitions"

    #-----------------------------------------------------------------------------------------------
    # DETECT PROJECT ADDITIONAL INCLUDE DIRECTORIES AND CONSTRUCT INCLUDE PATHS

    [string[]] $global:additionalIncludeDirectories = @(Get-ProjectAdditionalIncludes)
    Write-Verbose-Array -array $additionalIncludeDirectories -name "Additional include directories"
    Add-ToProjectSpecificVariables 'additionalIncludeDirectories'

    [string[]] $includeDirectories = @(Get-ProjectIncludeDirectories)
    Write-Verbose-Array -array $includeDirectories -name "Include directories"
    Add-ToProjectSpecificVariables 'includeDirectories'

    Write-InformationTimed "Detected include directories"

    #-----------------------------------------------------------------------------------------------
    # FIND LIST OF CPPs TO PROCESS

    $global:projectAllCpps = @{}
    foreach ($fileToCompileInfo in (Get-ProjectFilesToCompile))
    {
      if ($fileToCompileInfo.File)
      {
        $global:projectAllCpps[$fileToCompileInfo.File] = $fileToCompileInfo
      }
    }
    
    Add-ToProjectSpecificVariables 'projectAllCpps'
    
    Write-InformationTimed "Detected cpps to process"
  } # past the caching boundary here, we must see what else needs to be computed live 
  
  $global:cptFilesToProcess = $global:projectAllCpps # reset to full project cpp list
  
  #-----------------------------------------------------------------------------------------------
  # LOCATE STDAFX.H DIRECTORY

  [string] $stdafxCpp    = ""
  [string] $stdafxDir    = ""
  [string] $stdafxHeader = ""
  [string] $stdafxHeaderFullPath = ""

  [bool] $kPchIsNeeded = $global:cptFilesToProcess.Keys.Count -ge 2
  if ($kPchIsNeeded)
  {
    # if we have only one rooted file in the script parameters, then we don't need to detect PCH
    if ($aCppToCompile.Count -eq 1 -and [System.IO.Path]::IsPathRooted($aCppToCompile[0]))
    {
      $kPchIsNeeded = $false
    }
  }

  if ($kPchIsNeeded)
  {
    foreach ($projCpp in $global:cptFilesToProcess.Keys)
    {
      if ( (Get-ProjectFileSetting -fileFullName $projCpp -propertyName 'PrecompiledHeader') -ieq 'Create')
      {
        $stdafxCpp = $projCpp
      }
    }
  }

  if (![string]::IsNullOrEmpty($stdafxCpp))
  {
    Write-Verbose "PCH cpp name: $stdafxCpp"

    if ($forceIncludeFiles.Count -gt 0)
    {
      $stdafxHeader = $forceIncludeFiles[0]
    }

    if (!$stdafxHeader)
    {
      $stdafxHeader = Get-PchCppIncludeHeader -pchCppFile $stdafxCpp
    }

    if (!$stdafxHeader)
    {
      try
      {
        $stdafxHeader = Get-ProjectFileSetting -fileFullName $stdafxCpp -propertyName 'PrecompiledHeaderFile'
      }
      catch {}
    }

    Write-Verbose "PCH header name: $stdafxHeader"
    $stdafxDir = Get-ProjectStdafxDir -pchHeaderName                $stdafxHeader       `
                                      -includeDirectories           $includeDirectories `
                                      -additionalIncludeDirectories $additionalIncludeDirectories
  }

  if ([string]::IsNullOrEmpty($stdafxDir))
  {
    Write-Verbose ("PCH not enabled for this project!")
    $kPchIsNeeded = $false
  }
  else
  {
    Write-Verbose ("PCH directory: $stdafxDir")

    $includeDirectories = @(Remove-PathTrailingSlash -path $stdafxDir) + $includeDirectories

    $stdafxHeaderFullPath = Canonize-Path -base $stdafxDir -child $stdafxHeader -ignoreErrors
  }
  
  Write-InformationTimed "Detected PCH information"


  #-----------------------------------------------------------------------------------------------
  # FILTER LIST OF CPPs TO PROCESS
  if ($global:cptFilesToProcess.Count -gt 0 -and $aCppToIgnore.Count -gt 0)
  {
    [System.Collections.Hashtable] $filteredCpps = @{}
    foreach ($cpp in $global:cptFilesToProcess.Keys)
    {
      if ( ! (Should-IgnoreFile -file $cpp) )
      {
        $filteredCpps[$cpp] = $global:cptFilesToProcess[$cpp]
      }
    }
    $global:cptFilesToProcess = $filteredCpps
  }

  if ($global:cptFilesToProcess.Count -gt 0 -and $aCppToCompile.Count -gt 0)
  {
    [System.Collections.Hashtable] $filteredCpps = @{}
    [bool] $dirtyStdafx = $false
    foreach ($cpp in $aCppToCompile)
    {
      if ($cpp -ieq $stdafxHeaderFullPath)
      {
        # stdafx modified => compile all
        $dirtyStdafx = $true
        break
      }

      if (![string]::IsNullOrEmpty($cpp))
      {
        if ([System.IO.Path]::IsPathRooted($cpp))
        {
          if ($global:cptFilesToProcess.ContainsKey($cpp))
          {
            # really fast, use cache
            $filteredCpps[$cpp] = $global:cptFilesToProcess[$cpp]
          }
        }
        else
        {
          # take the slow road and check if it matches
          $global:cptFilesToProcess.Keys | Where-Object {  IsFileMatchingName -filePath $_ -matchName $cpp } | `
                                           ForEach-Object { $filteredCpps[$_] = $global:cptFilesToProcess[$_] }
        }
      }
    }

    if (!$dirtyStdafx)
    {
      $global:cptFilesToProcess = $filteredCpps
    }
    else
    {
      Write-Verbose "PCH header has been targeted as dirty. Building entire project"
    }
  }
  
  Write-InformationTimed "Filtered out CPPs from bucket"

  Write-Verbose ("Processing " + $global:cptFilesToProcess.Count + " cpps")

  #-----------------------------------------------------------------------------------------------
  # CREATE PCH IF NEED BE, ONLY FOR TWO CPPS OR MORE
  #
  # JSON Compilation Database file will outlive this execution run, while the PCH is temporary 
  # so we disable PCH creation for that case as well.

  if ($kPchIsNeeded -and $global:cptFilesToProcess.Count -lt 2)
  {
    $kPchIsNeeded = $false
  }

  [string] $pchFilePath = ""
  if ($kPchIsNeeded -and $workloadType -ne [WorkloadType]::TidyFix -and !$aExportJsonDB)
  {
    # COMPILE PCH
    Write-Verbose "Generating PCH..."
    $pchFilePath = Generate-Pch -stdafxDir        $stdafxDir    `
                                -stdafxCpp        $stdafxCpp    `
                                -stdafxHeaderName $stdafxHeader `
                                -preprocessorDefinitions $preprocessorDefinitions `
                                -includeDirectories $includeDirectories `
                                -additionalIncludeDirectories $additionalIncludeDirectories
    Write-Verbose "PCH: $pchFilePath"
    if ([string]::IsNullOrEmpty($pchFilePath) -and $aContinueOnError)
    {
      Write-Output "Skipping project. Reason: cannot create PCH."
      return
    }
    Write-InformationTimed "Created PCH"
  }  

  if ($kCacheRepositorySaveIsNeeded)
  {
    Write-InformationTimed "Before serializing project"
    Save-ProjectToCacheRepo
    Write-InformationTimed "After serializing project"
  }

  #-----------------------------------------------------------------------------------------------
  # PROCESS CPP FILES. CONSTRUCT COMMAND LINE JOBS TO BE INVOKED

  $clangJobs = @()

  foreach ($cpp in $global:cptFilesToProcess.Keys)
  {
    [string] $cppPchSetting = Get-ProjectFileSetting -propertyName 'PrecompiledHeader' -fileFullName $cpp -defaultValue 'Use'

    if ($cppPchSetting -ieq 'Create')
    {
        continue # no point in compiling the PCH CPP
    }

    [string] $exeToCall = Get-ExeToCall -workloadType $workloadType

    [string] $finalPchPath = $pchFilePath
    if ($cppPchSetting -ieq 'NotUsing')
    {
      $finalPchPath = ""
      Write-Verbose "`n[PCH] Will ignore precompiled headers for $cpp`n"
    }

    [string] $exeArgs   = Get-ExeCallArguments -workloadType            $workloadType `
                                               -pchFilePath             $finalPchPath `
                                               -preprocessorDefinitions $preprocessorDefinitions `
                                               -forceIncludeFiles       $forceIncludeFiles `
                                               -currentFile             $cpp `
                                               -includeDirectories      $includeDirectories `
                                               -additionalIncludeDirectories $additionalIncludeDirectories

    $newJob = New-Object PsObject -Prop @{ 'FilePath'          = $exeToCall
                                         ; 'WorkingDirectory'  = Get-SourceDirectory
                                         ; 'ArgumentList'      = $exeArgs
                                         ; 'File'              = $cpp
                                         ; 'JobCounter'        = 0 <# will be lazy initialized #>
                                         ; 'TidyFlagsTempFile' = $kClangTidyFlagTempFile
                                         }
    $clangJobs += $newJob
  }
  
  Write-InformationTimed "Created job workers"

  #-----------------------------------------------------------------------------------------------
  # PRINT DIAGNOSTICS

  if ($clangJobs.Count -ge 1)
  {
    [string] $exeToCallVerbosePath  = $exeToCall
    if (![string]::IsNullOrWhiteSpace($global:llvmLocation))
    {
      $exeToCallVerbosePath = "$($global:llvmLocation)\$exeToCallVerbosePath"
    }
    Write-Verbose "INVOKE: $exeToCallVerbosePath $($clangJobs[0].ArgumentList)"
  }

  Write-InformationTimed "Running workers"

  #-----------------------------------------------------------------------------------------------
  # RUN CLANG JOBS
  
  if ($aExportJsonDB)
  {
   foreach ($job in $clangJobs)
   {
     [string] $clangToolPath = $job.FilePath
     if (Exists-Command $clangToolPath)
     {
       # see precisely what path the tool has, to prevent ambiguities.
       $clangToolPath = (Get-Command $job.FilePath).Source
     }
     [string] $clangCommand = """$clangToolPath"" $($job.ArgumentList)"
     JsonDB-Push -directory $job.WorkingDirectory -file $job.File -command $clangCommand
   }
  }
  else 
  {
    Run-ClangJobs -clangJobs $clangJobs -workloadType $workloadType
  }
}

#-------------------------------------------------------------------------------------------------
# Script entry point

Clear-Host # clears console

Write-InformationTimed "Cleared console. Let's begin..."

#-------------------------------------------------------------------------------------------------
# If we didn't get a location to run CPT at, use the current working directory

if (!$aSolutionsPath)
{
  $aSolutionsPath = (Get-Location).Path
}

# ------------------------------------------------------------------------------------------------
# Load param values from configuration file (if exists)

Update-ParametersFromConfigFile
Write-InformationTimed "Updated script parameters from cpt.config"

# ------------------------------------------------------------------------------------------------
# Initialize the Visual Studio version variable

$global:cptVisualStudioVersion = If ( $aVisualStudioVersion ) `
                                    { $aVisualStudioVersion } Else `
                                    { $global:cptDefaultVisualStudioVersion }

#-------------------------------------------------------------------------------------------------
# Print script parameters

Print-InvocationArguments
Write-InformationTimed "Print args"

[WorkloadType] $workloadType = [WorkloadType]::Compile

if (![string]::IsNullOrEmpty($aTidyFlags))
{
   $workloadType = [WorkloadType]::Tidy
   if (Test-Path -LiteralPath $aTidyFlags)
   {
     $kClangTidyFlagTempFile = $aTidyFlags
   }
}

if (![string]::IsNullOrEmpty($aTidyFixFlags))
{
   $workloadType = [WorkloadType]::TidyFix
   if (Test-Path -LiteralPath $aTidyFixFlags)
   {
     $kClangTidyFlagTempFile = $aTidyFixFlags
   }
}

#-------------------------------------------------------------------------------------------------
# Script entry point

Write-Verbose "CPU logical core count: $kLogicalCoreCount"

# If LLVM is not in PATH try to detect it automatically
[string] $global:llvmLocation = ""

$clangToolWeNeed = Get-ExeToCall -workloadType $workloadType
if (! (Exists-Command($clangToolWeNeed)) )
{
  foreach ($locationLLVM in $kLLVMInstallLocations)
  {
    if (Test-Path -LiteralPath $locationLLVM)
    {
      $env:Path += ";$locationLLVM"
      break
    }
  }
}

if (!(Exists-Command($clangToolWeNeed)) -and (Has-InternetConnectivity))
{
  # the displayed progress slows downloads considerably, so disable it
  $prevPreference = $ProgressPreference
  $ProgressPreference = 'SilentlyContinue'
  [string] $clangCompilerWebPath = "$kCptGithubLlvm/$clangToolWeNeed"
  # grab ready-to-use LLVM binaries from Github
  Invoke-WebRequest -Uri $clangCompilerWebPath -OutFile "$PSScriptRoot/$clangToolWeNeed"
  $ProgressPreference = $prevPreference
  $locationLLVM = $PSScriptRoot
  $env:Path += ";$locationLLVM"
}

if (![string]::IsNullOrEmpty($global:llvmLocation))
{
  Write-Verbose "LLVM location: $locationLLVM"
  $env:Path += ";$($global:llvmLocation)"
}

# initialize JSON compilation db support, if required
if ($aExportJsonDB) 
{ 
  JsonDB-Init 
}

Push-Location -LiteralPath (Get-SourceDirectory)

Write-InformationTimed "Searching for solutions"

# fetch .sln paths and data
Load-Solutions

Write-InformationTimed "End solution search"

# This PowerShell process may already have completed jobs. Discard them.
Remove-Job -State Completed
Write-InformationTimed "Discarded already finished jobs"

Write-Verbose "Source directory: $(Get-SourceDirectory)"
Write-Verbose "Scanning for project files"

Write-InformationTimed "Searching for project files"
[System.IO.FileInfo[]] $projects = @(Get-Projects)
[int] $initialProjectCount       = $projects.Count
Write-Verbose ("Found $($projects.Count) projects")
Write-InformationTimed "End project files search"

# ------------------------------------------------------------------------------------------------
# If we get headers in the -file arg we have to detect CPPs that include that header

if ($aCppToCompile -and $aCppToCompile.Count -gt 0)
{
  # We've been given particular files to compile. If headers are among them
  # we'll find all source files that include them and tag them for processing.
  Write-Progress -Activity "#Include discovery" -Status "Detecting CPPs which include the specified headers..."
  [string[]] $headerRefs = @(Get-HeaderReferences -files $aCppToCompile)
  Write-Progress -Activity "#Include discovery" -Completed
  if ($headerRefs.Count -gt 0)
  {
    Write-Verbose-Array -name "Detected referenced source files to process" -array $headerRefs

    $aCppToCompile += @($headerRefs | Where-Object { ![string]::IsNullOrWhiteSpace($_) })
  }
}

if ($aCppToIgnore -and $aCppToIgnore.Count -gt 0)
{
  # We've been given particular files to ignore. If headers are among them
  # we'll find all source files that include them and tag them to be ignored.

  Write-Progress -Activity "CPP Ignore Detection" -Status "Detecting CPPs which include the specified ignore-headers..."
  [string[]] $headerRefs = @(Get-HeaderReferences -files $aCppToIgnore)
  Write-Progress -Activity "CPP Ignore Detection" -Completed

  if ($headerRefs.Count -gt 0)
  {
    Write-Verbose-Array -name "Detected referenced source files to ignore" -array $headerRefs

    $global:cptIgnoredFilesPool += @($headerRefs | Where-Object { ![string]::IsNullOrWhiteSpace($_) })
  }
}

# ------------------------------------------------------------------------------------------------

Write-InformationTimed "Starting projects"

[System.IO.FileInfo[]] $projectsToProcess = @()
[System.IO.FileInfo[]] $ignoredProjects   = @()

if (!$aVcxprojToCompile -and !$aVcxprojToIgnore)
{
  $projectsToProcess = $projects # we process all projects
}
else
{
  # some filtering has to be done

  if ($aVcxprojToCompile)
  {
    $projects = $projects | Where-Object { Should-CompileProject -vcxprojPath $_.FullName }
    $projectsToProcess = @($projects)
  }

  if ($aVcxprojToIgnore)
  {
    $projectsToProcess = @($projects | `
                         Where-Object { !(Should-IgnoreProject  -vcxprojPath $_.FullName ) })

    $ignoredProjects = ($projects | Where-Object { $projectsToProcess -notcontains $_ })
  }
}

if ($projectsToProcess.Count -eq 0)
{
  Write-Err "Cannot find given project(s)"
}

if ($aCppToCompile -and $projectsToProcess.Count -gt 1)
{
  # We've been given particular files to compile, we can narrow down
  # the projects to be processed (those that include any of the particular files)

  # For obvious performance reasons, no filtering is done when there's only one project to process.
  [System.IO.FileInfo[]] $projectsThatIncludeFiles = @(Get-SourceCodeIncludeProjects -projectPool $projectsToProcess `
                                                                                     -files $aCppToCompile)
  Write-Verbose-Array -name "Detected projects" -array $projectsThatIncludeFiles

  # some projects include files using wildcards, we won't match anything in them
  # so when matching nothing we don't do filtering at all
  if ($projectsThatIncludeFiles)
  {
    $projectsToProcess = $projectsThatIncludeFiles
  }
}

if ($projectsToProcess.Count -eq $initialProjectCount)
{
  Write-Verbose "PROCESSING ALL PROJECTS"
}
else
{
  if ($projectsToProcess.Count -gt 1)
  {
      Write-Array -name "PROJECTS" -array $projectsToProcess
  }

  if ($ignoredProjects)
  {
    Write-Array -name "IGNORED PROJECTS" -array $ignoredProjects
  }
}

# ------------------------------------------------------------------------------------------------

if (!$aResumeAfterError)
{
  $global:cptProjectCounter = $projectsToProcess.Length
}
else
{
  if (!(VariableExists 'cptProjectCounter') -or !(VariableExists 'cptProjectsBucket'))
  {
    Write-Warning "Can't resume. Previous state is unreliable. Processing all projects..."
    $global:cptProjectCounter = $projectsToProcess.Length
  }
  elseif ((Compare-Object $projectsToProcess $global:cptProjectsBucket))
  {
    Write-Warning "Can't resume. Previous state is unreliable. Processing all projects...`n`nREMINDER: Don't change arguments when adding -resume.`n`n"
    $global:cptProjectCounter = $projectsToProcess.Length
  }
  else
  {
    Write-Output "Resuming from project #$($global:cptProjectCounter)"
  }
}

[System.IO.FileInfo[]] $global:cptProjectsBucket = $projectsToProcess

[int] $localProjectCounter = $projectsToProcess.Length;
foreach ($project in $projectsToProcess)
{
  if ($localProjectCounter -gt $global:cptProjectCounter)
  {
    $localProjectCounter--;
    continue
  }

  [string] $vcxprojPath = $project.FullName;
  

  [string[]] $configPlatforms = $aVcxprojConfigPlatform
  if ($configPlatforms.Count -eq 0)
  {
    $configPlatforms += @("")
  }

  foreach ($crtPlatformConfig in $configPlatforms)
  {    
    Write-InformationTimed "Before project process"
    Process-Project -vcxprojPath $vcxprojPath -workloadType $workloadType -platformConfig $crtPlatformConfig
    Write-InformationTimed "After project process"

    Write-Output "" # empty line separator
  }

  $localProjectCounter -= 1
  $global:cptProjectCounter = $localProjectCounter
}

if ($aExportJsonDB) 
{ 
  JsonDB-Finalize
}


Write-InformationTimed "Goodbye"

if ($global:FoundErrors)
{
  Fail-Script
}
else
{
  Exit-Script
}
