# Overview

Now that Azure user are created and granted Power Platform license ths page covers setup of power platform resource required by users. These resources include

## Power App Developer Plan Developer Environment

Power Platform Developer environment is a dedicated space within the Power Platform where developers can create, test, and deploy custom business applications.

It is important for a demo evaluation as it allows the assigned user of a Power App Developer Plan a safe place to build and experiment because it provides a sandbox environment where developers can try out new ideas without affecting the production environment.

This allows developers to test and refine their applications before deploying them to the live environment, which can help to ensure that the application is stable and meets the needs of the business. Additionally, having a dedicated development environment can help to prevent conflicts between developers who may be working on different aspects of the same application.

## Power Platform Setup

You can use a Power Platform Hosted Machine to setup your workshop environment for multiple workshop users.

## Why is this important?

If you want to setup a class size of 20+ students each environment setup could take up to 20 minutes. By using an Power Platform based Virtual Machine you could automate a set of machines without the need to have an active PC connected to the Internet using Power Automate Desktop.

## Initial Machine Setup Machine

In an environment [create a Hosted machine](https://learn.microsoft.com/power-automate/desktop-flows/hosted-machines#create-a-hosted-machine).

1. Open the Hosted machine the browser

2. Open Command Line in Windows Command prompt using cmd.exe as Administrator

3. Install Git using Winget

```cmd
winget install --id Git.Git -e --source winget
```

4. Change to the temp directory

```cmd
cd %TEMP%
```

4. Clone the repository

```cmd
https://github.com/microsoft/powercat-business-approvals-kit.git
```

5. Change to workshop scripts

```cmd
cd powercat-business-approvals-kit\Workshop\src\scripts
PowerShell -Command ". .\install.ps1"
```

6. Install the App Gateway using instructions https://powerapps.microsoft.com/en-us/downloads/ for On-Premises Data Gateway

7. Notes on install process are available from https://learn.microsoft.com/data-integration/gateway/service-gateway-install

## Agent Setup

To setup the Hosted Cloud PC as an agent to setup workshop users

1. Open a Power Shell session

2. Change to the scripts

```pwsh
cd $env:TEMP\powercat-business-approvals-kit\Workshop\src\scripts
. .\users.ps1
```

3. Login to Azure Account that has the ability create and update users

```pwsh
Invoke-AzureLogin
```

4. Create secure values folder

```pwsh
cd $env:TEMP\powercat-business-approvals-kit\Workshop
mkdir secure
```

5. Create secure key

```pwsh
SecureStore create secrets.json --keyfile secret.key
```

5. Add secure values


## What next

Use our low code Approvals Kit setup solution to use the Hosted machine to automate the setup of developer environments for each workshop user.