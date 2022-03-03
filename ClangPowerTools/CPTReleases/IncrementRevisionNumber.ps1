param([Parameter(Mandatory=$true, HelpMessage="Increments revision number in manifest file")][string] $loc)

#Get version from xml and increment revision value
$filepath = "$loc\..\ClangPowerTools\source.extension.vsixmanifest"
$filepathToAip = "$loc\..\ClangPowerTools.aip"
if((Test-Path $filepath) -and (Test-Path $filepathToAip))
{
    #Get xml data from manifest file
    [xml] $data = Get-Content $filepath
    $currentVersion = [Version]::new($data.PackageManifest.Metadata.Identity.Version.ToString())
    $nextVersion = [Version]::new($currentVersion.Major, $currentVersion.Minor, $currentVersion.Build, $currentVersion.Revision + 1)
    
    #Get xml data from aip file
    [xml] $aipData = Get-Content $filepathToAip
		
    #Replace old version with new one in manifest
    $data.PackageManifest.Metadata.Identity.Version = $nextVersion.ToString()
    $data.Save("$filepath")

    #Replace old version with new one in aip file
    $aipData.DOCUMENT.COMPONENT[0].ROW[5].Value = $nextVersion.ToString()
    $aipData.Save("$filepathToAip")
    $resultData = Get-Content $filepathToAip
    $result = $resultData -replace " />", "/>"
    $result > $filepathToAip
}
else 
{
    Write-Error "Invalid manifest file path"    
}


