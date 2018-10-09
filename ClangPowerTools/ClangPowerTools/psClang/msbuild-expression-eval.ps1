# REQUIRES io.ps1 to be included

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
        , ("([\s\)\'""])!="                  , '$1  -ne '                )`
        , ("([\s\)\'""])<="                  , '$1  -le '                )`
        , ("([\s\)\'""])>="                  , '$1  -ge '                )`
        , ("([\s\)\'""])=="                  , '$1  -eq '                )`
        , ("([\s\)\'""])<"                   , '$1 -lt '                 )`
        , ("([\s\)\'""])>"                   , '$1 -gt '                 )`
        , ("([\s\)\'""])or"                  , '$1 -or '                 )`
        , ("([\s\)\'""])and"                 , '$1 -and '                )`
        <# Use only double quotes #>                                      `
        , ("\'"                              , '"'                       )`
        , ("Exists\((.*?)\)(\s|$)"           , '(Exists($1))$2'          )`
        , ("HasTrailingSlash\((.*?)\)(\s|$)" , '(HasTrailingSlash($1))$2')`
        , ("(\`$\()(Registry:)(.*?)(\))"     , '$$(GetRegValue("$3"))'   )`
        , ("\[MSBuild\]::GetDirectoryNameOfFileAbove\((.+?),\s*`"?'?((\$.+?\))|(.+?))((|`"|')\))+"`
        ,'GetDirNameOfFileAbove -startDir $1 -targetFile ''$2'')'        )`
        , ("\[MSBuild\]::MakeRelative\((.+?),\s*""?'?((\$.+?\))|(.+?))((|""|')\)\))+"`
        ,'MakePathRelative -base $1 -target "$2")'                       )`
)

Set-Variable -name "kMsbuildConditionToPsRules" <#-option Constant#>      `
             -value   @(<# Use only double quotes #>                      `
                       ,("\'"                , '"'                       )`
)

function GetDirNameOfFileAbove( [Parameter(Mandatory = $true)][string] $startDir
                              , [Parameter(Mandatory = $true)][string] $targetFile
                              )
{
    if ($targetFile.Contains('$'))
    {
        $targetFile = Invoke-Expression $targetFile
    }

    [string] $base = $startDir
    while ([string]::IsNullOrEmpty((Canonize-Path -base  $base        `
                    -child $targetFile  `
                    -ignoreErrors)))
    {
        $base = [System.IO.Path]::GetDirectoryName($base)
        if ([string]::IsNullOrEmpty($base))
        {
            return ""
        }
    }
    return $base
}

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
        if ($expression[$i] -eq '(')
        {
            if ($i -gt 0 -and $expressionStartIndex -lt 0 -and $expression[$i - 1] -eq '$')
            {
                $expressionStartIndex = $i - 1
            }

            if ($expressionStartIndex -ge 0)
            {
                $openParantheses += 1
            }
        }

        if ($expression[$i] -eq ')' -and $expressionStartIndex -ge 0)
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

    $expression = $expression.replace('"', '""')
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
function Evaluate-MSBuildCondition([Parameter(Mandatory = $true)][string] $condition)
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
