
# Triggering a defined approval

The prior [first approval module](./first-approval.md) provided tasks and steps required to create a Contoso Coffee machine request and a simple Approval workflow that when triggered will send an Approval to you for approval. This module extends this functionality by using a Power Automate Cloud flow to begin a business approval process when a Machine Request is created in Dataverse.

> NOTE: If you are not familiar with what Approvals are in Power Automate [get started with approvals](/power-automate/get-started-approvals) section provides some examples of what can be achieved with the Standard Power Platform Approvals Connector together with Power Automate. The Approvals Kit extends Approvals scenarios like these with additional features discussed below.

## Example process

As an example, we start with a simple self-approval using the Contoso Coffee scenario, where a request is triggered when a new request data is added to Dataverse. In the later modules, we'll extend this workflow where a second manager approval is requested when the value of the machine request is greater than $400.

This combined scenario could be built using multiple Power Automate Cloud Flows but can take some time to develop. By using the Approvals Kit, you can model the same process quickly and focus on configuring the business approvals process.

## How this workshop is structured

The goal of this workshop is designed to incrementally build your knowledge of the Approvals Kit. The first module is intentionally simple to help you get familiar with the kit.

Approvals Kit provides a no-code way of defining [multi stage approvals and conditions](./add-conditional-stages.md) like we'll cover in the next module.

In later modules of this workshop, we'll also demonstrate feature like handling for Delegated approvals and out of office support make it easy for you to build complicated Approvals process easily without needing to build these features using Power Platform solutions yourself.

> NOTE: If you want to dive deeper you can read the [Approvals Kit Comparison](../../approvals-kit-comparison.md) for more information.

## Task 1 - Creating a solution

The first task is to create a Power Platform solution to create a container to you to group your related Platform resources together.

> NOTE: If solutions are a new concept for you, the [Solution Overview](/power-apps/maker/data-platform/solutions-overview) and [Solution Concepts](/power-platform/alm/solution-concepts-alm) provides some further reading.

1. Sign in to [Power Apps](https://make.powerapps.com)

1. Select the assigned Approvals Kit environment for this workshop content

1. Select Solutions from the left navigation. If the item isn’t in the left navigation pane, select **… More** and then select Solutions.

1. Select **New Solution**

1. Enter a Solution display name of **Contoso Coffee Approvals**.

1. Select publisher **Contoso**.

> To understand more on publishers for solutions review [Solution Publisher](/power-apps/maker/data-platform/create-solution#solution-publisher).

1. Select **Create**.

1. Wait for your solution to be created.

## Task 2 - Creating Cloud Flow

In your created solution perform these steps:

1. Select **New** > **Automation** > **Cloud flow** > **Automated**

> For more guidance on create a Cloud flow with solutions you can reference [Create a cloud flow in a solution](../../../../create-flow-solution.md).

1. Enter Flow name of **Machine request**

1. Search for trigger by entering **Dataverse** to choose your trigger

1. Select trigger **When a row is added, modified or deleted (Microsoft Dataverse)**

1. Select **Create**.

1. Select change type of **Added**.

1. Select Table name of **Machine Orders**.

1. Select the scope of the change. For example **Organization** read more on [Scope](../../../../dataverse/create-update-delete-trigger.md#scope)

> NOTE: For more information on Dataverse trigger parameters you can reference [Trigger flows when a row is added, modified, or deleted](../../../../dataverse/create-update-delete-trigger.md)

  > [!div class="mx-imgBorder"]
  ![Screenshot of Power Automate Dataverse trigger when Machine Order is Added](./media/power-automate-cloud-flow-dataverse-trigger.png)

1. Select **New Step**.

1. Select **Custom** tab.

1. Select **Start business approval process** action.

  ![Screenshot of adding Approvals kit start business approvals process inside Power Automate Cloud Flow](./media/power-automate-approvals-kit-custom-connector.png)

1. Select **Sign in** if prompted and select your account.

1. If prompted select **Allow access** to confirm creation of Approvals Kit connection.

  ![Screenshot of adding Approvals kit connection confirmation](./media/approvals-kit-connector-confirmation.png)

1. Select the **Machine Requests (v1)** workflow process that you published in the [First Approval](./first-approval.md) module.

  ![Screenshot of adding Approvals kit connector with a selected workflow](./media/power-automate-approvals-kit-connector-select-workflow.png)

1. Select **Save**.

1. Wait for the cloud flow to be saved.

## Task 3 - Creating a new Machine Request

Now that a cloud flow trigger has been defined for the **Machine Order** table, perform these steps to create a new Machine request that will trigger an approval workflow.

1. Select **Apps** and select the **Machine Ordering App**

  ![Screenshot of starting the Contoso Coffee Machine Ordering app](./media/machine-ordering-app-play.png)

1. If prompted select Allow for the Office 365 users connector.

  ![Screenshot of Power Platform Office 365 USers connection consent dialog](./media/office-365-users-connection-allow.png)

1. Select a few machines and select **Compare**.

 ![Screenshot of Contoso Coffee Machine Ordering app with multiple machines selected](./media/contoso-coffee-select-machines.png)

1. Select one of the machines and select **Submit**

 ![Screenshot of Contoso Coffee Machine Ordering app for submit screen request](./media/contoso-coffee-submit-request.png)

1. Select OK to close the submitted Machine Request

  > [!div class="mx-imgBorder"]
  ![Screenshot of Contoso Coffee Machine Ordering app for submitted request](./media/contoso-coffee-submitted-request.png)

1. Close the application.

## Task 4 - Approve the request

After you submit the Machine request, the cloud flow is triggered and will begin your defined business approval process. Use these steps to approve the request in the Power Automate portal.

> NOTE: This workshop guide performs in the Approval inside the Power Automate portal. If the user is configured with an Office 365 license, the the approval will also be available via Outlook or in [Microsoft Teams](../../../../teams/native-approvals-in-teams.md).

1. Open the [Power Automate portal](https://make.powerautomate.com)

1. Select the assigned Approvals Kit environment for this workshop content.

1. From the left navigation, select **Approvals**.

1. Wait for the approval to be sent.

1. Select the received approval.

1. Select **Approve**.

 ![Screenshot of selecting an Approval in the Power Automate portal](./media/power-automate-approvals-select.png)

1. Select **Confirm** to approve the approval.

  ![Screenshot of selecting an Approval in the Power Automate portal approval confirm](./media/power-automate-approvals-approve-confirm.png)

1. Select **Done** to close the Approval once confirmed.

## Task 5 - View the Completed Approval

In this task, use the Business Approval Management application of the Approvals Kit to view the updated status of the approved request.

1. Select **Solutions** from the left navigation of the Power Automate web portal

1. Select **Business Approval Kit** from the list of solution

1. Select **Apps** from the Objects navigation item

1. Select **Business Approval Management** from the list of apps.

1. Select the **...** next to the application name and select **Play**.
  
1. Select **Approval Instances** from the left navigation.

1. Select the completed approval.

  ![Screenshot of active Business Approval Instances](./media/business-approval-management-approval-instances.png).

1. Review the details and outcome of the Approval

  ![Screenshot of active Business Approval Details view](./media/business-approval-management-approval-instance-info.png).

## Summary

In this module, we combined the Contoso Coffee Machine Request solution with the business approval created in the [first approval](./first-approval.md) module. A Power Automate cloud flow was created combining a Dataverse trigger for the Machine Order table and the Approvals kit connector to begin a selected business approval process.

The **Business Approval Manager** application was used to view the results of the completed Business Approval. If there was an error in the process or the stage wasn't completed, the instance status would be **Running** or **Error**.

> [!div class="nextstepaction"]
> [Next step: Add conditions and stages](add-conditional-stages.md)