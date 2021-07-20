#Clear-Host

# IMPORT code blocks

Set-Variable -name "kScriptLocation"                                              `
             -value (Split-Path -Path $MyInvocation.MyCommand.Definition -Parent) <#`
             -option Constant#>

@(
 , "$kScriptLocation\io.ps1"
 , "$kScriptLocation\msbuild-expression-eval.ps1"
 , "$kScriptLocation\msbuild-project-load.ps1"
 , "$kScriptLocation\msbuild-project-data.ps1"
 ) | ForEach-Object { . $_ }

Describe "VC++ Project Data Processing" {
  It "To be implemented" {
  }
}
