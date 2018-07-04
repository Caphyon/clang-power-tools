#Clear-Host

# IMPORT code blocks

Set-Variable -name "kScriptLocation"                                              `
             -value (Split-Path -Path $MyInvocation.MyCommand.Definition -Parent) <#`
             -option Constant#>

@(
 , "$kScriptLocation\io.ps1"
 ) | ForEach-Object { . $_ }

Describe "File IO" {
  It "Remove-PathTrailingSlash" {
    Remove-PathTrailingSlash "c:\windows\" | Should -BeExactly "c:\windows"
    Remove-PathTrailingSlash "c:\windows" | Should -BeExactly "c:\windows"
    Remove-PathTrailingSlash "..\foo\bar\" | Should -BeExactly "..\foo\bar"
  }

  It "Get-FileDirectory" {
    Get-FileDirectory "$env:SystemRoot\explorer.exe" | Should -BeExactly "$env:SystemRoot\"
    Get-FileDirectory "$env:SystemRoot\explorer.exe" | Should -BeExactly "$env:SystemRoot\"
    Get-FileDirectory "$env:SystemRoot\foobar.nonexistent" | Should -BeExactly "$env:SystemRoot\"
    Get-FileDirectory "foo\bar" | Should -BeExactly "foo\"
  }

  It "Get-FileName" {
    Get-FileName "$env:SystemRoot\explorer.exe" | Should -BeExactly "explorer.exe"
    Get-FileName "$env:SystemRoot\foobar.nonexistent" | Should -BeExactly "foobar.nonexistent"
  }

  It "IsFileMatchingName - no regex" {
    # Mocking script parameter aDisableNameRegexMatching
    [bool] $aDisableNameRegexMatching = $true

    $path = "$env:SystemRoot\notepad.exe"
    IsFileMatchingName -filePath $path -matchName "notepad" | Should     -BeExactly $true
    IsFileMatchingName -filePath $path -matchName "notepad.exe" | Should -BeExactly $true
    IsFileMatchingName -filePath $path -matchName "notepad.ex" | Should  -BeExactly $false
    IsFileMatchingName -filePath $path -matchName "note" | Should        -BeExactly $false
    IsFileMatchingName -filePath $path -matchName ".*" | Should          -BeExactly $false
  }

  It "IsFileMatchingName - with regex" {
    # Mocking script parameter aDisableNameRegexMatching
    [bool] $aDisableNameRegexMatching = $false

    $path = "$env:SystemRoot\notepad.exe"
    IsFileMatchingName -filePath $path -matchName "notepad" | Should     -BeExactly $true
    IsFileMatchingName -filePath $path -matchName "notepad.exe" | Should -BeExactly $true
    IsFileMatchingName -filePath $path -matchName "notepad.ex" | Should  -BeExactly $true
    IsFileMatchingName -filePath $path -matchName "note" | Should        -BeExactly $true
    IsFileMatchingName -filePath $path -matchName ".*" | Should          -BeExactly $true
  }

  It "FileHasExtension" {
    FileHasExtension -filePath "c:\foo.bar" -ext 'bar' | Should -BeExactly $true
    FileHasExtension -filePath "c:\foo.bar" -ext 'bar2' | Should -BeExactly $false
    FileHasExtension -filePath "c:\foo.bar" -ext @('bar') | Should -BeExactly $true
    FileHasExtension -filePath "c:\foo.bar" -ext @('bar2') | Should -BeExactly $false
    FileHasExtension -filePath "c:\foo.bar" -ext @('bar', 'bar2') | Should -BeExactly $true
    FileHasExtension -filePath "c:\foo.bar" -ext @('bar2', 'bar') | Should -BeExactly $true
    FileHasExtension -filePath "c:\foo.bar" -ext @('bar2', 'bar2') | Should -BeExactly $false
  }

  It "Canonize-Path" {
    $sysDrive = "$env:SystemDrive\"
    Canonize-Path -base $sysDrive -child "Windows" | Should -Be $env:SystemRoot
    { Canonize-Path -base $sysDrive -child "foobar" } | Should -throw
    { Canonize-Path -base $sysDrive -child "foobar" -ignoreErrors } | Should -not -throw
    Canonize-Path -base $sysDrive -child "foobar" -ignoreErrors | Should -BeExactly $null

    [string[]] $files = Canonize-Path -base $sysDrive -child "*" # get all children
    $files.Count | Should -BeGreaterThan 1
  }

  It "Exists" {
    [string] $winDir = $env:SystemRoot
    Exists $winDir | should -BeExactly $true
    Exists "$winDir\notepad.exe" | should -BeExactly $true
    Exists "$winDir\foobar_surely_nonextant" | should -BeExactly $false
  }

  It "HasTrailingSlash" {
    HasTrailingSlash "ab" | should -BeExactly $false
    HasTrailingSlash "ab\" | should -BeExactly $true
    HasTrailingSlash "ab/" | should -BeExactly $true
    HasTrailingSlash "a/b/" | should -BeExactly $true
  }

  It "EnsureTrailingSlash" {
    EnsureTrailingSlash "ab" | should -BeExactly "ab\"
    EnsureTrailingSlash "ab\" | should -BeExactly "ab\"
    EnsureTrailingSlash "ab/" | should -BeExactly "ab/"
    EnsureTrailingSlash "a/b/" | should -BeExactly "a/b/"
  }
}

Describe "Command IO" {
  It "Exists-Command" {
    Exists-Command "Get-Process" | Should -BeExactly $true
    Exists-Command "Get-JiggyWithIt" | Should -BeExactly $false
  }
}
