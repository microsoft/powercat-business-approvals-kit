<#  
.SYNOPSIS  
    Sample Power Platform Demo Utilties PowerShell Class
.DESCRIPTION  
    This script common PowerShell Module with suage pattern Import-Module $PSScriptRoot\UtilityMethods.psm1. 
    
    Class methods are exported as PowerShell functions so that a using statement is not needed and class functionality is avilable to other PowerShell scripts 

    Note: Any changes made to this file in a PowerShell session will require a new PowerShell instance created for the changes to be made
.NOTES  
    File Name  : UtilityMethods.psm1  
    Author     : Grant Archibald - grant.archibald@microsoft.com
    Requires   : 
        PowerShell Core 7.1.3 or greater
        az cli 2.39.0 or greater
        pac cli 1.27.6+g9f62afb or greater

Copyright (c) Microsoft Corporation. All rights reserved.
Licensed under the MIT License.
#>

<#
.SYNOPSIS
    This class contains utility methods for various tasks.

.DESCRIPTION
    The UtilityMethods class provides a set of utility methods that can be used for various tasks. 
    These methods are designed to exclude standard output from commands in the return results.

#>
class UtilityMethods {

    <#
    .SYNOPSIS
    This function retrieves a secure value from a secrets file.

    .DESCRIPTION
    The GetSecureValue function takes a string parameter called $Name, which is the name of the value to retrieve. If the secrets file exists, the function changes the current location to the secrets directory and retrieves the value using the SecureStore command with the specified keyfile. If the secrets file does not exist, the function retrieves the value using the SecureStore command with the specified keyfile.

    .PARAMETER Name
    The name of the value to retrieve.

    .EXAMPLE
    PS C:\> GetSecureValue -Name "MySecretValue"

    This example retrieves the value named "MySecretValue" from the secrets file.

    .OUTPUTS
    A string value representing the retrieved secure value.
    #>
    static [string] GetSecureValue([String] $Name) {
        $secureFolder = @(
            [System.IO.Path]::Combine($PSScriptRoot, "..", "..","secure"),
            [System.IO.Path]::Combine((Get-Location),"secure")
        )
        foreach ($folder in $secureFolder) {
            if ( Test-Path $folder ) {
                Push-Location 
                Set-Location $folder
                $value = ( SecureStore get $Name --keyfile secret.key  )
                Pop-Location
                return $value
            }
        }
        
        return ( SecureStore get $Name --keyfile secret.key  )      
    }

    <#
    .SYNOPSIS
        Returns the developer environment for the specified user.

    .DESCRIPTION
        The GetDeveloperEnvironment method returns the developer environment for the specified user by authenticating the pac cli and retrieving the environment information. 
        If the display name is not provided, it will be retrieved from the Azure AD user list using the specified user UPN.
        The method also checks for the DEMO_PASSWORD environment variable and returns an error if it is missing.
        The environment name is constructed from the display name and the word "Dev".

    .PARAMETER UserUPN
        The user's UPN.

    .PARAMETER displayName
        The user's display name.

    .OUTPUTS
        Returns a JSON object containing the environment information.

    .EXAMPLE
        $env = [UtilityMethods]::GetDeveloperEnvironment("user@contoso.com", "John Doe")
        $env

    #>
    static [string]GetDeveloperEnvironment($UserUPN, $displayName) {
        if ( [System.String]::IsNullOrEmpty($displayName)) {
            $user = (az ad user list --upn $UserUPN | ConvertFrom-Json)
            $displayName = $user[0].displayName
        }

        $password = [UtilityMethods]::GetSecureValue("DEMO_PASSWORD")
    
        if ( [System.String]::IsNullOrEmpty($password) ) {
            return '{"error":"Missing secure variable DEMO_PASSWORD"}'
        }

        Write-Host "Authenticating pac cli as $displayName"
        pac auth clear
        pac auth create --name User --username $UserUPN --password $password
    
        $auth = (pac auth list)
        if ( $auth -like  "*No profiles were found on this computer*" ) {
            return  '{"error":"Login failed"}'
        }
    
        $environmentName = "$displayName Dev"
        $envs = ((pac admin list --json | ConvertFrom-Json) | Where-Object { $_.DisplayName -eq $environmentName  })
    
        return $envs | ConvertTo-Json
    }

    <#
    .SYNOPSIS
        Returns or creates the development environment for the specified user.

    .DESCRIPTION
        The GetOrCreateDevelopmentEnvironment method returns the development environment for the specified user if it already exists, or creates it if it does not. 
        The method first retrieves the user's display name from the Azure AD user list using the specified user UPN.
        If the "Feature.ResetUserPassword" configuration value is set to "Y", the method resets the user's password.
        The method then checks for an existing development environment for the user using the GetDeveloperEnvironment method.
        If an error occurs during the retrieval of the environment information, it is returned as a JSON object.
        If no environment is found, the method creates a new development environment with the specified name using the pac admin create command.
        The method then authenticates the pac cli for the new environment using the DevEnvironmentAuth method.
        If multiple environments are found, an error is returned as a JSON object.
        If a single environment is found, the method authenticates the pac cli for the environment using the DevEnvironmentAuth method and returns the environment information as a JSON object.

    .PARAMETER UserUPN
        The user's UPN.

    .OUTPUTS
        Returns a JSON object containing the environment information.

    .EXAMPLE
        $env = [UtilityMethods]::GetOrCreateDevelopmentEnvironment("user@contoso.com")
        $env

    #>
    static [string] GetOrCreateDevelopmentEnvironment($UserUPN) {
        $user = (az ad user list --upn $UserUPN | ConvertFrom-Json)
        $displayName = $user[0].displayName
        $environmentName = "$displayName Dev"
    
        if ( ([UtilityMethods]::GetConfigValue("Feature.ResetUserPassword")) -eq "Y" ) 
        {
            Write-Host "Reset user"
            Reset-User $UserUPN | Out-Null
        }
    
        $envs = ([UtilityMethods]::GetDeveloperEnvironment($UserUPN,$displayName) | ConvertFrom-Json)

        if (Get-Member -inputobject $envs -name "error" -Membertype Properties) {
            return $envs | ConvertTo-Json
        } 
        
        if (  $NULL -eq $envs -or $envs.Count -eq 0 ) {
            Write-Host "Creating development environment $environmentName"
            pac admin create --name $environmentName --type Developer
            $envs = (pac admin list --json | ConvertFrom-Json) | Where-Object { $_.DisplayName -eq $environmentName  }
            if ( $envs.Count -eq 0 ) {
                Write-Error "Unable to create $environmentName"
                return @{
                    error = "Unable to create $environmentName"
                } | ConvertTo-Json
            }
            [UtilityMethods]::DevEnvironmentAuth($UserUPN,$envs[0])
            return $envs[0] | ConvertTo-Json
        } else {
            if ( $envs.Count -gt 1 ) {
                return @{
                    error = "Duplicate environments exist"
                } | ConvertTo-Json
            }
            Write-Host "$environmentName Exists"
            [UtilityMethods]::DevEnvironmentAuth($UserUPN,$envs[0])
            return $envs[0] | ConvertTo-Json
        }
    }

    <#
    .SUMMARY
        Authenticates the pac cli for the specified development environment.

    .DESCRIPTION
        The DevEnvironmentAuth method authenticates the pac cli for the specified development environment using the pac auth create command.
        The method first retrieves the existing authentication profiles using the pac auth list command.
        The method then validates the specified user using the ValidateUser method and retrieves the user's display name.
        If an error occurs during the validation of the user, it is returned as a JSON object.
        The method then checks if the specified development environment is already authenticated by searching for a matching environment name in the existing authentication profiles.
        If the environment is not already authenticated, the method creates a new authentication profile using the pac auth create command.
        The method then selects the specified environment using the pac org select command.

    .PARAMETER UserUPN
        The user's UPN.

    .PARAMETER Environment
        The environment information.

    .OUTPUTS
        Returns "true" if the authentication was successful.

    .EXAMPLE
        $env = [UtilityMethods]::GetDeveloperEnvironment("user@contoso.com", "John Doe")
        [UtilityMethods]::DevEnvironmentAuth("user@contoso.com", $env)

    #>
    static [string]DevEnvironmentAuth($UserUPN,$Environment) {
        $existingAuth = (pac auth list)
    
        $user = [UtilityMethods]::ValidateUser($UserUPN) | ConvertFrom-Json
        if (Get-Member -inputobject $user -name "error" -Membertype Properties) {
            return $user | ConvertTo-Json
        } 
    
        $displayName = $user[0].displayName
        $developmentEnvironmentName = "$displayName Dev"
        $match = $existingAuth | Where-Object { $_.IndexOf($developmentEnvironmentName) -gt 0 }
    
        if ( $match.Count -eq 0 ) {
            $password = [UtilityMethods]::GetSecureValue("DEMO_PASSWORD")
            pac auth create -n User -un $UserUPN -p $password -env $Environment.EnvironmentId
        } else {
            Write-Host "Already authenticated with environment $developmentEnvironmentName"
        }
    
        pac org select --environment $Environment.EnvironmentId

        $match = $existingAuth | Where-Object { $_.IndexOf($developmentEnvironmentName) -gt 0 -and $_.IndexOf("*") -gt 0 }
        if ( $match.Count -eq 0 ) {
            return "false"
        } else {
            return "true"
        }
    }

    <#
    .SYNOPSIS
    Retrieves a configuration value from a JSON file.

    .DESCRIPTION
    This method reads a JSON file located at the current directory and returns the value associated with the specified configuration name.

    .PARAMETER Name
    The name of the configuration value to retrieve.

    .EXAMPLE
    GetConfigValue -Name "DatabaseConnectionString"
    Returns the value of the "DatabaseConnectionString" configuration setting.

    .NOTES
    This method requires a JSON file named "config.json" to be present in the current directory. The JSON file should contain key-value pairs representing configuration settings.
    #>
    static [string] GetConfigValue($Name) {

        $configFile = [System.IO.Path]::Join( ( Get-Location ),"config.json")
        if ( Test-Path $configFile ) {
            $config = Get-Content $configFile | ConvertFrom-Json
            return ( $config | Select-Object -ExpandProperty $Name )
        }
    
        return $NULL       
    }

    <#
    .SYNOPSIS
    Validates a user's UPN and returns their user object.

    .DESCRIPTION
    This method uses the Azure CLI to retrieve the user object associated with the specified UPN. If the user cannot be found, an error message is displayed and, depending on configuration settings, the user's password may be reset.

    .PARAMETER UserUPN
    The user's UPN to validate.

    .EXAMPLE
    ValidateUser -UserUPN "jdoe@example.com"
    Returns the user object associated with the "jdoe@example.com" UPN.

    .NOTES
    This method requires the Azure CLI to be installed and configured on the local machine. If the user is not found, an error message is displayed and, depending on configuration settings, the user's password may be reset.
    #>
    static [string] ValidateUser($UserUPN) {
        $user = (az ad user list --upn $UserUPN | ConvertFrom-Json)
        if ( $NULL -eq $user ) {
            Write-Host "Unable to find user $UserUPN"
            if ( [UtilityMethods]::GetConfigValue("Feature.ResetUserPassword") -eq "Y" ) 
            {
                return [UtilityMethods]::ResetUser($UserUPN)
            }
        }
        return $user | ConvertTo-Json
    }

    <#
    .SYNOPSIS
    Resets a user's password and updates their name and location.

    .DESCRIPTION
    This method resets the password for the specified user and updates their name and location in Azure Active Directory. If the user does not exist, a new user is created with the specified UPN, first name, and last name.

    .PARAMETER UserUPN
    The user's UPN to reset.

    .PARAMETER First
    The user's first name. If not specified, the first name is derived from the UPN.

    .PARAMETER Last
    The user's last name. If not specified, the last name is derived from the UPN.

    .EXAMPLE
    ResetUser -UserUPN "jdoe@example.com" -First "John" -Last "Doe"
    Resets the password for the "jdoe@example.com" user and updates their name to "John Doe".

    ResetUser -UserUPN "john.doe@example.com"
    Resets the password for the "john.doe@example.com" user and updates their name to "John Doe".

    .NOTES
    This method requires the Azure CLI to be installed and configured on the local machine. It also requires the user to have a phone number associated with their account for multi-factor authentication. The method uses the Azure AD Graph API and Microsoft Graph API to update the user's information and reset their password.
    #>
    static [string] ResetUser($UserUPN, $First, $Last) {
        $password = [UtilityMethods]::GetSecureValue("DEMO_PASSWORD")
        if ( [System.String]::IsNullOrEmpty($password)) {
            return @{
                error = "Missing demo password"
            }
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

        $graphToken=(az account get-access-token --resource=https://graph.windows.net/ --query accessToken --output tsv)
        $graphHeaders = @{
            Authorization="Bearer $graphToken"
        }

        # https://learn.microsoft.com/previous-versions/azure/ad/graph/api/users-operations#get-a-user--
        $userInfo = Invoke-RestMethod -Method GET -Headers $graphHeaders -Uri "https://graph.windows.net/myorganization/users('$userId')?api-version=1.6-internal"
        
        if ( $NULL -eq $userInfo.strongAuthenticationDetail.verificationDetail ) {
            $azureToken=(az account get-access-token --resource=74658136-14ec-4630-ad9b-26e160ff0fc6 --query accessToken --output tsv)
            $azureHeader = @{
                Authorization="Bearer $azureToken"
            }
            Write-Host "Adding Phone Authentication"
            # TODO: Review and determine alternate method to set Mfa Properties for Phone number
            Invoke-RestMethod -Method PATCH -Headers $azureHeader -Uri "https://main.iam.ad.ext.azure.com/api/UpdateUser/$userId/MfaProperties" -Body "{""authenticationPhoneNumber"":""+1 4255551223"",""authenticationEmail"":null,""authenticationAlternativePhoneNumber"":null,""isAuthenticationContactInfoUpdated"":true}" -ContentType 'application/json'
        }
    
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
    
        return (az ad user list --upn $UserUPN)
    }

    <#
    .DESCRIPTION
    This PowerShell function retrieves a list of connections for a specified development environment. It takes one parameter, `$Environment`, which is the development environment object.

    .NOTES
    Assumes that the Power Platform CLI is installed
    
    .PARAMETER Environment
    Specifies the development environment object.

    .INPUTS
    None. You can't pipe objects to Get-Connections.

    .OUTPUTS
    Returns an array of connection objects with the following properties:
    - Id: the connection ID
    - Name: the connection name
    - API: the connection API
    - Status: the connection status

    .EXAMPLE
    PS> Get-Connections -Environment $env
    Retrieves a list of connections for the specified development environment.

    #> 
    static [System.Collections.ArrayList] GetConnections($Environment) {
        $items = [System.Collections.ArrayList]@()

        $auth = (pac auth list)

        if ( ( $auth | Where-Object { $_.IndexOf($Environment.EnvironmentURL) -gt 0 } ).Count -eq 0 ) {
            Write-Error "Missing authentication for requested environment"
            return $items
        }

        $connections = (pac connection list --environment $Environment.EnvironmentId)
        
        if ( $connections.Count -gt 1 ) {
            $index = 0
            foreach ($connection in $connections)
            {
                if ( $connection -like "*Error:*") {
                    return [System.Collections.ArrayList]@()
                }
                if ( $index -gt 0) {
                    $parts = ($connection.Split(' ')).Trim() | Where-Object { $_ }
                    if ( $parts.Count -ge 4) {
                        $connectionInfo = [PSCustomObject]@{ 
                            Id = $parts[0]
                            Name = $parts[1]
                            API = $parts[2]
                            Status = $parts[3]
                        }
                        $items.Add($connectionInfo) | out-null
                    }
                }
                $index = $index + 1
            }
        }

        return $items
    }

    
<#
    .DESCRIPTION
    This PowerShell function adds a new connection of a specified type to a development environment for a given user. It takes three parameters: `$UserUPN`, `$Environment`, and `$ConnectorName`, which are the user's UPN, the development environment object, and the name of the connector to create, respectively.

    .NOTES
    Assumes that the Power Platform CLI is installed and that the specified connector type is installed in the development environment.
    
    .PARAMETER UserUPN
    Specifies the user's UPN.

    .PARAMETER Environment
    Specifies the development environment object.

    .PARAMETER ConnectorName
    Specifies the name of the connector to create.

    .PARAMETER waitForSignIn
    Specifies if should wait to sign in with the current user

    .INPUTS
    None. You can't pipe objects to Add-Connection.

    .OUTPUTS
    None. AddConnection does not return any objects.

    .EXAMPLE
    PS> [UtilityMethods::AddConnection("first.last@contoso.com", $env, "approvals")
    Adds a new connection of type "MyConnector" to the specified development environment for the specified user.

    #> 
    static [object] AddConnection([String] $UserUPN,$Environment, [String] $ConnectorName, $waitForSignIn) {
        $connections = [UtilityMethods]::GetConnections($Environment)

        $match = $connections | Where-Object { $_.API.IndexOf("shared_$ConnectorName") -gt 0 -and $_.Status -eq "Connected" }

        if ( $match.Count -gt 0 ) {
            Write-Output "Connection of type $ConnectorName already exists"
            return $match[0]
        }

        $appPath = [System.IO.Path]::Join($PSScriptRoot,"..","install", "bin", "Debug", "net7.0", "install.dll")
        Push-Location
        Set-Location ([System.IO.Path]::Join($PSScriptRoot,"..", ".."))
        dotnet $appPath connection create --upn $UserUPN --env $Environment.EnvironmentId --connector $ConnectorName --signIn $waitForSignIn --record Y
        Pop-Location

        $started = Get-Date
        $waiting = (Get-Date).Subtract($started).TotalMinutes
        while ( $True ) {
            $connections = [UtilityMethods]::GetConnections($Environment)
            $match = $connections | Where-Object { $_.API.IndexOf("shared_$ConnectorName") }

            if ( $match.Count -gt 0 ) {
                return $match[0]
            } else {
                $diff = (Get-Date).Subtract($started).ToString("hh\:mm\:ss")
                Write-Host "Waiting for connection. Executing $diff"
                Start-Sleep -Seconds 2
            }

            $waiting = (Get-Date).Subtract($started).TotalMinutes

            if ( $waiting -gt 2 ) {
                break
            }
        }

        if ( $match.Count -eq 0 ) {
            Write-Error "Unable to create $ConnectorName"
            return @{ error = "Unable to create $ConnectorName"}
        }

        return $match[0]
    }



    <#
    .DESCRIPTION
    This PowerShell function installs a Power Platform connection for a specified connector in a specified development environment and user. It takes three parameters, `$UserUPN`, `$Environment`, and `$connector`, which are the user's UPN, the development environment object, and the name of the connector, respectively.

    .NOTES
    Assumes that the Power Platform CLI is installed.

    .PARAMETER UserUPN
    Specifies the user's UPN to create the connection fo

    .PARAMETER Environment
    Specifies the development environment object.

    .PARAMETER connector
    Specifies the name of the connector.

    .PARAMETER waitForSignIn
    Specifies if should wait for sign in when creating the connection.

    .INPUTS
    None. You can't pipe objects to Install-ConnectionSetup.

    .OUTPUTS
    Returns the ID of the created / existing connection.

    .EXAMPLE
    PS> [UtilityMethods]::SetupConnection("first.last@contoso.com",$env,"approvals",$False)
    Installs a connection setup for the approvals connector in the specified development environment.

    #> 
    static [string]SetupConnection([String] $UserUPN, $Environment, [String] $connector, $waitForSignIn) {
        Write-Host "Checking for ${connector} connection"

        $connections = [UtilityMethods]::GetConnections($Environment)
        $match = $connections | Where-Object { $_.API.IndexOf("shared_${connector}") -gt 0 -and $_.Status -eq "Connected" }

        $attempt = 0
        while ( $match.Count -eq 0 -and $attempt -le 5 ) {
            [UtilityMethods]::AddConnection($UserUPN,$Environment,$connector,$waitForSignIn)

            $connections = [UtilityMethods]::GetConnections($Environment)
            $match = $connections | Where-Object { $_.API.IndexOf("shared_${connector}") -gt 0 }

            $started = Get-Date
            $waiting = (Get-Date).Subtract($started).TotalMinutes
            while ( $match.Count -eq 0 ) {
                $diff = (Get-Date).Subtract($started).ToString("hh\:mm\:ss")
                Write-Host "Polling for ${connector} connection. Executing $diff"
                Start-Sleep -Seconds 30
                $connections = [UtilityMethods]::GetConnections($Environment)
                $match = $connections | Where-Object { $_.API.IndexOf("shared_${connector}") -gt 0 }
                $waiting = (Get-Date).Subtract($started).TotalMinutes
                if ( $waiting -gt 2 ) {
                    $attempt = $attempt + 1
                    break
                }
            }
        }

        if ( $match.Count -gt 0) {
            return @{
                Id = $match[0].Id
            } | ConvertTo-Json
        } else {
            return @{
                error = "Unable to find ${connector} connection"
            } | ConvertTo-Json
        }
    }
}

function Invoke-DevEnvironmentAuthUtility {
    param (
        $UserUPN, $Environment
    )
    $result = [UtilityMethods]::DevEnvironmentAuth($UserUPN,$Environment) | ConvertFrom-Json
    return $result
}

function Invoke-GetDeveloperEnvironment{
    param (
        $UserUPN, $displayName
    )
    $result = [UtilityMethods]::GetDeveloperEnvironment($UserUPN,$displayName) | ConvertFrom-Json
    return $result
}

function Invoke-GetOrCreateDevelopmentEnvironment {
    param (
        $UserUPN
    )
    return [UtilityMethods]::GetOrCreateDevelopmentEnvironment($UserUPN) | ConvertFrom-Json
}

<#
    .DESCRIPTION
    This PowerShell function installs a Power Platform connection for a specified connector in a specified development environment and user. It takes three parameters, `$UserUPN`, `$Environment`, and `$connector`, which are the user's UPN, the development environment object, and the name of the connector, respectively.

    .NOTES
    Assumes that the Power Platform CLI is installed.

    .PARAMETER UserUPN
    Specifies the user's UPN to create the connection fo

    .PARAMETER Environment
    Specifies the development environment object.

    .PARAMETER connector
    Specifies the name of the connector.

    .PARAMETER waitForSignIn
    Specifies if should wait for sign in when creating the connection.

    .INPUTS
    None. You can't pipe objects to Install-ConnectionSetup.

    .OUTPUTS
    Returns the ID of the created / existing connection.

    .EXAMPLE
    PS> Install-ConnectionSetup -UserUPN "first.last@contoso.com" -Environment $env -connector "approvals" $False
    Installs a connection setup for the Salesforce connector in the specified development environment.

#> 
function Install-ConnectionSetup {
    param (
        [Parameter(Mandatory)] [String] $UserUPN,
        [Parameter(Mandatory)] $Environment,
        [Parameter(Mandatory)] $connector,
        $waitForSignIn
    )
    return [UtilityMethods]::SetupConnection($UserUPN, $Environment, $connector, $waitForSignIn)
}

function Get-SecureValue {
    param (
        $Name
    )

    return [UtilityMethods]::GetSecureValue($Name)     
}

Export-ModuleMember -Function Invoke-GetDeveloperEnvironment
Export-ModuleMember -Function Invoke-GetOrCreateDevelopmentEnvironment 
Export-ModuleMember -Function Install-ConnectionSetup
Export-ModuleMember -Function Invoke-DevEnvironmentAuthUtility
Export-ModuleMember -Function Get-SecureValue

