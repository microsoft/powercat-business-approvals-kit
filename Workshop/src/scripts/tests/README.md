# Overview

This folder includes Pester PowerShell integration tests for the Approvals Kit workshop

## Setup

Ensure that you have the latest version of Pester installed. You can read more on installation at [Pester Installation](https://pester.dev/docs/introduction/installation)

```pwsh
Install-Module -Name Pester -Force -SkipPublisherCheck
```

## Run Tests

To run the workshop tests

```powershell
Push-Location
Set-Location Workshop/src/scripts/tests
Invoke-Pester
Pop-Location
```
