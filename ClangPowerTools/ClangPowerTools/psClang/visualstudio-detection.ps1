# ------------------------------------------------------------------------------------------------
# Helpers for locating Visual Studio on the computer

# VsWhere is available starting with Visual Studio 2017 version 15.2.
Set-Variable -name   kVsWhereLocation `
    -value  "${Env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" #`
#-option Constant

# Default installation path of Visual Studio 2017. We'll use when VsWhere isn't available.
Set-Variable -name   kVs15DefaultLocation `
    -value  "${Env:ProgramFiles(x86)}\Microsoft Visual Studio\$global:cptVisualStudioVersion\$aVisualStudioSku" #`
#-option Constant

Function Get-MscVer()
{
    return ((Get-Item "$(Get-VisualStudio-Path)\VC\Tools\MSVC\" | Get-ChildItem) | select -last 1).Name
}

Function Get-VisualStudio-Includes([Parameter(Mandatory = $true)][string]  $vsPath,
    [Parameter(Mandatory = $false)][string] $mscVer)
{
    [string] $mscVerToken = ""
    If (![string]::IsNullOrEmpty($mscVer))
    {
        $mscVerToken = "Tools\MSVC\$mscVer\"
    }

    return @( "$vsPath\VC\$($mscVerToken)include"
        , "$vsPath\VC\$($mscVerToken)atlmfc\include"
    )
}

Function Get-VsWhere-VisualStudio-Version()
{
    switch ($global:cptVisualStudioVersion)
    {
        "2013"  { return "[12.0, 13)" }
        "2015"  { return "[14.0, 15)" }
        "2017"  { return "[15.0, 16)" }
        "2019"  { return "[16.0, 17)" }
        default { throw "Unsupported Visual Studio version: $cptVisualStudioVersion" }
    }
}

Function Get-VisualStudio-VersionNumber([Parameter(Mandatory = $true)][string]  $vsYearVersion)
{
    switch ($vsYearVersion)
    {
        "2013"  { return "12.0" }
        "2015"  { return "14.0" }
        "2017"  { return "15.0" }
        "2019"  { return "16.0" }
        default { throw "Unsupported Visual Studio version: $vsYearVersion" }
    }
}

# Newer Visual Studio versions support installing older toolset versions, for compatibility reasons.
# Returns default instalation path of the current VS version/toolset.
Function Get-VisualStudio-CompatiblityToolset-InstallLocation()
{
    return "${Env:ProgramFiles(x86)}\Microsoft Visual Studio " + (Get-VisualStudio-VersionNumber $global:cptVisualStudioVersion)
}

Function Get-VisualStudio-RegistryLocation()
{
    return "HKLM:SOFTWARE\Wow6432Node\Microsoft\VisualStudio\" + (Get-VisualStudio-VersionNumber $global:cptVisualStudioVersion)
}

Function Get-VisualStudio-Path()
{
    # Depending of the version of Visual Studio, we have different approaches to locating it.

    if ( ([int] $global:cptVisualStudioVersion) -le 2015 )
    {
        # Older Visual Studio (<= 2015). VSWhere is not available.

        [string] $installLocation = (Get-Item (Get-VisualStudio-RegistryLocation)).GetValue("InstallDir")
        if ($installLocation)
        {
            $installLocation = Canonize-Path -base $installLocation -child "..\.." -ignoreErrors
        }
        if ($installLocation)
        {
            return $installLocation
        }

        # we may have a newer VS installation with an older toolset feature installed
        [string] $toolsetDiskLocation = (Get-VisualStudio-CompatiblityToolset-InstallLocation)
        [string] $iostreamLocation = Canonize-Path -base toolsetDiskLocation `
                                                   -child "VC\include\iostream" -ignoreErrors
        if ($iostreamLocation)
        {
            return $toolsetDiskLocation
        }

        Write-Err "Visual Studio $($global:cptVisualStudioVersion) installation location could not be detected"
    }
    else
    {
        # modern Visual Studio (> 2017). Use VSWhere to locate it.
        if (Test-Path $kVsWhereLocation)
        {

            [string] $product = "*"
            if (![string]::IsNullOrEmpty($aVisualStudioSku))
            {
              $product = "Microsoft.VisualStudio.Product.$aVisualStudioSku"
            }

            [string] $version = Get-VsWhere-VisualStudio-Version
            [string[]] $output = (& "$kVsWhereLocation" -nologo `
                                                        -property installationPath `
                                                        -products $product `
                                                        -version $version `
                                                        -prerelease)

            # the -prerelease switch is not available on older VS2017 versions
            if (($output -join "").Contains("0x57")) <# error code for unknown parameter #>
            {
                $output = (& "$kVsWhereLocation" -nologo `
                                                 -property installationPath `
                                                 -version $version `
                                                 -products $product)
            }

            if (!$output)
            {
                throw "VsWhere could not detect Visual Studio $($global:cptVisualStudioVersion) $product."
            }

            [string] $installationPath = $output[0]
            Write-Verbose "Detected (vswhere) VisualStudio installation path: $installationPath"
            return $installationPath
        }

        if (Test-Path -Path $kVs15DefaultLocation)
        {
            return $kVs15DefaultLocation
        }

        throw "Cannot locate Visual Studio $($global:cptVisualStudioVersion)"
    }
}
