#Clear-Host

# IMPORT code blocks

BeforeAll {
  @(
   , "$PSScriptRoot\io.ps1"
   , "$PSScriptRoot\msbuild-expression-eval.ps1"
   , "$PSScriptRoot\msbuild-project-load.ps1"
   ) | ForEach-Object { . $_ }
}

Describe "VC++ Project Data Loading" {
  It "To be implemented" {
  }
}
