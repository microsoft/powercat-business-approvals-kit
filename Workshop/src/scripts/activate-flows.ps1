<#  
.SYNOPSIS  
    Activate Power Automate flows
.DESCRIPTION  
    This script provides a sample of activating Power Automate flows
.NOTES  
    File Name  : activate-flows.ps1  
    Author     : Grant Archibald - grant.archibald@microsoft.com
    Requires   : 
        PowerShell Core 7.1.3 or greater
        az cli 2.39.0 or greater
        pac cli 1.27.6+g9f62afb or greater

Copyright (c) Microsoft Corporation. All rights reserved.
Licensed under the MIT License.

Adapted from https://github.com/microsoft/coe-alm-accelerator-templates/blob/c7ca2e7bf00313b82969f7cd6b9e8272c5fe0555/PowerShell/activate-flows.ps1
#>


<#
This function reads the flow's configuration settings.
Activates the flows in target environment as per the specified order post solution Import.
#>
function Invoke-ActivateFlows {
    param (
        [Parameter(Mandatory)] [String]$UserUPN,
        [Parameter(Mandatory)] $Environment,
        [Parameter(Mandatory)] [String]$solutionName
    )

    $token=(az account get-access-token --resource=$($Environment.EnvironmentUrl) --query accessToken --output tsv)

    $headers = @{Authorization="Bearer $token"}

    $user = Get-SystemUserByEmail $UserUPN $headers $Environment

    if ( $NULL -eq $user ) {
        Write-Error "Unable to find system user $UserUPN"
        return
    }

    $attempts = 0

    while ( $true ) {
        $flowsToActivate = [System.Collections.ArrayList]@()
        Get-FlowsToActivate $headers $Environment $solutionName $flowsToActivate

        if ( $flowsToActivate.Count -eq 0 ) {
            Write-Host "No flows to activate"
            break
        } else {
            Write-Host "Flows to activate" $flowsToActivate.Count
            Invoke-ActivateFlow $Environment $flowsToActivate

            $flowsToActivateUpdate = [System.Collections.ArrayList]@()
            Get-FlowsToActivate $headers $Environment $solutionName $flowsToActivateUpdate

            if ( $flowsToActivate.Count -eq $flowsToActivateUpdate.Count ) {
                $attempts = $attempts + 1
            }

            if ( $attempts -gt 5 ) {
                Write-Error "Exiting activation as no flows activated in processing loop."
                break
            }
        }
    }
    

    # TODO
    # Activation Order (Look for child flows)
    # Connection References
    # User defined order
}

function Get-SystemUserByEmail {
    param (
        [Parameter(Mandatory)] [String]$UserUPN,
        [Parameter(Mandatory)] $headers,
        [Parameter(Mandatory)] $Environment
    )

    $users = Invoke-RestMethod -Method GET -Headers $headers -Uri "$($Environment.EnvironmentUrl)api/data/v9.2/systemusers?`$filter=internalemailaddress eq '$UserUPN'"

    if ( $users.value.length -eq 1 ) {
        return $users.value[0]
    }

    return $NULL
}

function Get-WhoAmI {
    param (
        [Parameter(Mandatory)] $headers,
        [Parameter(Mandatory)] $Environment
    )

    return Invoke-RestMethod -Method GET -Headers $headers -Uri "$($Environment.EnvironmentUrl)api/data/v9.2/WhoAmI"
}

function Get-FlowsToActivate {
    param (
        [Parameter(Mandatory)] $headers,
        [Parameter(Mandatory)] $Environment,
        [Parameter(Mandatory)] [String] [AllowEmptyString()]$solutionName,
        [Parameter()] [System.Collections.ArrayList] [AllowEmptyCollection()]$flowsToActivate
    )

    # Fetch the 'solution' using 'solutionComponentUniqueName' tag
    $solutions = Invoke-RestMethod -Method GET -Headers $headers -Uri "$($Environment.EnvironmentUrl)api/data/v9.2/solutions?`$filter=uniquename eq '$solutionName'&`$select=solutionid"

    if ( $solutions.value.Count -eq 1) {
        $solutionId = $solutions.value[0].solutionid

        $solutionComponents = Invoke-RestMethod -Method GET -Headers $headers -Uri "$($Environment.EnvironmentUrl)api/data/v9.2/solutioncomponents?`$filter=_solutionid_value eq $solutionId&`$select=objectid,componenttype"

        foreach ($solutionComponent in $solutionComponents.value) {
            # https://learn.microsoft.com/power-apps/developer/data-platform/reference/entities/solutioncomponent
            $workflowComponent = 29
            if ($solutionComponent.componenttype -eq $workflowComponent) {
                $workflows = Invoke-RestMethod -Method GET -Headers $headers -Uri "$($Environment.EnvironmentUrl)api/data/v9.2/workflows?`$filter=workflowid eq $($solutionComponent.objectid)&`$select=statecode,name,workflowidunique"

                if ( $workflows.value.length -eq 1 ) {
                    $workflow = $workflows.value[0]
                    if ( $workflow.statecode -ne 1  ) {
                        $flowActivation = [PSCustomObject]@{}
                        $flowActivation | Add-Member -MemberType NoteProperty -Name 'name' -Value $workflow.name
                        $flowActivation | Add-Member -MemberType NoteProperty -Name 'solutionid' -Value $solutionId
                        $flowActivation | Add-Member -MemberType NoteProperty -Name 'workflowidunique' -Value $workflow.workflowidunique
                        $flowsToActivate.Add($flowActivation) | Out-Null
                    }
                }
            }
        }
    }
    else
    {
        Write-Error "Solution not found"
    }
}

function Invoke-ActivateFlow {
    param (
        [Parameter(Mandatory)] $Environment,
        [Parameter()] [System.Collections.ArrayList] [AllowEmptyCollection()]$flowsToActivate
    )

    if ( $flowsToActivate.Count -eq 0 ) {
        return
    }

    $sb = [System.Text.StringBuilder]::new()

    foreach ( $flow in $flowsToActivate ) {
        Write-Host $flow.name
        if ( $sb.Length -gt 0) {
            [void]$sb.Append([Environment]::NewLine);
        }
        [void]$sb.Append($flow.workflowidunique)
    }

    $flowFile = [System.IO.Path]::Combine((Get-Location), "flows.txt")

    Set-Content $flowFile $sb.ToString()

    $solutionId = $flowsToActivate[0].solutionid

    $workshopPath = [System.IO.Path]::Join((Get-AssetPath), "..")

    Push-Location
    Set-Location $workshopPath
    

    # Activate the flow via Playwright as the active user. Reasons:
    # - The user has permissions to the connections
    # - Can work even if the connection is not shared with the service principal
    #
    # Other possible options:
    # - Share the connections with ServicePrincipal and then Active Flow as Service Principal by changing the statecode of the workflow to 1 using dataverse REST API
    $appPath = [System.IO.Path]::Join($PSScriptRoot,"..","install", "bin", "Debug", "net7.0", "install.dll")
    dotnet $appPath flow activate --upn $UserUPN --env $Environment.EnvironmentId --solution $solutionId --id $flowFile # --headless "N"
    Pop-Location
}
