param(
    [string]$OutFile = "input_checks.txt",
    [string]$Url     = "https://clang.llvm.org/extra/clang-tidy/checks/list.html"
)

# Make sure TLS is OK on older PowerShells
[Net.ServicePointManager]::SecurityProtocol =
    [Net.SecurityProtocolType]::Tls12 -bor [Net.SecurityProtocolType]::Tls13

# Download HTML
$response = Invoke-WebRequest -Uri $Url
$html = $response.Content

# Regex:
# <a class="reference internal" ...>
#   <span class="doc">CHECK-NAME</span>
# </a>
$pattern = '<a[^>]*class=["'']reference internal["''][^>]*>\s*<span[^>]*class=["'']doc["''][^>]*>([^<]+)</span>\s*</a>'

$matches = [regex]::Matches(
    $html,
    $pattern,
    [System.Text.RegularExpressions.RegexOptions]::IgnoreCase
)

$checks = @()

foreach ($m in $matches) {
    $name = $m.Groups[1].Value.Trim()
    if ([string]::IsNullOrWhiteSpace($name)) { continue }

    # Names can contain letters, digits, -, _, .
    if ($name -match '^[A-Za-z0-9_.-]+$') {
        $checks += $name
    }
}

# Sort & dedupe
$checks = $checks | Sort-Object -Unique

# Write to file
$checks | Set-Content -Path $OutFile -Encoding UTF8

Write-Host "$($checks.Count) checks written to '$OutFile'"


#-------------------------------------------------------------- Format here --------------------------------------------------------------------------------------------------------

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
        $uniqueChecks[$check] = "new TidyCheckModel { Name = `"$check`", IsChecked = false },"
    }
}

# Sort the unique checks alphabetically again
$sortedUniqueChecks = $uniqueChecks.Values | Sort-Object

# Write sorted unique checks to output file
$sortedUniqueChecks | Out-File $outputFilePath

Write-Host "Formatted unique checks written to $outputFilePath"
