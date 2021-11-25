param([Parameter(Mandatory=$true, HelpMessage="Location in which to create the 2022 VSIX starting from an existing one")][string] $loc)

Function UpdateManifest()
{
  $fileContent = Get-Content .\extension.vsixmanifest
  $fileContent = $fileContent -replace "<DisplayName>Clang Power Tools</DisplayName>", "<DisplayName>Clang Power Tools 2022</DisplayName>"
  $fileContent = $fileContent -replace "<Identity Id=""Caphyon.705559db-5755-43fa-a023-41a3b14d2935""", "<Identity Id=""Caphyon.9ce239f2-d27a-432c-906c-1d55a123dbfd"""
  $fileContent > .\extension.vsixmanifest
}

$errorActionPreference = "Stop"

Set-Location $loc

Write-Output "Updating manifest for 2022-vsix starting from ($loc)\ClangPowerTools.vsix..."
If (!(Test-Path .\ClangPowerTools.vsix))
{
  Write-Error "Source VSIX does not exist. Aborting..."
}

Copy-Item .\ClangPowerTools.vsix .\ClangPowerTools2022.vsix

7z e .\ClangPowerTools2022.vsix extension.vsixmanifest
UpdateManifest
7z u .\ClangPowerTools2022.vsix extension.vsixmanifest

Remove-Item .\extension.vsixmanifest

explorer $loc
