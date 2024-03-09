# Overview

Simple web API service to start the setup of a workshop user.

## Usage

You can start a new instance, if an instance is already running it will be closed.

```cmd
dotnet run
```

You can close all instances

```cmd
dotnet run -- kill
```

## Commands

The following POST commands can be used to control the API

### Start

Start a workshop user setup

```pwsh
$data = @{ User = "isaiahl" }
Invoke-RestMethod -Uri http://localhost:8000/start -Method POST -ContentType "application/json" -Body ($data|ConvertTo-Json)
```

### Check Status

Check the status of workshop setup

```pwsh
 Invoke-RestMethod -Uri http://localhost:8000/status -Method POST
```

Which will return JSON response similar to the following

```json
{
  "running": true,
  "info": "Single user setup",
  "started": "2024-03-07T20:47:06.9103904-08:00"
}
```

### Stop Process

```pwsh
 Invoke-RestMethod -Uri http://localhost:8000/stop -Method POST
```

### Validate

To start a workshop for user to validate the state you can start the check by posting the following message

```pwsh
$data = @{ User = "isaiahl" }
Invoke-RestMethod -Uri http://localhost:8000/validate -Method POST -ContentType "application/json" -Body ($data|ConvertTo-Json)
```

### Validation Results

Request validation results

```pwsh
Invoke-RestMethod -Uri http://localhost:8000/result -Method POST -ContentType "application/json"
```

Which will return empty object or response if results found

```json
{}
```

```json
{  
  "clientId": "a1230000-1111-2222-33333-444455556666",
  "tenantId": "common",
  "resourceUri": "https://yourenv.crm.dynamics.com",
  "redirectUrl": "https://global.consent.azure-apim.net/redirect/cat-5fapprovals-20kit-123456789012345678",
  "azureResourceId": "https://yourenv.crm.dynamics.com/",
  "operations": "CreateWorkflowInstance, GetApprovalDataFields",
  "checks": {
        "checks": {
            "Resource Id Match": true,
            "Client Id Match": true,
            "Redirect Found": true,
            "Found connector": true,
            "Get Workflows": true,
            "Found operations": true,
            "Resource Uri": true  
          },
          "connectorCount": 1,
          "environmentUrl": "https://yourenv.crm.dynamics.com/",
          "valid": true
    }
}
```
