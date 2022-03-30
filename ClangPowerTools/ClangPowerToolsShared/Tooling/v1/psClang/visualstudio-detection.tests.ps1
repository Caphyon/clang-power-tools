#Clear-Host

# IMPORT code blocks

BeforeAll {
  @(
   , "$PSScriptRoot\io.ps1"
   , "$PSScriptRoot\visualstudio-detection.ps1"
   ) | ForEach-Object { . $_ }
}

Describe "Visual Studio detection" {
  # Mock script parameters
  $global:cptVisualStudioVersion = "2017"
  $aVisualStudioSku     = "Professional"

  It "Get-MscVer" {
    [string[]] $mscVer = Get-MscVer
    $mscVer.Count | Should -BeExactly 1
    $mscVer[0] | Should -Not -BeNullOrEmpty
    $mscVer[0].Length | Should -BeGreaterThan 3
    $mscVer[0].Contains(".") | Should -BeExactly $true
  }

  It "Get-VisualStudio-Path" {
    $vsPath = Get-VisualStudio-Path
    $vsPath | Should -Not -BeNullOrEmpty
  }

  It "Get-VisualStudio-Path [2015]" {
    # see first if VS 2015 is installed
    [Microsoft.Win32.RegistryKey] $vs14Key = Get-Item "HKLM:SOFTWARE\Wow6432Node\Microsoft\VisualStudio\14.0"
    [bool] $vs2015isInstalled = $vs14Key -and ![string]::IsNullOrEmpty($vs14Key.GetValue("InstallDir"))

    $oldMockValue = $global:cptVisualStudioVersion

    $vsPath = Get-VisualStudio-Path
    $vsPath | Should -Not -BeNullOrEmpty

    # Re-Mock script parameter
    $global:cptVisualStudioVersion = "2015"

    # Maybe we have a VS 2017 installation with v140 toolset installed
    [string] $vs2017ToolsetV140Path = "${Env:ProgramFiles(x86)}\Microsoft Visual Studio 14.0"
    if (Test-Path "$vs2017ToolsetV140Path\VC\include\iostream")
    {
      $vs2015isInstalled = $true
    }

    if ($vs2015isInstalled)
    {
      $vs2015Path = Get-VisualStudio-Path
      $vs2015Path | Should -Not -BeNullOrEmpty
      $vs2015Path | Should -Not -Be $vsPath
    }
    else
    {
      { Get-VisualStudio-Path } | Should -Throw
    }

    $global:cptVisualStudioVersion = $oldMockValue
  }

  It "Get-VisualStudio-Includes" {
    [string] $vsPath = Get-VisualStudio-Path
    [string] $mscver = Get-MscVer
    [string[]] $includes = Get-VisualStudio-Includes -vsPath $vsPath -mscVer $mscver
    $includes.Count | Should -BeGreaterThan 1
    $includes | ForEach-Object { [System.IO.Directory]::Exists($_) | Should -BeExactly $true }
  }
}
