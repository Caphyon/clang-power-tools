#Clear-Host

BeforeAll {
  @(
   , "$PSScriptRoot\io.ps1"
   ) | ForEach-Object { . $_ }
}

Describe "VariableExists" {
  
  It "Should verify VariableExists" {
    VariableExists 'Foobar_VariableExists' | Should -BeExactly $false
    $Foobar_VariableExists = 1
    VariableExists 'Foobar_VariableExists' | Should -BeExactly $true
    $Foobar_VariableExists = @()
    VariableExists 'Foobar_VariableExists' | Should -BeExactly $true

    Remove-Variable 'Foobar_VariableExists' | Out-Null
    VariableExistsAndNotEmpty 'Foobar_VariableExists' | Should -BeExactly $false
    $Foobar_VariableExists = "       "
    VariableExists            'Foobar_VariableExists' | Should -BeExactly $true
    VariableExistsAndNotEmpty 'Foobar_VariableExists' | Should -BeExactly $false
    $Foobar_VariableExists = " "
    VariableExistsAndNotEmpty 'Foobar_VariableExists' | Should -BeExactly $false
    $Foobar_VariableExists = "1"
    VariableExistsAndNotEmpty 'Foobar_VariableExists' | Should -BeExactly $true
  }
}

Describe "HasProperty" {
  
  It "Should verify HasProperty" {
    [string] $s = "abc"
    HasProperty $s "Length"  | Should -BeExactly $true
    HasProperty Ss "Lengthh" | SHould -BeExactly $false
    HasProperty $s "Trim"    | Should -BeExactly $false # this is a method
  }
}

Describe "File IO" {

  It "Get-QuotedPath" {
   Get-QuotedPath ''       | Should -BeExactly ''
   Get-QuotedPath '    '   | Should -BeExactly '    '
   Get-QuotedPath '   '    | Should -BeExactly '   '
   Get-QuotedPath 'test'   | Should -BeExactly '"test"'
   Get-QuotedPath '"test"' | Should -BeExactly '"test"'
   Get-QuotedPath 'c:\some file'   | Should -BeExactly '"c:\some file"'
   Get-QuotedPath '"c:\some file"' | Should -BeExactly '"c:\some file"'
  }

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
    $path = "$env:SystemRoot\notepad.exe"
    IsFileMatchingName -filePath $path -matchName "notepad" | Should     -BeExactly $true
    IsFileMatchingName -filePath $path -matchName "notepad.exe" | Should -BeExactly $true
    IsFileMatchingName -filePath $path -matchName "notepad.ex" | Should  -BeExactly $false
    IsFileMatchingName -filePath $path -matchName "note" | Should        -BeExactly $false
    IsFileMatchingName -filePath $path -matchName ".*" | Should          -BeExactly $false
  }

  It "IsFileMatchingName - with regex" {
    $path = "$env:SystemRoot\notepad.exe"
    IsFileMatchingName -filePath $path -matchName ([regex]"notepad") | Should     -BeExactly $true
    IsFileMatchingName -filePath $path -matchName ([regex]"notepad.exe") | Should -BeExactly $true
    IsFileMatchingName -filePath $path -matchName ([regex]"notepad.ex") | Should  -BeExactly $true
    IsFileMatchingName -filePath $path -matchName ([regex]"note") | Should        -BeExactly $true
    IsFileMatchingName -filePath $path -matchName ([regex]".*") | Should          -BeExactly $true
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
    cpt::Exists $winDir | should -BeExactly $true
    cpt::Exists "$winDir\notepad.exe" | should -BeExactly $true
    cpt::Exists "$winDir\foobar_surely_nonextant" | should -BeExactly $false
  }

  It "HasTrailingSlash" {
    cpt::HasTrailingSlash "ab" | should -BeExactly $false
    cpt::HasTrailingSlash "ab\" | should -BeExactly $true
    cpt::HasTrailingSlash "ab/" | should -BeExactly $true
    cpt::HasTrailingSlash "a/b/" | should -BeExactly $true
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
