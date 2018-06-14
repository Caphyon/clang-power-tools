[System.IO.FileInfo[]] $global:filePool = @()

# line limit for scanning files for #include
[int] $global:cpt_header_include_line_limit = 100

[string[]] $global:cppExtensions = @()
[string[]] $global:headerExtensions = @('h', 'hh', 'hpp', 'hxx')
[string[]] $global:sourceExtensions = @('c', 'cc', 'cpp', 'cxx')

Function detail:FindHeaderReferences( [Parameter(Mandatory = $false)] [string[]] $files
                                    , [Parameter(Mandatory = $false)][System.Collections.Hashtable] $alreadyFound = @{}
                                    )
{
    if ($files.Count -eq 0)
    {
        return @()
    }
    
    [string] $regexHeaders = ($files | ForEach-Object { ([System.IO.FileInfo]$_).BaseName } `
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
    if ($global:filePool.Count -eq 0)
    {
        [string[]] $allFileExts = ($global:sourceExtensions + `
                                   $global:headerExtensions) | ForEach-Object { "*.$_" }
        $global:filePool = Get-ChildItem -recurse -include $allFileExts
    }

    foreach ($file in $filePool)
    {
        if ($alreadyFound.ContainsKey($file.FullName))
        {
            continue
        }
        
        [int] $lineCount = 0
        foreach($line in [System.IO.File]::ReadLines($file))
        {
            $lineCount += 1
            if ($lineCount -gt $global:cpt_header_include_line_limit)
            {
                break
            }

            if ($line -match $regex -and !$alreadyFound.ContainsKey($file.FullName))
            {
                $alreadyFound[$file.FullName] = $true
                $returnRefs += $file.FullName
                break
            }
        }
    }

    if ($returnRefs.Count -gt 0)
    {
        [string[]] $headersLeftToSearch = ($returnRefs | Where-Object `
                                          { FileHasExtension -filePath $_ `
                                                             -ext $global:headerExtensions } )
        if ($headersLeftToSearch.Count -gt 0)
        {
            Write-Debug "[!] Recursive reference detection in progress for: "
            Write-Debug ($headersLeftToSearch -join "`n")
            $returnRefs += detail:FindHeaderReferences -files        $headersLeftToSearch `
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
.RETURNS 
#>
Function Get-HeaderReferences([Parameter(Mandatory = $false)][string[]] $files)
{
    if ($files.Count -eq 0)
    {
        return @()
    }

    # we use this as a cache for storing paths of all headers and files detected
    # in the source directory, will get really large.
    $global:filePool = @()

    # we take interest only in files that reference headers
    $files = $files | Where-Object { FileHasExtension -filePath $_ `
                                                      -ext $global:headerExtensions }

    [string[]] $refs = detail:FindHeaderReferences -file $files

    $global:filePool = @() # free memory
    return $refs
}
