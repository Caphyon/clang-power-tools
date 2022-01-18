param([Parameter(Mandatory=$true, HelpMessage="Increments revision number in manifest file")][string] $loc)

cd $loc
cd ..\ClangPowerTools\

#Get version from xml and increment revision value
$fileName = "source.extension.vsixmanifest"
[xml] $data = Get-Content $fileName
$versionString = $data.PackageManifest.Metadata.Identity.Version
$currentVersion = [Version]::new($versionString.ToString())
$revision = $currentVersion.Revision -as [int]
++$revision
$nextVersion = [Version]::new($currentVersion.Major, $currentVersion.Minor, $currentVersion.Build, $revision)
$nextVersion

#Replace old version with new one
$data.PackageManifest.Metadata.Identity.Version = $nextVersion
$PSDefaultParameterValues['Out-File:Encoding'] = 'utf8' 


$data.Save("$pwd\$fileName")
