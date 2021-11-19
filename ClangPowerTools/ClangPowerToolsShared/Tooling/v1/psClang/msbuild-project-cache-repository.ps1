Set-Variable -name kCptCacheRepo -value "$env:APPDATA\ClangPowerTools\CacheRepository" -option Constant

Function Is-CacheLoadingEnabled()
{
  if ("${Env:CPT_CACHEREPO}" -eq "0")
  {
    return $false
  }
  # if the cache repository directory exists, then we use caching
  return (Test-Path $kCptCacheRepo)
}

Function Remove-CachedProjectFile([Parameter(Mandatory = $true)][string] $aCachedFilePath)
{
  if ($aCachedFilePath.StartsWith($kCptCacheRepo))
  {
    Remove-Item $aCachedFilePath | Out-Null
  }
}

Function Get-CacheRepositoryIndex()
{
  Write-Verbose "Loading project cache repository index"
  [System.Collections.Hashtable] $cacheIndex = @{}
  if (Is-CacheLoadingEnabled)
  {
    [string] $cptCacheRepoIndex = "$kCptCacheRepo\index.dat"
    if (Test-Path $cptCacheRepoIndex)
    {
      try
      {
        $cacheIndex = [System.Management.Automation.PSSerializer]::Deserialize((Get-Content $cptCacheRepoIndex))
      }
      catch
      {
        Write-Verbose "Error: Could not deserialize corrupted cache repository index. Rebuilding from scratch..."
        return $cacheIndex
      }
    }
  }
  return $cacheIndex
}

Function Save-CacheRepositoryIndex([Parameter(Mandatory = $true)][System.Collections.Hashtable] $cacheIndex)
{
  Write-Verbose "Saving project cache repository index"
  if (!  (Is-CacheLoadingEnabled) )
  {
    return
  }

  [System.Collections.ArrayList] $indexKeys = @()
  foreach ($keyIndex in $cacheIndex.Keys)
  {
    # make sure we don't invalidate the cacheIndex.Keys collection iterators when modifying it
    $indexKeys.Add($keyIndex) > $null
  }

  foreach ($indexKey in $indexKeys)
  {
    [string] $CachedDataPath = $cacheIndex[$indexKey].CachedDataPath
    if (! (Test-Path $CachedDataPath))
    {
      Write-Verbose "Pruning zombie entry ($($indexKey) : $($CachedDataPath)) from cache repository index"
      $cacheIndex.Remove($indexKey) > $null
    }
  }

  [string] $cptCacheRepoIndex = "$kCptCacheRepo\index.dat"
  $serialized = [System.Management.Automation.PSSerializer]::Serialize($cacheIndex)
  $serialized > $cptCacheRepoIndex
}

Function Join-ConfigurationPlatformScriptArgs()
{
  return ($aVcxprojConfigPlatform -join ",")
}

Function Save-ProjectToCacheRepo()
{
  Write-Verbose "Saving current project data to cache repository"
  if (!  (Is-CacheLoadingEnabled) )
  {
    return
  }

  Write-Verbose "Collecting $($global:ProjectSpecificVariables.Count) project specific variables"
  [System.Collections.Hashtable] $projectVariablesMap = @{}
  foreach ($varName in $global:ProjectSpecificVariables)
  {
    $projectVariablesMap[$varName] = Get-Variable -name $varName -ValueOnly
  }
  
  [System.Collections.Hashtable] $genericVariablesMap = @{}
  $genericVariablesMap['cptVisualStudioVersion'  ] = Get-Variable 'cptVisualStudioVersion'   -scope Global -ValueOnly
  $genericVariablesMap['cptCurrentConfigPlatform'] = Get-Variable 'cptCurrentConfigPlatform' -scope Global -ValueOnly

  $dataToSerialize = New-Object PsObject -Prop @{ "ProjectSpecificVariables"  = $projectVariablesMap
                                                ; "GenericVariables"          = $genericVariablesMap
                                                }
  [string] $pathToSave = ""
  while ($true)
  {
    [string] $pathToSave = "$kCptCacheRepo\$(Get-RandomString).dat"
    # make sure we don't overwrite an already existing cache entry
    if (! (Test-Path $pathToSave))
    {
      break
    }
  }
  $serialized = [System.Management.Automation.PSSerializer]::Serialize($dataToSerialize)
  $serialized > $pathToSave
  Write-Verbose "Wrote project to cache repository using moniker $pathToSave"
  
  [System.Collections.Hashtable] $projectFilesHashes = @{}
  foreach ($projectFile in $global:ProjectInputFiles)
  {
    $projectFilesHashes[$projectFile] = (Get-FileHash $projectFile -Algorithm "SHA1").Hash
  }
  
  [System.Collections.Hashtable] $cacheIndex = Get-CacheRepositoryIndex

  $cacheObject = New-Object PsObject -Prop @{ "ProjectFilesHashes"     = $projectFilesHashes
                                            ; "CachedDataPath"         = $pathToSave
                                            ; "ConfigurationPlatform"  = Join-ConfigurationPlatformScriptArgs
                                            ; "CptCacheSyntaxVersion"  = $kCacheSyntaxVer
                                            }
  $cacheIndex[$MSBuildProjectFullPath] = $cacheObject
  
  Save-CacheRepositoryIndex $cacheIndex
}

Function Load-ProjectFromCache([Parameter(Mandatory = $true)][string] $aVcxprojPath)
{
  Write-Verbose "Trying to load project $aVcxprojPath from cache repository"
  if (!  (Is-CacheLoadingEnabled) )
  {
    Write-Verbose "Cache repository not enabled in %APPDATA%/ClangPowerTools/CacheRepository"
    return $false
  }

  [System.Collections.Hashtable] $cacheIndex = Get-CacheRepositoryIndex
  if ( ! $cacheIndex.ContainsKey($aVcxprojPath))
  {
    Write-Verbose "Cache repository does not contain record of project"
    return $false
  }

  $projectCacheObject = $cacheIndex[$aVcxprojPath]

  if ( ! (Test-Path $projectCacheObject.CachedDataPath))
  {
    Write-Verbose "Error: Cache repository contains record of project but cached file no longer exists"
    return $false
  }
  
  if ($projectCacheObject.CptCacheSyntaxVersion -ne $kCacheSyntaxVer)
  {
    Write-Verbose "Cached version of project uses older syntax version. Discarding..."
    # the cached version uses an outdated syntax, safely discard it
    Remove-CachedProjectFile $projectCacheObject.CachedDataPath
    return $false
  }
  
  [System.Collections.Hashtable] $projectFilesHashes = $projectCacheObject.ProjectFilesHashes
  foreach ($projectFilePath in $projectFilesHashes.Keys)
  {
    Write-Verbose "Checking hash of project file $projectFilePath"

    $newFileHash = (Get-FileHash $projectFilePath -Algorithm "SHA1").Hash

    if ($newFileHash -ne $projectFilesHashes[$projectFilePath])
    {
      Write-Verbose "Cached version of project has different file hash. Discarding..."
      # project file hash not matching, safely discard cached version
      Remove-CachedProjectFile $projectCacheObject.CachedDataPath
      return $false
    }
  }

  if ($projectCacheObject.ConfigurationPlatform -ne (Join-ConfigurationPlatformScriptArgs))
  {
    Write-Verbose "Cached version of project uses different configuration platform. Discarding..."
    # config-platform not maching, safely discard cached version
    Remove-CachedProjectFile $projectCacheObject.CachedDataPath
    return $false
  }

  # Clean global variables that have been set by a previous project load
  Clear-Vars

  $global:vcxprojPath = $aVcxprojPath

  Write-Verbose "Loading cached project from $($projectCacheObject.CachedDataPath)"
  [string] $data = Get-Content $projectCacheObject.CachedDataPath
  $deserialized = $null
  try 
  {
    $deserialized = [System.Management.Automation.PSSerializer]::Deserialize($data)
  }
  catch
  {
    Write-Verbose "Error: Failure to deserialize cached project data. Discarding corrupted file"
    Remove-CachedProjectFile $projectCacheObject.CachedDataPath
    return $false
  }

  [System.Collections.Hashtable] $projectSpecificVariablesMap = $deserialized.ProjectSpecificVariables
  [System.Collections.Hashtable] $genericVariablesMap = $deserialized.GenericVariables

  Write-Verbose "Cached version of project has $($projectSpecificVariablesMap.Count) variables to load"
  if ($projectSpecificVariablesMap.Keys -notcontains "MSBuildThisFileFullPath")
  {
    Write-Verbose "Cached project does not contain MSBuildThisFileFullPath. Discarding..."
    Remove-CachedProjectFile $projectCacheObject.CachedDataPath
    return $false
  }
  
  if ( $projectSpecificVariablesMap['MSBuildThisFileFullPath'] -ne $aVcxprojPath )
  {
    Write-Verbose "Cached project looks to be a different project ($($projectSpecificVariablesMap['MSBuildThisFileFullPath'])). Discarding..."
    Remove-CachedProjectFile $projectCacheObject.CachedDataPath
    return $false
  }

  foreach ($var in $projectSpecificVariablesMap.Keys)
  {
    Set-Var -name $var -value $projectSpecificVariablesMap[$var]
  }
  
  # these variables should be garbage collected between projects
  # using our custom Set-Var would allow that to happen
  foreach ($var in $genericVariablesMap.Keys)
  {
    Set-Variable -name $var -value $genericVariablesMap[$var] -scope Global
  }

  Write-Verbose "Cache repository - project load was successful"
  return $true
}
