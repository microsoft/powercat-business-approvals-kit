<#  
.SYNOPSIS  
    Pester Integration tests for Approvals Kit workshop
.DESCRIPTION  
    Ensure that you have Install-Module of 
.NOTES  
    File Name  : approvalskit-install.tests.ps1  
    Author     : Grant Archibald
    Requires   : 
        PowerShell Core 7.1.3 or greater
        az cli 2.39.0 or greater
        pac cli 1.27.6+g9f62afb or greater

Copyright (c) Microsoft Corporation. All rights reserved.
Licensed under the MIT License.
#>

BeforeAll {
    . $PSScriptRoot\..\test.ps1
}

Describe 'Approval Kit Install Tests' {
    It 'Can Validate Demo User' -Tag "Install" {
        $user = (Get-SecureValue "DEMO_USER")
        $result = Invoke-ValidateEnvironment $user
        $result.connectorCount | Should -Be 1
        $result.clientId | Should -Be (Get-SecureValue "CLIENT_ID")
        $result.azureResourceId | Should -Be $result.environmentUrl
        $result.resourceUri | Should -Be $result.environmentUrl
        $result.valid | Should -BeTrue
    }

    It 'Client Id has admin consent for Dataverse' -Tag "Install" {
        $grants = (az ad app permission list-grants --id  (Get-SecureValue CLIENT_ID) --show-resource-name) | ConvertFrom-Json
        ($grants | Where-Object { `
            $_.resourceDisplayName -eq "Dataverse" `
            -and $_.consentType -eq "AllPrincipals" `
            -and $_.scope -eq "user_impersonation" }).Count | Should -Be 1
    }
}