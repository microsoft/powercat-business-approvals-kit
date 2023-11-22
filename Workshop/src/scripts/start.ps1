<#  
.SYNOPSIS  
    Starting point for powershell based configuation setup or reset a developer environment
.DESCRIPTION  
    This script provides a sample how to setup workshop environment
.NOTES  
    File Name  : start.ps1  
    Author     : Grant Archibald - grant.archibald@microsoft.com
    Requires   : 
        az cli 2.39.0 or greater
        pac cli 1.27.6+g9f62afb or greater

Copyright (c) Microsoft Corporation. All rights reserved.
Licensed under the MIT License.
#>

. $PSScriptRoot\users.ps1

$account = (az account show | ConvertFrom-Json )
if ( $NULL -eq $account -or $NULL -eq $account.id ) {
    Invoke-AzureLogin
} else {
    Write-Host "Logged into Azure CLI as $($account.user.name)"
}

if ( -not [System.String]::IsNullOrEmpty($env:INSTALL_USER) ) {
    $domain=(az account show --query "user.name" -o tsv).Split('@')[1]

    $user = $env:INSTALL_USER
    if ( $user.IndexOf("@") -lt 0 ) {
        $user = "$user@$domain"
    }

    $reset = $env:RESET -eq "Y"
    if ( $reset ) {
        Reset-UserDevelopmentEnvironment $user
    } else {
        Invoke-SetupUserForWorkshop $user
    }
} else {
    Write-Host "Environment variable INSTALL_USER not defined"
}
