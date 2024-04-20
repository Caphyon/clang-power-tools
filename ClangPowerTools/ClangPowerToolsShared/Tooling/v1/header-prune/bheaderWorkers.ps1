param($repoDir, $file, [string[]] $compileCmd=@())

$ErrorActionPreference = "Continue"

cd $repoDir

[int] $prunedCount = 0
function invokeCPT($file, [string[]] $compileCmd)
{  
  foreach ($command in $compileCmd)
  {
    $commandToRun = $command -replace 'c\+\+ ".+?"', "c++ ""$file"""

     $callOutput = cmd /c "$commandToRun 2>&1" | Out-String

    [bool] $success = $LASTEXITCODE -eq 0
    if (!$success)
    {
      return $false
    }
  }
  return $true
}

function duplicateFile($sourceLines, $destPath, $linesToSkip)
{
  $output = ""
  $crtLine = 0
  foreach ($line in $sourceLines)
  {
    if ($linesToSkip -contains $crtLine)
    {
      $crtLine++
      continue 
    }

    if ($crtLine -eq 0)
    {
      $output += "$line"
    }  
    else 
    {
      $output += "`r`n$line"
    }
    $crtLine++
  }
  $output > $destPath
}

function processFile($file, $compileCmd)
{
  [string[]] $fileLines = Get-Content $file
  [string[]] $includes = @()

  $cppShortName = [regex]::match($file,'\\(\w+).cpp').Groups[1].Value

  $includesLineMap = @{}
  for ($i = 0; $i -le $fileLines.Count; ++$i)
  {
    $line = $fileLines[$i]
    if ($line -match '#include "' -and ($line -ne '#include "stdafx.h"') -and ($line -ne "#include ""$($cppShortName).h"""))
    {
      $includes += @($line)
      $includesLineMap[$line] = $i
    }
  }

  $linesToExclude = @()
  foreach ($include in $includes)
  {
    duplicateFile $fileLines $file ($linesToExclude + $includesLineMap[$include])
    $res = invokeCPT -file $file -compileCmd $compileCmd
    if ($res)
    {
      $linesToExclude += @($includesLineMap[$include])
      Write-Verbose "Excluding $include"
      $global:prunedCount++
    }
    else 
    {
      Write-Verbose "Keeping $include"
    }
  }

  duplicateFile $fileLines $file $linesToExclude
}

processFile -file $file -compileCmd $compileCmd

Write-Output ($global:prunedCount)
