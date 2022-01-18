param([Parameter(Mandatory=$true, HelpMessage="Increments revision number in manifest file")][string] $loc)

#Get version from xml and increment revision value
$filepath = "$loc\..\ClangPowerTools\source.extension.vsixmanifest"
if(Test-Path $filepath)
{
    [xml] $data = Get-Content $filepath
    $currentVersion = [Version]::new($data.PackageManifest.Metadata.Identity.Version.ToString())
    $nextVersion = [Version]::new($currentVersion.Major, $currentVersion.Minor, $currentVersion.Build, $currentVersion.Revision + 1)

    #Replace old version with new one
    $data.PackageManifest.Metadata.Identity.Version = $nextVersion.ToString()
    $data.Save("$filepath")
}
else 
{
    Write-Error "Invalid manifest file path"    
}


