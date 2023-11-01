
# Approvals delegation and out of office

One key element of Approvals kit is the ability to specify delegate approvers when you're out of the office or on holiday. These settings ensure that the approval process can continue smoothly even if the primary approver is unavailable. In the "Configure basic settings" section of the Business Approval Management application, you can easily set these delegate approver values.

This workshop module provides step-by-step instructions for defining delegated approvers and handling out of office approvers. You learn how to configure the basic settings of the Business Approval Management application to set delegate approvers, and how to handle out of office approvers using the Approvals kit.

By the end of this module, you should have a good understanding of how to manage delegate approvers and out of office settings using the Power Platform Approvals Kit.

> NOTE: Further information on [out of office and delegation](../../setup-out-of-office-and-delegation.md) is available for reference.

## Task 1 - Open the basic settings configuration

1. Open the **Business Approval Management** application in the workshop environment assigned to you.

1. From the Home page, select the **Configure basic settings** panel

   ![Screenshot of Configure basic settings from the Home page of Business Approval Management application](./media/business-approval-management-configure-basic-settings.png)

1. Review the settings screen to get familiar with the information that you can provide

   ![Screenshot of basic settings inside the Business Approval Management application](./media/business-approval-management-basic-settings.png)

## Task 2 - Define approval delegate and set out of office

1. In the **Approval Delegate** field search for another user who can act as an approver or enter the auto-approver test user that your instructor configured in Power Platform for workshop users.

1. Select the **Out of Office** so that is displays Enabled

> NOTE: For this workshop we will use Out of office. Defining your work profile and public holidays are other scenarios where your delegated approvers will also be used to route the approval request to the correct approver.

1. Select **Save**

   ![Screenshot of basic settings inside the Business Approval Management application with sample delegate and outof office](./media/business-approval-management-basic-settings-sample.png)

## Task 3 - Create a new Machine Request

1. Sign in to [Power Apps](https://make.powerapps.com)

1. Select the assigned Approvals Kit environment for this workshop content

1. Select **Apps** and select the **Machine Ordering App**

1. If prompted select Allow for the Office 365 users connector.

1. Select a few machines that have a price less than **$400** and select **Compare**.

1. Select one of the machines  and select **Submit**

1. Select **OK** to close the submitted Machine Request

## Task 4 - Check Approvals

1. Open the [Power Automate Portal](https://make.powerautomate.com)

1. Select the assigned Approvals Kit environment for this workshop content.

1. From the left navigation, select **Approvals**.

1. Wait for a few minutes and refresh and verify that an approval haven't been generated.

## Summary

This workshop module covered the basic out of office scenario for the Approvals kit. It provided step-by-step instructions for defining and testing delegated approvers when you're out of office.

The workshop demonstrated how to configure the basic settings of the Business Approval Management application to set delegate approvers. It also included how to handle out of office approvers using the Approvals kit. You could extend workshop to testing your work define work profile or public holidays as examples of when delegation can be used.
