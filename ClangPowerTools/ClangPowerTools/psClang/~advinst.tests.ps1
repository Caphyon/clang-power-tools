. "$PsScriptRoot\io.ps1"
Describe "ai" {
  It "Should build Advanced Installer" {

   [string] $advinstRepo = $env:ADVINST

   if ($advinstRepo)
   {
    Push-Location $advinstRepo

    [string] $scriptLocation = Canonize-Path -base "$PSScriptRoot" -child "..\sample-clang-build.ps1"
    &"$scriptLocation" -parallel -proj-ignore coder,lexer -treat-sai -file-ignore '.\.c$' 2>&1 | Out-Default
    [int] $exitCode = $LASTEXITCODE
    Pop-Location

    Write-Output "$PSScriptRoot"
    $exitCode | Should -BeExactly 0
   }
  }
}