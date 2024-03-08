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
