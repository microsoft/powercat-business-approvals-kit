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
