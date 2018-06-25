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

Function Write-Verbose-Timed([parameter(ValueFromPipeline, Mandatory = $true)][string] $msg)
{
    Write-Verbose "$([DateTime]::Now.ToString("[HH:mm:ss]")) $msg"
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

Function FileHasExtension( [Parameter(Mandatory = $true)][string]   $filePath
                         , [Parameter(Mandatory = $true)][string[]] $ext
                         )
{
    foreach ($e in $ext)
    {
        if ($filePath.EndsWith($e))
        {
            return $true
        }
    }
    return $false
}

<#
  .DESCRIPTION
  Merges an absolute and a relative file path.
  .EXAMPLE
  Having base = C:\Windows\System32 and child = .. we get C:\Windows
  .EXAMPLE
  Having base = C:\Windows\System32 and child = ..\..\..\.. we get C:\ (cannot go further up)
  .PARAMETER base
  The absolute path from which we start.
  .PARAMETER child
  The relative path to be merged into base.
  .PARAMETER ignoreErrors
  If this switch is not present, an error will be triggered if the resulting path
  is not present on disk (e.g. c:\Windows\System33).

  If present and the resulting path does not exist, the function returns an empty string.
  #>
Function Canonize-Path( [Parameter(Mandatory = $true)][string] $base
    , [Parameter(Mandatory = $true)][string] $child
    , [switch] $ignoreErrors)
{
    [string] $errorAction = If ($ignoreErrors) {"SilentlyContinue"} Else {"Stop"}

    if ([System.IO.Path]::IsPathRooted($child))
    {
        if (!(Test-Path $child))
        {
            return ""
        }
        return $child
    }
    else
    {
        [string[]] $paths = Join-Path -Path "$base" -ChildPath "$child" -Resolve -ErrorAction $errorAction
        return $paths
    }
}

# Command IO
# ------------------------------------------------------------------------------------------------
Function Exists-Command([Parameter(Mandatory = $true)][string] $command)
{
    try
    {
        Get-Command -name $command -ErrorAction Stop | out-null
        return $true
    }
    catch
    {
        return $false
    }
}
