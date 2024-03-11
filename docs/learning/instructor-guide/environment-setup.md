
# Environment setup

This section of the Power Platform Business Approvals kit focuses on creating isolated development environments for learners.

## Prerequisites

- Complete the previous section [User setup](./user-setup.md), which explains how to create the Microsoft Entra ID users, groups, and assigned licenses.

## Create developer environments

To create the developer environment, use these steps to automate setup:

1. Create a developer environment for the learner.

1. Set the required environment settings.

1. Import the Creator Kit.

1. Import dependent solutions and components.

1. Import the Contoso Coffee Application from the Power Platform App in a day instructor download.

1. Install and set up the Approvals Kit.

1. Perform post-install steps of publishing applications and enabling flows.

## Power Shell Scripts

Use can use the following PowerShell scripts to provision developer environments for workshop users.

> [!NOTE]
> These commands assume that the powershell scripts have been imported from the Workshop folder
>
> . .\src\scripts\test.ps1

```pwsh
cd ~/powercat-business-approvals-kit/Workshop
. .\src\scripts\test.ps1
```

### Invoke-SetupUserForWorkshop

This command will setup the workshop in a Development environment for a user. If the user does not exist it will be created.

```pwsh
Invoke-SetupUserForWorkshop (Get-SecureValue DEMO_USER)
```

This setup process will take around 20 minutes to execute.

## Verify Setup

After the demo user has been setup using Invoke-SetupUserForWorkshop

1. Open scripts folder

```pwsh
pwsh
cd ~/powercat-business-approvals-kit/Workshop/src/scripts
```

2. Ensure Pester module installed

```pwsh
Get-Module -Name Pester -ListAvailable
```

3. If Pester module is not installed

```pwsh
Install-Module -Name Pester -Force
```

1. Run scripts

```pwsh
Invoke-Pester
```

### Reset-UserDevelopmentEnvironment

This optional command will setup the workshop in a Development environment for a user. If a development environment exists it will be deleted and recreated.

```pwsh
Reset-UserDevelopmentEnvironment (Get-SecureValue DEMO_USER)
```

### Integration Tests

Assuming that the workshop had been completed with a multistage approvals and Cloud Flow.

```pwsh
Invoke-ValidateTwoStageMachineRequestApproval (Get-SecureValue DEMO_USER)
```

The [Tests README](../../../Workshop/src/scripts/tests/README.md) also includes instructions on how to run Pester PowerShell integration tests
