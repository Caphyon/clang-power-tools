#Console IO
# ------------------------------------------------------------------------------------------------
Function Write-Message([parameter(Mandatory = $true)][string] $msg
    , [Parameter(Mandatory = $true)][System.ConsoleColor] $color)
{
    $foregroundColor = $host.ui.RawUI.ForegroundColor
    $host.ui.RawUI.ForegroundColor = $color
    Write-Output $msg
    $host.ui.RawUI.ForegroundColor = $foregroundColor
}

# Writes an error without the verbose PowerShell extra-info (script line location, etc.)
Function Write-Err([parameter(ValueFromPipeline, Mandatory = $true)][string] $msg)
{
    Write-Message -msg $msg -color Red
}

Function Write-Success([parameter(ValueFromPipeline, Mandatory = $true)][string] $msg)
{
    Write-Message -msg $msg -color Green
}

Function Write-Verbose-Array($array, $name)
{
    Write-Verbose "$($name):"
    $array | ForEach-Object { Write-Verbose "  $_" }
}

Function Print-InvocationArguments()
{
    $bParams = $PSCmdlet.MyInvocation.BoundParameters
    if ($bParams)
    {
        [string] $paramStr = "clang-build.ps1 invocation args: `n"
        foreach ($key in $bParams.Keys)
        {
            $paramStr += "  $($key) = $($bParams[$key]) `n"
        }
        Write-Verbose $paramStr
    }
}

# File IO
# ------------------------------------------------------------------------------------------------
Function Remove-PathTrailingSlash([Parameter(Mandatory = $true)][string] $path)
{
    return $path -replace '\\$', ''
}

Function Get-FileDirectory([Parameter(Mandatory = $true)][string] $filePath)
{
    return ([System.IO.Path]::GetDirectoryName($filePath) + "\")
}

Function Get-FileName( [Parameter(Mandatory = $true)][string] $path
    , [Parameter(Mandatory = $false)][switch] $noext)
{
    if ($noext)
    {
        return ([System.IO.Path]::GetFileNameWithoutExtension($path))
    }
    else
    {
        return ([System.IO.Path]::GetFileName($path))
    }
}

Function IsFileMatchingName( [Parameter(Mandatory = $true)][string] $filePath
    , [Parameter(Mandatory = $true)][string] $matchName)
{
    if ([System.IO.Path]::IsPathRooted($matchName))
    {
        return $filePath -ieq $matchName
    }

    [string] $fileName = (Get-FileName -path $filePath)
    [string] $fileNameNoExt = (Get-FileName -path $filePath -noext)
    if ($aDisableNameRegexMatching)
    {
        return (($fileName -eq $matchName) -or ($fileNameNoExt -eq $matchName))
    }
    else
    {
        return (($fileName -match $matchName) -or ($fileNameNoExt -match $matchName))
    }
}

# Command IO
# ------------------------------------------------------------------------------------------------
Function Exists-Command([Parameter(Mandatory = $true)][string] $command)
{
    try
    {
        Get-Command -name $command -ErrorAction Stop
        return $true
    }
    catch
    {
        return $false
    }
}
