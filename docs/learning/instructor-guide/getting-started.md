
# Get started

The Automation kit includes a set of automation scripts to help you set up isolated environments for each attendee, reducing the amount of time required for setup.

This guide assumes that you use one of the following methods to set up and run the automation scripts:

- **Local install** - you have the ability to install the required components on your local environment.
- **Power Platform Hosted Machine** - Use Power Platform Hosted Machine to install setup components.

The installer components are based on cross platform tools. The required tools include PowerShell, Azure CLI, Power Platform CLI and the .NET SDK. Some of the components like the package install require Windows.

## Local install

Perform a local install using the following steps:

1. Install .NET Runtime 6.0 from [Download .NET](https://dotnet.microsoft.com/download)

1. Install .NET SDK 7.0 from [Download .NET](https://dotnet.microsoft.com/download)

1. Azure CLI is installed. One option using .NET is

```pwsh
dotnet tool install Microsoft.PowerApps.CLI.Tool
```

1. PowerShell is installed one option using .NET is

```pwsh
dotnet tool install --global PowerShell
```

1. Install SecureStore one option using .NET is

```pwsh



dotnet tool install --global SecureStore.Client
```

1. Clone the Approvals Kit GitHub repository

```bash
git clone https://www.github/microsoft/powercat-business-approvals-kit.git
```

1. Build the .NET library and playwright on Windows

```pwsh
cd .\powercat-business-approvals-kit\Workshop\src\install
dotnet build
./bin/Debug/net7.0/playwright.ps1 install
```

> NOTE: If you are installing on a non Windows operating system the additional command may be needed

```pwsh
./bin/Debug/net7.0/playwright.ps1 install-deps
```

1. Ensure that latest managed release of Business Approvals kit has been downloaded to the Workshop assets folder.

## Power Platform Hosted Machine

One approach to allow you to create development environments for learners is to use a Power Platform Hosted Machine.

### Setup for scale

Setup for a class of 20+ students could take up to 20 minutes for each user environment. Using an Power Platform Hosted Machine, you can automate a setup of machines without the need to have an active PC connected to the Internet.

You can the the [Power Platform Setup](../../../Workshop/docs/power-platform.md) for the steps to use a Power Platform hosted Cloud PC to assist with the steup process.

## Next

Now that you have either local or a Power Platform hosted VM environment ready move to [Tenant Setup](./tenant-setup.md) to create tenants and apply DLP.
