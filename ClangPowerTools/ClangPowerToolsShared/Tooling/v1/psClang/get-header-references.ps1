# line limit for scanning files for #include
[int] $global:cpt_header_include_line_limit = 30

# after the line limit, if any includes are still found we
# extend the limit with this value
[int] $global:cpt_header_include_line_extension = 10

[string[]] $global:headerExtensions = @('h', 'hh', 'hpp', 'hxx')
[string[]] $global:sourceExtensions = @('c', 'cc', 'cpp', 'cxx')

Function detail:FindHeaderReferences( [Parameter(Mandatory = $false)] [string[]] $headers
                                    , [Parameter(Mandatory = $false)] [System.IO.FileInfo[]] $filePool
                                    , [Parameter(Mandatory = $false)] [System.Collections.Hashtable] $alreadyFound = @{}
                                    )
{
    if (!$headers)
    {
        return @()
    }

    [string] $regexHeaders = @($headers | ForEach-Object { ([System.IO.FileInfo]$_).BaseName } `
                                       | Select-Object -Unique `
                                       | Where-Object { $_ -ine "stdafx" -and $_ -ine "resource" } `
                              ) -join '|'

    if ($regexHeaders.Length -eq 0)
    {
        return @()
    }

    [string] $regex = "[/""]($regexHeaders)\.($($global:headerExtensions -join '|'))"""
    Write-Debug "Regex for header reference find: $regex`n"

    [string[]] $returnRefs = @()
    if (!$filePool)
    {
        # initialize pool of files that we look into
        [string[]] $allFileExts = @(($global:sourceExtensions + `
                                     $global:headerExtensions) | ForEach-Object { "*.$_" })
        $filePool = Get-ChildItem -recurse -include $allFileExts
    }

    foreach ($file in $filePool)
    {
        if ($alreadyFound.ContainsKey($file.FullName))
        {
            continue
        }

        [int] $lineCount = 0
        [int] $lineLimit = $global:cpt_header_include_line_limit
        foreach($line in [System.IO.File]::ReadLines($file))
        {
            if ([string]::IsNullOrWhiteSpace($line))
            {
                # skip empty lines
                continue
            }

            if ($line -match $regex)
            {
                if ( ! $alreadyFound.ContainsKey($file.FullName))
                {
                    $alreadyFound[$file.FullName] = $true
                    $returnRefs += $file.FullName
                }

                if ($lineCount -eq $lineLimit)
                {
                    # we still have includes to scan
                    $lineLimit += $global:cpt_header_include_line_extension
                }
            }

            if ( (++$lineCount) -gt $lineLimit)
            {
                break
            }
        }
    }

    if ($returnRefs.Count -gt 0)
    {
        [string[]] $headersLeftToSearch = @($returnRefs | Where-Object `
                                            { FileHasExtension -filePath $_ `
                                                               -ext $global:headerExtensions } )
        if ($headersLeftToSearch.Count -gt 0)
        {
            Write-Debug "[!] Recursive reference detection in progress for: "
            Write-Debug ($headersLeftToSearch -join "`n")
            $returnRefs += detail:FindHeaderReferences -headers      $headersLeftToSearch `
                                                       -filePool     $filePool `
                                                       -alreadyFound $alreadyFound
        }
    }

    $returnRefs = $returnRefs | Select-Object -Unique
    Write-Debug "Found header refs (regex $regex)"
    Write-Debug ($returnRefs -join "`n")
    return $returnRefs
}

<#
.SYNOPSIS
Detects source files that reference given headers.

Returns an array with full paths of files that reference the header(s).
.DESCRIPTION
When modifying a header, all translation units that include that header
have to compiled. This function detects those files that include it.
.PARAMETER files
Header files of which we want references to be found
Any files that are not headers will be ignored.
#>
Function Get-HeaderReferences([Parameter(Mandatory = $false)][string[]] $files)
{
    if ($files.Count -eq 0)
    {
        return @()
    }

    # we take interest only in files that reference headers
    $files = @($files | Where-Object { FileHasExtension -filePath $_ `
                                                        -ext $global:headerExtensions })
    if ($files.Count -eq 0)
    {
        return @()
    }

    [string[]] $refs = @()

    if ($files.Count -gt 0)
    {
        Write-Verbose-Timed "Headers changed. Detecting which source files to process..."
        $refs = detail:FindHeaderReferences -headers $files
        Write-Verbose-Timed "Finished detecting source files."

        $refs = $refs | Where-Object { ! [string]::IsNullOrWhiteSpace($_) }
    }

    return $refs
}

<#
.SYNOPSIS
Detects projects that reference given source files (i.e. cpps).

Returns an array with full paths of detected projects.
.DESCRIPTION
When modifying a file, only projects that reference that file should be recompiled.
.PARAMETER projectPool
Projects in which to look
.PARAMETER files
Source files to be found in projects.
#>
Function Get-SourceCodeIncludeProjects([Parameter(Mandatory = $false)][System.IO.FileInfo[]] $projectPool,
                                       [Parameter(Mandatory = $false)][string[]] $files)
{
    [System.Collections.Hashtable] $fileCache = @{}
    foreach ($file in $files)
    {
        if ($file)
        {
            $fileCache[$file.Trim().ToLower()] = $true
        }
    }

    [System.IO.FileInfo[]] $matchedProjects = @()

    [string] $clPrefix    = '<ClCompile Include="'
    [string] $clSuffix    = '" />'
    [string] $endGroupTag = '</ItemGroup>'

    foreach ($proj in $projectPool)
    {
        [string] $projDir  = $proj.Directory.FullName

        [bool] $inClIncludeSection = $false
        foreach($line in [System.IO.File]::ReadLines($proj.FullName))
        {
            $line = $line.Trim()

            if ($line.StartsWith($clPrefix))
            {
                if (!$inClIncludeSection)
                {
                    $inClIncludeSection = $true
                }

                [string] $filePath = $line.Substring($clPrefix.Length, `
                                                     $line.Length - $clPrefix.Length - $clSuffix.Length)
                if (![System.IO.Path]::IsPathRooted($filePath))
                {
                    $filePath = Canonize-Path -base $projDir -child $filePath -ignoreErrors
                }
                if ([string]::IsNullOrEmpty($filePath))
                {
                    continue
                }

                [System.IO.FileInfo] $sourceFile = $filePath
                if ($fileCache.ContainsKey($sourceFile.FullName.Trim().ToLower()) -or `
                    $fileCache.ContainsKey($sourceFile.Name.Trim().ToLower()))
                {
                    $matchedProjects += $proj
                    break
                }
            }

            if ($inClIncludeSection -and $line -eq $endGroupTag)
            {
                # nothing more to check in this project
                break
            }
        }
    }

    return $matchedProjects
}
