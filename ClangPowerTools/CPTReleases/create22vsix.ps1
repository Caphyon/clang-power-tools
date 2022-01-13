param([Parameter(Mandatory=$true, HelpMessage="Location in which to create the 2022 VSIX starting from an existing one")][string] $loc)

Function ReplaceContentInFile([Parameter(Mandatory=$true)][string] $extensionName,
							  [Parameter(Mandatory=$true)][string] $fileName,
                              [Parameter(Mandatory=$true)][string] $oldString,
                              [Parameter(Mandatory=$true)][string] $newString)
{
  7z e $extensionName $filename
  $fileContent = Get-Content $fileName
  $fileContent = $fileContent -replace $oldString, $newString
  $fileContent > $fileName
  # 7z u .\ClangPowerTools2022.vsix $filename
  7z u $extensionName $filename
  Remove-Item $fileName
}

function Get-IncrementedRevisionVersion([Parameter(Mandatory=$true)][string] $extensionName,
 										[Parameter(Mandatory=$true)][string] $filename)
{
	7z e $extensionName $filename
	$data = Get-Content $filename
	Clear-Variable -Name "Matches" -ErrorAction SilentlyContinue
	$version = 0
	7z u $extensionName $filename
	Remove-Item $fileName
	$Null = @(
		$data | Where-Object {$_ -match "\d+\.\d+\.\d+.\d+"} | Foreach {$Matches[0]}
	)
	if($Matches.Count -eq 0)
	{
		$Null = @(
			$data | Where-Object {$_ -match "\d+\.\d+\.\d+"} | Foreach {$Matches[0]}
		)
		foreach($m in $Matches)
		{
			if($Matches[0].split(".")[0] -as [int] -ge 7)
			{
				$version = $m
			} 
			$message = "Detected version " + $version
			Write-Verbose $message
			$versionUpdatedRevision = $Matches[0] + ".0"
			$result = @{"init" = $version[0]; "final" = $versionUpdatedRevision}
		}
		
		
	}else
	{
		$version = $Matches[0]
		$message = "Detected version " + $version
		Write-Verbose $message	
		
		$versionNumbers = $version.split(".")
		$revisionNumber = $versionNumbers[3] -as [int]
		++$revisionNumber
		$versionUpdatedRevision = $versionNumbers[0] + "." + $versionNumbers[1] + "." + $versionNumbers[2] + "." + $revisionNumber 
		$result = @{"init" = $version; "final" = $versionUpdatedRevision}

	}

	#replace versions in ClangPowerTools
	ReplaceContentInFile -extensionName ".\ClangPowerTools.vsix" -fileName "extension.vsixmanifest" -oldString $result["init"] -newString $result["final"]
	ReplaceContentInFile -extensionName ".\ClangPowerTools.vsix" -fileName "manifest.json" -oldString $result["init"] -newString $result["final"]
	ReplaceContentInFile -extensionName ".\ClangPowerTools.vsix" -fileName "catalog.json" -oldString $result["init"] -newString $result["final"]

	
	#replace versions in ClangPowerTools2022
	ReplaceContentInFile -extensionName ".\ClangPowerTools2022.vsix" -fileName  "extension.vsixmanifest" -oldString $result["init"] -newString $result["final"]
	ReplaceContentInFile -extensionName ".\ClangPowerTools2022.vsix" -fileName "manifest.json" -oldString $result["init"] -newString $result["final"]
	ReplaceContentInFile -extensionName ".\ClangPowerTools2022.vsix" -fileName "catalog.json" -oldString $result["init"] -newString $result["final"]

	return $result
}


$errorActionPreference = "Stop"

Set-Location $loc

Write-Output "Updating manifest for 2022-vsix starting from ($loc)\ClangPowerTools.vsix..."
If (!(Test-Path .\ClangPowerTools.vsix))
{
  Write-Error "Source VSIX does not exist. Aborting..."
}

Copy-Item .\ClangPowerTools.vsix .\ClangPowerTools2022.vsix

#replace id
ReplaceContentInFile -extensionName ".\ClangPowerTools2022.vsix" -fileName "extension.vsixmanifest" -oldString "<DisplayName>Clang Power Tools</DisplayName>" -newString "<DisplayName>Clang Power Tools 2022</DisplayName>"
ReplaceContentInFile -extensionName ".\ClangPowerTools2022.vsix" -fileName "extension.vsixmanifest" -oldString "Caphyon.705559db-5755-43fa-a023-41a3b14d2935" -newString "Caphyon.9ce239f2-d27a-432c-906c-1d55a123dbfd"
ReplaceContentInFile -extensionName ".\ClangPowerTools2022.vsix" -fileName "manifest.json" -oldString "Caphyon.705559db-5755-43fa-a023-41a3b14d2935" -newString "Caphyon.9ce239f2-d27a-432c-906c-1d55a123dbfd"
ReplaceContentInFile -extensionName ".\ClangPowerTools2022.vsix" -fileName "catalog.json" -oldString "Caphyon.705559db-5755-43fa-a023-41a3b14d2935" -newString "Caphyon.9ce239f2-d27a-432c-906c-1d55a123dbfd"

$versions = Get-IncrementedRevisionVersion -extensionName ".\ClangPowerTools.vsix" -filename "manifest.json"


explorer $loc
