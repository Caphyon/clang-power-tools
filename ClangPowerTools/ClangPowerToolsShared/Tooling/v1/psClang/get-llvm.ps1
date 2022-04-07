<#
.SYNOPSIS
    Downloads ready-to-use one or more LLVM binaries, if not already available on system.

.DESCRIPTION
    Purpose of this script is to ensure that LLVM binaries are available for use.
    The strategy used, for each tool, is: 
     * search in PATH
     * search in Program Files
     * search in %APPDATA%\ClangPowerTools\LLVM_Lite
     * if not found, download tool and save in LLVM_Lite location

.PARAMETER aTool
    Alias 'tool'. The LLVM tool(s) to potentially download.

.NOTES
    Author: Gabriel Diaconita, Marina Rusu
#>
#Requires -Version 3
param( [alias("tool")]
       [Parameter(Mandatory=$false, HelpMessage="LLVM tool(s) to ensure exist")]
       [string[]] $aTools = @()
     )

Set-StrictMode -version latest
$ErrorActionPreference = 'Continue'

. "$PSScriptRoot\get-llvm-helper.ps1"

$ret = @()
foreach ($tool in $aTools)
{
  [string] $toolLocation = Ensure-LLVMTool-IsPresent $tool
  $ret += @($toolLocation)
}

Write-Output $ret