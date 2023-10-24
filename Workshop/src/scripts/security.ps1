<#  
.SYNOPSIS  
    Sample Power Platform Demo security related Setup
.DESCRIPTION  
    This script provides a sample of deploying security related features. To use this file in Powershell . .\security.ps1 
.NOTES  
    File Name  : security.ps1  
    Author     : Grant Archibald - grant.archibald@microsoft.com
    Requires   : 
        PowerShell Core 7.1.3 or greater
        az cli 2.39.0 or greater
        pac cli 1.27.6+g9f62afb or greater

Copyright (c) Microsoft Corporation. All rights reserved.
Licensed under the MIT License.
#>

. $PSScriptRoot\common.ps1

<#
    .DESCRIPTION
    This PowerShell function logs in to Azure using the Azure CLI. It logs out any currently logged in users and then logs in with the `--allow-no-subscriptions` flag, which allows the user to log in without having any subscriptions.

    .NOTES
    This function assumes that the Azure CLI is installed.

    .PARAMETER None
    This function takes no parameters.

    .INPUTS
    None. You can't pipe objects to Invoke-AzureLogin.

    .OUTPUTS
    None. Invoke-AzureLogin does not return any objects.

    .EXAMPLE
    PS> Invoke-AzureLogin
    Logs in to Azure.

#> 
function Invoke-AzureLogin {
    az logout | Out-Null
    az login --allow-no-subscriptions
}

<#
    .DESCRIPTION
    This PowerShell function creates a security group for makers in the organization using the Azure CLI. If the group already exists, it does nothing.

    .NOTES
    This function assumes that the Azure CLI is installed.

    .PARAMETER None
    This function takes no parameters.

    .INPUTS
    None. You can't pipe objects to Add-SecurityMakersGroup.

    .OUTPUTS
    None. Add-SecurityMakersGroup does not return any objects.

    .EXAMPLE
    PS> Add-SecurityMakersGroup
    Creates a security group for makers in the organization.

#> 
function Add-SecurityMakersGroup {
    $makerGroup = az ad group list --filter "displayname eq 'Makers'"
    if ( $makerGroup.Count -eq 0 ) {
        az ad group create --display-name Makers --mail-nickname makers --description "Security group for Makers in the organization"
    } else {
        Write-Host "Makers group already exists"
    }
}

<#
    .DESCRIPTION
    This PowerShell function assigns the Power Apps Developer Plan and Flow Free licenses to the Makers security group using the Azure CLI and Microsoft Graph API. If the licenses are already assigned, it does nothing.

    .NOTES
    This function assumes that the Azure CLI is installed and that the user has the necessary permissions to access the Microsoft Graph API.

    .PARAMETER None
    This function takes no parameters.

    .INPUTS
    None. You can't pipe objects to Add-SecurityMakersGroupAssignDeveloperPlan.

    .OUTPUTS
    None. Add-SecurityMakersGroupAssignDeveloperPlan does not return any objects.

    .EXAMPLE
    PS> Add-SecurityMakersGroupAssignDeveloperPlan
    Assigns the Power Apps Developer Plan and Flow Free licenses to the Makers security group.

#> 
function Add-SecurityMakersGroupAssignDeveloperPlan {
    $token=(az account get-access-token --resource=https://graph.microsoft.com --query accessToken --output tsv)
    $headers = @{Authorization="Bearer $token"}
    # https://learn.microsoft.com/graph/api/subscribedsku-list?view=graph-rest-1.0&tabs=http
    $sku = (Invoke-RestMethod -Method GET -Headers $headers -Uri "https://graph.microsoft.com/beta/subscribedSkus" )
    $powerAppDeveloperPlan = $sku.value | Where-Object { $_.skuPartNumber -eq "POWERAPPS_DEV" }
    $freeFlow = $sku.value | Where-Object { $_.skuPartNumber -eq "FLOW_FREE" }
    if ( $NULL -eq $powerAppDeveloperPlan  ) {
        Write-Host "Power Apps Developer Plan License Not exist"
        return
    } 
    if ( $NULL -eq $freeFlow  ) {
        Write-Host "Power Automate Free License Not exist"
        return
    } 
    
    $group = (az ad group show -g makers) | ConvertFrom-Json
    # https://learn.microsoft.com/graph/api/group-get?view=graph-rest-1.0&tabs=http
    $assigned = (Invoke-RestMethod -Method GET -Headers $headers -Uri "https://graph.microsoft.com/v1.0/groups/$($group.id)?`$select=assignedLicenses")
    if ( $assigned.assignedLicenses.Count -lt 2 ) {
        $assignGroup = @{ 
            addLicenses=@( 
                @{ skuId=$powerAppDeveloperPlan.skuId },
                @{ skuId=$freeFlow.skuId }
            ) 
            removeLicenses=@() 
        }
        Write-Host "Power Apps Developer Plan / Flow Free License assigned to Makers"
        # https://learn.microsoft.com/graph/api/user-assignlicense?view=graph-rest-1.0&tabs=http
        Invoke-RestMethod -Method POST -Headers $headers -Uri "https://graph.microsoft.com/v1.0/groups/$($group.id)/assignLicense" -Body ( $assignGroup | ConvertTo-Json ) -ContentType 'application/json' | Out-Null
    } else {
        Write-Host "Power Apps Developer Plan / Automate License already assigned to Makers"
    }
}

<#
    .DESCRIPTION
    This PowerShell function validates a user's UPN and returns the user object. It takes one parameter, `$UserUPN`, which is the user's UPN.

    .NOTES
    This function assumes that the Azure CLI is installed and that the user running the script has logged in to Azure using the `az login` command.

    .PARAMETER UserUPN
    Specifies the user's UPN.

    .INPUTS
    None. You can't pipe objects to Validate-User.

    .OUTPUTS
    Returns the user object if the user is found, otherwise returns `$NULL`.

    .EXAMPLE
    PS> Validate-User -UserUPN "first.last@contoso.com"
    Validates the user's UPN and returns the user object if found.

#> 
function Validate-User {
    param (
        $UserUPN
    )

    $user = (az ad user list --upn $UserUPN | ConvertFrom-Json)
    if ( $NULL -eq $user ) {
        Write-Host "Unable to find user $UserUPN"
        if ( Get-ConfigValue("Feature.ResetUserPassword") -eq "Y" ) 
        {
            return Reset-User $UserUPN
        }
    }
    return $user
}

<#
    .DESCRIPTION
    This PowerShell function resets a user's name, location, and password. It takes three parameters, `$UserUPN`, `$First`, and `$Last`, which are the user's UPN, first name, and last name, respectively.

    .NOTES
    This function assumes that the Azure CLI is installed and that the user running the script has logged in to Azure using the `az login` command. It also assumes that the user has a demo password stored in SecureStore.

    .PARAMETER UserUPN
    Specifies the user's UPN.

    .PARAMETER First
    Specifies the user's first name. If not provided, the function will attempt to extract the first name from the UPN.

    .PARAMETER Last
    Specifies the user's last name. If not provided, the function will attempt to extract the last name from the UPN.

    .INPUTS
    None. You can't pipe objects to Reset-User.

    .OUTPUTS
    Returns the user object if the user is found, otherwise returns `$NULL`.

    .EXAMPLE
    PS> Reset-User -UserUPN "first.last@contoso.com" -First "First" -Last "Last"
    Resets the user's name, location, and password.

#> 
function Reset-User {
    param (
        $UserUPN,
        $First,
        $Last
    )

    $password = Get-SecureValue "DEMO_PASSWORD"

    if ( [System.String]::IsNullOrEmpty($password)) {
        Write-Error "Missing demo password"
        return $NULL
    }

    if ( [System.String]::IsNullOrEmpty($First) ) {
        $parts = $UserUPN -Split "@"
        $firstLast = $parts[0] -Split "\."
        $First =  $firstLast[0].ToLower()
        $First = $First.Substring(0,1).ToUpper() + $First.Substring(1)
        $Last =  $firstLast[1].ToLower()
        $Last = $Last.Substring(0,1).ToUpper() + $Last.Substring(1)
    }

    $user = (az ad user list --upn $UserUPN | ConvertFrom-Json)
    $name = "$First $Last"
    if ( $NULL -eq $user ) {
        Write-Host "Creating $name"
        az ad user create --display-name "$name" --user-principal-name $UserUPN --password $password
        $user = (az ad user list --upn $UserUPN | ConvertFrom-Json)
    }

    $userId = $($user[0].id)
    Write-Host "Reset Name and Location"

    $token=(az account get-access-token --resource=https://graph.microsoft.com --query accessToken --output tsv)
    $headers = @{
        Authorization="Bearer $token"
        ConsistencyLevel="eventual"
    }

    # https://learn.microsoft.com/previous-versions/azure/ad/graph/api/users-operations#ResetUserPassword
    Invoke-RestMethod -Method PATCH -Uri "https://graph.microsoft.com/v1.0/users/$userId" -Headers $headers -ContentType "application/json" -Body "{
        ""givenName"": ""$First"",
        ""surname"": ""$Last"",
        ""usageLocation"": ""US""
    }"

    Write-Host "Reset Password"

    $headers = @{
        Authorization="Bearer $token"
    }

    # https://learn.microsoft.com/previous-versions/azure/ad/graph/api/users-operations#ResetUserPassword
    $graphToken=(az account get-access-token --resource=https://graph.windows.net/ --query accessToken --output tsv)
    $graphHeaders = @{
        Authorization="Bearer $graphToken"
    }
    $body = @{
        passwordProfile = @{
            password = $password
            forceChangePasswordNextLogin =  $false
        }
    } | ConvertTo-Json

    Invoke-RestMethod -Method PATCH -Headers $graphHeaders -Uri "https://graph.windows.net/myorganization/users/$($userId)?api-version=1.6" -ContentType "application/json" -Body $body

    return (az ad user list --upn $UserUPN | ConvertFrom-Json)
}

<#
    .DESCRIPTION
    This PowerShell function adds a user to the Makers group and assigns the PowerApps Developer plan to them. It takes one parameter, `$UserUPN`, which specifies the user's UPN.

    .NOTES
    Assumes that the Azure CLI is installed 

    .PARAMETER UserUPN
    Specifies the user's UPN.

    .INPUTS
    None. You can't pipe objects to Add-SecurityUserToMakersGroup.

    .OUTPUTS
    Returns the validated user object.

    .EXAMPLE
    PS> Add-SecurityUserToMakersGroup -UserUPN "first.last@contoso.com"
    Adds the specified user to the Makers group and assigns the PowerApps Developer plan to them.
#> 

function Add-SecurityUserToMakersGroup {
    param (
        $UserUPN
    )

    $group = (az ad group show -g makers) | ConvertFrom-Json
    $user = Validate-User $UserUPN

    if ( $NULL -eq $user ) {
        return $NULL
    }

    $userId = $user.id
    $adminToken=(az account get-access-token --resource=https://admin.microsoft.com --query accessToken --output tsv)
    $adminHeaders = @{
        Authorization="Bearer $adminToken"
    }
    $token=(az account get-access-token --resource=https://graph.microsoft.com --query accessToken --output tsv)
    $headers = @{
        Authorization="Bearer $token"
        ConsistencyLevel="eventual"
    }

    # https://learn.microsoft.com/graph/aad-advanced-queries?tabs=http#user-properties
    $url = "https://admin.microsoft.com/admin/api/users/$userId/assignedproducts"
    $response = (Invoke-RestMethod -Method GET -Headers $adminHeaders -Uri $url)   
    $match = ($response.Products | Where-Object { $_.SkuPartNumber -eq "POWERAPPS_DEV" -and $_.IsSelected -eq $True })

    if ( $match.Count -eq 0 ) {
        Write-Host "Adding user to Makers group"
        # https://learn.microsoft.com/graph/api/group-post-members?view=graph-rest-1.0&tabs=http
        Invoke-RestMethod -Method POST -Headers $headers -Uri "https://graph.microsoft.com/v1.0/groups/$($group.id)/members/`$ref" -Body ( @{ "@odata.id" = "https://graph.microsoft.com/v1.0/users/$($user.id)" } | ConvertTo-Json ) -ContentType 'application/json'
        $started = Get-Date
        while ( $match.Count -eq 0 ) {
            $diff = (Get-Date).Subtract($started).ToString("hh\:mm\:ss")
            Write-Host "Waiting for licenses to be assigned. Executing $diff"
            Start-Sleep -Seconds 30
            $response = (Invoke-RestMethod -Method GET -Headers $adminHeaders -Uri $url)   
            $match = ($response.Products | Where-Object { $_.SkuPartNumber -eq "POWERAPPS_DEV" -and $_.IsSelected -eq $True })
        
            if ( $waiting -gt 10 ) {
                break
            }
        }
        
    } else {
        Write-Host "User already in Makers group"
    }

    return $user
}

function Get-AdminAccessToken {

    $tenantId = ( az account show | ConvertFrom-Json).tenantId
    # https://learn.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-client-creds-grant-flow
    $url = "https://login.microsoftonline.com/$tenantId/oauth2/v2.0/token"

    $body = @{
        client_id = (Get-SecureValue "ADMIN_APP_ID")
        scope= "https://graph.microsoft.com/.default"
        client_secret = (Get-SecureValue "ADMIN_APP_SECRET")
        grant_type = "client_credentials"
    }

    $response = (Invoke-RestMethod -Method POST -Uri $url -ContentType "application/x-www-form-urlencoded" -Body $body)

    return $response.access_token
}

<#
    .DESCRIPTION
    This PowerShell function adds phone authentication to a user's account if it is not already set up. It takes one parameter, `$UserUPN`, which specifies the user's UPN.

    .NOTES
    Assumes that the Azure CLI is installed.

    .PARAMETER UserUPN
    Specifies the user's UPN.

    .INPUTS
    None. You can't pipe objects to Add-SecurityUserPhone.

    .OUTPUTS
    None. Add-SecurityUserPhone does not return any objects.

    .EXAMPLE
    PS> Add-SecurityUserPhone -UserUPN "first.last@contoso.com"
    Adds phone authentication to the specified user's account if it is not already set up.
#>
function Add-SecurityUserPhone {
    param (
        $UserUPN
    )

    $user = Validate-User $UserUPN
    if ( $NULL -eq $user ) {
        return $NULL
    }

    $userId = $user.id

    # Assume that the Application Token has been given and granted application permissions of UserAuthenticationMethod.ReadWrite.All
    $graphToken=( Get-AdminAccessToken )
    $graphHeaders = @{
        Authorization="Bearer $graphToken"
    }

    # https://learn.microsoft.com/en-us/graph/api/phoneauthenticationmethod-get?view=graph-rest-beta&tabs=http
    $phoneMethods = Invoke-RestMethod -Method GET -Headers $graphHeaders -Uri "https://graph.microsoft.com/v1.0/users/$userId/authentication/phoneMethods"
    if ( $phoneMethods.value.Count -eq 0 ) {
        Write-Host "Adding Phone Authentication"
        $results = Invoke-RestMethod -Method POST -Headers $graphHeaders -Uri "https://graph.microsoft.com/v1.0/users/$userId/authentication/phoneMethods" -Body "{""phoneNumber"":""+1 4255551223"",""phoneType"":""mobile""}" -ContentType 'application/json'
    } else {
        Write-Host "Phone Authentication already allocated"
    }
}

function Add-Manager {
    param (
        $UserUPN,
        $UserManagerUPN
    )

    $user = Validate-User $UserUPN
    if ( $NULL -eq $user ) {
        return $NULL
    }

    $manager = Validate-User $UserManagerUPN
    if ( $NULL -eq $manager ) {
        return $NULL
    }

    $userId = $user.id
    $managerId = $manager.id

    # Assume that the Application Token has been given and granted application permissions of User.ReadWrite.All
    $graphToken=( Get-AdminAccessToken )
    $graphHeaders = @{
        Authorization="Bearer $graphToken"
    }

    Write-Host "Updating manager"
    # https://learn.microsoft.com/en-us/graph/api/user-post-manager?view=graph-rest-1.0&tabs=http
    $results = Invoke-RestMethod -Method PUT -Headers $graphHeaders -Uri "https://graph.microsoft.com/v1.0/users/$userId/manager/`$ref" -Body "{""@odata.id"":""https://graph.microsoft.com/v1.0/users/$managerId""}" -ContentType 'application/json'
}