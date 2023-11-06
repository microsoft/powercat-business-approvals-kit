<#  
.SYNOPSIS  
    Sample Power Platform Demo common
.DESCRIPTION  
    This script provides a common powershell functionality used by other functions
.NOTES  
    File Name  : common.ps1  
    Author     : Grant Archibald - grant.archibald@microsoft.com
    Requires   : 
        PowerShell Core 7.1.3 or greater
        az cli 2.39.0 or greater
        pac cli 1.27.6+g9f62afb or greater

Copyright (c) Microsoft Corporation. All rights reserved.
Licensed under the MIT License.
#>

<#
    .DESCRIPTION
    This PowerShell function reads a value from a configuration file. It takes one parameter, `$Name`, which specifies the name of the value to read.

    .NOTES
    Assumes that the configuration file is named "config.json" and is located in the current directory.

    .PARAMETER Name
    Specifies the name of the value to read.

    .INPUTS
    None. You can't pipe objects to Get-ConfigValue.

    .OUTPUTS
    Returns the value from the configuration file with the specified name.

    .EXAMPLE
    PS> Get-ConfigValue -Name "ConnectionString"
    Reads the value of the "ConnectionString" setting from the configuration file.

#> 
function Get-ConfigValue {
    param (
        $Name
    )

    $configFile = [System.IO.Path]::Join( ( Get-Location ),"config.json")
    if ( Test-Path $configFile ) {
        $config = Get-Content $configFile | ConvertFrom-Json
        return ( $config | Select-Object -ExpandProperty $Name )
    }

    return $NULL       
}