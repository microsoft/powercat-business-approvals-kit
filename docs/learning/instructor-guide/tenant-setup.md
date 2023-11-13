# Tenant Setup

Welcome to the Tenant Setup section of the Power Platform Approvals Kit instructor guide. Tenant setup covers the various options available for setting up a tenant to host the Approvals Kit. With a range of choices available, it's important to understand the benefits and drawbacks of each option to ensure your organization is set up for success. If your organization already has a tenant set up, then you're able to skip the section.

If using a tenant isn't an option, such as when you can create a standalone demonstration tenant for learners. We explore other options such as Office 365 Development Environments or creating a new Microsoft Entra ID tenant. Let's dive in and explore the various options available for setting up your tenant to host the Approvals Kit.

## Options

The following table outlines possible options for tenant selection

|Choice         |Description|Notes|
|---------------|-----------|-----|
|Existing Tenant|The organization already has an existing Microsoft Entra ID tenant||
|[Microsoft 365 Developer Plan](https://developer.microsoft.com/microsoft-365/dev-program)|Get an instant sandbox preconfigured with sample data, including Teams Developer Portal, and start developing on the Microsoft 365 platform.|Requires credit card and phone number for registration|
|[Microsoft Customer Digital Experiences](https://cdx.transform.microsoft.com/)|Experience the latest Demo content, Customer Immersion and Labs. Demo environments give you a hands on opportunity to explore Microsoft 365 and Dynamics product and features.|Available to Microsoft Partner Network and limited how tenants can be used. Consult terms and conditions for acceptable use|
|[Get a Microsoft Entra tenant](/entra/identity-platform/quickstart-create-new-tenant)|Requires an Azure account that has an active subscription. [Create an account for free](https://azure.microsoft.com/free/?WT.mc_id=A261C142F).|Doesn't provide the required Office 365 license. You can explore purchasing or adding a trial license to complete the workshop|

## Data Loss Prevention Policy

To enable a data loss prevention policy for the tenant or a set of selected environments you can use the following policy. You can use the task below to manually setup a Data Loss Prevention Policy (DLP) restricted to only connectors required for the workshop.

> [!NOTE] If Data Loss Prevention Policies (DLP) is a new concept for you, review the [Establishing a DLP strategy](https://learn.microsoft.com/power-platform/guidance/adoption/dlp-strategy) will provide a good starting point for further reading.

### Task 1 - Open Data Policies

1. Open the Power Platform admin Center [https://aka.ms/ppac](https://aka.ms/ppac).

2. Select **Policies** from the left navigation.

3. Select **Data Policies**

### Task 2 - Create New Policy

1. Select **New Policy**

1. Enter a policy name of **Approvals Kit Workshop**

### Task 3 - Select Business Connectors

This task will select the minimum pre-built connectors needed for Contoso Coffee application and the Approvals Kit.

1. Select the following connectors:

- Approvals
- Microsoft Dataverse (legacy)
- Microsoft Dataverse
- Office 365 Users

2. Select **Move to Business**

### Task 4 - Move non blockable connectors to Blocked

> [!NOTE] This step is optional depending on your DLP strategy.

1. Filter Non Business Default by Blockable Yes.

2. Select all the connectors

3. Select **Block**

4. Select **Next**

### Task 5 - Configure custom connectors

The Approvals Kit custom connector will connect to Dataverse REST API for each development environment. You can deny all custom connectors except Dataverse using the following steps.

1. Select **Add connector pattern**

2. Choose Data group of **Business**

3. Enter host URL pattern of **https://*.crm.dynamics.com**

> [!NOTE] Depending on the geography of environments this url pattern may need to be altered. Review the [Data center regions](https://learn.microsoft.com/power-platform/admin/new-datacenter-regions) for URL that should be applied.

4. Select **Save**

5. Select connector pattern for  * select **Edit**

6. Change the Data group to **Blocked**

### Task 6 - Define Scope

> [!IMPORTANT] Establishing scope for a DLP policy is a critical step of the SLP setup process. The [Establishing a DLP strategy](https://learn.microsoft.com/power-platform/guidance/adoption/dlp-strategy) includes guidance on the default environment with restrictive policy by default.

Use can use the define scope to apply to all environments, selected environments or exclude certain environments.

When you have completed the scope definition select **Next**

### Task 7 - Review policy

Review the policy and when verified it is correct select **Create Policy**.

## Summary

The options provided a high-level overview of the various options available for setting up a tenant to host the Approvals Kit. It's important to note that each tenant setup option has its own benefits and drawbacks, and the best choice depends on the specific needs of your organization.

However, if your organization already has an existing tenant with available licenses then it can provide the most long-term path for building a community of learners and scaling knowledge. By using an existing tenant, you can bring in cohorts of learners over time and take advantage of the Approvals Kit without incurring extra costs or administrative overhead.

Once you have a tenant, consider the Data Loss Prevention Policy steps as an example of how you can limit the connectors used in the workshop to a defined list.
