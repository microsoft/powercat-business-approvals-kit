<#  
.SYNOPSIS  
    Sample Power Platform Demo user related Setup
.DESCRIPTION  
    This script provides a sample of deploying demo user related features. To use this file in Powershell . .\users.ps1 
.NOTES  
    File Name  : users.ps1  
    Author     : Grant Archibald - grant.archibald@microsoft.com
    Requires   : 
        PowerShell Core 7.1.3 or greater
        az cli 2.39.0 or greater
        pac cli 1.27.6+g9f62afb or greater

Copyright (c) Microsoft Corporation. All rights reserved.
Licensed under the MIT License.
#>

. $PSScriptRoot\common.ps1
. $PSScriptRoot\security.ps1
. $PSScriptRoot\activate-flows.ps1
Import-Module $PSScriptRoot\UtilityMethods.psm1

<#
    .DESCRIPTION
    This PowerShell function creates a development environment for a specified user and returns the environment object. If the environment already exists, it returns the existing environment object. It takes one parameter, `$UserUPN`, which is the user's UPN.

    .PARAMETER UserUPN
    Specifies the user's UPN.

    .INPUTS
    None. You can't pipe objects to Invoke-UserDevelopmentEnvironment.

    .OUTPUTS
    System.Object. Invoke-UserDevelopmentEnvironment returns a development environment object.

    .EXAMPLE
    PS> $env = Invoke-UserDevelopmentEnvironment -UserUPN "first.last@contoso.com"
    Returns the development environment object for first.last@contoso.com.

#> 

function Invoke-UserDevelopmentEnvironment {
    param (
        [Parameter(Mandatory)] [String] $UserUPN
    )

    return Invoke-GetOrCreateDevelopmentEnvironment $UserUPN
}

function Remove-UserDevelopmentEnvironment {
    param (
        [Parameter(Mandatory)] [String] $UserUPN
    )

    $envs = Invoke-GetDeveloperEnvironment $UserUPN
    if ( $envs.Count -gt 0 ) {
        Write-Host "Removing $($envs.Count) environment(s) for user $UserUPN"
        foreach ($env in $envs)
        {
            pac admin delete --environment $env.EnvironmentId
        }
    }
}

<#
    .DESCRIPTION
    This PowerShell function authenticates a user with a specified development environment. It takes two parameters, `$UserUPN` and `$Environment`, which are the user's UPN and the development environment object, respectively.

    .NOTES
    Assumes that the Power Platform CLI is installed

    .PARAMETER UserUPN
    Specifies the user's UPN.

    .PARAMETER Environment
    Specifies the development environment object.

    .INPUTS
    None. You can't pipe objects to Invoke-DevEnvironmentAuth.

    .OUTPUTS
    None. Invoke-DevEnvironmentAuth does not return any objects.

    .EXAMPLE
    PS> Invoke-DevEnvironmentAuth -UserUPN "first.last@contoso.com" -Environment $env
    Authenticates the user with the specified development environment.

#> 
function Invoke-DevEnvironmentAuth {
    param (
        [Parameter(Mandatory)] [String] $UserUPN,
        $Environment
    )

    Invoke-GetOrCreateDevelopmentEnvironment $UserUPN $Environment
    Invoke-DevEnvironmentAuthUtility $UserUPN $Environment
}

<#
    .DESCRIPTION
    This PowerShell function enables custom controls for canvas apps in a specified development environment. It takes one parameter, `$Environment`, which is the development environment object.

    .NOTES
    Assumes that the Azure CLI is installed and that the user has the necessary permissions to change environment settings.

    .PARAMETER Environment
    Specifies the development environment object.

    .INPUTS
    None. You can't pipe objects to Enable-DevelopmentEnvironmentCustomControls.

    .OUTPUTS
    None. Enable-DevelopmentEnvironmentCustomControls does not return any objects.

    .EXAMPLE
    PS> Enable-DevelopmentEnvironmentCustomControls -Environment $env
    Enables custom controls for canvas apps in the specified development environment.
#>
function Enable-DevelopmentEnvironmentCustomControls {
    param (
        [Parameter(Mandatory)] $Environment
    )

    $token=(az account get-access-token --resource=$($Environment.EnvironmentUrl) --query accessToken --output tsv)
    $headers = @{Authorization="Bearer $token"}
    $org = (Invoke-RestMethod -Method GET -Headers $headers -Uri "$($Environment.EnvironmentUrl)api/data/v9.2/organizations" )
    if ( $org.value[0].iscustomcontrolsincanvasappsenabled -eq $False ) {
        Write-Host "Enabling Custom controls for canvasapps"
        Invoke-RestMethod -Method PATCH -Headers $headers -Uri "$($Environment.EnvironmentUrl)api/data/v9.2/organizations($($org.value[0].organizationid))" -Body "{ iscustomcontrolsincanvasappsenabled:true }" -ContentType "application/json"
    } else {
        Write-Host "Custom controls for canvasapps is already enabled"
    }
}

<#
    .DESCRIPTION
    This PowerShell function installs the Creator Kit application in a specified development environment if it is not already installed. It takes one parameter, `$Environment`, which is the development environment object.

    .NOTES
    Assumes that the Power Platform CLI is installed and that the user has the necessary permissions to install applications.

    .PARAMETER Environment
    Specifies the development environment object.

    .INPUTS
    None. You can't pipe objects to Install-CreatorKit.

    .OUTPUTS
    None. Install-CreatorKit does not return any objects.

    .EXAMPLE
    PS> Install-CreatorKit -Environment $env
    Installs the Creator Kit application in the specified development environment if it is not already installed.
#>
function Install-CreatorKit {
    param (
        [Parameter(Mandatory)] $Environment
    )

    $apps = (pac application list)
    $creatorKitExists = (($apps | Where-Object { $_.IndexOf("Creator Kit") -ge 0 }).Count -ge 0)
    if ( -not $creatorKitExists ) {
        pac application install --environment $Environment.EnvironmentId --application-name CreatorKitCore
    }

    $installed = Invoke-SolutionInstalled $Environment "CreatorKitCore"
    if ( -not $installed ) {
        pac application install --environment $Environment.EnvironmentId --application-name CreatorKitCore
    } else {
        Write-Host "Creator Kit already installed"
    }
}

<#
    .DESCRIPTION
    This PowerShell function checks if a solution with a specified unique name is installed in a specified development environment. It takes two parameters, `$Environment` and `$UniqueName`, which are the development environment object and the unique name of the solution, respectively.

    .NOTES
    Assumes that the Power Platform CLI is installed and that the user has the necessary permissions to access solutions.

    .PARAMETER Environment
    Specifies the development environment object.

    .PARAMETER UniqueName
    Specifies the unique name of the solution to check.

    .INPUTS
    None. You can't pipe objects to Invoke-SolutionInstalled.

    .OUTPUTS
    Returns a Boolean value indicating whether the solution is installed.

    .EXAMPLE
    PS> Invoke-SolutionInstalled -Environment $env -UniqueName "MySolution"
    Returns $True if the solution with the unique name "MySolution" is installed in the specified development environment, and False otherwise.
#>
function Invoke-SolutionInstalled {
    param (
        [Parameter(Mandatory)] $Environment,
        [Parameter(Mandatory)] [String] $UniqueName
    )
    $token=(az account get-access-token --resource=$($Environment.EnvironmentUrl) --query accessToken --output tsv)
    $headers = @{Authorization="Bearer $token"}
    $solution = (Invoke-RestMethod -Method GET -Headers $headers -Uri "$($Environment.EnvironmentUrl)/api/data/v9.2/solutions?`$filter=uniquename%20eq%20%27$UniqueName%27&`$select=uniquename") 
    return $solution.value.Count -gt 0
}



<#
    .DESCRIPTION
    This PowerShell function enables approvals for a specified development environment. It takes two parameters, `$UserUPN` and `$Environment`, which are the user's UPN and the development environment object, respectively.

    .NOTES
    Assumes that the Power Platform CLI is installed and that the "msdyn_FlowApprovals" solution is available in the environment. 

    .PARAMETER UserUPN
    Specifies the user's UPN.

    .PARAMETER Environment
    Specifies the development environment object.

    .INPUTS
    None. You can't pipe objects to Enable-Approvals.

    .OUTPUTS
    None. Enable-Approvals does not return any objects.

    .EXAMPLE
    PS> Enable-Approvals -UserUPN "first.last@contoso.com" -Environment $env
    Enables approvals for the specified development environment.

#> 
function Enable-Approvals {
    param (
        [Parameter(Mandatory)] [String] $UserUPN,
        [Parameter(Mandatory)] $Environment
    )

    if ( $NULL -eq $Environment ) {
        $Environment = Invoke-UserDevelopmentEnvironment $UserUPN
    }

    $flowApprovalsInstalled = Invoke-SolutionInstalled $Environment "msdyn_FlowApprovals"
    if ( -not $flowApprovalsInstalled ) {
       
        Write-Host "Approvals need to be enabled"
        if (  ( Get-ConfigValue "Approvals.Install" ) -eq "pac") {
            pac application install --environment $Environment.EnvironmentId --application-name "msdyn_FlowApprovals"
            $flowApprovalsInstalled = Invoke-SolutionInstalled $Environment "msdyn_FlowApprovals"
            if ( $flowApprovalsInstalled ) {
                Write-Host "Setup completed successfully"
            } else {
                Write-Host "Setup failed"
            }
        }
        else
        {
            Write-Host "Skipping Approvals Setup"
        }
    } else {
        Write-Host "Approvals are enabled"
    }
}

<#
    .DESCRIPTION
    This PowerShell function waits for a cloud flow to complete. It takes four parameters, `$UserUPN`, `$Environment`, `$FlowUrl`, and `$FlowId`, which are the user's UPN, the development environment object, the URL of the flow, and the ID of the flow, respectively.

    .NOTES
    Assumes that the Power Platform CLI is installed.

    .PARAMETER UserUPN
    Specifies the user's UPN.

    .PARAMETER Environment
    Specifies the development environment object.

    .PARAMETER FlowUrl
    Specifies the URL of the flow.

    .PARAMETER FlowId
    Specifies the ID of the flow.

    .INPUTS
    None. You can't pipe objects to Invoke-WaitUntilFlowComplete.

    .OUTPUTS
    None. Invoke-WaitUntilFlowComplete does not return any objects.

    .EXAMPLE
    PS> Invoke-WaitUntilFlowComplete -UserUPN "first.last@contoso.com" -Environment $env -FlowUrl "https://prod-00.westus.logic.azure.com:443/workflows/1234567890abcdefg" -FlowId "1234567890abcdefg"
    Waits for the specified cloud flow to complete.

#> 
function Invoke-WaitUntilFlowComplete {
    param (
        [Parameter(Mandatory)] [String] $UserUPN,
        [Parameter(Mandatory)] $Environment,
        [Parameter(Mandatory)] [String] $FlowUrl,
        [Parameter(Mandatory)] [String] $FlowId
    )

    $runs = (Get-CloudFlowRuns $UserUPN $FlowUrl).value
    $success = $runs | Where-Object { $NULL -ne $_.properties -and $_.properties.status -eq "Succeeded" }

    $running = $runs | Where-Object { $NULL -ne $_.properties -and $_.properties.status -eq "Running" }

    if ( $running.Count -ge 1 ) {
        Write-Host "Flow already running"
        $started = Get-Date
        $waiting = (Get-Date).Subtract($started).TotalMinutes
        while ( $success.Count -eq 0 ) {
            $diff = (Get-Date).Subtract($started).ToString("hh\:mm\:ss")
            Write-Host "Waiting for flow to complete. Executing $diff"
            Start-Sleep -Seconds 30
            $runs = (Get-CloudFlowRuns $UserUPN $FlowUrl).value
            $success = $runs | Where-Object { $NULL -ne $_.properties -and $_.properties.status -eq "Succeeded" }
            $waiting = (Get-Date).Subtract($started).TotalMinutes

            if ( $waiting -gt 10 ) {
                break
            }
        }
    }

    $started = Get-Date
    $waiting = (Get-Date).Subtract($started).TotalMinutes
    while ( $success.Count -eq 0 ) {
        $diff = (Get-Date).Subtract($started).ToString("hh\:mm\:ss")
        Write-Host "Waiting for flow to complete. Executing $diff"
        Start-Sleep -Seconds 30
        $runs = (Get-CloudFlowRuns $UserUPN $FlowUrl).value
        $success = $runs | Where-Object { $NULL -ne $_.properties -and $_.properties.status -eq "Succeeded" }
        $waiting = (Get-Date).Subtract($started).TotalMinutes

        if ( $waiting -gt 10 ) {
            break
        }
    }
}

function Invoke-IsError($value) {
    if ($value -is [string]) {
        $value = ($value | ConvertFrom-Json)
    }

    if (Get-Member -inputobject $value -name "error" -Membertype Properties) {
        return $True
    } else {
        return $False
    } 
}

<#
    .DESCRIPTION
    This PowerShell function installs and sets up the Approvals Kit solution for a specified development environment. It takes two parameters, `$UserUPN` and `$Environment`, which are the user's UPN and the development environment object, respectively.

    .NOTES
    Assumes that the Power Platform CLI is installed. The user has system customizer rights to import the Approvals Kit

    .PARAMETER UserUPN
    Specifies the user's UPN.

    .PARAMETER Environment
    Specifies the development environment object.

    .INPUTS
    None. You can't pipe objects to Install-ApprovalsKit.

    .OUTPUTS
    None. Install-ApprovalsKit does not return any objects.

    .EXAMPLE
    PS> Install-ApprovalsKit -UserUPN "first.last@contoso.com" -Environment $env
    Installs and sets up the Approvals Kit solution for the specified development environment.

#> 
function Install-ApprovalsKit {
    param (
        [Parameter(Mandatory)] [String] $UserUPN,
        $Environment
    )

    if ( $NULL -eq $Environment ) {
        Invoke-ConfigureUser $UserUPN
        $Environment = Invoke-UserDevelopmentEnvironment $UserUPN
        if (Get-Member -inputobject $Environment -name "error" -Membertype Properties) {
            return $Environment | ConvertTo-Json
        } 
        Invoke-SaveUserEnvironment $UserUPN $Environment
    }

    if ( $Environment -is [string]) {
        $Environment = ($Environment | ConvertFrom-Json)
    }

    Enable-DevelopmentEnvironmentCustomControls $Environment

    if ( ( (Get-ConfigValue "Feature.CreatorKit") -eq "Y") ) {
        Install-CreatorKit $Environment
    }

    if ( ( (Get-ConfigValue "Feature.Approvals") -eq "Y") ) {
        Enable-Approvals $UserUPN $Environment
    }

    Write-Host "Checking if Approvals kit solution installed"

    $assetsPath = [System.IO.Path]::Combine($PSScriptRoot,"..", "..", "assets")
    if ( -not (Test-Path($assetsPath)) ) {
        $assetsPath = [System.IO.Path]::Combine($PSScriptRoot, "..", "assets")
    }

    $installed = Invoke-SolutionInstalled $Environment "BusinessApprovalKit"

    if ( $installed -eq $False ) {
        Write-Host "Approvals Kit not installed"
        $approvalConnectionId = (Install-ConnectionSetup $UserUPN $Environment "approvals" $False | ConvertFrom-Json)
        $dataverseConnectionId = (Install-ConnectionSetup $UserUPN $Environment "commondataserviceforapps" $True | ConvertFrom-Json)
        $office365usersConnectionId = (Install-ConnectionSetup $UserUPN $Environment "office365users" $True | ConvertFrom-Json)

        if ( Invoke-IsError $approvalConnectionId ) {
            Write-Host $approvalConnectionId
            return
        }

        if ( Invoke-IsError $dataverseConnectionId ) {
            Write-Host $dataverseConnectionId
            return
        }

        if ( Invoke-IsError $office365usersConnectionId ) {
            Write-Host $office365usersConnectionId
            return
        }

        $settingsFile = [System.IO.Path]::Combine($assetsPath, "BusinessApprovalsKit.json")

        if ( -not (Test-Path($settingsFile)) ) {
            Write-Error "Unable to find BusinessApprovalsKit.json"
        }

        $settings = Get-Content $settingsFile
        $settings = $settings.replace('#Environment.EnvironmentUrl#', ( [System.UriBuilder]$Environment.EnvironmentURL ).Host )
        $settings = $settings.replace('#shared_approvals#', $approvalConnectionId.Id)
        $settings = $settings.replace('#shared_commondataserviceforapps#', $dataverseConnectionId.Id)
        $settings = $settings.replace('#shared_office365users#', $office365usersConnectionId.Id)

        $tempFile = [System.IO.Path]::Combine($assetsPath, "temp.json")
        $settings | Set-Content $tempFile

        $files = (Get-ChildItem -Path $assetsPath -Filter "BusinessApprovalKit*.zip")

        if ( $files.Count -le 0 ) {
            Write-Error "Unable to find install solution file"
            return
        }

        $sourceFile = [System.IO.Path]::Combine( $assetsPath, $files[0].Name )
        $installFile = [System.IO.Path]::Combine( $assetsPath, "temp.zip" )
        Copy-Item $sourceFile -Destination $installFile

        Invoke-UpdateOAuthSettings $installFile $Environment

        pac solution import --path $installFile --settings-file $tempFile
    } else {
        Write-Host "Approvals Kit is installed"
    }

    $installed = Invoke-SolutionInstalled $Environment "BusinessApprovalKit"
    if ( $installed -eq $True ) {
        Invoke-ApprovalsKitPostInstall $UserUPN $Environment
    } else {
        Write-Host "Approvals Kit is not installed"
    }
}

function Invoke-ApprovalsKitPostInstall {
    param (
        [Parameter(Mandatory)] [String] $UserUPN,
        $Environment
    )

    if ( $NULL -eq $Environment ) {
        Invoke-ConfigureUser $UserUPN
        $Environment = Invoke-UserDevelopmentEnvironment $UserUPN
        if (Get-Member -inputobject $Environment -name "error" -Membertype Properties) {
            return $Environment | ConvertTo-Json
        } 
        Invoke-SaveUserEnvironment $UserUPN $Environment
    }

    Invoke-PublishModelDrivenApp $Environment "cat_BusinessApprovalManagement"

    Invoke-ActivateFlows $UserUPN $Environment "BusinessApprovalKit" 

    # TODO
    # - Enable Child Flows improvements
    # - pac data import
    # - Convert to package?
}

function Install-ContosoCoffee {
    param (
        [Parameter(Mandatory)] [String] $UserUPN,
        $Environment
    )

    if ( $NULL -eq $Environment ) {
        Invoke-ConfigureUser $UserUPN
        $Environment = Invoke-UserDevelopmentEnvironment $UserUPN
        if (Get-Member -inputobject $Environment -name "error" -Membertype Properties) {
            return $Environment | ConvertTo-Json
        } 
        Invoke-SaveUserEnvironment $UserUPN $Environment
    }

    Write-Host "Checking if contoso coffee solution exists"
    $installed = Invoke-SolutionInstalled $Environment "ContosoCoffee"
    if ( -not $installed ) {
        $packageZip = [System.IO.Path]::Combine($PSScriptRoot,"..","..", "AppinaDay Trainer Package.zip")
        if ( -not (Test-Path $packageZip) ) {
            Write-Host "Downloading App In a Day Trainer Package"
            Invoke-WebRequest -Uri "https://aka.ms/appinadayTrainer" -OutFile $packageZip
        }
        $installZip = [System.IO.Path]::Combine($PSScriptRoot,"..","..", "ContosoCoffee_1_0_0_2.zip")
        
        if ( -not (Test-Path $installZip) ) {
            Invoke-ExtractZipContents $packageZip "Completed Lab Solution for students/Module 2/ContosoCoffee_1_0_0_2.zip" $installZip
        }
        Invoke-DevEnvironmentAuthUtility $UserUPN $Environment
        pac org select --environment $Environment.EnvironmentId
        pac solution import --path $installZip
    } else {
        Write-Host "Contoso coffee solution exists"
    }
}

function Invoke-SetupUserForWorkshop {
    param (
        [Parameter(Mandatory)] [String] $UserUPN,
        $Environment
    )

    if ( $NULL -eq $Environment ) {
        Invoke-ConfigureUser $UserUPN
        $Environment = Invoke-UserDevelopmentEnvironment $UserUPN
    }

    Invoke-WaitUnilEnvironmentExists $UserUPN $Environment

    Install-ContosoCoffee $UserUPN $Environment

    Install-ApprovalsKit $UserUPN $Environment
}

<#
.SYNOPSIS
    Waits for a development environment to exist.

.DESCRIPTION
    This function waits for a development environment to exist by continuously invoking the `Invoke-DevEnvironmentAuthUtility` function until the environment is found or a timeout occurs.

.PARAMETER UserUPN
    The User Principal Name (UPN) of the user who is trying to access the development environment.

.PARAMETER Environment
    The development environment object to wait for.

.EXAMPLE
    Invoke-WaitUnilEnvironmentExists -UserUPN "first.last@contoso.onmicrosoft.com" -Environment $devEnv

#>
function Invoke-WaitUnilEnvironmentExists {
    param (
        [Parameter(Mandatory)] [String] $UserUPN,
        [Parameter(Mandatory)] $Environment
    )

    $started = Get-Date
    $waiting = (Get-Date).Subtract($started).TotalMinutes
    while ( $true ) {
        $diff = (Get-Date).Subtract($started).ToString("hh\:mm\:ss")
        Write-Host "Waiting for environment to exist. Executing $diff"

        try {
            $result = Invoke-DevEnvironmentAuthUtility $UserUPN $Environment
            if ( $result ) {
                return
            } else {
                $attempts = $attempts + 1
                Write-Host "Waiting for development environment"
                Start-Sleep -Seconds 60
            }
        }
        catch {
            <#Do this if a terminating exception happens#>
        }

        Start-Sleep -Seconds 30
        $waiting = (Get-Date).Subtract($started).TotalMinutes

        if ( $waiting -gt 5 ) {
            Write-Error "Could not find development environment"
            return
        }
    }
}

function Invoke-UpdateOAuthSettings {
    param (
        [Parameter(Mandatory)] [String]$zipFile,
        [Parameter(Mandatory)] $Environment
    )

    $clientId = (Get-SecureValue "CLIENT_ID")

    if ( $NULL -eq $clientId ) {
        Write-Error "Missing secure variable CLIENT_ID"
        return
    }

    $secret = (Get-SecureValue "CLIENT_SECRET")

    if ( $NULL -eq $secret ) {
        Write-Error "Missing secure variable CLIENT_SECRET"
        return
    }

    $apiPropertiesFile = [System.IO.Path]::Combine((Get-AssetPath),"apiProperties.json")
    $current = (Get-Content $apiPropertiesFile | ConvertFrom-Json)
    
    $current.token.oAuthSettings.clientId = $clientId
    $current.token.oAuthSettings | Add-Member -MemberType NoteProperty -Name 'clientSecret' -Value $secret
    $current.token.oAuthSettings.properties.AzureActiveDirectoryResourceId =  $Environment.EnvironmentUrl
    $current.token.oAuthSettings.customParameters.tenantId.value = (az account show | ConvertFrom-Json).tenantId
    $current.token.oAuthSettings.customParameters.resourceUri.value = $Environment.EnvironmentUrl

    $newJson = ($current | ConvertTo-Json -compress -Depth 100)

    Set-ZipContents $zipFile "Connector/cat_approvals-20kit_connectionparameters.json" $newJson
}

function Invoke-SaveUserEnvironment {
    param (
        [Parameter(Mandatory)] [String]$UserUPN,
        [Parameter(Mandatory)] $Environment
    )

    $parts = $UserUPN -split "@"
    $name = $parts[0].ToUpper()
    $name = $name.Replace(".","_")
    
    Write-Host "Saved env:$name"
    Set-Item "env:$name" ($Environment | ConvertTo-Json)
}

<#
.SYNOPSIS
Publishes a model-driven app with the specified name to the specified environment.

.DESCRIPTION
This function publishes a model-driven app with the specified name to the specified environment using the Azure CLI and the Dynamics 365 Web API. It checks if the app is already published and if not, it publishes it.

.PARAMETER Name
The name of the model-driven app to publish.

.PARAMETER Environment
The environment where the app should be published. This should be an object with the following properties:
- EnvironmentUrl: the URL of the Dynamics 365 environment.

.EXAMPLE
Invoke-PublishModelDrivenApp -Name "MyApp" -Environment @{EnvironmentUrl="https://myorg.crm.dynamics.com"}

.NOTES
This function requires the Azure CLI. It also requires the user to have the necessary permissions to access the environment and publish apps.
#>
function Invoke-PublishModelDrivenApp {
    param (
        [Parameter(Mandatory)] $Environment,
        [Parameter(Mandatory)] [String] $Name
    )

    $token=(az account get-access-token --resource=$($Environment.EnvironmentUrl) --query accessToken --output tsv)
    $headers = @{Authorization="Bearer $token"}

    # https://learn.microsoft.com/en-us/power-apps/developer/model-driven-apps/create-manage-model-driven-apps-using-code#retrieve-unpublished-apps
    $appQueryUrl = "$($Environment.EnvironmentUrl)api/data/v9.2/appmodules?`$filter=uniquename%20eq%20%27$Name%27&`$select=appmoduleid,componentstate"
    
    Write-Host "Checking if can find application unpublished"
    $appModules = (Invoke-RestMethod -Method GET -Headers $headers -Uri $appQueryUrl) 
    
    $publishRequired = $false


    if ( $appModules.value.Count -eq 1 -and $appModules.value[0].componentstate -eq 0 ) {
        Write-Host "Found unpublished Application" 
        $publishRequired = $true
    }

    if ( $appModules.value.Count -eq 0 ) {
        # No published appmodule was found, check if the application is unpublished

        # https://learn.microsoft.com/en-us/power-apps/developer/model-driven-apps/create-manage-model-driven-apps-using-code#retrieve-unpublished-apps
        $appQueryUrl = "$($Environment.EnvironmentUrl)api/data/v9.2/appmodules/Microsoft.Dynamics.CRM.RetrieveUnpublishedMultiple()?`$filter=uniquename%20eq%20%27$Name%27&`$select=appmoduleid,componentstate"
        $appModules = (Invoke-RestMethod -Method GET -Headers $headers -Uri $appQueryUrl) 
    
        if ( $appModules.value.Count -eq 1 ) {
            Write-Host "Found unpublished Application" 
            $publishRequired = $true
        } else {
            # No application found that was either published or unpublished
            Write-Error "Unable to find $Name"
            return
        }
    }

    if ( $publishRequired ) {
        # https://learn.microsoft.com/power-apps/developer/model-driven-apps/create-manage-model-driven-apps-using-code#publish-your-model-driven-app
        Write-Host "Publishing Application"
        $content = @{ 
            ParameterXml = "<importexportxml><appmodules><appmodule>$($appModules.value[0].appmoduleid)</appmodule></appmodules></importexportxml>"
        }
    
        Invoke-RestMethod -Method POST -Headers $headers -Uri "$($Environment.EnvironmentUrl)/api/data/v9.0/PublishXml" -Body ($content | ConvertTo-Json) -ContentType "application/json; charset=utf-8"
    }
}

function Get-AssetPath {
    $assetsPath = [System.IO.Path]::Combine($PSScriptRoot,"..", "..", "assets")
    if ( -not (Test-Path($assetsPath)) ) {
        $assetsPath = [System.IO.Path]::Combine($PSScriptRoot, "..", "assets")
    }
    return $assetsPath
}

<#
.DESCRIPTION
This PowerShell function authenticates a user with a specified development environment and updates the Approvals Kit connector. It takes two parameters, `$UserUPN` and `$Environment`, which are the user's UPN and the development environment object, respectively.

.NOTES
Assumes that the Power Platform CLI is installed and the environment has been selected via pac auth select

.PARAMETER UserUPN
Specifies the user's UPN.

.PARAMETER Environment
Specifies the development environment object.

.INPUTS
None. You can't pipe objects to Invoke-ApprovalsConnectorUpdate.

.OUTPUTS
None. Invoke-ApprovalsConnectorUpdate does not return any objects.

.EXAMPLE
PS> Invoke-ApprovalsConnectorUpdate -UserUPN "first.last@contoso.com" -Environment $env
Authenticates the user with the specified development environment and updates the Approvals Kit connector.
#>
function Invoke-ApprovalsConnectorUpdate {
    param (
        [Parameter(Mandatory)] [String] $UserUPN,
        [Parameter(Mandatory)] $Environment
    )

    if ( $NULL -eq $Environment ) {
        $Environment = Invoke-UserDevelopmentEnvironment $UserUPN
    }

    $secret = Get-SecureValue "CLIENT_SECRET"

    if ( $NULL -eq $secret ) {
        Write-Error "Secure variable CLIENT_SECRET not set"
        return
    }

    $assetsPath = Get-AssetPath
    $tempFile = [System.IO.Path]::Combine($assetsPath, "connector", "temp.json")

    $connectorPath = [System.IO.Path]::Combine($assetsPath, "connector")
    $apiPropertiesFile = [System.IO.Path]::Combine($assetsPath, "apiProperties.json")
    $apiDefinitionFile = [System.IO.Path]::Combine($assetsPath, "connector", "apiDefinition.json")

    if ( -not (Test-Path($apiPropertiesFile)) ) {
        Write-Error "Unable to find apiProperties.json"
        return
    }

    $connectionParameters = Get-Content $apiPropertiesFile
    $clientId = Invoke-GetOrCreateApprovalsKitApplicationId
    $tenantId = (az account show | ConvertFrom-Json).tenantId
    $connectionParameters = $connectionParameters.replace('#ClientId#', $clientId )
    $connectionParameters = $connectionParameters.replace('#ClientSecret#', $secret )
    $connectionParameters = $connectionParameters.replace('#EnvironmentUrl#', $Environment.EnvironmentURL )
    $connectionParameters = $connectionParameters.replace('#TenantId#', $tenantId )

    # Check that the custom connector found (Without environment so that works with service principal)
    #$connectors = (pac connector list --environment $Environment.EnvironmentId --json | ConvertFrom-Json)
    $connectors = (pac connector list --json | ConvertFrom-Json)
    if ( $connectors.Count -gt 0) {
        $api = $connectors | Where-Object { $_.Name -eq "cat_approvals-20kit" }
        if ( $api.Count -eq 1 ) {
            Set-Content $tempFile $connectionParameters
            # Download the connector imported from the solution so we have the latest version
            pac connector download --connector-id $api[0].ConnectorId --outputDirectory $connectorPath

            # IMPORTANT NOTES: ----------------------------------------------------------------------------------
            # 1. Managed Solutions
            # If the solution is managed. the pac connector update may create an unmanaged layer
            # While this should be ok for a workshop situation for a DevOps pipeline this may have unintended
            # consequences. TODO Further validation required to look at this scenario
            #
            # 2. oAuthSettings clientSecret
            # The use of clientSecret in the apiDefinitionFile may change. Other options:
            # Environment Variables of type secret. This will require Azure Key Vault and if secret changes then the connector will need to be updated
            #
            # 3. Service Principal Auth
            # Have Removed environment so that can optionally work with Service Principal. Assumption that the environment to update is the selcted pac auth environment  
            # ----------------------------------------------------------------------------------------------  
            pac connector update --connector-id $api[0].ConnectorId --api-definition-file $apiDefinitionFile --api-properties-file $tempFile #--environment $Environment.EnvironmentId
            Remove-Item $tempFile
            Write-Host "Update complete"
        } else {
            Write-Error "Approvals Kit connector not found"
        }
        
    } else {
        Write-Error "No connectors found"
    }
}

<#
.DESCRIPTION
This PowerShell function gets the contents of a file in a zip archive. It takes three parameters: `$zipFile`, and `$fileName`, which are the path to the zip file, the name of the file to open respectively.

.NOTES
Assumes that the zip file exists and can be opened for writing.

.PARAMETER zipFile
Specifies the path to the zip file.

.PARAMETER fileName
Specifies the name of the file to update in the zip archive.

.INPUTS
None. You can't pipe objects to Get-ZipContents.

.OUTPUTS
None. Get-ZipContents return string contents of file inside the zip.

.EXAMPLE
PS> Get-ZipContents -zipFile "C:\temp\example.zip" -fileName "example.json"
Returns the contents of the file "example.json" in the zip archive located at "C:\temp\example.zip".

#> 
function Get-ZipContents {
    param (
        [Parameter(Mandatory)] [String] $zipFile,
        [Parameter(Mandatory)] [String] $fileName
    )

    # Open the zip file
    $stream = New-Object IO.FileStream $zipFile , 'Open', 'ReadWrite','Read'
    # Open the Zip file un update mode so that we can replace the file content
    $archive = New-Object System.IO.Compression.ZipArchive $stream, 'Update'
    # Open the existing zipped file
    $fileEntry = $archive.GetEntry($fileName)

    # Open a stream to the existing file
    $reader = New-Object System.IO.StreamReader $fileEntry.Open()
    $content = $reader.ReadToEnd()


    # Close and dispose of the created zip and file stream
    $reader.Close()
    $archive.Dispose()
    $stream.Dispose()

    return $content
}

<#
.DESCRIPTION
This PowerShell function extracts the contents of a file in a zip archive. It takes three parameters: `$zipFile`, and `$fileName`, which are the path to the zip file, the name of the file to open respectively.

.NOTES
Assumes that the zip file exists and can be opened for writing.

.PARAMETER zipFile
Specifies the path to the zip file.

.PARAMETER fileName
Specifies the name of the file to update in the zip archive.

.PARAMETER fileName
Specifies the name of the file to extract the the zip archive contents.

.INPUTS
None. You can't pipe objects to Invoke-ExtractZipContents.

.OUTPUTS
None. Invoke-ExtractZipContents does not have a return.

.EXAMPLE
PS> Invoke-ExtractZipContents -zipFile "C:\temp\example.zip" -fileName "example.json" output "test.json"
Extracts the contents of the file "example.json" in the zip archive located at "C:\temp\example.zip".

#> 
function Invoke-ExtractZipContents {
    param (
        [Parameter(Mandatory)] [String] $zipFile,
        [Parameter(Mandatory)] [String] $fileName,
        [Parameter(Mandatory)] [String] $output
    )

    # Open the zip file
    $stream = New-Object IO.FileStream $zipFile , 'Open', 'ReadWrite','Read'
    # Open the Zip file un update mode so that we can replace the file content
    $archive = New-Object System.IO.Compression.ZipArchive $stream, 'Update'
    # Open the existing zipped file
    $fileEntry = $archive.GetEntry($fileName)

    # Open a stream to the existing file
    $zipStream = $fileEntry.Open()

    $fileStream = [System.IO.File]::Create($output);
    $zipStream.Position = 0
    $zipStream.CopyTo($fileStream)

    # Close and dispose of the created zip and file stream
    $fileStream.Close()
    $zipStream.Close()
    $archive.Dispose()
    $stream.Dispose()

    return $content
}

<#
.DESCRIPTION
This PowerShell function updates the contents of a file in a zip archive. It takes three parameters: `$zipFile`, `$fileName`, and `$contents`, which are the path to the zip file, the name of the file to update, and the new contents to write to the file, respectively.

.NOTES
Assumes that the zip file exists and can be opened for writing.

.PARAMETER zipFile
Specifies the path to the zip file.

.PARAMETER fileName
Specifies the name of the file to update in the zip archive.

.PARAMETER contents
Specifies the new contents to write to the file.

.INPUTS
None. You can't pipe objects to Set-ZipContents.

.OUTPUTS
None. Set-ZipContents does not return any objects.

.EXAMPLE
PS> Set-ZipContents -zipFile "C:\temp\example.zip" -fileName "example.json" -contents "{`"key`": `"value`"}"
Updates the contents of the file "example.json" in the zip archive located at "C:\temp\example.zip".

#> 
function Set-ZipContents {
    param (
        [Parameter(Mandatory)] [String] $zipFile,
        [Parameter(Mandatory)] [String] $fileName,
        [Parameter(Mandatory)] [String] $contents
    )

    # Open the zip file
    $stream = New-Object IO.FileStream $zipFile , 'Open', 'ReadWrite','Read'
    # Open the Zip file un update mode so that we can replace the file content
    $archive = New-Object System.IO.Compression.ZipArchive $stream, 'Update'
    # Open the existing zipped file
    $fileEntry = $archive.GetEntry($fileName)
    # Open a stream to the existing file
    $update = New-Object System.IO.StreamWriter $fileEntry.Open()
    # Clear the current file contents
    $update.BaseStream.SetLength(0)
    # Write the contents of the stream with the provided contents
    $json = $contents | ConvertFrom-Json | ConvertTo-Json -Depth 100 -compress
    $newContent = [System.Text.Encoding]::UTF8.GetBytes($json)
    $update.BaseStream.Write($newContent, 0, $newContent.Length)


    # Close and dispose of the created zip and file stream
    $update.Close()
    $archive.Dispose()
    $stream.Dispose()
}

<#
.DESCRIPTION
This PowerShell function gets or creates an Azure Active Directory (AAD) application ID for the Approvalskit application. If the application already exists, the function returns its ID. If not, it creates a new application with the specified name and returns its ID. 

.NOTES
Assumes that the Azure CLI is installed and that the user has the necessary permissions to create AAD applications.

.PARAMETER None.

.INPUTS
None. You can't pipe objects to Invoke-GetOrCreateApprovalsKitApplicationId.

.OUTPUTS
System.String. Returns the application ID of the Approvalskit application.

.EXAMPLE
PS> Invoke-GetOrCreateApprovalsKitApplicationId
Gets or creates an Azure Active Directory application ID for the Approvalskit application.

#> 
function Invoke-GetOrCreateApprovalsKitApplicationId {
    $match = (az ad app list --display-name Approvalskit | ConvertFrom-Json)
    if ( $match.Count -eq 0 ) {
        az ad app create --display-name Approvalskit --web-redirect-uris https://global.consent.azure-apim.net/redirect
        $match = (az ad app list --display-name Approvalskit | ConvertFrom-Json)
    }
    if ( $match.Count -gt 0 ) {
        return $match[0].appId
    } else {
        Write-Error "Unable to get ApprovalKit app id"
        return ""
    }
}

<#
.DESCRIPTION
This PowerShell function opens a browser window to authenticate a user with a specified development environment. It takes two parameters, `$UserUPN` and `$Environment`, which are the user's UPN and the development environment object, respectively.

.NOTES
This function assumes that the Power Platform CLI is installed and that the `Validate-User` function is available. It also assumes that the `install.dll` file is available in the expected location.

.PARAMETER UserUPN
Specifies the user's UPN.

.PARAMETER Environment
Specifies the development environment object.

.INPUTS
None. You can't pipe objects to Invoke-OpenBrowser.

.OUTPUTS
None. Invoke-OpenBrowser does not return any objects.

.EXAMPLE
PS> Invoke-OpenBrowser -UserUPN "first.last@contoso.com" -Environment $env
Opens a browser window to authenticate the user with the specified development environment.

.DEPENDENCIES
This function depends on the `Validate-User` function being available.
#>
function Invoke-OpenBrowser {
    param (
        [Parameter(Mandatory)] [String] $UserUPN,
        $Environment
    )

    if ( $NULL -eq $Environment ) {
        $user = Validate-User $UserUPN
        if ( $NULL -eq $user ) {
            return $NULL
        }

        $password = Get-SecureValue "DEMO_PASSWORD"

        if ( [System.String]::IsNullOrEmpty($password) ) {
            Write-Error "DEMO_PASSWORD is not set"
            return
        }

        $displayName = $user.displayName
        $environmentName = "$displayName Dev"
        pac auth clear
        pac auth create -n User -un $UserUPN -p $password

        $envs = (pac admin list --json | ConvertFrom-Json) | Where-Object { $_.DisplayName -eq $environmentName  }
        if ( $envs.Count -eq 1 ) {
            $Environment = $envs[0]
            Invoke-DevEnvironmentAuth $UserUPN $Environment
        } else {
            Write-Error "Development environment not found"
            return
        }
    }
    $appPath = [System.IO.Path]::Join($PSScriptRoot,"..","install", "bin", "Debug", "net7.0", "install.dll")
    dotnet $appPath user start --upn $UserUPN --env $Environment.EnvironmentId --headless "N" --record "N"
}

<#
    .DESCRIPTION
    This PowerShell function retrieves the ID of a cloud flow with a specified name in a specified development environment. It takes two parameters: `$Environment` and `$CloudFlowName`, which are the development environment object and the name of the cloud flow, respectively.

    .NOTES
    Assumes that the Azure CLI is installed and that the user has the necessary permissions to access the development environment.
    
    .PARAMETER Environment
    Specifies the development environment object.

    .PARAMETER CloudFlowName
    Specifies the name of the cloud flow to retrieve the ID for.

    .INPUTS
    None. You can't pipe objects to Get-CloudFlowId.

    .OUTPUTS
    Returns the ID of the specified cloud flow, or "UNKNOWN" if the flow could not be found.

    .EXAMPLE
    PS> Get-CloudFlowId -Environment $env -CloudFlowName "MyCloudFlow"
    Retrieves the ID of the cloud flow with the specified name in the specified development environment.

#> 
function Get-CloudFlowId {
    param (
        [Parameter(Mandatory)] $Environment,
        [Parameter(Mandatory)] [String] $CloudFlowName
    )

    $token=(az account get-access-token --resource=$($Environment.EnvironmentUrl) --query accessToken --output tsv)
    $headers = @{Authorization="Bearer $token"}
    $flow = (Invoke-RestMethod -Method GET -Headers $headers -Uri "$($Environment.EnvironmentUrl)/api/data/v9.2/workflows?`$filter=name%20eq%20%27$CloudFlowName%27&`$select=workflowidunique") 

    if ( $flow.value.Count -gt 0 ) {
        return $flow.value[0].workflowidunique
    }

    # First attempt failed, use a retry pattern to lookup data
    $started = Get-Date
    $waiting = (Get-Date).Subtract($started).TotalMinutes
    while (  $flow.value.Count -eq 0 ) {
        $diff = (Get-Date).Subtract($started).ToString("hh\:mm\:ss")
        Write-Host "Waiting for flow to exist. Executing $diff"
        Start-Sleep -Seconds 30
        $flow = (Invoke-RestMethod -Method GET -Headers $headers -Uri "$($Environment.EnvironmentUrl)/api/data/v9.2/workflows?`$filter=name%20eq%20%27$CloudFlowName%27&`$select=workflowidunique") 
        
        if ( $waiting -gt 10 ) {
            break;
        }
    }

    if ( $flow.value.Count -gt 0 ) {
        return $flow.value[0].workflowidunique
    }
    
    Write-Error "Unable to find flow $CloudFlowName"
    return "UNKNOWN"
}

<#
    .DESCRIPTION
    This PowerShell function retrieves information about the runs of a specified cloud flow. It takes two parameters, `$UserUPN` and `$flowUrl`, which are the user's UPN and the URL of the cloud flow, respectively.

    .NOTES
    Assumes that the .NET Core runtime is installed and that the user has the necessary permissions to access the cloud flow. 

    .PARAMETER UserUPN
    Specifies the user's UPN.

    .PARAMETER flowUrl
    Specifies the URL of the cloud flow.

    .INPUTS
    None. You can't pipe objects to Get-CloudFlowRuns.

    .OUTPUTS
    Returns a JSON object containing information about the runs of the specified cloud flow.

    .EXAMPLE
    PS> Get-CloudFlowRuns -UserUPN "first.last@contoso.com" -flowUrl "https://contoso.com/flows/12345"
    Retrieves information about the runs of the specified cloud flow.

#> 
function Get-CloudFlowRuns {
    param (
        [Parameter(Mandatory)] [String] $UserUPN,
        [Parameter(Mandatory)] [String] $flowUrl
    )

    if ( [System.String]::IsNullOrEmpty($flowUrl) ) {
        Write-Error "Invalid cloud flow"
        return
    }

    $appPath = [System.IO.Path]::Join($PSScriptRoot,"..","install", "bin", "Debug", "net7.0", "install.dll")
    $result = (dotnet $appPath user api --upn $UserUPN --page $flowUrl --request "/runs") -split [Environment]::NewLine 

    $json = "{}"
    $match = $False
    ForEach ( $line in $result ) {
        if ( $line -eq "------------") {
            $match = $True
            continue;
        }

        if ( $match ) {
            $json = $line
            break
        }
    }

    return ($json | ConvertFrom-Json)
}

<#
    .DESCRIPTION
    This PowerShell function retrieves the URL of the details page for a specified approvals cloud flow. It takes two parameters, `$Environment` and `$CloudFlowId`, which are the development environment object and the ID of the approvals cloud flow, respectively.

    .NOTES
    Assumes that the Azure CLI is installed and that the user has the necessary permissions to access the development environment.

    .PARAMETER Environment
    Specifies the development environment object.

    .PARAMETER CloudFlowId
    Specifies the ID of the approvals cloud flow.

    .INPUTS
    None. You can't pipe objects to Get-ApprovalSetupFlowUrl.

    .OUTPUTS
    Returns the URL of the details page for the specified approvals cloud flow.

    .EXAMPLE
    PS> Get-ApprovalSetupFlowUrl -Environment $env -CloudFlowId "12345"
    Retrieves the URL of the details page for the specified approvals cloud flow.

#> 
function Get-ApprovalSetupFlowUrl {
    param (
        [Parameter(Mandatory)] $Environment,
        [Parameter(Mandatory)] [String] $CloudFlowId
    )

    if ( $NULL -eq $CloudFlowId) {
        Write-Error "No approvals cloud flow id provided"
        return $NULL
    }


    $token=(az account get-access-token --resource=$($Environment.EnvironmentUrl) --query accessToken --output tsv)
    $headers = @{Authorization="Bearer $token"}
    $UniqueName = "ApprovalsConnectorSetup"
    $solution = (Invoke-RestMethod -Method GET -Headers $headers -Uri "$($Environment.EnvironmentUrl)/api/data/v9.2/solutions?`$filter=uniquename%20eq%20%27$UniqueName%27&`$select=solutionid") 
    
    $solutionId = $solution.value[0].solutionid

    $envId = "$($Environment.EnvironmentId)"

    $url = "https://make.powerautomate.com/environments/$envId/solutions/$($solutionId)/flows/$($CloudFlowId)/details"

    $url = $url.Replace("environments/ ","environments/")

    return $url
}

<#
    .DESCRIPTION
    This PowerShell function retrieves the solution object for a specified unique name. It takes two parameters, `$Environment` and `$UniqueName`, which are the development environment object and the unique name of the solution, respectively.

    .NOTES
    Assumes that the Azure CLI is installed and that the user has the necessary permissions to access the development environment.

    .PARAMETER Environment
    Specifies the development environment object.

    .PARAMETER UniqueName
    Specifies the unique name of the solution.

    .INPUTS
    None. You can't pipe objects to Get-Solution.

    .OUTPUTS
    Returns the solution object for the specified unique name.

    .EXAMPLE
    PS> Get-Solution -Environment $env -UniqueName "ApprovalsConnectorSetup"
    Retrieves the solution object for the specified unique name.

#> 
function Get-Solution {
    param (
        [Parameter(Mandatory)] $Environment,
        [Parameter(Mandatory)] [String]$UniqueName
    )

    $token=(az account get-access-token --resource=$($Environment.EnvironmentUrl) --query accessToken --output tsv)
    $headers = @{Authorization="Bearer $token"}
    $UniqueName = "ApprovalsConnectorSetup"
    return (Invoke-RestMethod -Method GET -Headers $headers -Uri "$($Environment.EnvironmentUrl)/api/data/v9.2/solutions?`$filter=uniquename%20eq%20%27$UniqueName%27&`$select=solutionid") 
}

<#
    .DESCRIPTION
    This PowerShell function starts a cloud flow and checks if it has started running. It takes five parameters: `$UserUPN`, `$Environment`, `$CloudFlowId`, `$CloudFlowUrl`, and `$Attempt`. `$UserUPN` is the user's UPN, `$Environment` is the development environment object, `$CloudFlowId` is the ID of the cloud flow to start, `$CloudFlowUrl` is the URL of the cloud flow to start, and `$Attempt` is the number of attempts to start the flow.

    .NOTES
    Assumes that the Power Platform CLI is installed. This function also requires the `Get-CloudFlowRuns` function to be defined.

    .PARAMETER UserUPN
    Specifies the user's UPN.

    .PARAMETER Environment
    Specifies the development environment object.

    .PARAMETER CloudFlowId
    Specifies the ID of the cloud flow to start.

    .PARAMETER CloudFlowUrl
    Specifies the URL of the cloud flow to start.

    .PARAMETER Attempt
    Specifies the number of attempts to start the flow.

    .INPUTS
    None. You can't pipe objects to Invoke-CloudFlow.

    .OUTPUTS
    None. Invoke-CloudFlow does not return any objects.

    .EXAMPLE
    PS> Invoke-CloudFlow -UserUPN "first.last@contoso.com" -Environment $env -CloudFlowId "1234" -CloudFlowUrl "https://contoso.com/flows/1234" -Attempt 1
    Starts a cloud flow and checks if it has started running. If the flow is not running, it will attempt to start it again up to two times.

#>
function Invoke-CloudFlow {
    param (
        [Parameter(Mandatory)] [String] $UserUPN,
        [Parameter(Mandatory)] $Environment,
        $CloudFlowId,
        $CloudFlowUrl,
        $Attempt
    )

    if ( [System.String]::IsNullOrEmpty($CloudFlowId) ) {
        Write-Host "Missing Cloud Flow Id"
        return
    }

    if ( $Attempt -gt 2 ) {
        Write-Error "Flow not started. Attempts $Attempt"
        return
    }

    $appPath = [System.IO.Path]::Join($PSScriptRoot,"..","install", "bin", "Debug", "net7.0", "install.dll")
    dotnet $appPath flow start --upn $UserUPN --url $url

    Write-Host "Checking if flow was started"
    $runs = (Get-CloudFlowRuns $UserUPN $CloudFlowUrl).value
    $running = $runs | Where-Object { $NULL -ne $_.properties -and $_.properties.status -eq "Running" }

    $started = Get-Date
    $waiting = (Get-Date).Subtract($started).TotalMinutes
    while ( $runs.Count -eq 0 ) {
        $diff = (Get-Date).Subtract($started).ToString("hh\:mm\:ss")
        Write-Host "Waiting for flow to start run. Executing $diff"
        Start-Sleep -Seconds 30
        $runs = (Get-CloudFlowRuns $UserUPN $url).value
        $waiting = (Get-Date).Subtract($started).TotalMinutes

        if ( $waiting -gt 1 ) {
            break
        }
    }

    if ( $running.Count -eq 0 ) {
        if ( $NULL -eq $Attempt ) {
            $Attempt = 1
        }
        Write-Host "Restarting cloud flow"
        Invoke-CloudFlow $UserUPN $Environment $CloudFlowId $CloudFlowUrl ($Attempt + 1)
    } else {
        Write-Host "Cloud flow running"
    }
}

<#
    .DESCRIPTION
    This PowerShell function configures a user's development environment by adding the user to the makers group, adding a phone number, creating a development environment, enabling custom controls, installing the creator kit, and enabling approvals. It takes one parameter, `$UserUPN`, which specifies the user's UPN.

    .NOTES
    Assumes that the 
    - Power Platform CLI is installed
    - Azure CLM installed and the logged in user has at least security administrator rights
    - The config.json file exists to determine components to install for the user

    .PARAMETER UserUPN
    Specifies the user's UPN.

    .INPUTS
    None. You can't pipe objects to Invoke-ConfigureUser.

    .OUTPUTS
    None. Invoke-ConfigureUser does not return any objects.

    .EXAMPLE
    PS> Invoke-ConfigureUser -UserUPN "first.last@contoso.com"
    Configures the user's development environment.

#> 
function Invoke-ConfigureUser {
    param (
        [Parameter(Mandatory)] [String] $UserUPN
    )

    $user = Add-SecurityUserToMakersGroup $UserUPN

    if ( $NULL -eq $user ) {
        return $NULL
    }

    Add-SecurityUserPhone $UserUPN

    $devEnv = $NULL
    try {
        $devEnv = (Invoke-UserDevelopmentEnvironment $UserUPN)
    }
    catch {
        Write-Host "Unable to create development environment"
        $exitNow = $True
        Write-Host "An error occurred:"
        Write-Host $_
        return
    }
    
    if ( $NULL -eq $devEnv -or $NULL -eq $devEnv.EnvironmentId ) {
        return
    }
}

<#
.SYNOPSIS
Reset-UserDevelopmentEnvironment -UserUPN <string>

.DESCRIPTION
This function resets the development environment for a user by removing their current environment, creating a new environment, and setting up the user for a workshop.

.PARAMETER UserUPN
The User Principal Name (UPN) of the user whose development environment needs to be reset.

.EXAMPLE
Reset-UserDevelopmentEnvironment -UserUPN "first.last@contoso.onmicrosoft.com"

#>
function Reset-UserDevelopmentEnvironment {
    param (
        $UserUPN
    )

    Remove-UserDevelopmentEnvironment $UserUPN

    $Environment = Invoke-UserDevelopmentEnvironment $UserUPN

    Invoke-SetupUserForWorkshop $UserUPN $Environment
}
