
# Creating First Approval Kit workflow

## Task 1 - Create a Contoso Coffee Machine Request

By completing the second module of App In a Day workshop or having the Contoso Coffee Application imported into your environment, you have the ability create a machine request. Lets review the request process before you proceed with creating the Business Approval for this process.

> NOTE: If you do not have the Contoso Coffee Machine Ordering app imported in your environment follow the [Import Contoso Coffee](./import-contoso-coffee.md) guide to get started.

1. Sign in to [Power Apps](https://make.powerapps.com)

1. Select the assigned Approvals Kit environment for this workshop content

1. Select **Apps** and select the **Machine Ordering App**

  ![Screenshot of starting the Contoso Coffee Machine Ordering app](./media/machine-ordering-app-play.png)

1. If prompted select Allow for the Office 365 users connector.

  ![Screenshot of Power Platform Office 365 USers connection consent dialog](./media/office-365-users-connection-allow.png)

1. Select a few machines and select **Compare**.

  ![Screenshot of Contoso Coffee Machine Ordering app with multiple machines selected](./media/contoso-coffee-select-machines.png)

1. Select one of the machines and select **Submit**

  ![Screenshot of Contoso Coffee Machine Ordering app for submit screen request](./media/contoso-coffee-submit-request.png)

1. Select OK to close the submitted Machine Request

  ![Screenshot of Contoso Coffee Machine Ordering app for submit screen request submitted](./media/contoso-coffee-submitted-request.png)

1. Close the application.

1. select **Tables**, search for **Machine Order** and select it.

  ![Screenshot of Contoso Coffee Machine Ordering table selected](./media/machine-order-table-select.png)

1. Select the Data tab and make sure you have at least one record in the table.

## Task 2 - Create Approvals Kit Workflow

Having verified that Contoso Coffee machine request can be successfully created lets create a one stage and one node approval process that you can self approve the Machine Request.

> NOTE: If you do not have the Approvals Kit installed in your environment the [Approvals Kit Setup Guide](../../setup.md) or the [Instructor Guide](../instructor-guide/overview.md) could help your Center of Excellence team or Trainer provide an environment so that you can follow this workshop module

1. Select **Apps**, select the … button of the **Business Approval Management** and select **Play**.

1. If prompted select your account and enter your account password.

1. Select **Continue** to close the welcome screen if it appears

1. Select **Configure a Workflow** from the Home navigation item

  ![Screenshot of Approvals Kit Home screen](./media/approvals-kit-home-screen.png)

1. Enter the name of your workflow as **Machine Requests**

1. Select the + button to create the first approval stage.

1. Enter a name **Self Approval** for the stage and then select **Save**

  ![Screenshot of Approvals Kit add first stage to approval](./media/approvals-kit-create-first-stage.png)

1. Wait for the stage to be created. Select the gray + button to create a node for the created stage named **Self Approval**

  ![Screenshot of Approvals Kit first stage with no node defined](./media/approvals-kit-first-stage-no-node.png)

1. Create a first node with name **Submit** and select your user account from the Approver list

  ![Screenshot of Approvals Kit add node stage to stage](./media/approvals-kit-create-first-node.png)

1. Select **Save** to save the first node

1. Wait for the node to be created

  ![Screenshot of Approvals Kit first stage with node created](./media/approvals-kit-first-stage-node-created.png)

1. Select the Save button to save your workflow

  ![Screenshot of Approvals Kit workflow process saved successfully](./media/approvals-kit-workflow-saved.png)

## Task 3 - Publish Approval Workflow

Having created the definition of your workflow, the next step is to publish the workflow so that you can trigger an approval when a Machine request is created. Everytime you publish the workflow, a new version is created.

1. Select **Publish**.

1. Verify that the workflow can be published and select **Publish**.

 ![Screenshot of Approvals Kit workflow publish](./media/approvals-kit-workflow-publish.png).

1. Wait for the workflow to be published. You can use the **Refresh** to update the publish status.

 ![Screenshot of Approvals Kit workflow process after successful publish](./media/approvals-kit-workflow-published.png).

## Summary

In this module you stepped through the process of creating a Contoso Coffee Machine request, defined a simple Approval workflow and published the first version of this workflow.

The publish task created an active version of you work flow. As you change and adapt a workflow you can publish new versions to react to changing business rules or revert back to an earlier version.

The next [Trigger Approval](./trigger-approval.md) module will build on these two elements to use a Power Automate cloud flow to begin an approval workflow every time a Machine request is submitted.

> [!div class="nextstepaction"]
> [Next step: Setup trigger approvals](./trigger-approval.md)