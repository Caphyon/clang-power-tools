#Clear-Host

# IMPORT code blocks

BeforeAll {
  @(
   , "$PSScriptRoot\io.ps1"
   , "$PSScriptRoot\itemdefinition-context.ps1"
   , "$PSScriptRoot\msbuild-expression-eval.ps1"
   ) | ForEach-Object { . $_ }
}

Describe "MSBuild - Powershell Expression translation" {
  It "Plain expressions" {
    Evaluate-MSBuildExpression "MyProjectString" | Should -BeExactly "MyProjectString"
    Evaluate-MSBuildExpression "1905" | Should -BeExactly "1905"
    Evaluate-MSBuildExpression "a;;b;c" | Should -BeExactly "a;;b;c"
    Evaluate-MSBuildExpression "a-b-c" | Should -BeExactly "a-b-c"
    Evaluate-MSBuildExpression "1-2-3" | Should -BeExactly "1-2-3"
    Evaluate-MSBuildExpression "{1-2-3-4}" | Should -BeExactly "{1-2-3-4}"
    Evaluate-MSBuildExpression "1.2.3.4" | Should -BeExactly "1.2.3.4"
    Evaluate-MSBuildExpression "c:\foo\bar.ini" | Should -BeExactly "c:\foo\bar.ini"
    Evaluate-MSBuildExpression "..\foo\bar" | Should -BeExactly "..\foo\bar"
  }

  It "Arithmetical operators" {
    Evaluate-MSBuildExpression "`$(1+2+3)" | Should -BeExactly "6"
    Evaluate-MSBuildExpression "`$(1-2-3)" | Should -BeExactly "-4"
    Evaluate-MSBuildExpression "`$(1*2*3)" | Should -BeExactly "6"
    Evaluate-MSBuildExpression "`$(10/2)" | Should -BeExactly "5"
  }

  It "Read from registry" {
    $e = '$(Registry:HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion@ProgramFilesDir)'
    Evaluate-MSBuildExpression $e | Should -BeExactly $env:ProgramFiles

    $e = '$(GetRegValue("HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion@ProgramFilesDir"))'
    Evaluate-MSBuildExpression $e | Should -BeExactly $env:ProgramFiles
  }

  It "Property expansion" {
    $ProjectDir = "C:\Users\Default"
    Evaluate-MSBuildExpression "`$Foo;`$(ProjectDir);..\..;..\..\third-party" `
                    | Should -BeExactly     '$Foo;C:\Users\Default;..\..;..\..\third-party'

    $TargetName = "Test"
    Evaluate-MSBuildExpression "%(ASDASD);`$(TargetName)" | Should -BeExactly ";Test"

    $prop = "123"
    Evaluate-MSBuildExpression 'plaintext;"$(prop)"' | Should -BeExactly 'plaintext;"123"'
    Evaluate-MSBuildExpression 'plaintext;''$(prop)''' | Should -BeExactly 'plaintext;"123"'
    Evaluate-MSBuildExpression 'plaintext;$(prop)-$(prop)' | Should -BeExactly 'plaintext;123-123'

    $TestDir = $env:ProgramFiles
    Evaluate-MSBuildExpression '$(TestDir)\first\second' | Should -BeExactly "$env:ProgramFiles\first\second"
  }

  It "GetDirectoryNameOfFileAbove() / GetPathOfFileAbove() MSBuild builtin functions" {
    [string] $MSBuildThisFileDirectory = $env:SystemRoot

    $e = '$([MSBuild]::GetDirectoryNameOfFileAbove($(MSBuildThisFileDirectory), ''Program Files'')Program Files'
    Evaluate-MSBuildExpression $e | Should -BeExactly $env:ProgramFiles
    $e = '$([MSBuild]::GetPathOfFileAbove(''Program Files'', $(MSBuildThisFileDirectory))'
    Evaluate-MSBuildExpression $e | Should -BeExactly $env:ProgramFiles

    $e = '$([MSBuild]::GetDirectoryNameOfFileAbove($(MSBuildThisFileDirectory), "Program Files")Program Files'
    Evaluate-MSBuildExpression $e | Should -BeExactly $env:ProgramFiles
    $e = '$([MSBuild]::GetPathOfFileAbove("Program Files", $(MSBuildThisFileDirectory))'
    Evaluate-MSBuildExpression $e | Should -BeExactly $env:ProgramFiles
    $e = '$([MSBuild]::GetPathOfFileAbove("Program Files", ''$(MSBuildThisFileDirectory)''))'
    Evaluate-MSBuildExpression $e | Should -BeExactly $env:ProgramFiles

    [string] $MSBuildThisFileDirectory2 = $MSBuildThisFileDirectory + "\System32"
    $e = '$([MSBuild]::GetPathOfFileAbove(''Program Files'', "$(MSBuildThisFileDirectory2)../"))'
    Evaluate-MSBuildExpression $e | Should -BeExactly $env:ProgramFiles

    $e = "`$([MSBuild]::GetDirectoryNameOfFileAbove('$MSBuildThisFileDirectory', 'Program Files')Program Files"
    Evaluate-MSBuildExpression $e | Should -BeExactly $env:ProgramFiles
    $e = "`$([MSBuild]::GetPathOfFileAbove('Program Files', '$MSBuildThisFileDirectory')"
    Evaluate-MSBuildExpression $e | Should -BeExactly $env:ProgramFiles

    [string] $whatToFind = "Program Files"
    $e = '$([MSBuild]::GetDirectoryNameOfFileAbove($(MSBuildThisFileDirectory), ''$(whatToFind)'')Program Files'
    Evaluate-MSBuildExpression $e | Should -BeExactly $env:ProgramFiles
    $e = '$([MSBuild]::GetPathOfFileAbove(''$(whatToFind)'', $(MSBuildThisFileDirectory))'
    Evaluate-MSBuildExpression $e | Should -BeExactly $env:ProgramFiles

    $e = '$([MSBuild]::GetDirectoryNameOfFileAbove($(MSBuildThisFileDirectory), Program Files)Program Files'
    Evaluate-MSBuildExpression $e | Should -BeExactly $env:ProgramFiles
    $e = '$([MSBuild]::GetPathOfFileAbove(Program Files, $(MSBuildThisFileDirectory)))'
    Evaluate-MSBuildExpression $e | Should -BeExactly $env:ProgramFiles
    $e = '$([MSBuild]::GetPathOfFileAbove(Program Files, ''$(MSBuildThisFileDirectory)''))'
    Evaluate-MSBuildExpression $e | Should -BeExactly $env:ProgramFiles
    $e = '$([MSBuild]::GetPathOfFileAbove(''Program Files'', ''$(MSBuildThisFileDirectory)''))'
    Evaluate-MSBuildExpression $e | Should -BeExactly $env:ProgramFiles

    [string] $_DirectoryBuildPropsFile = "clang-build.ps1"
    [string] $MSBuildProjectDirectory  = "$PSScriptRoot"
    [string] $DirParent = [System.IO.Directory]::GetParent($MSBuildProjectDirectory)

    $e = '$([MSBuild]::GetDirectoryNameOfFileAbove($(MSBuildProjectDirectory), ''$(_DirectoryBuildPropsFile)''))'
    Evaluate-MSBuildExpression $e | Should -Be "$DirParent"
    $e = '$([MSBuild]::GetPathOfFileAbove(''$(_DirectoryBuildPropsFile)'', $(MSBuildProjectDirectory)))'
    Evaluate-MSBuildExpression $e | Should -Be "$DirParent\$_DirectoryBuildPropsFile"
  }

  It "MakeRelative() MSBuild builtin function" {
    $SystemDrive  = $env:SystemDrive
    $SystemRoot   = $env:SystemRoot
    $ProgramFiles = $env:ProgramFiles

    $e = "`$([MSBuild]::MakeRelative('$SystemDrive', '$SystemRoot'))"
    Evaluate-MSBuildExpression $e | Should -Be "Windows"

    $e = "`$([MSBuild]::MakeRelative(`$(SystemDrive), '$SystemRoot'))"
    Evaluate-MSBuildExpression $e | Should -Be "Windows"

    $e = '$([MSBuild]::MakeRelative($(SystemDrive), $(SystemRoot)\System32))'
    Evaluate-MSBuildExpression $e | Should -Be "Windows\System32"

    $e = '$([MSBuild]::MakeRelative($(SystemRoot), $(SystemRoot)\System32))'
    Evaluate-MSBuildExpression $e | Should -Be "System32"

    $e = '$([MSBuild]::MakeRelative($(ProgramFiles), $(SystemRoot)\System32))'
    Evaluate-MSBuildExpression $e | Should -Be "..\Windows\System32"
  }

  It ".NET Method invocation" {
    $Sys32Folder = "System32"
    $WinDir = $env:SystemRoot
    $e = '$([System.IO.Path]::Combine(''$(WinDir)'', ''$(Sys32Folder)''))'
    Evaluate-MSBuildExpression $e | Should -BeExactly "$WinDir\$Sys32Folder"
  }
}

Describe "Condition evaluation" {
  It "Logical operators" {
    Evaluate-MSBuildCondition '0 != 1' | Should -BeExactly $true
    Evaluate-MSBuildCondition '1 != 1' | Should -BeExactly $false
    Evaluate-MSBuildCondition '1 == 1' | Should -BeExactly $true
    Evaluate-MSBuildCondition '0 == 1' | Should -BeExactly $false
    Evaluate-MSBuildCondition '0 < 1' | Should -BeExactly $true
    Evaluate-MSBuildCondition '1 <= 1' | Should -BeExactly $true
    Evaluate-MSBuildCondition '1 < 0' | Should -BeExactly $false
    Evaluate-MSBuildCondition '1 <= 0' | Should -BeExactly $false
    Evaluate-MSBuildCondition '1 > 0' | Should -BeExactly $true
    Evaluate-MSBuildCondition '1 >= 1' | Should -BeExactly $true
    Evaluate-MSBuildCondition '1 < 0 or 0 < 1' | Should -BeExactly $true
    Evaluate-MSBuildCondition '!(1 < 0 or 0 < 1)' | Should -BeExactly $false
    Evaluate-MSBuildCondition '1 < 0 and 0 < 1' | Should -BeExactly $false
    Evaluate-MSBuildCondition '1 < 0 and 0 < 1' | Should -BeExactly $false
    Evaluate-MSBuildCondition '((1 < 0) or (0 < 1)) and !("a"=="b")' | Should -BeExactly $true
    Evaluate-MSBuildCondition '"apple" == "apple"' | Should -BeExactly $true
    Evaluate-MSBuildCondition '''apple'' == ''apple''' | Should -BeExactly $true
    Evaluate-MSBuildCondition '''apple'' == ''pear''' | Should -BeExactly $false
    Evaluate-MSBuildCondition '"apple" != "pear"' | Should -BeExactly $true
  }

  It "Registry access" {
    $c = "'`$(Registry:HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion@ProgramFilesDir)' != ''"
    Evaluate-MSBuildCondition $c | Should -BeExactly $true

    $ProgramFiles = $env:ProgramFiles
    $c = "'`$(Registry:HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion@ProgramFilesDir)' == '`$(ProgramFiles)'"
    Evaluate-MSBuildCondition $c | Should -BeExactly $true

    $c = "'`$(Registry:HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion@NonexistentValue)' == ''"
    Evaluate-MSBuildCondition $c | Should -BeExactly $true
  }

  It "Variable expansion" {

    $Configuration = "Release2"
    $Platform = "Win32"

    Evaluate-MSBuildCondition "'`$(Configuration)|`$(Platform)'=='Debug|Win32' or '`$(Configuration)' == 'Release2'" | Should -BeExactly $true
    Evaluate-MSBuildCondition "'`$(Configuration)|`$(Platform)'=='Release|Win32'" | Should -BeExactly $false
    Evaluate-MSBuildCondition '$(Platform.Replace(" ", "")) and $(testB)' | Should -BeExactly $false
  }

  It "Exists() MSBuild builtin function" {

    $WinDir = $env:SystemRoot
    Evaluate-MSBuildCondition "exists('`$(WinDir)')" | Should -BeExactly $true
    Evaluate-MSBuildCondition "1 == 1 and exists('`$(WinDir)')" | Should -BeExactly $true
    Evaluate-MSBuildCondition "exists('`$(WinDir)\System32')" | Should -BeExactly $true
    Evaluate-MSBuildCondition "exists('`$(WinDir)\System64')" | Should -BeExactly $false

    $WinDir += "gibberish12345"
    Evaluate-MSBuildCondition "exists('`$(WinDir)')" | Should -BeExactly $false
    Evaluate-MSBuildCondition "0 == 1 and exists('`$(WinDir)')" | Should -BeExactly $false

    [System.Reflection.Assembly]::LoadWithPartialName("System.IO")
    $eression = 'Exists("$([System.IO.Directory]::GetCurrentDirectory())")'
    Evaluate-MSBuildCondition $eression | Should -BeExactly $true
    $eression = 'Exists("$([System.IO.Directory]::GetCurrentDirectory())\nonexistent12345")'
    Evaluate-MSBuildCondition $eression | Should -BeExactly $false

    $Sys32 = "$env:SystemRoot\System32"
    $WinDir = "$Sys32..\.."
    Evaluate-MSBuildCondition "Exists('`$(Sys32)\..')" | Should -BeExactly  $true
  }

  It "Access to [String] builtin functions" {

    $TargetName    = "AnotherTest"
    Evaluate-MSBuildCondition "`$(TargetName.EndsWith('Test'))" | Should -BeExactly $true
    Evaluate-MSBuildCondition "`$(TargetName.EndsWith('Test2'))" | Should -BeExactly $false

    $myVar = 'TwoThree'
    $MySelector = "One;Two;Three"
    Evaluate-MSBuildCondition "`$(MySelector.Contains(`$(myVar.Substring(3, 3))))"`
                   | Should -BeExactly  $true

    $MySelector = "One;Two;Three"
    $myVar = "Two"
    Evaluate-MSBuildCondition "`$(MySelector.Contains(`$(myVar)))" | Should -BeExactly $true

    $MySelector = "One;Two;Three"
    Evaluate-MSBuildCondition "`$(MySelector.Contains('Three'))" | Should -BeExactly $true
    Evaluate-MSBuildCondition "`$(MySelector.Contains('Four'))" | Should -BeExactly $false
  }

  It ".NET method invocation" {
    $year = (Get-Date).Year
    Evaluate-MSBuildCondition "`$([System.DateTime]::Now.Year) == `$(year)" | Should -BeExactly $true
    Evaluate-MSBuildCondition "`$([System.DateTime]::Now.Year) != `$(year)" | Should -BeExactly $false
  }

  It "HasTrailingSlash() MSBuild builtin function" {
    Evaluate-MSBuildCondition "HasTrailingSlash('c:\windows\')" | Should -BeExactly $true
    Evaluate-MSBuildCondition "HasTrailingSlash('c:\windows')" | Should -BeExactly $false

    $c = "HasTrailingSlash('c:\windows\') and hasTrailingSlash('c:\temp/')"
    Evaluate-MSBuildCondition $c | Should -BeExactly $true

    $c = "HasTrailingSlash('c:\windows\') and !hasTrailingSlash('c:\temp/')"
    Evaluate-MSBuildCondition $c | Should -BeExactly $false

    $prop = "c:\windows\"
    Evaluate-MSBuildCondition "hasTrailingSlash(`$(prop))" | Should -BeExactly $true

    $prop = "c:\windows"
    Evaluate-MSBuildCondition "hasTrailingSlash(`$(prop))" | Should -BeExactly $false
  }

  It "Itemgroupdefinition" {
    $defVal = "bar"

    Set-ProjectItemProperty "foo" $defVal
    Evaluate-MSBuildExpression "%(foo)" | Should -BeExactly $defVal
    Evaluate-MSBuildCondition "'%(foo)' == '$defVal'" | Should -BeExactly $true
    Evaluate-MSBuildCondition "'%(foo)' != '$defVal'" | Should -BeExactly $false

    $P1 = "prop_value"
    Evaluate-MSBuildExpression '%(foo)|$(P1)' | Should -BeExactly "$defVal|$P1"
    Evaluate-MSBuildExpression '%(foo);$(P1)' | Should -BeExactly "$defVal;$P1"
  }
}