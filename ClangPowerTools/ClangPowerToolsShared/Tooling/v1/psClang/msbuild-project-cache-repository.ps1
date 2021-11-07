Set-Variable -name kCptCacheRepo -value "$env:APPDATA\ClangPowerTools\CacheRepository" -option Constant

Function Is-CacheLoadingEnabled()
{
  # if the cache repository directory exists, then we use caching
  return (Test-Path $kCptCacheRepo)
}

Function Remove-CachedProjectFile([Parameter(Mandatory = $false)][string] $aCachedFilePath)
{
  if ($aCachedFilePath.StartsWith($kCptCacheRepo))
  {
    Remove-Item $aCachedFilePath | Out-Null
  }
}

Function Load-CacheRepositoryIndex()
{
  [System.Collections.Hashtable] $cacheIndex = @{}
  if (Is-CacheLoadingEnabled)
  {
    [string] $cptCacheRepoIndex = "$kCptCacheRepo\index.dat"
    if (Test-Path $cptCacheRepoIndex)
    {
      $cacheIndex = [System.Management.Automation.PSSerializer]::Deserialize((Get-Content $cptCacheRepoIndex))
    }
  }
  return $cacheIndex
}

Function Save-CacheRepositoryIndex([System.Collections.Hashtable] $cacheIndex)
{
  if (!  (Is-CacheLoadingEnabled) )
  {
    return
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
  if (!  (Is-CacheLoadingEnabled) )
  {
    return
  }

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
  
  [string] $pathToSave = "$kCptCacheRepo\$(Get-RandomString).dat"
  $serialized = [System.Management.Automation.PSSerializer]::Serialize($dataToSerialize)
  $serialized > $pathToSave

  $projHash = (Get-FileHash $MSBuildProjectFullPath)
  
  [System.Collections.Hashtable] $cacheIndex = Load-CacheRepositoryIndex

  $cacheObject = New-Object PsObject -Prop @{ "ProjectFile"            = $MSBuildProjectFullPath
                                            ; "ProjectHash"            = $projHash.Hash
                                            ; "CachedDataPath"         = $pathToSave
                                            ; "ConfigurationPlatform"  = Join-ConfigurationPlatformScriptArgs
                                            ; "CptCacheSyntaxVersion"  = $kCacheSyntaxVer
                                            }
  $cacheIndex[$MSBuildProjectFullPath] = $cacheObject
  
  Save-CacheRepositoryIndex $cacheIndex
}

Function Load-ProjectFromCache([Parameter(Mandatory = $true)][string] $aVcxprojPath)
{    
  if (!  (Is-CacheLoadingEnabled) )
  {
    return $false
  }

  [System.Collections.Hashtable] $cacheIndex = Load-CacheRepositoryIndex
  if ( ! $cacheIndex.ContainsKey($aVcxprojPath))
  {
    return $false
  }

  $projectCacheObject = $cacheIndex[$aVcxprojPath]

  if ( ! (Test-Path $projectCacheObject.CachedDataPath))
  {
    return $false
  }
  
  if ($projectCacheObject.CptCacheSyntaxVersion -ne $kCacheSyntaxVer)
  {
    # the cached version uses an outdated syntax, safely discard it
    Remove-CachedProjectFile $projectCacheObject.CachedDataPath
    return $false
  }
  
  $projHash = (Get-FileHash $aVcxprojPath)
  if ($projectCacheObject.ProjectHash -ne $projHash.Hash)
  {
    # project file hash not matching, safely discard cached version
    Remove-CachedProjectFile $projectCacheObject.CachedDataPath
    return $false
  }

  if ($projectCacheObject.ConfigurationPlatform -ne (Join-ConfigurationPlatformScriptArgs))
  {
    # config-platform not maching, safely discard cached version
    Remove-CachedProjectFile $projectCacheObject.CachedDataPath
    return $false
  }

  # Clean global variables that have been set by a previous project load
  Clear-Vars

  $global:vcxprojPath = $aVcxprojPath

  [string] $data = Get-Content $projectCacheObject.CachedDataPath
  $deserialized = [System.Management.Automation.PSSerializer]::Deserialize($data)

  [System.Collections.Hashtable] $projectSpecificVariablesMap = $deserialized.ProjectSpecificVariables
  [System.Collections.Hashtable] $genericVariablesMap = $deserialized.GenericVariables

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

  return $true
}
