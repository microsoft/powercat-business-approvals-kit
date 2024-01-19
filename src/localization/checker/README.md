# Localization Checker Overview

This application is an experimental application that will parse the preview yaml of a Power App page and scan it for literal text that may need to be updated to help with localization of the application.

## Prerequisites

The sample assumed that you have the .Net 7.0 SDK installed

## Getting started

1. Change to application test folder

```cmd
cd src\localization\checker\test
```

2. Run the unit tests and verify no errors

```cmd
dotnet test
```

3. Run the console application on a sample yaml file

```cmd
cd ..\src
dotnet run -- scan --file "..\..\..\..\BusinessApprovalKit\SolutionPackage\src\CanvasApps\src\cat_processdesigner_57f47\Src\scrWorkflowDesignerScreen.fx.yaml" --config config.yaml
```

The analyzed results will be created in the data subfolder as *.csv file delimited by semicolons.

The columns of the data file are

| Column | Description |
|--------|-------------|
| when   | The date and time of when the record relates to |
| control | The name of the control and parent controls that contain it |
| parent | The name of the parent control that the property belongs to |
| property | The name of the property that the text belongs to |
| text | The detected literal text |

## Power BI Report

The generated data files can be analyzed using the [Power BI Desktop Project](https://learn.microsoft.com/power-bi/developer/projects/projects-overview). Power BI Desktop projects allow the definition of the report to be integrated with source control.

The path to the data needs to be an absolute location. Given the location that you clone the repository is different on each machine it is assume that the report folder has been loaded using the windows [subst](https://learn.microsoft.com/windows-server/administration/windows-commands/subst) command as R:

1. Open the command prompt

2. Change to the checker folder

```cmd
cd src\localization\checker
```

3. Substitute R: with the current folder

```cmd
subst R: .
```

4. Open the report.pbip in the Power BI Desktop
