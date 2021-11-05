# Powershell code for creating item definition group contexts

[System.Collections.Hashtable] $global:itemProperties = @{}
[string] $global:itemPropertyNamespace = ""

function Reset-ProjectItemContext()
{
    $global:itemProperties = @{}
    $global:itemPropertyNamespace = ""
}

function Push-ProjectItemContext([Parameter(Mandatory = $true)][string] $name)
{
    [string] $toAdd = ""
    if ($global:itemPropertyNamespace.Length -gt 0)
    {
        $toAdd = ".";
    }
    $toAdd += $name

    $global:itemPropertyNamespace += $toAdd

    Write-Verbose "[CONTEXT] item namespace = @($global:itemPropertyNamespace)"
}

function Pop-ProjectItemContext()
{
    [int] $dotPos = $global:itemPropertyNamespace.LastIndexOf(".")
    if ($dotPos -ge 0)
    {
        $global:itemPropertyNamespace = $global:itemPropertyNamespace.Substring(0, $dotPos)
    }
    else
    {
        $global:itemPropertyNamespace = ""
    }

    Write-Verbose "[CONTEXT] item namespace = @($global:itemPropertyNamespace)"
}

function Set-ProjectItemContext([Parameter(Mandatory = $true)][AllowEmptyString()][string] $name)
{
    if ( (VariableExists 'itemPropertyNameSpace') -and ($global:itemPropertyNamespace -eq $name) )
    {
        return
    }

    $global:itemPropertyNamespace = $name
    Write-Verbose "[CONTEXT] item namespace = @($global:itemPropertyNamespace)"
}

function Get-ProjectItemContext()
{
    return $global:itemPropertyNamespace
}

function Get-ProjectItemProperty([Parameter(Mandatory = $false)][string] $propertyName)
{
    if (! $global:itemProperties.ContainsKey($global:itemPropertyNamespace))
    {
        return $null
    }

    [System.Collections.Hashtable] $propMap = $global:itemProperties[$global:itemPropertyNamespace]
    if (!$propertyName)
    {
        return $propMap
    }

    if (! $propMap.ContainsKey($propertyName))
    {
        return $null
    }

    return $propMap[$propertyName]
}

function Set-ProjectItemProperty([Parameter(Mandatory = $true)][string] $propertyName,
                                 [Parameter(Mandatory = $true)] $value)
{
    if (! $global:itemProperties.ContainsKey($global:itemPropertyNamespace))
    {
        $global:itemProperties.Add($global:itemPropertyNamespace, @{})
        $global:ProjectSpecificVariables.Add('itemProperties');
    }

    [System.Collections.Hashtable] $propMap = $global:itemProperties[$global:itemPropertyNamespace]
    if (! $propMap.ContainsKey($propertyName))
    {
        $propMap.Add($propertyName, $value)
    }
    else
    {
        $propMap[$propertyName] = $value
    }

    Write-Verbose "[CONTEXT] propSet: $propertyName = $value"
}
