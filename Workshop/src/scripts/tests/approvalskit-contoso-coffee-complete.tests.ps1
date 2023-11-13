<#  
.SYNOPSIS  
    Pester Integration tests for Approvals Kit workshop
.DESCRIPTION  
    Ensure that you have Install-Module of 
.NOTES  
    File Name  : approvalskit-contoso-coffee-complete.tests.ps1  
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

Describe 'Approval Kit Workshop Integration Tests' {
    It 'Has test user defined' {
        $user = (Get-TestConfigValue $PSScriptRoot "TestUser")
        $user.Length | Should -BeGreaterThan 0
        [System.String]::IsNullOrEmpty($user) | Should -BeFalse
    }

    It 'Can find development environment' {
        $user = (Get-TestConfigValue $PSScriptRoot "TestUser")
        $Environment = Invoke-UserDevelopmentEnvironment $user
        [System.String]::IsNullOrEmpty($Environment.EnvironmentUrl) | Should -BeFalse
    }

    It 'Contoso Coffee And Approval Kit Installed' {
        $user = (Get-TestConfigValue $PSScriptRoot "TestUser")
        $Environment = Invoke-UserDevelopmentEnvironment $user
        $installed = Invoke-SolutionInstalled $Environment "ContosoCoffee"
        $installed | Should -BeTrue
        
        $installed = Invoke-SolutionInstalled $Environment "BusinessApprovalKit"
        $installed | Should -BeTrue
    }
}