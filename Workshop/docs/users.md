# Demo Users

## Getting Started

1. Import the user powershell commands

```pwsh
. .\src\scripts\users.ps1
```

2. Setup a user for the workshop

```pwsh
Invoke-SetupUserForWorkshop "AdeleV@M365x63805008.OnMicrosoft.com"
```

3. (Optional) To setup a group of users, assuming they have the same password the following PowerShell could be used

```pwsh
"LidiaH","LynneR" | Foreach-Object { 
    $upn = "$($_)@M365x63805008.OnMicrosoft.com"
    Write-Host "-------------------------------------------------------------------------"
    Write-Host $upn
    Write-Host "-------------------------------------------------------------------------"
    Invoke-SetupUserForWorkshop $upn
}
```

## Individual Steps

1. Create a development environment

```pwsh
$devEnv = Invoke-UserDevelopmentEnvironment "first.last@contoso.onmicrosoft.com"
```

2. Enable Custom Control in the development environment

When enabling Custom Controls in a canvas application an environment level setting is required. The following example will query and enable the setting if it is currently disabled.

```pwsh
Enable-DevelopmentEnvironmentCustomControls $devEnv
```

3. Install the Creator Kit

Ensure that creator kit is installed

```pwsh
Install-CreatorKit $devEnv
```

4. Enable approvals in the development environment

```pwsh
Enable-Approvals "first.last@contoso.onmicrosoft.com" $devEnv
``````

5. (Optional) Start Browser as user to investigate setup

```pwsh
Invoke-StartBrowser $devEnv
```

## Discussion

Key to a demo environment or trial environment is the ability to create users. When considering users some important factors to keep in mind:

| Factor | Summary | Pros | Cons |
| --- | --- | --- | --- |
| Number of Users | Total number of users in the demo or trial environment | Provides a more realistic test environment, identifies potential scalability issues | More difficult to manage, requires more resources, may not be representative of target audience |
| User Personas | Fictional representations of different types of users | Ensures product is designed with target audience in mind, identifies potential issues or pain points | Time-consuming, may not accurately reflect target audience |
| Security Roles | Different levels of access and permissions for users | Ensures users only have access to relevant features, ensures sensitive information is only accessible to authorized users | Complex to set up, potential security vulnerabilities or data breaches if not set up correctly |
| Password Management | Ensure users can easily manage passwords and strong password policies are in place | Ensures security of user accounts | Can be difficult to manage and enforce |
| Ability to Claim Demo User Accounts | Allow users to claim demo user accounts to access the lab | Improves user experience and access to the lab | Can be difficult to manage and may require additional resources |
| User Feedback Mechanisms | Provide users with a way to provide feedback on their experience | Improves user experience and identifies areas for improvement | Feedback may not always be accurate or actionable |
| User Support Resources | Provide users with access to support resources | Improves user experience and ensures successful completion of the lab | Can be time-consuming to set up and manage |
| User Training | Provide users with training materials or a training session | Improves user experience and ensures successful completion of the lab | Can be time-consuming to set up and manage |
| User Communication Channels | Provide users with a way to communicate with each other or with lab administrators | Improves user experience and facilitates collaboration | Can be difficult to manage and monitor |
| User Tracking and Analytics | Track user activity and gather analytics | Provides insights into user behavior and identifies areas for improvement | Can be time-consuming to set up and manage |
| User Incentives | Provide users with incentives for completing tasks or providing feedback | Encourages user engagement and participation | Can be difficult to manage and may not always be effective |
| User Access Control | Ensure users only have access to relevant features and functions | Ensures security of user accounts and sensitive information | Can be complex to set up and manage |

## Minimum Features

Looking at these factors one approach is to consider a set of minimum features and investigate other features as stretch goals based on your time and the availability of solutions to meet your needs. The minimum list could be the following as an example when considering a hands on lab style of setup.

1. Minimum number of Users: Decide on the the number of users in the demo or trial environment is representative of the product's target audience. This will provide a more realistic test environment and also possible take into account different personas if you provide more than one account.

2. Security Roles: Set up security roles to ensure that users only have access to the features and functions that are relevant to their role. This will ensure that sensitive information is only accessible to authorized users and reduce the risk of potential security vulnerabilities or data breaches. To help emphasize this consider providing the user more than one user account so that they can compare and contrast the different user accounts (For example provide Admin, User account)

3. Password Management: Ensure that users are able to easily manage their passwords and that strong password policies are in place to ensure security. This will help to protect user accounts and sensitive information.

4. User Support Resources: Provide users with access to support resources, such as documentation, to ensure that they are able to successfully evaluate solutions setup.

5. User Training: Provide users with training materials or a training session to ensure that they understand how to use the lab and complete the necessary tasks. This will improve the user experience and increase the likelihood of successful completion of the lab.

By focusing on these factors, the hands-on lab can be set up in a way that ensures a positive user experience, while also reducing the risk of potential security vulnerabilities or data breaches.

## Security Roles

In the Power Platform, the role of Environment Administrator is responsible for managing and administering the development environment. As the developer environment is considered a personal development environment, the user is also an Administrator of this environment. However, it is important to note that the user is still bound by enterprise data loss prevention (DLP) policies, which they cannot change. This means that while the user has administrative access to the development environment, they must still adhere to the organization's DLP policies.

In contrast, in shared environments such as the Center of Excellence environment or shared test and production environments, the user's access is managed by Dataverse security roles. The Basic User role is a user who has limited access to the environment, and is only able to perform basic tasks such as creating and editing records. The user's access to these shared environments is managed by Dataverse security roles, which are configured by the Environment Administrator. This ensures that the user only has access to the necessary features and functions, and that sensitive information is only accessible to authorized users.

In summary, while the user is an Administrator of their personal development environment, they are still bound by enterprise DLP policies. In shared environments, the user's access is managed by Dataverse security roles, which are configured by the Environment Administrator to ensure that users only have access to the necessary features and functions.

## Environment Strategy

When considering users one environment strategy for Power Platform that includes personal development environments, a central Center of Excellence environment, and shared test and production environments could be the following:

1. Personal Development Environments: Provide each attendee with their own personal development environment, which they can use to develop and test their own solutions. This will ensure that attendees have a dedicated space to work on their own projects and experiment with different features and functions.

2. Center of Excellence Environment: Set up a central Center of Excellence environment, which can be used to manage and govern the Power Platform environment. This environment can be used to manage security roles, monitor usage and performance, and provide support and guidance to attendees.

3. Shared Test Environment: Set up a shared test environment, which can be used to deploy solutions for testing and quality assurance. This environment should be configured with least privilege users, to ensure that only authorized users have access to sensitive information.

4. Shared Production Environment: Set up a shared production environment, which can be used to deploy solutions for production use. This environment should also be configured with least privilege users, to ensure that only authorized users have access to sensitive information.

By setting up this environment strategy, attendees will have a dedicated space to work on their own projects, while also ensuring that solutions are thoroughly tested and deployed in a secure and controlled manner. The Center of Excellence environment can be used to provide guidance and support to attendees, while also ensuring that the environment is properly managed and governed.

## Distributing Demo Accounts

Having a printed out list of usernames as paper slips would be a simpler approach in terms of technology and setup. However, it would require manual distribution and tracking of the slips, which could be time-consuming and prone to errors. Additionally, making last-minute changes with paper printing and allocating new users could be difficult with paper slips. It would require reprinting the entire list or manually adding new slips, which could be time-consuming and potentially lead to errors. Furthermore, there would be no way to verify the identity of the person claiming a username, which could lead to issues with duplicate or fraudulent accounts.

On the other hand, an online sign-up process using low code Power Platform Pages would require more technical setup, but would allow for self-service sign-up and verification of user identity. This approach could also automate the allocation of user accounts, reducing the need for manual tracking and distribution of usernames. Overall, while the online sign-up process may be more complex to set up initially, it would likely be a more efficient and secure approach in the long run.