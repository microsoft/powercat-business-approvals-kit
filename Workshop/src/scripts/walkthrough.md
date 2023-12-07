## 01-Intro

Welcome to this walkthrough of the Power Platform Approvals Kit! In this video, we'll be exploring how to build end-to-end approval processes using the Contoso Coffee Machine request as an example. Whether you're new to the Power Platform or a seasoned pro, this video will provide you with valuable insights and practical tips on how to create increasingly complex approvals that match your business needs. From simple requests to multi-step workflows, the Approvals Kit has everything you need to streamline your approval processes and save time and effort. So sit back, relax, and let's dive into the world of Power Platform approvals with the Approvals Kit!

## 01a-Learning path

This example is part of the learning path for the Approvals kit where you can explore what the kit is and how to set it up. The learning module enables you to get hands on with an example we will cover in this video. Finally as an instructor you can work with a group of people to adopt the approvals kit. Now we have established the learning path lets continue with a guided walkthrough.

## 02-Contoso Coffee Application

Let's take a closer look at the Contoso Coffee application included in the Power Apps App in a Day and learning modules. With this app, you can select coffee machines to order and save the requests to Dataverse. However, depending on the machine requested, a manager approval may be required before the order can be processed. This app is a great example of low-code integration with the Approvals Kit.

## 03-Business Approval Management

The approvals portal provides you a central hub to configure no code approvals workflow, track the progress of approvals and configure settings for approvers. This application will mainly be used by people creating new workflows and monitoring the progress of approvals that are submitted, track the current state and the outcomes of approvals. This video will focus mainly on the create and update process of an approval and other videos will focus on the tracking and operational monitoring of approvals that use the created workflows.

## 04-Creating a workflow

Lets start by modelling the process for the Contoso coffee machine request approval creating a new approval workflow. We start by naming the workflow, after the workflow is created the first step is to create a first stage that will be run by every request. For this approval, the first stage is a self approval and creating an approval node that will send an approval request to myself as the approver.

## 05-Creating a node

Within a stage you can include one or more nodes where you can select who will approve the request and how the approval can be delegated. In this case we will select my user as the approver and not select any delegation or out of office settings.

## 06-Publishing a workflow

The final step in this initial process to create a workflow with single stage self approval is to publish. By publishing a workflow you create a version of the workflow that users can select from. Versioning is important in the Approvals kit as it allows you to to continue to revise and update your workflow without disrupting existing published workflows.

## 07-Create Power Platform solution

Now that a approvals kit workflow has been published, one option to begin an approval is to create a new Power Platform solution that contains a Power Automate Cloud flow to respond to Machine requests being created fronm the Contoso Coffee application.

## 08-Create the Power Automate Cloud Flow

Once the power platform solution is created you can add a new Automation Cloud flow to the solution

## 09-Create trigger

We will skip the initial wizard and select Dataverse as the initial trigger as the first step of the Power Automate Cloud flow. The trigger we will select is when a row is added, modified or deleted.

## 10-Configure the Dataverse trigger

With the dataverse trigger we will select a change type of Added. Select the table of Machine Orders so that the Power Automate cloud flow will be triggered when a new Machine Order is added to Dataverse inside the organization.

## 11-Add approvals connector

Next we will add a step to include the Approvals Kit from the custom connectors list and select the start business approval process action. The first time that the Approvals Kit connector is used in an environment you will need to signin with your organization account. After sign in you can now select a published version of the approval workflow to be started when the cloud flow is triggered. Once the workflow version has been selected you can save the cloud flow.

## 12-Create new machine request

Now that the Power Automate Cloud flow is saved and that will begin the approval workflow lets create a new Contoso Coffee Machine Request so we can see the created Approval request.

## 13-Approval self approval

Switching to the Power Automate portal we can view approvals requests, alternatively we could have selected the Teams application or the mobile application to view approvals requests. The approval can take up to a couple of minutes to be sent, you can refresh the page to view the received approval request. Once the self approval is available it can be  accepted or declined.

## 14-Example review

Reviewing the example we see we are only half way through the process as we need to add a conditional approval if the value of the request is above $400. Lets look at the process to edit the workflow to add a new stage for conditional manager approval.

## 15-Add variables

We edit the workflow to add two new variables for the machine name and price. The price numeric value will be used to control when the additional approval is required.

## 16-Add conditional stage

Now that the variables have been added to the business approval lets create a new stage. This new stage will be named Manager Approval. In this stage we will select a condition of If Else. Using this condition will allow two nodes to be created for TRUE or FALSE based on the value of Price. The operator of greater than or equal and a value of 400 wil be used to define the conditions inside this stage. With these changes made save the new stage.

## 17-Add TRUE node

Once the new stage is refreshed in the designer you can select the TRUE node to add a new conditional approval where a manager needs to approve the machine order request as the request value is greater than $400. In this example we will select the same approver so that two approvals will be sent to the user. The first approval will be for the self approval and the second approval as the manager.

## 18-Publish new conditional approval

With the new stage added and a conditional approval created lets publish a new version of the approval workflow. Once published this new version can be used by the Approvals Kit connector to add the additional manager approval stage.

## 19-Edit the Cloud Flow

Switching back to the created Power Platform Solution we can now edit the Power Automate Cloud flow.

## 20-Update to v2 version with parameters

Then select the Approvals kit action. The action will now automatically select the latest version of the published approval workflow. Because this version of the workflow has variables for Name and Price two additional values can now be provided from the cloud flow. First we select the Machine name and then the Price values for the added Machine Order request from the Dataverse trigger and add them as values for the action. Having made these changes we can save the cloud flow for these changes to be applied.

## 21-Final approval request

For the final time we can switch back to the Contoso Coffee Machine request application to create another machine request for a machine that has a value greater than $400.

## 22-Two approvals

Switching back to the the Power Automate portal for approvals we can now validate that two approvals requests are sent to this user when we accept the first request. After the first approval is accepted the approvals kit will then generate a second approval request given the value of the machine was greater than $400. We can accept this second request which will complete the overall machine request.

## 23-Approval summary

Switching back to the APproval Manager we have focused only on the process design part of the process. As a author we can use the designer to create a no code approval process. However this designer is only one part of the features of the kit. The Approvals Center also enables you to view and track the status of individual approvals requests and track the process through stages and specific nodes.

## 24-Settings

In addition using the settings page you can decide who you will nominate as your delegate in the case an approval request is received when you are out of the office or a request is received on a day when is not a working day for you.

## 25-Summary

We hope you have found this video a good quick overview of the Approvals kit and how it can help you quickly build approvals workflows.