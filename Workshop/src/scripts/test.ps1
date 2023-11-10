<#  
.SYNOPSIS  
    Sample Power Platform Demo test scruorts
.DESCRIPTION  
    This script provides a sample of testing deployed instances of Approvals Kit and Contoso Coffee solution
.NOTES  
    File Name  : test.ps1  
    Author     : Grant Archibald - grant.archibald@microsoft.com
    Requires   : 
        PowerShell Core 7.1.3 or greater
        az cli 2.39.0 or greater
        pac cli 1.27.6+g9f62afb or greater

Copyright (c) Microsoft Corporation. All rights reserved.
Licensed under the MIT License.
#>

. $PSScriptRoot\users.ps1

<#
.SYNOPSIS
Validates a two-stage machine request approval with the Contoso Coffee solution.

.PARAMETER UserUPN
The user's UPN (user principal name) to run the validation as.

.PARAMETER EnvironmentName
The name of the environment to run the validation in. If not specified, the user's display name followed by "Dev" will be used.

.EXAMPLE
Invoke-ValidateTwoStageMachineRequestApproval -UserUPN "user@example.com" -EnvironmentName "ContosoCoffee Dev"
Validates a two-stage machine request approval in the "ContosoCoffee Dev" environment.

#>
function Invoke-ValidateTwoStageMachineRequestApproval {
    param (
        [Parameter(Mandatory)] [String] $UserUPN,
        [String] $EnvironmentName
    )
    
    $envs = ( Get-Environments $UserUPN )

    if ( [System.String]::IsNullOrEmpty($EnvironmentName) ) {
        $user = (az ad user list --upn $UserUPN | ConvertFrom-Json)
        $EnvironmentName = $user.displayName + " Dev"
    }

    $devEnv = $envs | Where-Object { $_.Name -eq $EnvironmentName } | Select-Object -First 1

    if ( $NULL -eq $devEnv ) {
        Write-Error "Environment not found"
        return
    }
    
    $dataverse = $devEnv.EnvironmentUrl
    $token = (az account get-access-token --resource=$dataverse --query accessToken --output tsv)

    $solution = (Get-Solution $token $devEnv.EnvironmentUrl "ContosoCoffee")

    if ( $solution.value.count -eq 0 ) {
        Write-Error "Solution ContosoCoffee not found"
        return
    }

    $solutionComponents = (Get-SolutionComponents $token $devEnv.EnvironmentUrl $solution.value[0].solutionid) 

    $environmentId = $devEnv.Id
    $appId = ($solutionComponents.value | Where-Object { $_.msdyn_displayname -eq "Machine Ordering App"} | Select-Object -First 1).msdyn_objectid

    $data = (@{
        contosoCoffeeApplication = "https://apps.powerapps.com/play/e/${environmentId}/a/${appId}"
        powerAutomateApprovals = "https://make.powerautomate.com/environments/${environmentId}/approvals/received"
    } | ConvertTo-Json)

    Invoke-PlaywrightScript $UserUPN $devEnv.Id "contoso-coffee-submit-machine-request.csx" $data

    # TODO Validate Approved in Dataverse
}

<#
.SYNOPSIS
Runs a Playwright script with the specified user UPN, environment ID, script file, and data.

.PARAMETER UserUPN
The user's UPN (user principal name) to run the script as.

.PARAMETER EnvironmentId
The ID of the environment to run the script in.

.PARAMETER ScriptFile
The path to the script file to run.

.PARAMETER Data
The data to pass to the script.

.EXAMPLE
Invoke-PlaywrightScript -UserUPN "user@example.com" -EnvironmentId "1234" -ScriptFile "C:\Scripts\myScript.ps1" -Data "{'key': 'value'}"
Runs the Playwright script located at "C:\Scripts\myScript.ps1" with the user UPN "user@example.com", environment ID "1234", and data '{"key": "value"}'.

#>
function Invoke-PlaywrightScript {
    param (
        [Parameter(Mandatory)] [String] $UserUPN,
        [Parameter(Mandatory)] [String] $EnvironmentId,
        [Parameter(Mandatory)] [String] $ScriptFile,
        [Parameter(Mandatory)] [String] $Data
    )

    if ( -not [System.IO.Path]::IsPathRooted($ScriptFile) ) {
        $ScriptFile = [System.IO.Path]::Join($PSScriptRoot,$ScriptFile)
        
    }

    #Get the bytes of the data with encode      
    $dataBytes = [System.Text.Encoding]::UTF8.GetBytes($Data)
    # Base64 Encode content 
    $dataEncoded = [System.Convert]::ToBase64String($dataBytes)

    $appPath = [System.IO.Path]::Join($PSScriptRoot,"..","install", "bin", "Debug", "net7.0", "install.dll")
    dotnet $appPath user script --upn $UserUPN --env $EnvironmentId --file $ScriptFile --data  $dataEncoded --headless "Y" --record "Y"
}

<#
.SYNOPSIS
Gets a list of environments for the specified user.

.PARAMETER UserUPN
The user's UPN (user principal name) to get the environments for.

.EXAMPLE
Get-Environments -UserUPN "user@example.com"
Gets a list of environments for the user with the UPN "user@example.com".

#>
function Get-Environments {
    param (
        [Parameter(Mandatory)] [String] $UserUPN
    )

    $appPath = [System.IO.Path]::Join($PSScriptRoot,"..","install", "bin", "Debug", "net7.0", "install.dll")
    $envs = (dotnet $appPath environment list --upn $UserUPN --headless "Y" --record "N")

    $json = ""
    foreach ( $line in $envs ) {
        if ( $line.StartsWith("[") ) {
            $json = $line
        }
    }
    
    return ($json | ConvertFrom-Json)
}

<#
.SYNOPSIS
Gets a solution with the specified unique name in the specified environment using the specified access token.

.PARAMETER token
The access token to use for authentication.

.PARAMETER environmentUrl
The URL of the environment to get the solution from.

.PARAMETER uniqueName
The unique name of the solution to get.

.EXAMPLE
Get-Solution -token "abc123" -environmentUrl "https://contoso.crm.dynamics.com/" -uniqueName "ContosoCoffee"
Gets the solution with the unique name "ContosoCoffee" in the environment at "https://contoso.crm.dynamics.com/" using the access token "abc123".

#>
function Get-Solution {
    param (
        [Parameter(Mandatory)] [String] $token,
        [Parameter(Mandatory)] [String] $environmentUrl,
        [Parameter(Mandatory)] [String] $uniqueName
    )

    $headers = @{Authorization = "Bearer $token" }

    return (Invoke-RestMethod -Method GET -Headers $headers -Uri "${environmentUrl}api/data/v9.2/solutions?`$select=uniquename,solutionid&`$filter=uniquename eq '$uniqueName'" )
}

<#
.SYNOPSIS
Gets the components of a solution with the specified ID in the specified environment using the specified access token.

.PARAMETER token
The access token to use for authentication.

.PARAMETER environmentUrl
The URL of the environment to get the solution components from.

.PARAMETER solutionId
The ID of the solution to get the components for.

.EXAMPLE
Get-SolutionComponents -token "abc123" -environmentUrl "https://contoso.crm.dynamics.com/" -solutionId "1234"
Gets the components of the solution with the ID "1234" in the environment at "https://contoso.crm.dynamics.com/" using the access token "abc123".

#>
function Get-SolutionComponents {
    param (
        [Parameter(Mandatory)] [String] $token,
        [Parameter(Mandatory)] [String] $environmentUrl,
        [Parameter(Mandatory)] [String] $solutionId
    )

    $headers = @{Authorization = "Bearer $token" }

    return (Invoke-RestMethod -Method GET -Headers $headers -Uri "${environmentUrl}api/data/v9.2/msdyn_solutioncomponentsummaries?`$filter=msdyn_solutionid eq $solutionId&`$select=msdyn_displayname,msdyn_objectid" )
}