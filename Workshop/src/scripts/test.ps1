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

    # TODO
    
    # Record - Open Machine Request (Select Item, Compare, Submit)
    # Verify Machine Order Exists

    # 
}

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
    dotnet $appPath user script --upn $UserUPN --env $EnvironmentId --file $ScriptFile --data  $dataEncoded --headless "N" --record "N"
}

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


function Get-Solution {
    param (
        [Parameter(Mandatory)] [String] $token,
        [Parameter(Mandatory)] [String] $environmentUrl,
        [Parameter(Mandatory)] [String] $uniqueName
    )

    $headers = @{Authorization = "Bearer $token" }

    return (Invoke-RestMethod -Method GET -Headers $headers -Uri "${environmentUrl}api/data/v9.2/solutions?`$select=uniquename,solutionid&`$filter=uniquename eq '$uniqueName'" )
}

function Get-SolutionComponents {
    param (
        [Parameter(Mandatory)] [String] $token,
        [Parameter(Mandatory)] [String] $environmentUrl,
        [Parameter(Mandatory)] [String] $solutionId
    )

    $headers = @{Authorization = "Bearer $token" }

    return (Invoke-RestMethod -Method GET -Headers $headers -Uri "${environmentUrl}api/data/v9.2/msdyn_solutioncomponentsummaries?`$filter=msdyn_solutionid eq $solutionId&`$select=msdyn_displayname,msdyn_objectid" )
}