<#
.SYNOPSIS
    Compiles or tidies up code from Visual Studio .vcxproj project files.
    It sets up the scene required for clang-build.ps1 to do its job, and makes
    command-line usage for projects and files quicker.

    Before calling sample-clang-build.ps1 you need to set the current directory
    to the root source directory.

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
      
.PARAMETER aTidyFixFlags
      Alias 'tidy-fix'. If not empty clang-tidy will be called with given flags, instead of clang++. 
      The tidy operation is applied to whole translation units, meaning all directory headers 
      included in the CPP will be tidied up too. Changes will be applied to the file(s).

      If present, this parameter takes precedence over aTidyFlags.
      
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

.EXAMPLE
    PS .\sample-clang-build.ps1 -dir -proj foo,bar -file meow -tidy "-*,modernize-*"
    <Description of example>
    Runs clang-tidy, using "-*,modernize-*", on all CPPs containing 'meow' in their name from 
    the projects containing 'foo' or 'bar' in their names. 
    
    Doesn't actually apply the clang-tidy module changes to CPPs. 
    It will only print the tidy module output.
    
.EXAMPLE
    PS .\sample-clang-build.ps1 -dir -proj foo,bar -file meow -tidy-fix "-*,modernize-*"
    <Description of example>
    Runs clang-tidy, using "-*,modernize-*", on all CPPs containing 'meow' in their name from 
    the projects containing 'foo' or 'bar' in their names. 
    
    It will apply all tidy module changes to CPPs.

.EXAMPLE
    PS .\sample-clang-build.ps1 -dir -proj foo -proj-ignore foobar
    <Description of example>
    Runs clang++ on all CPPs in foo... projects, except foobar
  
.OUTPUTS
    Will output Clang warnings and errors to screen. The return code will be 0 for success, >0 for failure.

.NOTES
    Author: Gabriel Diaconita
#>
param( [alias("proj")]        [Parameter(Mandatory=$false)][string[]] $aVcxprojToCompile
     , [alias("proj-ignore")] [Parameter(Mandatory=$false)][string[]] $aVcxprojToIgnore
     , [alias("active-config")][Parameter(Mandatory=$false)][string]  $aVcxprojConfigPlatform
     , [alias("file")]        [Parameter(Mandatory=$false)][string]   $aCppToCompile
     , [alias("file-ignore")] [Parameter(Mandatory=$false)][string[]] $aCppToIgnore
     , [alias("parallel")]    [Parameter(Mandatory=$false)][switch]   $aUseParallelCompile
     , [alias("continue")]    [Parameter(Mandatory=$false)][switch]   $aContinueOnError
     , [alias("treat-sai")]    [Parameter(Mandatory=$false)][switch]  $aTreatAdditionalIncludesAsSystemIncludes
     , [alias("literal")]     [Parameter(Mandatory=$false)][switch]   $aDisableNameRegexMatching
     , [alias("tidy")]        [Parameter(Mandatory=$false)][string]   $aTidyFlags
     , [alias("tidy-fix")]    [Parameter(Mandatory=$false)][string]   $aTidyFixFlags
     , [alias("format-style")] [Parameter(Mandatory=$false)][string]  $aAfterTidyFixFormatStyle
     )

# ------------------------------------------------------------------------------------------------

Set-Variable -name kClangCompileFlags                                       -Option Constant `
                                            -value @( "-Werror"
                                                    , "-Wall"
                                                    , "-fms-compatibility-version=19.10"
                                                    , "-Wmicrosoft"
                                                    , "-Wno-invalid-token-paste"
                                                    , "-Wno-unknown-pragmas"
                                                    , "-Wno-unused-value"
                                                    )

Set-Variable -name kVisualStudioVersion -value "2017"                       -Option Constant
Set-Variable -name kVisualStudioSku     -value "Professional"               -Option Constant

# ------------------------------------------------------------------------------------------------

Function Merge-Array([string[]] $aArray)
{
  # we need to individually wrap items into quotes as values
  # can contain PS control characters (e.g. - in -std=c++14)
  $quotedArray = ($aArray | ForEach-Object { """$_"""})
  return ($quotedArray -join ",")
}

[string]   $scriptDirectory = (Split-Path -parent $PSCommandPath)

[string]   $clangScript     = "$scriptDirectory\clang-build.ps1"
[string[]] $scriptParams    = @("-aSolutionsPath", "'$(Get-Location)'")

if (![string]::IsNullOrEmpty($aVcxprojToCompile))
{
  $scriptParams += ("-aVcxprojToCompile", (Merge-Array $aVcxprojToCompile))
}

if (![string]::IsNullOrEmpty($aVcxprojToIgnore))
{
  $scriptParams += ("-aVcxprojToIgnore", (Merge-Array $aVcxprojToIgnore))
}

if (![string]::IsNullOrEmpty($aVcxprojConfigPlatform))
{
  $scriptParams += ("-aVcxprojConfigPlatform",  (Merge-Array $aVcxprojConfigPlatform))
}

if (![string]::IsNullOrEmpty($aCppToCompile))
{
  $scriptParams += ("-aCppToCompile", (Merge-Array $aCppToCompile))
}

if (![string]::IsNullOrEmpty($aCppToIgnore))
{
  $scriptParams += ("-aCppToIgnore", (Merge-Array $aCppToIgnore))
}

$scriptParams += ("-aClangCompileFlags", (Merge-Array $kClangCompileFlags))

if (![string]::IsNullOrEmpty($aTidyFlags))
{
  $scriptParams += ("-aTidyFlags", (Merge-Array (@($aTidyFlags))))
}

if (![string]::IsNullOrEmpty($aTidyFixFlags))
{
  $scriptParams += ("-aTidyFixFlags", (Merge-Array (@($aTidyFixFlags))))
}

if (![string]::IsNullOrEmpty($aAfterTidyFixFormatStyle))
{
  $scriptParams += ("-aAfterTidyFixFormatStyle", $aAfterTidyFixFormatStyle)
}

if ($aUseParallelCompile)
{
  $scriptParams += ("-aUseParallelCompile")
}

if ($aContinueOnError)
{
  $scriptParams += ("-aContinueOnError")
}

if ($aTreatAdditionalIncludesAsSystemIncludes)
{
  $scriptParams += ("-aTreatAdditionalIncludesAsSystemIncludes")
}

if ($aDisableNameRegexMatching)
{
  $scriptParams += ("-aDisableNameRegexMatching")
}

$scriptParams += ("-aVisualStudioVersion", $kVisualStudioVersion)
$scriptParams += ("-aVisualStudioSku",     $kVisualStudioSku)
$scriptParams += ("-aTidyHeaderFilter",    ".*")

Invoke-Expression "&'$clangScript' $scriptParams"
