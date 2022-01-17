param([Parameter(Mandatory=$true, HelpMessage="Location in which to create the 2022 VSIX starting from an existing one")][string] $loc)

function Get-IncrementedRevisionVersion([Parameter(Mandatory=$true)][string] $filename)
{
	$data = Get-Content $filename
	Clear-Variable -Name "Matches" -ErrorAction SilentlyContinue
	$version = 0
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

	$fileContent = Get-Content $fileName
	$fileContent = $fileContent -replace $result["init"], $result["final"]
	$PSDefaultParameterValues['Out-File:Encoding'] = 'utf8' 
	$fileContent > $fileName
	return $result
}

cd $loc
cd ..\ClangPowerTools\

$manifestFile = "source.extension.vsixmanifest"

Get-IncrementedRevisionVersion -filename $manifestFile