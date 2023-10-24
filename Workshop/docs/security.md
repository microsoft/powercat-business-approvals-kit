# Overview

This page summarize security setup required for the demo environments. These are implemeted as PowerShell scripts [security.ps1](../src/scripts/security.ps1)

## Getting Started

1. Import the security commands

```pwsh
. .\src\scripts\security.ps1
```

1. Login to Azure as the Administrator of the demo tenant. This command uses the Azure CLI to login

```pwsh
Invoke-AzureLogin
```

2. Create Makers security group

```pwsh
Add-SecurityMakersGroup
```

3. Assign Microsoft Developer Plan to the Makers group using guidance from https://learn.microsoft.com/azure/active-directory/enterprise-users/licensing-ps-examples

```pwsh
 Add-SecurityMakersGroupAssignDeveloperPlan
```

4. Add users to this Makers security group using Azure CLI and Microsoft Graph

```pwsh
Add-SecurityUserToMakersGroup "AdeleV@M365x63805008.OnMicrosoft.com"
```

5. Assign a MFA Phone number so that user not prompted on each login

```pwsh
Add-SecurityUserPhone "AdeleV@M365x63805008.OnMicrosoft.com"
```
