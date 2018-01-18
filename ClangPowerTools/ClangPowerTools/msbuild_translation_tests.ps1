# Tests for ClangPowerTools MSBUILD Expression/Condition translation

$Configuration = "Release2"
$Platform      = "Win32"
$UserRootDir   = "c:\test"
$SolutionDir   = "C:\AI Trunk\ClangPowerToolsProblem"
$ProjectDir    = "C:\AI Trunk\win"
$TargetName    = "YOLOTest"
$varB          = 1

# -------------------------------------------------------------------------------------------------------------------

Set-Variable -name "kMsbuildExpressionToPsRules" -option Constant `
             -value    @(<# backticks are control characters in PS, replace them #>
                         ('`'                     , ''''                 )`
                         <# Temporarily replace     $( #>                 `
                       , ('\$\s*\('               , '!@#'                )`
                         <# Escape $                   #>                 `
                       , ('\$'                    , '`$'                 )`
                         <# Put back $(                #>                 `
                       , ('!@#'                   , '$('                 )`
                         <# Various operators          #>                 `
                       , ("([\s\)\'""])!="        , '$1 -ne '            )`
                       , ("([\s\)\'""])<="        , '$1 -le '            )`
                       , ("([\s\)\'""])>="        , '$1 -ge '            )`
                       , ("([\s\)\'""])=="        , '$1 -eq '            )`
                       , ("([\s\)\'""])<"         , '$1 -lt '            )`
                       , ("([\s\)\'""])>"         , '$1 -gt '            )`
                       , ("([\s\)\'""])or"        , '$1 -or '            )`
                       , ("([\s\)\'""])and"       , '$1 -and '           )`
                         <# Use only double quotes #>                     `
                       , ("\'"                    , '"'                  )`
      , ("Exists\((.*?)\)(\s|$)"           , '(Exists($1))$2'            )`
      , ("HasTrailingSlash\((.*?)\)(\s|$)" , '(HasTrailingSlash($1))$2'  )`
      , ("(\`$\()(Registry:)(.*?)(\))"     , '$$(GetRegValue("$3"))'     )`
                       )

Set-Variable -name "kMsbuildConditionToPsRules"  -option Constant `
             -value   @(<# Use only double quotes #>                     `
                         ("\'"                    , '"'                 )`
                        <# We need to escape double quotes since we will eval() the condition #> `
                       , ('"'                     , '""'                )`
                       )

function GetRegValue([Parameter(Mandatory=$true)][string] $regPath)
{
  [int] $separatorIndex = $regPath.IndexOf('@')
  [string] $valueName = ""
  if ($separatorIndex -gt 0)
  {
    [string] $valueName = $regPath.Substring($separatorIndex + 1)
    $regPath = $regPath.Substring(0, $separatorIndex)
  }
  if ([string]::IsNullOrEmpty($valueName))
  {
    throw "Cannot retrieve an empty registry value"
  }
  $regPath = $regPath -replace "HKEY_LOCAL_MACHINE\\", "HKLM:\"

  if (Test-Path $regPath)
  {
    return (Get-Item $regPath).GetValue($valueName)
  }
  else
  {
    return ""
  }
}

function HasTrailingSlash([Parameter(Mandatory=$true)][string] $str)
{
  return $str.EndsWith('\') -or $str.EndsWith('/')
}

function Exists([Parameter(Mandatory=$false)][string] $path)
{
  if ([string]::IsNullOrEmpty($path))
  {
    return $false
  }
  return Test-Path $path
}

function Evaluate-MSBuildExpression([string] $expression, [switch] $isCondition)
{  
  Write-Debug "Start evaluate MSBuild expression $expression"

  foreach ($rule in $kMsbuildExpressionToPsRules)
  {
    $expression = $expression -replace $rule[0], $rule[1]
  }
  
  if ( !$isCondition -and ($expression.IndexOf('$') -lt 0))
  {
    # we can stop here, further processing is not required
    return $expression
  }
  
  [int] $expressionStartIndex = -1
  [int] $openParantheses = 0
  for ([int] $i = 0; $i -lt $expression.Length; $i += 1)
  {
    if ($expression.Substring($i, 1) -eq '(')
    {
      if ($i -gt 0 -and $expressionStartIndex -lt 0 -and $expression.Substring($i - 1, 1) -eq '$')
      {
        $expressionStartIndex = $i - 1
      }

      if ($expressionStartIndex -ge 0)
      {
        $openParantheses += 1
      }
    }

    if ($expression.Substring($i, 1) -eq ')'  -and $expressionStartIndex -ge 0)
    {
      $openParantheses -= 1
      if ($openParantheses -lt 0)
      {
        throw "Parse error"
      }
      if ($openParantheses -eq 0)
      {
        [string] $content = $expression.Substring($expressionStartIndex + 2, 
                                                  $i - $expressionStartIndex - 2)
        [int] $initialLength = $content.Length

        if ([regex]::Match($content, "[a-zA-Z_][a-zA-Z0-9_\-]+").Value -eq $content)
        {
          # we have a plain property retrieval
          $content = "`${$content}"
        }
        else
        {
          # dealing with a more complex expression
          $content = $content -replace '(^|\s+|\$\()([a-zA-Z_][a-zA-Z0-9_]+)(\.|\)|$)', '$1$$$2$3'
        }

        $newCond = $expression.Substring(0, $expressionStartIndex + 2) + 
                   $content + $expression.Substring($i)
        $expression = $newCond
        
        $i += ($content.Length - $initialLength)
        $expressionStartIndex = -1
      }
    }
  }

  Write-Debug "Intermediate PS expression: $expression"

  try
  {
    [string] $toInvoke = "(`$s = ""$expression"")"
    if ($isCondition)
    {
      $toInvoke = "(`$s = ""`$($expression)"")"
    }

    $res = Invoke-Expression $toInvoke
  }
  catch
  {
    write-debug $_.Exception.Message
  }

  Write-Debug "Evaluated expression to: $res"

  return $res
}

function Evaluate-MSBuildCondition([Parameter(Mandatory=$true)][string] $condition)
{
  Write-Debug "Evaluating condition $condition"
  foreach ($rule in $kMsbuildConditionToPsRules)
  {
    $condition = $condition -replace $rule[0], $rule[1]
  }
  $expression = Evaluate-MSBuildExpression -expression $condition -isCondition

  if ($expression -ieq "true")
  {
    return $true
  } 

  if ($expression -ieq "false")
  {
    return $false
  }

  [bool] $res = $false 
  try
  {
    $res = (Invoke-Expression $expression) -eq $true
  }
  catch
  {
    Write-Debug $_.Exception.Message
  }
  Write-Debug "Evaluated condition to $res" 

  return $res
}

Clear-Host

function Test-Condition([string] $condition, [bool]$expectation, [switch] $expectFailure)
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

function Test-Expression($expresion)
{
    $res = Evaluate-MSBuildExpression $expresion
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

Test-Condition -condition   "exists('c:\ai trunk')"`
               -expectation $true

Test-Condition -condition   "'`$(Configuration)|`$(Platform)'=='Release|Win32'"`
               -expectation $false
 
Test-Condition -condition    '$(Platform.Replace(" ", "")) and $(testB)'`
               -expectation $false

Test-Condition -condition    '$(Platform) and $(varB)'`
               -expectation $true

Test-Condition -condition    "exists('`$(UserRootDir)\Microsoft.Cpp.`$(Platform).user.props')"`
               -expectation $true

Test-Expression -expression  "`$(SolutionDir)\Tools\PropertySheets\Evolution.Module.props"
Test-Expression -expresion "WIN32_LEAN_AND_MEAN and `$(Configuration)"

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