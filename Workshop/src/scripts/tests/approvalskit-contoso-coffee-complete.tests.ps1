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
        $user = (Get-SecureValue "DEMO_USER")
        $user.Length | Should -BeGreaterThan 0
        [System.String]::IsNullOrEmpty($user) | Should -BeFalse
    }

    It 'Can find development environment' {
        $user = (Get-SecureValue "DEMO_USER")
        $Environment = Invoke-UserDevelopmentEnvironment $user
        [System.String]::IsNullOrEmpty($Environment.EnvironmentUrl) | Should -BeFalse
    }

    It 'Contoso Coffee And Approval Kit Installed' {
        $user = (Get-SecureValue "DEMO_USER")
        $Environment = Invoke-UserDevelopmentEnvironment $user
        $installed = Invoke-SolutionInstalled $Environment "ContosoCoffee"
        $installed | Should -BeTrue
        
        $installed = Invoke-SolutionInstalled $Environment "BusinessApprovalKit"
        $installed | Should -BeTrue
    }

    It 'Approval Kit Machine Requests process created and published' {
        $user = (Get-SecureValue "DEMO_USER")
        $Environment = Invoke-UserDevelopmentEnvironment $user

        $match = Get-ApprovalsKitProcess $Environment "Machine Requests"

        $match.value.length | Should -BeGreaterThan 0

        $match = Get-ApprovalsKitPublishedVersion $Environment "Machine Requests"

        $match.value.length | Should -BeGreaterThan 0
    }

    It 'Contoso Coffee Approvals Solution Created' {
        $state = Get-ApprovalKitContosoCoffeeState
        
        $state.Solution.value.count | Should -Be 1

        $state.SolutionComponents.value.count | Should -BeGreaterThan 1
    }

    It 'Contoso Coffee Approvals Two Stage Approval' -Tag EndState {
        $state = Get-ApprovalKitContosoCoffeeState
        
        $state.Solution.value.length | Should -Be 1

        $state.SolutionComponents.value.length | Should -BeGreaterThan 1

        $match = Get-ApprovalsKitPublishedVersion $state.Environment "Machine Requests"

        $match.value.length | Should -BeGreaterOrEqual 2

        if ( $match.value.length -ge 2 ) {
            $user = $state.User
            $environmentUrl = $state.Environment.EnvironmentUrl
            $environmentId = $state.Environment.EnvironmentId
            Invoke-ValidateTwoStageMachineRequestApproval -UserUPN $user -EnvironmentUrl $environmentUrl -EnvironmentId $environmentId
        }
    }
}