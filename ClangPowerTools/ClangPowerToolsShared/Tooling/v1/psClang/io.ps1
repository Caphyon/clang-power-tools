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

function Write-InformationTimed($message)
{
  if ($InformationPreference -eq "SilentlyContinue")
  {
    return
  }
  [DateTime] $lastTime = [DateTime]::Now
  [string] $kTimeStampVar = "lastCptTimestamp"
  if (VariableExists -name $kTimeStampVar)
  {
     $lastTime = (Get-Variable -name $kTimeStampVar -scope Global).Value
  }
  Set-Variable -name $kTimeStampVar -scope Global -value ([DateTime]::Now)
  
  [DateTime] $now = [DateTime]::Now;
  [System.TimeSpan] $delta = $now - $lastTime

  Write-Information "$message at $([DateTime]::Now.ToString("mm:ss:fff")). dt = $($delta.TotalMilliseconds)"
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

Function Write-Array($array, $name)
{
    Write-Output "$($name):"
    $array | ForEach-Object { Write-Output "  $_" }
    Write-Output "" # empty line separator
}

Function Write-Verbose-Array($array, $name)
{
  if ($VerbosePreference -eq "SilentlyContinue")
  {
    return
  }
  Write-Verbose "$($name):"
  $array | ForEach-Object { Write-Verbose "  $_" }
  Write-Verbose "" # empty line separator
}

Function Write-Verbose-Timed([parameter(ValueFromPipeline, Mandatory = $true)][string] $msg)
{
    Write-Verbose "$([DateTime]::Now.ToString("[HH:mm:ss]")) $msg"
}

Function Print-InvocationArguments()
{
  if ($VerbosePreference -eq "SilentlyContinue")
  {
    return
  }

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

Function Print-CommandParameters([Parameter(Mandatory = $true)][string] $command)
{
    $params = @()
    foreach ($param in ((Get-Command $command).ParameterSets[0].Parameters))
    {
        if (!$param.HelpMessage)
        {
            continue
        }

        $params += New-Object PsObject -Prop @{ "Option" = "-$($param.Aliases[0])"
                                              ; "Description" = $param.HelpMessage
                                              }
    }

   $params | Sort-Object -Property "Option" | Out-Default
}



# Function that gets the name of a command argument when it is only known by its alias
# For streamlining purposes, it also accepts the name itself.
Function Get-CommandParameterName([Parameter(Mandatory = $true)][string] $command
                                 ,[Parameter(Mandatory = $true)][string] $nameOrAlias)
{
  foreach ($param in ((Get-Command $command).ParameterSets[0].Parameters))
  {
    if ($param.Name    -eq       $nameOrAlias -or
        $param.Aliases -contains $nameOrAlias)
    {
      return $param.Name
    }
  }
  return ""
}
Function VariableExists([Parameter(Mandatory = $true)][string] $name)
{
    if ( ! ( Get-Variable $name  -ErrorAction 'Ignore') )
    {
        return $false
    }
    return $true
}

Function VariableExistsAndNotEmpty([Parameter(Mandatory = $true)][string] $name)
{
    if ( ! (VariableExists $name) )
    {
        return $false
    }

    if ( [string]::IsNullOrWhiteSpace( (Get-Variable $name).Value ) )
    {
        return $false
    }
    return $true
}
function HasProperty($object, $property)
{
    return ($property -in $object.PSobject.Properties.Name)
}

# File IO
# ------------------------------------------------------------------------------------------------
Function Get-QuotedPath([Parameter(Mandatory = $false)][string] $path)
{
    if ([string]::IsNullOrWhiteSpace($path))
    {
        return $path
    }

    [string] $returnPath = $path

    if (!$path.StartsWith('"'))
    {
        $returnPath = """$path"""
    }

    return $returnPath
}

Function Get-UnquotedPath([Parameter(Mandatory = $false)][string] $path)
{
    [string] $retPath = $path
    if ( ! [string]::IsNullOrWhiteSpace($retPath) -and $retPath.StartsWith('"') )
    {
        $retPath = $retPath.Remove(0, 1);

        if ( $retPath.EndsWith('"') )
        {
            $retPath = $retPath.Remove($retPath.Length - 1, 1)
        }
        else 
        {
            # if it begins with double quote, should end with double quote...
            # something else may be going on, return the original path.
            $retPath = $path
        }
    }
    return $retPath
}

Function Remove-PathTrailingSlash([Parameter(Mandatory = $true)][string] $path)
{
    return $path -replace '\\$', ''
}

Function Get-FileDirectory([Parameter(Mandatory = $true)][string] $filePath)
{
    return ([System.IO.Path]::GetDirectoryName($filePath) + "\")
}

Function Get-FileName( [Parameter(Mandatory = $false)][string] $path
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
                           , [Parameter(Mandatory = $true)] $matchName)
{
    [string] $matchString = $matchName.ToString() # works for both strings and regex types

    if ($matchName -is [string])
    {
        if ([System.IO.Path]::IsPathRooted($matchString))
        {
            if ($matchName.Length -le $filePath.Length -and
                ($filePath.Substring(0, $matchName.Length) -ieq $matchName))
            {
              return $true
            }
        }

        [string] $fileName      = (Get-FileName -path $filePath)
        if ($fileName -ieq $matchString)
        {
            return $true
        }

        [string] $fileNameNoExt = (Get-FileName -path $filePath -noext)
        if ($fileNameNoExt -ieq $matchString)
        {
            return $true
        }

        if ($filePath.ToLower().EndsWith($matchName.ToLower()))
        {
            return $true
        }

        while (![string]::IsNullOrWhiteSpace($filePath))
        {
            if ($filePath.ToLower().EndsWith($matchName.ToLower()))
            {
                return $true
            }
            $filePath = [System.IO.Path]::GetDirectoryName($filePath)
        }

        return $false
    }
    elseif ($matchName -is [regex])
    {
        return $filePath -match $matchString
    }
    else
    {
        throw "Unsupported match object type $($matchName.GetType().ToString())"
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

Function Get-RandomString( [Parameter(Mandatory=$false)][int] $aLength = 10)
{
  return (-Join ((65..90) + (97..122) | Get-Random -Count $aLength | % { [char] $_ }))
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
  The relative path(s) to be merged into base. If multiple paths are specified, they can be separated 
  semicolon or space.
  .PARAMETER ignoreErrors
  If this switch is not present, an error will be triggered if the resulting path
  is not present on disk (e.g. c:\Windows\System33).

  If present and the resulting path does not exist, the function returns an empty string.
  #>
Function Canonize-Path( [Parameter(Mandatory = $true)][string] $base
    , [Parameter(Mandatory = $true)][string] $child
    , [switch] $ignoreErrors)
{
    [string[]] $children = @()

    $tokensBySemicolon = $child.Trim().Split(';')

    foreach ($semicolonTok in $tokensBySemicolon)
    {
        if ([string]::IsNullOrWhiteSpace($semicolonTok))
        {
            continue
        }
        $tokensBySpace = $semicolonTok.Trim().Split(' ')
        $currentToken = ""
        foreach ($tok in $tokensBySpace)
        {
            if ($tok -match "[A-Z]:.*")
            {
                if ( ! [string]::IsNullOrWhiteSpace($currentToken))
                {
                    $children += $currentToken
                }
                $currentToken = $tok
            }
            else
            {
                if ($tok -ne $tokensBySpace[0])
                {
                    $currentToken += ' '
                }
                $currentToken += $tok
            }
        }
        $children += $currentToken
    }

    Write-Debug "Canonizing for base = $base and children = $children"

    [string[]] $retPaths = @()
    [string] $errorAction = If ($ignoreErrors) {"SilentlyContinue"} Else {"Stop"}

    if (Test-Path -LiteralPath $base)
    {
        # Join-Path doesn't support LiteralPath so make sure we sanitize
        # the base path for unsupported characters
        $base = $base.Replace('[', '`[');
        $base = $base.Replace(']', '`]');
    }

    foreach ($childPath in $children)
    {
        $childPath = Get-UnquotedPath $childPath

        if ([System.IO.Path]::IsPathRooted($childPath))
        {
            if ((Test-Path -LiteralPath $childPath))
            {
              $retPaths += @($childPath)
            }
        }
        else
        {
            [string[]] $paths = @(Join-Path -Path "$base" -ChildPath "$childPath" -Resolve -ErrorAction $errorAction)
            $retPaths += $paths
        }
    }

    return $retPaths
}

function cpt::HasTrailingSlash([Parameter(Mandatory = $true)][string] $str)
{
    return $str.EndsWith('\') -or $str.EndsWith('/')
}


function EnsureTrailingSlash([Parameter(Mandatory = $true)][string] $str)
{
    [string] $ret = If (cpt::HasTrailingSlash($str)) { $str } else { "$str\" }
    return $ret
}

function cpt::Exists([Parameter(Mandatory = $false)][string] $path)
{
    if ([string]::IsNullOrEmpty($path))
    {
        return $false
    }

    return Test-Path -LiteralPath $path
}

function cpt::MakePathRelative( [Parameter(Mandatory = $true)][string] $base
                         , [Parameter(Mandatory = $true)][string] $target
                         )
{
    Push-Location "$base\"
    [string] $relativePath = (Resolve-Path -Relative $target) -replace '^\.\\',''
    Pop-Location
    if ( (cpt::HasTrailingSlash $target) -or $target.EndsWith('.') )
    {
        $relativePath += '\'
    }
    return "$relativePath"
}


function cpt::GetDirNameOfFileAbove( [Parameter(Mandatory = $true)][string] $startDir
                                   , [Parameter(Mandatory = $true)][string] $targetFile
                                   )
{
    if ($targetFile.Contains('$'))
    {
        $targetFile = Invoke-Expression $targetFile
    }
    if ($startDir.Contains('$'))
    {
        $startDir = Invoke-Expression $startDir
    }

    [string] $base = $startDir
    while ([string]::IsNullOrEmpty((Canonize-Path -base  $base        `
                    -child $targetFile  `
                    -ignoreErrors)))
    {
        $base = [System.IO.Path]::GetDirectoryName($base)
        if ([string]::IsNullOrEmpty($base))
        {
            return ""
        }
    }
    return $base
}

function cpt::GetPathOfFileAbove([Parameter(Mandatory = $true)][string] $targetFile,
                                 [Parameter(Mandatory = $true)][string] $startDir
                                )
{
    if ($targetFile.Contains('$'))
    {
        $targetFile = Invoke-Expression $targetFile
    }

    $base = (cpt::GetDirNameOfFileAbove -targetFile $targetFile -startDir $startDir)
    if ([string]::IsNullOrWhiteSpace($base))
    {
        return ""
    }
    return "$(EnsureTrailingSlash $base)$targetFile"
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

Function Get-ClangVersion()
{
    if (Exists-Command "clang")
    {
        [string] $s = &"clang" --version
        $regexMatch = [regex]::match($s, 'clang version (\d+).')
        if ($regexMatch)
        {
            return ($regexMatch.Groups[1].Value -as [int])
        }
    }
    return 0
}
