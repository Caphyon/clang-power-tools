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
    If not specified, the first configuration-platform found in the current project is used.

.PARAMETER aCppToCompile
    Alias 'file'. What cpp(s) to compile from the found project(s). If empty, all CPPs are compiled.
    If the -literal switch is present, name is matched exactly. Otherwise, regex matching is used,
    e.g. "table" compiles all CPPs containing 'table'.

    Note: If any headers are given then all translation units that include them will be processed.

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
       [string[]] $aVcxprojToCompile

     , [alias("dir")]
       [Parameter(Mandatory=$false, HelpMessage="Source directory for finding solutions; projects will be found from each sln")]
       [string] $aSolutionsPath

     , [alias("proj-ignore")]
       [Parameter(Mandatory=$false, HelpMessage="Specify projects to ignore")]
       [string[]] $aVcxprojToIgnore

     , [alias("active-config")]
       [Parameter(Mandatory=$false, HelpMessage="Config/platform to be used, e.g. Debug|Win32")]
       [string] $aVcxprojConfigPlatform

     , [alias("file")]
       [Parameter(Mandatory=$false, HelpMessage="Filter file(s) to compile/tidy")]
       [string[]] $aCppToCompile

     , [alias("file-ignore")]
       [Parameter(Mandatory=$false, HelpMessage="Specify file(s) to ignore")]
       [string[]] $aCppToIgnore

     , [alias("parallel")]
       [Parameter(Mandatory=$false, HelpMessage="Compile/tidy projects in parallel")]
       [switch]   $aUseParallelCompile

     , [alias("continue")]
       [Parameter(Mandatory=$false, HelpMessage="Allow CPT to continue after encounteringan error")]
       [switch]   $aContinueOnError

     , [alias("treat-sai")]
       [Parameter(Mandatory=$false, HelpMessage="Treat project additional include directories as system includes")]
       [switch]   $aTreatAdditionalIncludesAsSystemIncludes

     , [alias("clang-flags")]
       [Parameter(Mandatory=$false, HelpMessage="Specify compilation flags to CLANG")]
       [string[]] $aClangCompileFlags

     , [alias("literal")]
       [Parameter(Mandatory=$false, HelpMessage="Disable regex matching for all paths received as script parameters")]
       [switch]   $aDisableNameRegexMatching

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
       [string]   $aVisualStudioVersion = "2017"

     , [alias("vs-sku")]
       [Parameter(Mandatory=$false, HelpMessage="Edition of Visual Studio toolset to use for loading project")]
       [string]   $aVisualStudioSku
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
Set-Variable -name kExtensionC               -value ".c"                -option Constant

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
Set-Variable -name kClangFlagFileIsC        -value "-x c"               -option Constant
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

 @( "$PSScriptRoot\psClang\io.ps1"
  , "$PSScriptRoot\psClang\visualstudio-detection.ps1"
  , "$PSScriptRoot\psClang\msbuild-expression-eval.ps1"
  , "$PSScriptRoot\psClang\msbuild-project-load.ps1"
  , "$PSScriptRoot\psClang\msbuild-project-data.ps1"
  , "$PSScriptRoot\psClang\get-header-references.ps1"
  ) | ForEach-Object { . $_ }

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

# version of VS currently used
[string] $global:cptVisualStudioVersion = $aVisualStudioVersion

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
   $slns = Get-ChildItem -recurse -LiteralPath "$aSolutionsPath" -Filter "*$kExtensionSolution"
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
  $matches = [regex]::Matches($global:slnFiles[$slnPath], 'Project\([{}\"A-Z0-9\-]+\) = \".*?\",\s\"(.*?)\"')
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
  if (Is-CProject)
  {
    Write-Verbose "Skipping PCH creation for C project."
    return ""
  }

  [string] $stdafx = (Canonize-Path -base $stdafxDir -child $stdafxHeaderName)
  [string] $vcxprojShortName = [System.IO.Path]::GetFileNameWithoutExtension($global:vcxprojPath);
  [string] $stdafxPch = (Join-Path -path (Get-SourceDirectory) `
                                   -ChildPath "$vcxprojShortName$kExtensionClangPch")
  Remove-Item -Path "$stdafxPch" -ErrorAction SilentlyContinue | Out-Null

  $global:FilesToDeleteWhenScriptQuits.Add($stdafxPch) | Out-Null

  # Suppress -Werror for PCH generation as it throws warnings quite often in code we cannot control
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

  # Remove empty arguments from the list because Start-Process will complain
  $compilationFlags = $compilationFlags | Where-Object { $_ } | Select -Unique

  Write-Verbose "INVOKE: ""$($global:llvmLocation)\$kClangCompiler"" $compilationFlags"

  [System.Diagnostics.Process] $processInfo = Start-Process -FilePath $kClangCompiler `
                                                            -ArgumentList $compilationFlags `
                                                            -WorkingDirectory "$(Get-SourceDirectory)" `
                                                            -NoNewWindow `
                                                            -Wait `
                                                            -PassThru
  if (($processInfo.ExitCode -ne 0) -and (!$aContinueOnError))
  {
    Fail-Script "Errors encountered during PCH creation"
  }

  if (Test-Path $stdafxPch)
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
    $projectCompileArgs += @($kClangFlagIncludePch , """$pchFilePath""")
  }

  $isCpp = $true
  $languageFlag = $kClangFlagFileIsCPP
  if ($fileToCompile.EndsWith($kExtensionC))
  {
    $isCpp = $false
    $languageFlag = $kClangFlagFileIsC
  }

  $projectCompileArgs += @( $languageFlag
                          , """$fileToCompile"""
                          , @(Get-ClangCompileFlags -isCpp $isCpp)
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

  $isCpp = $true
  $languageFlag = $kClangFlagFileIsCPP
  if ($fileToTidy.EndsWith($kExtensionC))
  {
    $isCpp = $false
    $languageFlag = $kClangFlagFileIsC
  }

  # We reuse flags used for compilation and preprocessor definitions.
  $tidyArgs += @(Get-ClangCompileFlags -isCpp $isCpp)
  $tidyArgs += $preprocessorDefinitions
  $tidyArgs += $languageFlag

  if (! [string]::IsNullOrEmpty($pchFilePath) -and ! $fileToTidy.EndsWith($kExtensionC))
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

    [string] $clangConfigFile = [System.IO.Path]::GetTempFileName()

    [string] $clangConfigContent = ""
    if ($job.FilePath -like '*tidy*')
    {
      # We have to separate Clang args from Tidy args
      $splitparams = $job.ArgumentList -split "--"
      $clangConfigContent = $splitparams[1]
      $job.ArgumentList = ($splitparams[0] + " -- --config ""$clangConfigFile""")
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

    # When PowerShell encounters errors, the first one is handled differently from consecutive ones
    # To circumvent this, do not execute the job directly, but execute it via cmd.exe
    # See also https://stackoverflow.com/a/35980675
    $callOutput = cmd /c $job.FilePath $job.ArgumentList.Split(' ') '2>&1' | Out-String

    $callSuccess = $LASTEXITCODE -eq 0

    Remove-Item $clangConfigFile
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
  # Load data
  LoadProject($vcxprojPath)

  #-----------------------------------------------------------------------------------------------
  # DETECT PLATFORM TOOLSET

  [string] $platformToolset = Get-ProjectPlatformToolset
  Write-Verbose "Platform toolset: $platformToolset"

  if ( ([int]$platformToolset.Remove(0, 1).Replace("_xp", "")) -le 140)
  {
    if ($global:cptVisualStudioVersion -ne '2015')
    {
      # we need to reload everything and use VS2015
      Write-Verbose "Switching to VS2015 because of v140 toolset. Reloading project..."
      $global:cptVisualStudioVersion = "2015"
      LoadProject($vcxprojPath)
    }
  }
  else
  {
    if ($global:cptVisualStudioVersion -ne $global:cptDefaultVisualStudioVersion)
    {
      # we need to reload everything and the default vs version
      Write-Verbose "Switching to default VsVer because of toolset. Reloading project..."
      $global:cptVisualStudioVersion = $global:cptDefaultVisualStudioVersion
      LoadProject($vcxprojPath)
    }
  }

  #-----------------------------------------------------------------------------------------------
  # FIND FORCE INCLUDES

  [string[]] $forceIncludeFiles = Get-ProjectForceIncludes
  Write-Verbose "Force includes: $forceIncludeFiles"

  #-----------------------------------------------------------------------------------------------
  # DETECT PROJECT PREPROCESSOR DEFINITIONS

  [string[]] $preprocessorDefinitions = Get-ProjectPreprocessorDefines
  if ($global:cptVisualStudioVersion -eq "2017")
  {
    # [HACK] pch generation crashes on VS 15.5 because of STL library, known bug.
    # Triggered by addition of line directives to improve std::function debugging.
    # There's a definition that supresses line directives.

    $preprocessorDefinitions += "-D_DEBUG_FUNCTIONAL_MACHINERY"
  }

  Write-Verbose-Array -array $preprocessorDefinitions -name "Preprocessor definitions"

  #-----------------------------------------------------------------------------------------------
  # DETECT PROJECT ADDITIONAL INCLUDE DIRECTORIES AND CONSTRUCT INCLUDE PATHS

  [string[]] $additionalIncludeDirectories = Get-ProjectAdditionalIncludes
  Write-Verbose-Array -array $additionalIncludeDirectories -name "Additional include directories"

  [string[]] $includeDirectories = Get-ProjectIncludeDirectories
  Write-Verbose-Array -array $includeDirectories -name "Include directories"

  #-----------------------------------------------------------------------------------------------
  # LOCATE STDAFX.H DIRECTORY

  [string] $stdafxCpp    = Get-Project-PchCpp
  [string] $stdafxDir    = ""
  [string] $stdafxHeader = ""
  [string] $stdafxHeaderFullPath = ""

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
      $pchNode = Select-ProjectNodes "//ns:ClCompile[@Include='$stdafxCpp']/ns:PrecompiledHeaderFile"
      if ($pchNode)
      {
        $stdafxHeader = $pchNode.InnerText
      }
    }

    Write-Verbose "PCH header name: $stdafxHeader"
    $stdafxDir = Get-ProjectStdafxDir -pchHeaderName                $stdafxHeader       `
                                      -includeDirectories           $includeDirectories `
                                      -additionalIncludeDirectories $additionalIncludeDirectories
  }

  if ([string]::IsNullOrEmpty($stdafxDir))
  {
    Write-Verbose ("PCH not enabled for this project!")
  }
  else
  {
    Write-Verbose ("PCH directory: $stdafxDir")

    $includeDirectories = @(Remove-PathTrailingSlash -path $stdafxDir) + $includeDirectories

    $stdafxHeaderFullPath = Canonize-Path -base $stdafxDir -child $stdafxHeader -ignoreErrors
  }

  #-----------------------------------------------------------------------------------------------
  # FIND LIST OF CPPs TO PROCESS

  [System.Collections.Hashtable] $projCpps = @{}
  foreach ($fileToCompileInfo in (Get-ProjectFilesToCompile -pchCppName $stdafxCpp))
  {
    if ($fileToCompileInfo.File)
    {
      $projCpps[$fileToCompileInfo.File] = $fileToCompileInfo
    }
  }

  if ($projCpps.Count -gt 0 -and $aCppToCompile.Count -gt 0)
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
          if ($projCpps.ContainsKey($cpp))
          {
            # really fast, use cache
            $filteredCpps[$cpp] = $projCpps[$cpp]
          }
        }
        else
        {
          # take the slow road and check if it matches
          $projCpps.Keys | Where-Object {  IsFileMatchingName -filePath $_ -matchName $cpp } | `
                          ForEach-Object { $filteredCpps[$_] = $true }
        }
      }
    }

    if (!$dirtyStdafx)
    {
      $projCpps = $filteredCpps
    }
    else
    {
      Write-Verbose "PCH header has been targeted as dirty. Building entire project"
    }
  }
  Write-Verbose ("Processing " + $projCpps.Count + " cpps")

  #-----------------------------------------------------------------------------------------------
  # CREATE PCH IF NEED BE, ONLY FOR TWO CPPS OR MORE

  [string] $pchFilePath = ""
  if ($projCpps.Keys.Count -ge 2 -and
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
    if ([string]::IsNullOrEmpty($pchFilePath) -and $aContinueOnError)
    {
      Write-Output "Skipping project. Reason: cannot create PCH."
      return
    }
  }

  #-----------------------------------------------------------------------------------------------
  # PROCESS CPP FILES. CONSTRUCT COMMAND LINE JOBS TO BE INVOKED

  $clangJobs = @()

  foreach ($cpp in $projCpps.Keys)
  {
    [string] $exeToCall = Get-ExeToCall -workloadType $workloadType

    [string] $finalPchPath = $pchFilePath
    if ($projCpps[$cpp].Pch -eq [UsePch]::NotUsing)
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
}

#-------------------------------------------------------------------------------------------------
# Script entry point

Clear-Host # clears console

#-------------------------------------------------------------------------------------------------
# If we didn't get a location to run CPT at, use the current working directory

if (!$aSolutionsPath)
{
  $aSolutionsPath = Get-Location
}

# ------------------------------------------------------------------------------------------------
# Load param values from configuration file (if exists)

Update-ParametersFromConfigFile

#-------------------------------------------------------------------------------------------------
# Print script parameters

Print-InvocationArguments

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

# This PowerShell process may already have completed jobs. Discard them.
Remove-Job -State Completed

Write-Verbose "Source directory: $(Get-SourceDirectory)"
Write-Verbose "Scanning for project files"

[System.IO.FileInfo[]] $projects = Get-Projects
[int] $initialProjectCount       = $projects.Count
Write-Verbose ("Found $($projects.Count) projects")

# ------------------------------------------------------------------------------------------------
# If we get headers in the -file arg we have to detect CPPs that include that header

if ($aCppToCompile.Count -gt 0)
{
  # We've been given particular files to compile. If headers are among them
  # we'll find all source files that include them and tag them for processing.
  [string[]] $headerRefs = Get-HeaderReferences -files $aCppToCompile
  if ($headerRefs.Count -gt 0)
  {
    Write-Verbose-Array -name "Detected source files" -array $headerRefs

    $aCppToCompile += $headerRefs
  }
}

# ------------------------------------------------------------------------------------------------

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
    $projectsToProcess = $projects
  }

  if ($aVcxprojToIgnore)
  {
    $projectsToProcess = $projects | `
                         Where-Object { !(Should-IgnoreProject  -vcxprojPath $_.FullName ) }

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
  [System.IO.FileInfo[]] $projectsThatIncludeFiles = Get-SourceCodeIncludeProjects -projectPool $projectsToProcess `
                                                                                   -files $aCppToCompile
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
  #Write-Output ("PROJECTS: `n`t" + ($projectsToProcess -join "`n`t"))
  Write-Array -name "PROJECTS" -array $projectsToProcess

  if ($ignoredProjects)
  {
    Write-Array -name "IGNORED PROJECTS" -array $ignoredProjects
  }
}

# ------------------------------------------------------------------------------------------------

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

  Write-Output ("PROJECT$(if ($projectCounter -gt 1) { " #$projectCounter" } else { } ): " + $vcxprojPath)
  Process-Project -vcxprojPath $vcxprojPath -workloadType $workloadType
  Write-Output "" # empty line separator

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
