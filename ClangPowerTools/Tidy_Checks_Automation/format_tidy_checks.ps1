# Will convert bugprone-empty-catch list in new TidyCheckModel { Name = "bugprone-empty-catch", IsChecked = false }, and remove dublicates

# Input and output file paths
$inputFilePath = "input_checks.txt"
$outputFilePath = "output_checks.txt"

# Read checks from input file
$checks = Get-Content $inputFilePath

# Sort the checks alphabetically
$sortedChecks = $checks | Sort-Object

# Create a hashtable to store unique checks
$uniqueChecks = @{}

# Format and add each unique check to the hashtable
foreach ($check in $sortedChecks) {
    if (-not $uniqueChecks.ContainsKey($check)) {
        $uniqueChecks[$check] = "    new TidyCheckModel { Name = `"$check`", IsChecked = false },"
    }
}

# Write formatted checks to output file
$uniqueChecks.Values | Out-File $outputFilePath

Write-Host "Formatted unique checks written to $outputFilePath"
