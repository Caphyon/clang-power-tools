param([Parameter(Mandatory=$true, HelpMessage="Location in which to create the 2022 VSIX starting from an existing one")][string] $loc)

Function ReplaceContentInFile([Parameter(Mandatory=$true)][string] $fileName,
                              [Parameter(Mandatory=$true)][string] $oldString,
                              [Parameter(Mandatory=$true)][string] $newString)
{
  7z e .\ClangPowerTools2022.vsix $filename
  $fileContent = Get-Content $fileName
  $fileContent = $fileContent -replace $oldString, $newString
  $fileContent > $fileName
  7z u .\ClangPowerTools2022.vsix $filename
  Remove-Item $fileName
}

$errorActionPreference = "Stop"

Set-Location $loc

Write-Output "Updating manifest for 2022-vsix starting from ($loc)\ClangPowerTools.vsix..."
If (!(Test-Path .\ClangPowerTools.vsix))
{
  Write-Error "Source VSIX does not exist. Aborting..."
}

Copy-Item .\ClangPowerTools.vsix .\ClangPowerTools2022.vsix

ReplaceContentInFile -fileName "extension.vsixmanifest" -oldString "<DisplayName>Clang Power Tools</DisplayName>" -newString "<DisplayName>Clang Power Tools 2022</DisplayName>"
ReplaceContentInFile -fileName "extension.vsixmanifest" -oldString "Caphyon.705559db-5755-43fa-a023-41a3b14d2935" -newString "Caphyon.9ce239f2-d27a-432c-906c-1d55a123dbfd"
ReplaceContentInFile -fileName "manifest.json" -oldString "Caphyon.705559db-5755-43fa-a023-41a3b14d2935" -newString "Caphyon.9ce239f2-d27a-432c-906c-1d55a123dbfd"
ReplaceContentInFile -fileName "catalog.json" -oldString "Caphyon.705559db-5755-43fa-a023-41a3b14d2935" -newString "Caphyon.9ce239f2-d27a-432c-906c-1d55a123dbfd"

explorer $loc