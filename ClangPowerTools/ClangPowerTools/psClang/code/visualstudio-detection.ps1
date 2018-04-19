# ------------------------------------------------------------------------------------------------
# Helpers for locating Visual Studio on the computer

# VsWhere is available starting with Visual Studio 2017 version 15.2.
Set-Variable -name   kVsWhereLocation `
             -value  "${Env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" `
             -option Constant

# Default installation path of Visual Studio 2017. We'll use when VsWhere isn't available.
Set-Variable -name   kVs15DefaultLocation `
             -value  "${Env:ProgramFiles(x86)}\Microsoft Visual Studio\$aVisualStudioVersion\$aVisualStudioSku" `
             -option Constant

# Registry key containing information about Visual Studio 2015 installation path.
Set-Variable -name   kVs2015RegistryKey `
             -value  "HKLM:SOFTWARE\Wow6432Node\Microsoft\VisualStudio\14.0" `
             -option Constant

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

Function Get-VisualStudio-Path()
{
    if ($aVisualStudioVersion -eq "2015")
    {
        $installLocation = (Get-Item $kVs2015RegistryKey).GetValue("InstallDir")
        return Canonize-Path -base $installLocation -child "..\.."
    }
    else
    {
        if (Test-Path $kVsWhereLocation)
        {
            [string] $product = "Microsoft.VisualStudio.Product.$aVisualStudioSku"
            [string] $output = (& "$kVsWhereLocation" -nologo `
                    -property installationPath `
                    -products $product `
                    -prerelease)

            # the -prerelease switch is not available on older VS2017 versions
            if ($output.Contains("0x57")) <# error code for unknown parameter #>
            {
                $output = (& "$kVsWhereLocation" -nologo `
                        -property installationPath `
                        -products $product)
            }

            return $output
        }

        if (Test-Path -Path $kVs15DefaultLocation)
        {
            return $kVs15DefaultLocation
        }

        throw "Cannot locate Visual Studio location"
    }
}