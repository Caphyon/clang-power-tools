param($repoDir)

###
### This script can remove #include directives from CPP files
### It will automatically find project files and process them
###
### Requirement: Add PS alias "cpt" for "clang-build.ps1"
###
$ErrorActionPreference = "Continue"

cd $repoDir

# -----------------------------------------------
# Generate PCH data
Write-Output "Generating PCHs..."
if ($true) # false if you want to skip
{
  $env:CPT_PCH_LIMIT = 0
  &"cpt" -aGenerateOnlyPCH
}

# -----------------------------------------------
# Generate proj data
if ($true) # false if you want to skip
{
  Write-Output "Generating project data..."
  &"cpt" -aGenerateCppList > cpt-data.txt
}

$totalPruned = 0
$totalProcessedFiles = 0

# -----------------------------------------------
# Parse project Data
[string[]] $projTextData = Get-Content cpt-data.txt 

[System.Collections.Hashtable] $allFiles = @{}
$workCount = 0
$crtProj = ""
foreach ($line in $projTextData)
{
  if ($line -match "PROJECT #")
  {
    $crtProj = [regex]::match($line,'PROJECT #\d+: (.+?) \[').Groups[1].Value
  }
  elseif ($line -match '\d+:' )
  {
    $file = [regex]::match($line,'\d+: (.+)').Groups[1].Value
    if (!$allFiles.ContainsKey($file))
    {     
      $allFiles[$file] = @($crtProj)
      $workCount++
    }
    else 
    {
      $allFiles[$file] += @($crtProj)
    }
  }
}
Write-Output "Will need to process $workCount cpp files"

Function Process-ProjectWorkerResultBH($compileResult)
{
  $global:totalPruned += $compileResult.PrunedCount
  $global:totalProcessedFiles++

  $tSeconds = ([DateTime]$compileResult.TimeEnd - [DateTime]$compileResult.TimeBegin).TotalSeconds
  $logMsg = "$($compileResult.File) T = $($tSeconds)s PrunedHeaders = $($compileResult.PrunedCount) PrunedHeadersSoFar = $($global:totalPruned) ProcessedFilesSoFar = $($global:totalProcessedFiles)" 
  Write-Output $logMsg
  $logMsg | Out-File header-prune.log -Append
}
Function Wait-AndProcessBuildWorkerJobsBH([switch]$any)
{
  $runningJobs = @(Get-Job -state Running)

  if ($any)
  {
    $runningJobs | Wait-Job -Any > $null
  }
  else
  {
    $runningJobs | Wait-Job > $null
  }

  $jobData = Get-Job -State Completed
  foreach ($job in $jobData)
  {
    $buildResult = Receive-Job $job
    Process-ProjectWorkerResultBH -compileResult $buildResult
  }

  Remove-Job -State Completed
}

Function Wait-ForJobSlotWorkerBH()
{
  # We allow as many background workers as we have logical CPU cores
  $runningJobs = @(Get-Job -State Running)

  if ($runningJobs.Count -ge $Env:NUMBER_OF_PROCESSORS)
  {
    Wait-AndProcessBuildWorkerJobsBH -any
  }
}

function RunWorkers($workerJobs)
{
  $jobWorkToBeDone = `
  {
    param( $job )

    cd $job.RepoDir

    $timeBegin = [DateTime]::Now
    $prunedCount = (.\headerWorkers.ps1 -repoDir $job.RepoDir `
                                        -file $job.File `
                                        -compileCmd $job.CompileCmd)

    return New-Object PsObject -Prop @{ "File" = $job.File
                                      ; "PrunedCount" = [int]$prunedCount
                                      ; "TimeBegin" = $timeBegin
                                      ; "TimeEnd" = [DateTime]::Now
                                      }
  }

  foreach ($job in $workerJobs)
  {
    Wait-ForJobSlotWorkerBH

    Start-Job -ScriptBlock  $jobWorkToBeDone `
              -ArgumentList $job `
              -ErrorAction Continue > $null
  }
  Wait-AndProcessBuildWorkerJobsBH
}
# -----------------------------------------------
# Start work

[System.Collections.Hashtable] $cmdLineHash = @{}

$workers = @()

$currentFileCount = $allFiles.Keys.Count
foreach ($cpp in $allFiles.Keys)
{
  Write-Output "Creating workers for file $currentFileCount : $cpp..."
  $projects = $allFiles[$cpp]

  $compileCommands = @()
  
  foreach ($crtProj in $projects)
  {
    $compileCommand = ""

    if ($cmdLineHash.ContainsKey($crtProj))
    {
      $compileCommand = $cmdLineHash[$crtProj]
    }
    else 
    {
      try {
        remove-item cpt-command.txt
        &"cpt" -proj $crtProj -aGenerateCompileCmdline | Out-Null
        $compileCommand = Get-Content cpt-command.txt
        $cmdLineHash[$crtProj] = $compileCommand
      }
      catch {
        <#Do this if a terminating exception happens#>
        Write-Error $_
      }
    }

    $compileCommands += @($compileCommand)
  }

  $newWorker = New-Object PsObject -Prop @{ 'File'       = $cpp
                                          ; 'CompileCmd' = $compileCommands
                                          ; 'RepoDir'    = $repoDir
                                          }
  $workers += $newWorker
  $currentFileCount--
}

Write-Output "Starting work..."
RunWorkers $workers
