BeforeAll {
  . "$PsScriptRoot\io.ps1"
}

Describe "ai" {
  It "Should build Advanced Installer" {

   [string] $advinstRepo = $env:ADVINST_CPT

   if ($advinstRepo)
   {
    Push-Location $advinstRepo

    [string] $scriptLocation = Canonize-Path -base "$PSScriptRoot" -child "..\clang-build.ps1"
    &"$scriptLocation" 2>&1 | Out-Default
    [int] $exitCode = $LASTEXITCODE
    Pop-Location

    Write-Output "$PSScriptRoot"
    $exitCode | Should -BeExactly 0
   }
  }
}