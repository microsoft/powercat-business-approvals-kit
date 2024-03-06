# Oveview

Welcome to this tutorial on setting up the custom connector for the Approvals Kit! If you're new to setting up custom connectors with the Power Platform, don't worry - we'll walk you through each step of the process. By the end of this tutorial, you'll be able to leverage the benefits of the Approvals Kit to streamline your workflows and improve your team's productivity. Let's get started!

## Context

Before we dive into the steps for setting up a custom connector for the Approvals Kit, it's important to note a few prerequisites. First, the Approvals Kit solution must already be imported by a user with System Administrator rights. This means that someone with the appropriate permissions has already installed the solution and made it available in your Microsoft Dataverse environment. If you're not sure whether this has been done, you can check with your Microsoft Dataverse administrator.

Additionally, to complete this tutorial, you'll need the ability to create a Microsoft Azure Application Registration. An Azure Application Registration is a way to create an identity for your custom connector that can be used to authenticate with Microsoft Dataverse. This is necessary because the custom connector needs to create a connection to Dataverse on behalf of the logged-in user. If you're not familiar with Azure Application Registrations, you can find more information in the Azure documentation.

Finally, you'll need a user with permissions to grant tenant-level consent to that registration. Tenant-level consent means that the user is granting permission for the custom connector to access data across the entire organization, rather than just for their own account. This is necessary because the custom connector needs to access published Approvals workflows and begin an approval process for any user in the organization.

To grant tenant-level consent to an Azure Application, the user must have the privileged role of [Global Administrator](https://learn.microsoft.com/entra/identity/role-based-access-control/permissions-reference#global-administrator) or [Cloud Application Administrator role](https://learn.microsoft.com/entra/identity/role-based-access-control/permissions-reference#cloud-application-administrator) assigned to them in Azure Active Directory. These roles have the necessary permissions to grant consent on behalf of the entire organization. If you don't have these roles, you'll need to work with someone who does in order to complete the necessary steps for setting up the custom connector.

If you're not sure who in your organization has these roles, you can check with your Microsoft Entra administrator or IT department. They should be able to help you identify the appropriate person to work with.

If setting up the permissions is a temporary blocker in getting hands on with the Approvals kit, one approach may be to set up a demo or development environment. A demo or development environment is a separate instance of Microsoft Dataverse that can be used for testing and experimentation without affecting the production environment. To set up a demo or development environment, you can use a trial subscription or request a development environment from the available cloud service options you may have available. The [LinkedIn post about Try Approvals Kit](https://www.linkedin.com/pulse/try-approvals-kit-grant-archibald-ammkf/) may provide you further information on how to explore this option.

## Creating you Microsoft Entra Application

To create a Microsoft Entra Application, follow these steps:

1. Navigate to the [Microsft Entra portal](https://entra.microsoft.com/) and sign in with your Microsoft Entra credentials.
2. In the left-hand menu, select  **Applications"
3. Select on **App registrations** and then select on the **New registration**"** button.
4. Enter a name for your application. This can be any name you choose, but it should be something that helps you identify the application later on, for example **Approvals Kit**
5. Under "Supported account types," select "Accounts in this organizational directory only."
6. Under "Redirect URI," select "Web" and enter the following URL: `https://global.consent.azure-apim.net/redirect` as a place holder. We will update this value later using information from the custom connector security tab.
7. Select "Register" button to create your application.

## App Registration Permissions

To make sure your application has the necessary permissions

1. Open the Microsoft Entra application registration you have created
2. Select **API Permissions** from the left navigation
3. Under API permission, select Add a permission and choose Dynamic CRM.
4. Choose Delegated permission and select user_impersonation.
5. Select Add Permissions.
6. Follow [Grant Admin Consent](https://learn.microsoft.com/entra/identity/enterprise-apps/grant-admin-consent?pivots=portal) to grant administration consent to the application

## Collect App Registration Information

Once you've created your application, you'll need to gather a few pieces of information to use in your custom connector:

1. Application (client) ID: This is the unique identifier for your application. You can find it on the "Overview" page of your application in the Azure portal.
2. Directory (tenant) ID: This is the unique identifier for your Microsoft Entra tenant. You can find it on the "Properties" page of your Azure Active Directory in the Azure portal.
3. Client secret: This is a secret key that you'll use to authenticate your custom connector with Microsoft Entra. To create a client secret, select "Certificates & secrets" tab of your application in the Azure portal. Select the "New client secret" button and follow the prompts to create a new secret.

## Get you Environment Url

To allow your custom connector to connect to your dataverse environment you will need to know your dataverse environment url. You can use the following steps to obtain this value

1. Open the Microsoft Power Apps portal [https://make.powerapps.com](https://make.powerapps.com)

2. Select the environment where the Approvals Kit has been installed

3. Select the gear icon in the top right of the page

4. Select **Session Details**

5. Note down the value of **Instance url**. This should be in the format https://yourenvironment.crm\[x\].dynamics.com

  > [!NOTE]
  > The \[x\] section above is optional and will depend on the data center that your environment has been created within.

## Updating your Custom Connector

The next step is to update and validate Approvals kit custom connector to use the Microsoft Entra Application information collected

1. Open the Microsoft Power automate portal [https://make.powerautomate.com](https://make.powerautomate.com)

2. Select the environment where the Approvals Kit has been installed

3. Select the **Approvals Kit** connector

4. Select the **Edit** menu item

5. Under the General tab, modify the following:

   - Set the **Host**. This will be in the format yourenvironment.crm\[x\].dynamics.com

6. Under the Security tab, modify the following:

   - Select Authentication type as OAuth 2.0.
   - Enter the Client ID from the Microsoft Entra App Registration overview section
   - Enter the Secret rom the Microsoft Entra App Registration secret section
   - Specify the environment URL under Resource URL section you obtained from the session details
   - Copy the Redirect URL

7. Open the created Entra App Registration

8. Select Authentication

9. In the Web Redirect URIs add the Redirect URL

10. Select Save to update the App Registration

11. Switch back to the custom connector.

12. Select Update connector.

13. Under the Test tab, create a New connection.

14. Specify the account details for the connection and allow access if prompted.

15. Edit the Custom connector again and test the **GetPublishedWorkflow** operation.

16. The operation should run successfully with status as 200.

  > [!IMPORTANT]
  > Selecting the **GetPublishedWorkflow** action is an important step as it does not require any parameters. If you select other options without supplying parameters an error will be generated.

## Summary

You have completed all the necessary steps to set up the custom connector for the Approvals Kit. It may have seemed like a lot of steps, involving different roles and permissions. This is feedback we have received and based in priority of feedback of customers who are looking to use the Approvals Kit in production we are investigating the process of replacing the custom connector with a verified connector to make this process easier over time.

Now that you have completed the setup, you can start using the Approvals Kit to improve your team's productivity and make your workflows more efficient.
