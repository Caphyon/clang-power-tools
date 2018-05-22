Clear-Host

# Tests for ClangPowerTools MSBUILD Expression/Condition translation

$Configuration = "Release2"
$Platform      = "Win32"
$UserRootDir   = "c:\test"
$WinDir        = "C:\Windows"
$ProjectDir    = "C:\Users\Default"
$TargetName    = "YOLOTest"
$varB          = 1

# -------------------------------------------------------------------------------------------------------------------

@( "$kScriptLocation\..\code\helper-functions.ps1"
 ) | ForEach-Object { . $_ }

Clear-Host

function Test-Condition([Parameter(Mandatory=$true)][string] $condition, [bool]$expectation, [switch] $expectFailure)
{
    [boolean] $condValue
    try
    {
      $condValue = Evaluate-MSBuildCondition $condition
    }
    catch
    {
      if ($expectFailure)
      {
        Write-Output "TEST OK"
        return
      }
      else
      {
        Write-Output $_.Exception.Message
        throw "Test failed"
      }
    }

    if ($condValue -ne $expectation)
    {
      Write-Output "Expected $expectation | Got $condValue"
      throw "Test failed"
    }
    Write-Output "TEST OK"
}

function Test-Expression([Parameter(Mandatory=$true)] [string] $expression)
{
    $res = Evaluate-MSBuildExpression $expression
    Write-output $res
}
# ----------------------------------------------------------------------------

Test-Condition "'`$(ImportDirectoryBuildProps)' == 'true' and exists('`$(DirectoryBuildPropsPath)')" -expectation $false

Test-Expression '$(Registry:HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\VisualStudio\15.0\AD7Metrics\ExpressionEvaluator\{3A12D0B7-C26C-11D0-B442-00A0244A1DD2}\{994B45C4-E6E9-11D2-903F-00C04FA302A1}@LoadInShimManagedEE)'
Test-Expression '$(Registry:HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SDKs\Windows\v10.0@InstallationFolder)'
Test-Expression '$(Registry:HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Microsoft SDKs\Windows\v10.0@InstallationFolder)'

Test-Expression '$(Registry:HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SDKs\Windows\v7.1A@InstallationFolder)'
Test-Expression '$(GetRegValue("HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SDKs\Windows\v10.0@InstallationFolder"))'


Test-Condition "'`$(Registry:HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SDKs\Windows\v7.1A@InstallationFolder)' != ''" `
               -expectation $true

Test-Condition "'`$(Registry:HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\DevDiv\vs\Servicing\11.0\professional@Version)' == ''" `
               -expectation $true


Test-Condition -condition   "'`$(Configuration)|`$(Platform)'=='Debug|Win32' or '`$(Configuration)' == 'Release2'" `
               -expectation $true

Test-Condition -condition   "'`$(Platform)'=='x64' or '`$(Platform)'=='Win32' or '`$(Platform)'=='Durango' or exists('`$(UserRootDir)\Microsoft.Cpp.`$(Platform).user.props2')"`
               -expectation $true

Test-Condition -condition   "exists('c:\windows')"`
               -expectation $true

Test-Condition -condition   "exists('`$(WinDir)')"`
               -expectation $true

Test-Condition -condition   "'`$(Configuration)|`$(Platform)'=='Release|Win32'"`
               -expectation $false

Test-Condition -condition    '$(Platform.Replace(" ", "")) and $(testB)'`
               -expectation $false

Test-Condition -condition    '$(Platform) and $(varB)'`
               -expectation $true

Test-Condition -condition    "exists('`$(UserRootDir)\Microsoft.Cpp.`$(Platform).user.props')"`
               -expectation $true

Test-Expression -expression  "`$(WinDir)\System32"
Test-Expression -expression "WIN32_LEAN_AND_MEAN and `$(Configuration)"

Test-Condition  -condition  "exists('`$([Microsoft.Build.Utilities.ToolLocationHelper]::GetPlatformExtensionSDKLocation(``WindowsMobile, Version=10.0.10240.0``, `$(TargetPlatformIdentifier), `$(TargetPlatformVersion), `$(SDKReferenceDirectoryRoot), `$(SDKExtensionDirectoryRoot), `$(SDKReferenceRegistryRoot)))\DesignTime\CommonConfiguration\Neutral\WindowsMobile.props')"`
                -expectFailure

Test-Expression -expression "`$Foo;`$(ProjectDir);..\..;..\..\third-party"

Test-Condition -condition    "`$(TargetName.EndsWith('Test'))"`
               -expectation  $true

Test-Condition -condition    "`$(TargetName.EndsWith('Test2'))"`
               -expectation  $false

$var = 4
Test-Condition  -condition    '$(var) == 2 and 4 == 4'`
                -expectation  $false

Test-Expression -expression "%(ASDASD);`$(TargetName)"

$PkgMicrosoft_Gsl = "..\.."
Test-Condition -condition    "Exists('`$(PkgMicrosoft_Gsl)\build\native\Microsoft.Gsl.targets') OR ! Exists('`$(PkgMicrosoft_Gsl)\build\native\Microsoft.Gsl.targets')"`
               -expectation  $true

$myVar = 'TwoThree'
$MySelector = "One;Two;Three"
Test-Condition -condition    "`$(MySelector.Contains(`$(myVar.Substring(3, 3))))"`
               -expectation  $true

$MySelector = "One;Two;Three"
$myVar = "Two"
Test-Condition -condition    "`$(MySelector.Contains(`$(myVar)))"`
               -expectation  $true

$MySelector = "One;Two;Three"
Test-Condition -condition    "`$(MySelector.Contains(Three))"`
               -expectFailure

$MySelector = "One;Two;Three"
Test-Condition -condition    "`$(MySelector.Contains('Three'))"`
               -expectation  $true

Test-Condition -condition    "`$([System.DateTime]::Now.Year) == 2018"`
               -expectation  $true

Test-Condition -condition    "HasTrailingSlash('c:\windows\')"`
               -expectation  $true

Test-Condition -condition    "HasTrailingSlash('c:\windows\') and hasTrailingSlash('c:\temp/')"`
               -expectation  $true

$prop = "c:\windows\"
Test-Condition -condition    "hasTrailingSlash(`$(prop))"`
               -expectation  $true

Test-Expression -expression '"$(prop)"'

$MSBuildThisFileDirectory = "C:\windows"
Test-Expression -expression '$([MSBuild]::GetDirectoryNameOfFileAbove($(MSBuildThisFileDirectory), ''Program Files'')Program Files'

Test-Expression -expression '$([MSBuild]::GetDirectoryNameOfFileAbove($(MSBuildThisFileDirectory), "Program Files")Program Files'

$whatToFind = "Program Files"
Test-Expression -expression '$([MSBuild]::GetDirectoryNameOfFileAbove($(MSBuildThisFileDirectory), ''$(whatToFind)'')Program Files'

Test-Expression -expression '$([MSBuild]::GetDirectoryNameOfFileAbove($(MSBuildThisFileDirectory), Program Files)Program Files'