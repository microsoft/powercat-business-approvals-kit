# Overview

This page includes a summary of possible issues

## Setup

This section outlines possible methods to create a demo environment with some possible issues related to each approach. Each method needs further investigation to validate what are allowed actions working with the terms and conditions of each approach.

|Method|Description|Issues|
|------|-----------|------|
|Microsoft Demo|Use [https://aks.ms/cdx](https://aks.ms/cdx) to create demo|Terms and conditions limit usage to Microsoft and Partners for demo only not allocated accounts with user name and passwords|
|Office 365 Developer|Use Office 365 developer tenant|Limited usage and locked to phone and credit card|
|Microsoft Entra Id|Create tenant and assign Power Apps Developer Plan|Requires Azure Subscription|

## Licenses

|Issue|Description|Mitigation|
|-----|-----------|----------|
|Availability|The tenant that you are looking to setup does not have licenses available|Remove dependencies on O365 or provide demo video / presnetation from a different tenant.
|Developer Plan|Has limitations on flow run and number managed||

### Power Apps Developer Plan

Assumptions for the Power Apps Developer Plan

1. Someone creates a new Microsoft Entra ID tenant using guide [Quickstart - Access & create new tenant - Microsoft Entra | Microsoft Learn](https://learn.microsoft.com/azure/active-directory/fundamentals/create-new-tenant) 

Using the created tenant a user can visit [Power Apps Developer Plan | Microsoft Power Apps](https://powerapps.microsoft.com/developerplan/) and select "Get started free" to get a "Microsoft Power Apps for Developer" license assigned and use the features described in
[About the Power Apps Developer Plan - Power Platform | Microsoft Learn](https://learn.microsoft.com/power-platform/developer/plan#which-features-are-included-in-the-power-apps-developer-plan)

2. The user from the created tenant can then use Power Platform subject to the following restrictions: (Content summarized from
[About the Power Apps Developer Plan - Power Platform | Microsoft Learn](https://learn.microsoft.com/power-platform/developer/plan#which-features-are-included-in-the-power-apps-developer-plan)):

- The Power Apps Developer Plan is restricted to building and testing apps to validate prior to production.
- To distribute apps for production purposes, choose a plan from the Power Apps pricing page. (Power Apps Developer Plan | Microsoft Power Apps)
- Flow runs/month 750
- Database size 2GB
- Can't increase capacity by applying add-ons
- Environments created using Power Apps Developer Plan that are inactive for the last 90 days will be deleted after notifying the environment owners
- A paid plan is required to deploy or run solutions in a production environment for production use.
- No Power Automate RPA. Users would need to start a Power Automate Trial
- No AI Builder use rights. Users would need to start an AI Builder Trial
- Managed Environment isn't included as an entitlement in the Developer Plan

## Security

|Issue|Description|Mitigation|
|-----|-----------|----------|
|No Licenses|Assign security group with licenses has delay|Need loop to check for inherited assignment|

## Automation

The following issues exist that prevent automated creation of demo environments

|Issue|Description|Mitigation|
|-----|-----------|----------|
|Invoke Cloud Flow|No public API or CLI command exists to start a clould flow|Use playwright to automate Cloud Flow Start|
|Power Automate Portal|Multiple dialogs when try automate https://make.powerautomate.com|Setting to disable message? Conditional Playwright checks to click?|
|Creating Connection|Limited number of connectors can be created automatically on user behalf|Use Playwright|
|Unable to execute Approvals|Require run of Cloud Flow to trigger install of Approvals Solutions|Install by default in environment. Import and run setup solution|
|PAC CLI Connections|Results not in JSON formal. Hard to pass connection id to pac solution import settings file|Need PowerShell to parse Text to Object|
