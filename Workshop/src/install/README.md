# Overview

This application is designed to add specific functionality that is not currently available to automate via command line or REST interfaces. The interfaces could be the Power Platform Command Line interface or via Power Platform API or Dataverse REST interfaces.

> [!IMPORTANT]
>
> 1. The functionality in the application may become unnecessary as administration features of the Power Platform API and REST services grow.
>
> 2. The application is not intended as a production solution and is the key use case is to support Approvals Kit workshop. Usage outside this scope is not a core use case and would require additional review and updates to expand the scope.

## Usage

The sample application is designed for demo environment usage and is not optimized for production use and would require additional considerations and safeguards beyond usage to automate demonstration environments.

If the application was used in production environment, there would be several issues and factors that would need to be considered. For example, secret management of user password would need to be taken into account, as well as key rotation. Additionally, the application would need to undergo a full security review and analysis before being used in a production environment. It is also important to note that non-demo usage is unlikely to know the user password outside of known System Accounts, and these special user accounts would need to have safeguards in place to protect user passwords in a production environment.

> [!IMPORTANT]
> 
> Overall, the application is designed to provide specific functionality within a demo environment, but could potentially be adapted for use in other contexts with appropriate modifications and safeguards which is currently out of scope for this application.

## Key functionality

1. An headless browser session automated via Playwright is used to add key actions required by the workshop
2. Start an interactive browser session as a user
3. Activate a cloud flow
4. Create a connection as an interactive user
5. Query all environment available to a user
6. Execute Playwright scripts

## Assumptions

1. Use of known secure value DEMO_PASSWORD is available to login as interactive user
1. The automation assumed that the user is not configured with Multi Factor Authentication (MFA)
