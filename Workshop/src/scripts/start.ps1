<#  
.SYNOPSIS  
    Starting point for powershell based configuation of user
.DESCRIPTION  
    This script provides a sample of deploying demo user related features. To use this file in Powershell . .\users.ps1 
.NOTES  
    File Name  : users.ps1  
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

Invoke-ConfigureUser (Get-SecureValue DEMO_USER)