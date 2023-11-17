# Overview

These scripts provide and use combination of PowerShell, Bash, .Net Apps and C# Script File (.csx) files to support the [Instructor Guide](../../../docs/learning/instructor-guide/README.md)

## Power Shell Scripts

The instructor guide makes use of the PowerShell scripts to provision developer environments for workshop users.

### Invoke-SetupUserForWorkshop

This command will setup the workshop in a Development environment for a user. If the user does not exist it will be created.

```pwsh
Invoke-SetupUserForWorkshop (Get-SecureValue DEMO_USER)
```

### Reset-UserDevelopmentEnvironment

This command will setup the workshop in a Development environment for a user. If a development environment exists it will be deleted and recreated.

```pwsh
Reset-UserDevelopmentEnvironment (Get-SecureValue DEMO_USER)
```

### Integration Tests

Assuming that the workshop had been completed with a multistage approvals and Cloud Flow.

```pwsh
Invoke-ValidateTwoStageMachineRequestApproval "first.last@contoso.onmicrosoft.com"
```

The [Tests README](./tests/README.md) also includes instructions on how to run Pester PowerShell integration tests

## Bash Scripts

The [setup.sh](./setup.sh) Bash script can be used to trigger a long running setup of a workshop user in the background. It makes use of the [nohup](https://www.linux.org/docs/man1/nohup.html) linux command so that the install continues even if ssh session is disconnected.

## C# Script File

The C# Script File are used as part of creating end to end integration test for the Approvals Kit and Contoso Coffee application used by the learn module and workshop setup of the instructor guide. The scope addressed by the C# Scripts for automation is very wide and needs to consider automation of the following elements:

- The ability to login as an interactive user account.

- Interact with Power Platform portal pages like Power Automate

- Interact with Power Platform Canvas Applications via the Document Object Model

> [!IMPORTANT]
>
> 1. The test approach adopted is more code first driven approach. It requires knowledge and install of PowerShell, C# and Playwright
>
> 2. The user account cannot have multi factor authentication enabled. This requirement

### Power Apps Test Engine Comparison

The scripts and samples here are not intended as a replacement of the Power Apps Test Engine. Each project is scoped to solve different problems.

|Feature | Power Apps Test Engine | Approvals Kit Testing |
|--------|------------------------|-----------------------|
|Scope   | Testing of Power Platform Applications | Support setup of Demo environments for workshop and integration testing of deployed solution |
| Languages | Power Fx + Yaml | Power Shell, C# and Playwright |
| Playwright USage | Wrapped inside Power Fx commands | Direct C# Access to page |
| Interaction Model | Headless access to Power Apps JavaScript Object model | Headless and Interactive Document Object model |
| Pages | Power Apps page only | Automate multiple Portal and Power Apps
| Dataverse

The C# Script files can be called via the PowerShell script Invoke-PlaywrightScript which will login as the provided used using secure value DEMO_PASSWORD

These scripts work at the Document Object Model. This is a fundamentally different approach than taken projects like [Power Apps Test Engine](https://github.com/microsoft/PowerApps-TestEngine). As a result these automation is subject to user experience changes. The Power Apps Test Engine in comparison works at the underlying JavaScript Object Model making it more context aware of the underlying entities independent of their rendering.

Power Apps Test Engine also tasks a more low code approach leveraging Power FX to provide support for Power Platform integration and low code Power FX functions rather than code first C# skills.

### JavaScript Object Model vs Document Object Model

Looking specifically at the JavaScript Object Model (JOM) of a Power App is designed to provide programmatic access to the Power Apps controls and data, not just the HTML representation of that data. This means that the JOM allows you to interact with the app's controls and data in a more meaningful way than just manipulating the HTML.

For example, if you have a text input control in your Power App, the JOM allows you to access the value of that control directly, without having to parse it from the HTML. You can also set the value of the control programmatically, without having to simulate user input.

Similarly, if you have a data source in your Power App, the JOM allows you to access the data directly, without having to scrape it from the HTML. You can also manipulate the data programmatically, such as filtering, sorting, and updating records.

Overall, the JOM of a Power App provides a more powerful and flexible way to interact with the app's controls and data, compared to just manipulating the HTML.

## .Net Application

The instructor guide also makes use of a .Net App to automate steps that are not available via the Power Platform Command Line or REST APIs. The [README](../install/README.md) provides more information on that app and capabilities that are used by the scripts
