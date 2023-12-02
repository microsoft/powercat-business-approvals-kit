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
    [CmdletBinding()]
    param (
        [Parameter(Mandatory)] [String] $UserUPN,
        [Parameter(Mandatory=$False)] $EnvironmentName = "",
        [Parameter(Mandatory=$False)] $EnvironmentUrl = "",
        [Parameter(Mandatory=$False)] $EnvironmentId = ""
    )

    Process 
    {
        if ( $UserUPN.IndexOf("@") -lt 0 ) {
            $domain=(az account show --query "user.name" -o tsv).Split('@')[1]
            $UserUPN = "$UserUPN@$domain"
        }

        if ( [System.String]::IsNullOrEmpty($EnvironmentUrl) ) {
            Write-Host "Query environments"
            $envs = Get-Environments
    
            if ( [System.String]::IsNullOrEmpty($EnvironmentName) ) {
                $user = (az ad user list --upn $UserUPN | ConvertFrom-Json)
                $EnvironmentName = $user.displayName + " Dev"
            }
    
            $devEnv = $envs | Where-Object { $_.Name -eq $EnvironmentName } | Select-Object -First 1
    
            if ( $NULL -eq $devEnv ) {
                Write-Error "Environment not found"
                return
            }
        
            Write-Host "Found environment"
            $dataverse = $devEnv.EnvironmentUrl
            $environmentId = $devEnv.EnvironmentId
        } else {
            $dataverse = $EnvironmentUrl
            $environmentId = $EnvironmentId
        }
    
        $token = (az account get-access-token --resource=$dataverse --query accessToken --output tsv)
    
        Write-Host "Searching for Contoso Coffee in $dataverse"
        $solution = (Get-Solution $token $dataverse "ContosoCoffee")
    
        if ( $solution.value.count -eq 0 ) {
            Write-Error "Solution ContosoCoffee not found"
            return
        }
    
        $solutionComponents = (Get-SolutionComponents $token $dataverse $solution.value[0].solutionid) 
    
        $appId = ($solutionComponents.value | Where-Object { $_.msdyn_displayname -eq "Machine Ordering App"} | Select-Object -First 1).msdyn_objectid
    
        $data = (@{
            contosoCoffeeApplication = "https://apps.powerapps.com/play/e/${environmentId}/a/${appId}"
            powerAutomateApprovals = "https://make.powerautomate.com/environments/${environmentId}/approvals/received"
        } | ConvertTo-Json)
    
        Invoke-PlaywrightScript $UserUPN $environmentId "contoso-coffee-submit-machine-request.csx" $data N    
    }
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
        [Parameter(Mandatory)] [String] $Data,
        [String] $Headless = "Y",
        [String] $Json = "N"
    )

    if ( -not [System.IO.Path]::IsPathRooted($ScriptFile) ) {
        $ScriptFile = [System.IO.Path]::Join($PSScriptRoot,$ScriptFile)
    }

    if ( $UserUPN.IndexOf("@") -eq -1 ) {
        $domain=(az account show --query "user.name" -o tsv).Split('@')[1]
        $UserUPN = "$UserUPN@$domain"
    }

    #Get the bytes of the data with encode      
    $dataBytes = [System.Text.Encoding]::UTF8.GetBytes($Data)
    # Base64 Encode content 
    $dataEncoded = [System.Convert]::ToBase64String($dataBytes)

    $workshopPath = [System.IO.Path]::Join((Get-AssetPath), "..")

    Push-Location
    Set-Location $workshopPath
    $appPath = [System.IO.Path]::Join($PSScriptRoot,"..","install", "bin", "Debug", "net7.0", "install.dll")
    if ( $Json -eq "Y" ) {
        $result = dotnet $appPath user script --upn $UserUPN --env $EnvironmentId --file $ScriptFile --data  $dataEncoded --headless $Headless --record "Y" | Out-String
        $start = $result.IndexOf("{")
        $end = $result.LastIndexOf("}")
        if ( ($start -ge 0  ) -and ($end -gt $start) ) {
            $data = $result.Substring($start,$end-$start+1)
        } else {
            $data = @{ error = "No response" } | ConvertTo-Json -Depth 100
        }
        
        return $data
    } else {
        dotnet $appPath user script --upn $UserUPN --env $EnvironmentId --file $ScriptFile --data  $dataEncoded --headless $Headless --record "Y"
    }
    Pop-Location
}

<#
.SYNOPSIS
Gets a list of environments for the current user.

.EXAMPLE
Get-Environments
Gets a list of environments for power platform authenticated user.

#>
function Get-Environments {
    $items = [System.Collections.ArrayList]@()

    # TODO - Replace with pac pac org list --json when supported
    $orgs = (pac org list)

    $index = 0
    foreach ($org in $orgs)
    {
        if ( $connection -like "*Error:*") {
            return [System.Collections.ArrayList]@()
        }
        if ( [System.String]::IsNullOrEmpty($org) ) {
            continue
        }
        if ( $index -eq 0) {
            $displayName = $orgs[0].IndexOf("Display Name")
            $environmentId = $orgs[0].IndexOf("Environment ID")
            $environmentURL = $orgs[0].IndexOf("Environment URL")
            $uniqueName = $orgs[0].IndexOf("Unique Name")
        }
        if ( $index -gt 0) {
            $environment = [PSCustomObject]@{ 
                Active = $org.Substring(0, $displayName).Trim() -eq "*"
                Name =  $org.Substring($displayName, $environmentId - $displayName).Trim()
                EnvironmentId = $org.Substring($environmentId, $environmentURL - $environmentId).Trim()
                EnvironmentUrl = $org.Substring($environmentURL, $uniqueName - $environmentURL).Trim()
                UniqueName = $org.Substring($uniqueName,$org.Length - $uniqueName).Trim()
            }
            $items.Add($environment) | out-null
        }
        $index = $index + 1
    }
    
    return $items
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

    if ( -not $environmentUrl.EndsWith("/") ) {
        $environmentUrl = $environmentUrl + "/"
    }

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

    if ( -not $environmentUrl.EndsWith("/") ) {
        $environmentUrl = $environmentUrl + "/"
    }

    return (Invoke-RestMethod -Method GET -Headers $headers -Uri "${environmentUrl}api/data/v9.2/msdyn_solutioncomponentsummaries?`$filter=msdyn_solutionid eq $solutionId&`$select=msdyn_displayname,msdyn_objectid,msdyn_componenttype" )
}

function Get-ApprovalsKitProcess {
    param (
        [Parameter(Mandatory)] $Environment,
        [Parameter(Mandatory)] [String] $workflowName
    )

    $dataverse = $Environment.EnvironmentUrl
    $token = (az account get-access-token --resource=$dataverse --query accessToken --output tsv)

    $headers = @{Authorization = "Bearer $token" }

    if ( -not $dataverse.EndsWith("/") ) {
        $environmentUrl = $environmentUrl + "/"
    }

    return (Invoke-RestMethod -Method GET -Headers $headers -Uri "${dataverse}api/data/v9.2/cat_businessapprovalprocesses?`$select=cat_businessapprovalprocessid,cat_name&`$filter=cat_name eq '$workflowName'" )
}

function Get-ApprovalsKitPublishedVersion {
    param (
        [Parameter(Mandatory)] $Environment,
        [Parameter(Mandatory)] [String] $workflowName
    )

    $dataverse = $Environment.EnvironmentUrl
    $token = (az account get-access-token --resource=$dataverse --query accessToken --output tsv)

    $headers = @{Authorization = "Bearer $token" }
    
    if ( -not $dataverse.EndsWith("/") ) {
        $environmentUrl = $environmentUrl + "/"
    }

    return (Invoke-RestMethod -Method GET -Headers $headers -Uri "${dataverse}api/data/v9.2/cat_businessapprovalversions?`$select=cat_businessapprovalversionid,cat_name,cat_version,cat_publishingstatus&`$filter=cat_name eq '$workflowName'" )
}

function Get-ApprovalKitContosoCoffeeState {
    $user = (Get-SecureValue "DEMO_USER")
    $Environment = Invoke-UserDevelopmentEnvironment $user
    $installed = Invoke-SolutionInstalled $Environment "ContosoCoffeeApprovals"
    $installed | Should -BeTrue

    $dataverse = $Environment.EnvironmentUrl
    $token = (az account get-access-token --resource=$dataverse --query accessToken --output tsv)

    $solution = (Get-Solution $token $Environment.EnvironmentUrl "ContosoCoffeeApprovals")

    $solution.value.count | Should -BeGreaterThan 0

    $solutionId =  $solution.value[0].solutionid

    $components = (Get-SolutionComponents $token $Environment.EnvironmentUrl $solutionId)

    return @{
        User = $user
        Environment = $Environment
        Dataverse = $dataverse
        Token = $token
        Solution = $solution
        SolutionComponents = $components
    }
}

function Invoke-ValidateEnvironments {
    param (
        [Parameter(Mandatory)] $UsersListFile
    )

    $domain=(az account show --query "user.name" -o tsv).Split('@')[1]

    if ( Test-Path $UsersListFile ) {
        Write-Host "Found user file"
        $lines = Get-Content $UsersListFile
        $total = $lines.Length
        $index = 0
        foreach($line in $lines) {
            if ( -not [System.String]::IsNullOrEmpty($line) ) {
                $index = $index + 1
                Write-Host "---------------------------------------------"
                Write-Host "$index of $total - $(Get-Date)"
                Write-Host "$line@$domain"
                Write-Host "---------------------------------------------"
                $result = Invoke-ValidateEnvironment "$line@$domain"
                if ( -not $result.valid ) {
                    Write-Host "Not Valid"
                } else {
                    Write-Host "Valid"
                }
            }
        }
    }
}

function Invoke-ValidateEnvironment {
    param (
        [Parameter(Mandatory)] $UserUPN
    )

    if ( -not ([System.String]::IsNullOrEmpty($UserUPN)) ) {
        if ( $UserUPN.IndexOf("@") -lt 0 ) {
            $domain=(az account show --query "user.name" -o tsv).Split('@')[1]
            $UserUPN = "$UserUPN@$domain"
        }
        $Environment = Invoke-UserDevelopmentEnvironment $UserUPN
    } else {
        $Environment = Invoke-UserDevelopmentEnvironment (Get-SecureValue "DEMO_USER")
    }

    $token=(az account get-access-token --resource=$($Environment.EnvironmentUrl) --query accessToken --output tsv)
    $headers = @{Authorization="Bearer $token"}
    $connectors = (Invoke-RestMethod -Method GET -Headers $headers -Uri "$($Environment.EnvironmentUrl)api/data/v9.2/connectors?`$filter=name eq 'cat_approvals-20kit'" )
    
    $clientId = (Get-SecureValue CLIENT_ID)

    $app = (az ad app show --id $clientId | ConvertFrom-Json)

    $result = @{
        connectorCount = $connectors.value.length
        environmentUrl = $Environment.EnvironmentUrl
        clientId = ""
        resourceId = ""
        redirectUrl = ""
        valid = $False
        operations = ""
        checks = @{}
    }

    if ( $connectors.value.length -eq 1 ) {
        $parameters = ($connectors.value[0].connectionparameters | ConvertFrom-Json )
        $result.clientId = $parameters.token.oAuthSettings.clientId
        $result.azureResourceId = $parameters.token.oAuthSettings.properties.AzureActiveDirectoryResourceId = $Environment.EnvironmentUrl
        $result.tenantId = $parameters.token.oAuthSettings.customParameters.tenantId.value
        $result.resourceUri = $parameters.token.oAuthSettings.customParameters.resourceUri.value 
        $result.redirectUrl = ( $connectors.value[0].connectionparameters | ConvertFrom-Json ).token.oAuthSettings.redirectUrl
        $result.checks["Redirect Found"] = ($app.web.redirectUris | Where-Object { $_ -eq $result.redirectUrl}).Count -eq 1
    }

    if ( $result.redirectUrl.Length -gt 0 -and -not ($result.redirectFound) ) {
        Invoke-UpdateCustomConnectorReplyUrl $Environment $UserUPN
        $app = (az ad app show --id $clientId | ConvertFrom-Json)
        $result.checks["Redirect Found"] = ($app.web.redirectUris | Where-Object { $_ -eq $result.redirectUrl}).Count -eq 1
    }

    if ( $connectors.value.length -eq 1 ) {
        $connectionId = $connectors.value[0].connectorinternalid
        $environmentId = $Environment.EnvironmentId
        $connections = Get-Connections($Environment)

        $data = (@{
            user = $UserUPN
            approvalsConnectionCount = ( $connections | Where-Object { $_.API.IndexOf("shared_cat-5fapprovals-20kit") -gt 0 -and $_.Status -eq "Connected" }).Count.ToString()
            editUrl = "https://make.powerautomate.com/environments/$environmentId/connections/available/custom/$connectionId/edit/general"
        } | ConvertTo-Json -Depth 100 -Compress )
        $connectorResult = (Invoke-PlaywrightScript $UserUPN $environmentId "validate-approvals-kit-custom-connector.csx" $data "Y" "Y" | ConvertFrom-Json)
        $result.operations = $connectorResult.operations
        $result.checks["Found connector"] = $True
        $result.checks["Found operations"] = $result.operations -eq "CreateWorkflowInstance, GetApprovalDataFields"
        $result.checks["Get Workflows"] = $connectorResult.status -eq "(200)"
    } else {
        $result.checks["Found connector"] = $False
    }

    $result.checks["Client Id Match"] = $result.clientId -eq (Get-SecureValue "CLIENT_ID")
    $result.checks["Resource Id Match"] = $result.azureResourceId -eq $Environment.EnvironmentUrl
    $result.checks["Resource Uri"] = $result.azureResourceId -eq $Environment.EnvironmentUrl

    if ( `
        (
            $result.checks.GetEnumerator() | Where-Object {
                $_.Value -eq $False
            }
        ).Count -eq 0
    ) {
        $result.valid = $true
    }

    return $result
}

function Invoke-RecordDemo {
    param (
        [Parameter(Mandatory)] $UserUPN
    )

    if ( -not ([System.String]::IsNullOrEmpty($UserUPN)) ) {
        if ( $UserUPN.IndexOf("@") -lt 0 ) {
            $domain=(az account show --query "user.name" -o tsv).Split('@')[1]
            $UserUPN = "$UserUPN@$domain"
        }
        $Environment = Invoke-UserDevelopmentEnvironment $UserUPN
    } else {
        $Environment = Invoke-UserDevelopmentEnvironment (Get-SecureValue "DEMO_USER")
    }

    $dataverse = $Environment.EnvironmentUrl
    if ( $NULL -ne $dataverse -and -not $dataverse.EndsWith("/") ) {
        $dataverse = $dataverse + "/"
    }

    $token=(az account get-access-token --resource=$($Environment.EnvironmentUrl) --query accessToken --output tsv)
    $headers = @{Authorization="Bearer $token"}
    $connectors = (Invoke-RestMethod -Method GET -Headers $headers -Uri "${dataverse}api/data/v9.2/connectors?`$filter=name eq 'cat_approvals-20kit'" )
    
    $environmentId = $Environment.EnvironmentId

    Write-Host "Searching for Contoso Coffee in $dataverse"
    $solution = (Get-Solution $token $dataverse "ContosoCoffee")
    $solutionComponents = (Get-SolutionComponents $token $dataverse $solution.value[0].solutionid) 
    $appId = ($solutionComponents.value | Where-Object { $_.msdyn_displayname -eq "Machine Ordering App"} | Select-Object -First 1).msdyn_objectid

    $solutionBusinessApprovalKit = (Get-Solution $token $dataverse "BusinessApprovalKit")
    $solutionBusinessApprovalKitComponents = (Get-SolutionComponents $token $dataverse $solutionBusinessApprovalKit.value[0].solutionid) 
    
    $modelDrivenAppComponentType = 80
    $kitAppId = ($solutionBusinessApprovalKitComponents.value | Where-Object { $_.msdyn_displayname -eq "Business Approval Management" -and $_.msdyn_componenttype -eq $modelDrivenAppComponentType} | Select-Object -First 1).msdyn_objectid

    $environmentUrl = $Environment.EnvironmentUrl
    if ( -not $environmentUrl.EndsWith("/")) {
        $environmentUrl = $environmentUrl + "/"
    }

    if ( $connectors.value.length -eq 1 ) {
        $data = (@{
            # ScreenWidth = "1920"
            # ScreenHeight = "1080"
            contosoCoffeeApplication = "https://apps.powerapps.com/play/e/${environmentId}/a/${appId}"
            businessApprovalManager = "${environmentUrl}main.aspx?appid=$kitAppId"
            powerAutomateApprovals = "https://make.powerautomate.com/environments/${environmentId}/approvals/received"
            powerAutomatePortal = "https://make.powerautomate.com/environments/${environmentId}"
            userEmail = $UserUPN
            token = $token
            environmentUrl = $dataverse
        } | ConvertTo-Json)
        Invoke-PlaywrightScript $UserUPN $environmentId "approvals-kit-record.csx" $data "N" "N" # | Out-Null
    }
}

function Invoke-CreateKeys {
    param (
        [Parameter(Mandatory)] $UsersListFile
    )

    $domain=(az account show --query "user.name" -o tsv).Split('@')[1]

    $keyPath = [System.IO.Path]::Combine( (Get-AssetPath),"keys" )
    $domainKeyPath = [System.IO.Path]::Combine($keyPath, $domain)

    if ( -not (Test-Path $keyPath )) {
        New-Item -Path (Get-AssetPath) -Name "keys" -ItemType Directory
    }

    if ( -not (Test-Path $domainKeyPath )) {
        New-Item -Path $keyPath -Name $domain -ItemType Directory
    }

    $environmentFile = [System.IO.Path]::Combine( $domainKeyPath,"environments.txt" )
    if ( (Test-Path $environmentFile) ) {
        Remove-Item $environmentFile
    }

    if ( Test-Path $UsersListFile ) {
        Write-Host "Found user file"
        $lines = Get-Content $UsersListFile
        $total = $lines.Length
        $index = 0
        foreach($line in $lines) {
            if ( -not [System.String]::IsNullOrEmpty($line) ) {
                $index = $index + 1
                $keyFile = [System.IO.Path]::Combine($domainKeyPath,"$line.key")
                Write-Host "---------------------------------------------"
                Write-Host "$index of $total - $(Get-Date)"
                Write-Host "$line@$domain"
                Write-Host "---------------------------------------------"
                $Key = New-Object Byte[] 16   # You can use 16, 24, or 32 for AES
                [Security.Cryptography.RNGCryptoServiceProvider]::Create().GetBytes($Key)
                $Key | out-file $keyFile

                $PasswordFile = [System.IO.Path]::Combine($domainKeyPath,"$line.txt")
                $Password = (Get-SecureValue DEMO_PASSWORD) | ConvertTo-SecureString -AsPlainText -Force
                $Password | ConvertFrom-SecureString -key $Key | Out-File $PasswordFile

                $Environment = Invoke-UserDevelopmentEnvironment "$line@$domain"
                "$line,https://make.powerapps.com/environments/$($Environment.EnvironmentId)" | Add-Content -Path $environmentFile
            }
        }
    }
}
