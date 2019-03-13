# REQUIRES io.ps1 to be included

[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.Build.Utilities.Core") > $null

Set-Variable -name "kMsbuildExpressionToPsRules" <#-option Constant#>     `
    -value @(                                                             `
        <# backticks are control characters in PS, replace them #>        `
          ('`'                               , ''''                      )`
        <# Temporarily replace $( #>                                      `
        , ('\$\s*\('                         , '!@#'                     )`
        <# Escape $ #>                                                    `
        , ('\$'                              , '`$'                      )`
        <# Put back $( #>                                                 `
        , ('!@#'                             , '$('                      )`
        <# Various operators #>                                           `
        , ("([\s\)\'""])!="                  , '$1 -ne '                 )`
        , ("([\s\)\'""])<="                  , '$1 -le '                 )`
        , ("([\s\)\'""])>="                  , '$1 -ge '                 )`
        , ("([\s\)\'""])=="                  , '$1 -eq '                 )`
        , ("([\s\)\'""])<"                   , '$1 -lt '                 )`
        , ("([\s\)\'""])>"                   , '$1 -gt '                 )`
        , ("([\s\)\'""])or"                  , '$1 -or '                 )`
        , ("([\s\)\'""])and"                 , '$1 -and '                )`
        <# Use only double quotes #>                                      `
        , ("\'"                              , '"'                       )`
        , ("Exists\((.*?)\)(\s|$)"           , '(cpt::Exists($1))$2'          )`
        , ("HasTrailingSlash\((.*?)\)(\s|$)" , '(cpt::HasTrailingSlash($1))$2')`
        , ("(\`$\()(Registry:)(.*?)(\))"     , '$$(GetRegValue("$3"))'   )`
        , ("\[MSBuild\]::GetDirectoryNameOfFileAbove\((.+?),\s*`"?'?((\$.+?\))|(.+?))((|`"|')\))+"`
        ,'cpt::GetDirNameOfFileAbove -startDir $1 -targetFile ''$2'')'        )`
        , ("\[MSBuild\]::GetPathOfFileAbove\(`"?(.+?)`"?,\s*`"?'?((\$.+?\))|(.+?))((|`"|')\))+"`
        ,'cpt::GetPathOfFileAbove -startDir $2 -targetFile ''$1'')'        )`
        , ("\[MSBuild\]::MakeRelative\((.+?),\s*""?'?((\$.+?\))|(.+?))((|""|')\)\))+"`
        ,'cpt::MakePathRelative -base $1 -target "$2")'                       )`
        , ('SearchOption\.', '[System.IO.SearchOption]::'                )`
        , ("@\((.*?)\)", '$(Get-Project-Item("$1"))'                     )`
        , ("%\((.*?)\)", '$(Get-ProjectItemProperty("$1"))'              )`
        , ('\$\(HOME\)', '$(CPT_SHIM_HOME)'                              )`
        <# Rules for making sure the $ sign is put on correctly in expressions #> `
        , ('\$\(([a-zA-Z_][a-zA-Z0-9_\-]+)\)', '$${$1}'                  )`
        , ('\(([a-zA-Z_][a-zA-Z0-9_\-]+\.)', '($$$1'                     )`
)

function GetRegValue([Parameter(Mandatory = $true)][string] $regPath)
{
    Write-Debug "REG_READ $regPath"

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

function Evaluate-MSBuildExpression([string] $expression, [switch] $isCondition)
{
    # A lot of MSBuild expressions refer uninitialized variables
    Set-StrictMode -Off

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

    Write-Debug "Intermediate PS expression: $expression"

    [string] $res = ""

    try
    {
        if ( ($expression.IndexOf('::') -ge 0) -or $isCondition)
        {
            try
            {
                $resInvokeResult = Invoke-Expression $expression

                if ($resInvokeResult -is [array])
                {
                    $res = $resInvokeResult -join ';'
                }
                else
                {
                    $res = $resInvokeResult
                }
            }
            catch
            {
                Write-Verbose $_.Exception.Message
                $res = $ExecutionContext.InvokeCommand.ExpandString($expression)
            }
        }
        else
        {
            $res = $ExecutionContext.InvokeCommand.ExpandString($expression)
        }
    }
    catch
    {
        Write-Verbose $_.Exception.Message
    }

    Write-Debug "Evaluated expression to: $res"

    return $res
}
function Evaluate-MSBuildCondition([Parameter(Mandatory = $true)][string] $condition)
{
    Write-Debug "Evaluating condition $condition"

    try
    {
        [string] $expression = Evaluate-MSBuildExpression -expression $condition -isCondition
    }
    catch
    {
        Write-Verbose $_.Exception.Message
        return $false
    }

    if ($expression -ieq "true")
    {
        return $true
    }

    if ($expression -ieq "false")
    {
        return $false
    }

    $expression = $expression -replace 'False', '$false'
    $expression = $expression -replace 'True', '$true'

    try
    {
        [bool] $res = (Invoke-Expression $expression) -eq $true
    }
    catch
    {
        Write-Verbose $_.Exception.Message
        return $false
    }
    Write-Debug "Evaluated condition to $res"

    return $res
}
