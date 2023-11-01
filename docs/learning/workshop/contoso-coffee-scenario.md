
# Working with Contoso Coffee

The Approvals Kit learning workshop builds on the second module of the App in a Day workshop from the [Power Platform Training Workshops](https://powerplatform.microsoft.com/training-workshops/) site.

## Workshop Scenario

Act on the request from a business owner to help remove the manual work around the business approval process at Contoso Coffee. The current process conditionally involves multiple stages of approval depending on the machines requested, resulting in delays and inefficiencies.

Our objective for today is to help you build an approval process that extends the current low code Contoso Coffee request process. The extension creates an automated business approvals process that is streamlined and efficient.

  ![Screenshot of starting the Contoso Coffee Machine Ordering app](./media/contoso-coffee-submit-request.png)

This workshop builds on the skills and knowledge gained from the App in a Day workshop, which covered how to build a canvas app and store data in Dataverse. In this workshop, we use the data stored in Dataverse to trigger the defined business approval process.

By the end of this workshop, you're able to identify the different levels of approval required for each machine request. You determine the appropriate level of approval based on the value of the machine, and create an automated workflow that facilitates the approvals process.

## Prerequisites

Before you start this module, ensure that you have the following in a Power Platform Environment:

- The Contoso Coffee solution is imported.

- A current version of the Approvals Kit is installed and configured

- You have an assign role of [Environment Maker](/power-platform/admin/database-security#environments-with-a-dataverse-database) so that you can add a new Business Approvals Cloud flow to integrate with Contoso Coffee.

## Missing Prerequisites?

Are you missing prerequisites for this workshop, the following links could be useful to assist you getting ready:

1. Create or request access to a shared development environment. When you don't have access to a shared development environment, consider [Creating a developer environment with the Power Apps Developer Plan](/power-platform/developer/create-developer-environment)

1. Review the [Import Contoso Coffee](./import-contoso-coffee.md) workshop guide.

1. Install or have your administration team install the Approvals Kit using the [Setup guide](../../setup.md)

> [!div class="nextstepaction"]
> [Next step: First approval](./first-approval.md)