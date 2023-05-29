function JsonDB-Init()
{
  [string] $outputPath = (EnsureTrailingSlash( Get-SourceDirectory ))
  $outputPath += "compile_commands.json"
  Set-Variable -name "kJsonCompilationDbPath" -value $outputPath -option Constant -scope Global
  Set-Variable -name "kJsonCompilationDbCount" -value 0 -scope Global

  JsonDB-Append "["
}

# Use StreamWriter for utf-8 (without BOM) encoding in powershell and windows powershell
function JsonDB-Append($text)
{
  $Utf8NoBomEncoding = New-Object System.Text.UTF8Encoding($false)
  $stream = [System.IO.File]::Open($kJsonCompilationDbPath, 'Append', 'Write', 'Read')
  $writer = New-Object System.IO.StreamWriter($stream, $Utf8NoBomEncoding)
  $writer.WriteLine($text)
  $writer.Close()
  $stream.Close()
}

function JsonDB-Finalize()
{
  JsonDB-Append "]"
  Write-Output "Exported JSON Compilation Database to $kJsonCompilationDbPath"
}

function JsonDB-Push($directory, $file, $command)
{
  if ($kJsonCompilationDbCount -ge 1)
  {
    JsonDB-Append "  ,"
  }
  
  # use only slashes
  $command = $command.Replace('\', '/')
  $file = $file.Replace('\', '/')
  $directory = $directory.Replace('\', '/')
  
  # escape double quotes
  $command = $command.Replace('"', '\"')
  
  # make paths relative to directory
  $command = $command.Replace("$directory/", "")
  $file = $file.Replace("$directory/", "")
  
  JsonDB-Append "  {`r`n    ""directory"": ""$directory"",`r`n    ""command"": ""$command"",`r`n    ""file"": ""$file""`r`n  }"
   
  Set-Variable -name "kJsonCompilationDbCount" -value ($kJsonCompilationDbCount + 1) -scope Global
}
