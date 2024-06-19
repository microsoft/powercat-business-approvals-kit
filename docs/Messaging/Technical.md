
# Technical Document: EvaluateRuntimeNodeMessage Plugin

## Overview
This document provides an explaination of how the messaging works. This documentation aims to provide a comprehensive understanding of how Node messages are evaluated within the system and replaced by dynamic values. How the expressions are evaluated and replaced by dynamic values.

This document explains the functionality of the `EvaluateNodeMessage` plugin and its integration with Dynamics 365 to handle custom messaging and dynamic value evaluation. 

### Custom API Integration
We have implemented a Custom API named `EvaluateNodeMessage` within Dynamics 365. This API is invoked before creating a node instance record in Power Automate through an unbound action. The Custom API facilitates the execution of a dataverse plugin `EvaluateRuntimeNodeMessage` registered on the custom message `cat_EvaluateNodeMessage`.

### Plugin Functionality
The plugin associated with `cat_EvaluateNodeMessage` performs essential tasks such as creating tables and populating them with data related to the current node, stage, workflow, variables,etc. It requires two input parameters:
- **NodeId (Guid):** Identifies the current node for which the evaluation is being performed.
- **WorkflowId (Guid):** Identifies the workflow context associated with the operation.

### Implementation Details
Internally, the plugin leverages the PowerFx Engine from the PowerFx NuGet package to facilitate dynamic value evaluation.
Upon receiving the input parameters, the plugin retrieves data from Dynamics 365 and evaluates expressions present within the node message column of the current node. These expressions, typically enclosed in curly braces {}, are replaced with actual values derived from the evaluation engine.

#### Steps Involved:
1. **Data Retrieval:** Retrieves necessary data entities such as current node,stage,workflow,etc. details using Dynamics 365 services.
2. **Data creation:** Creates records with necessary details which can be referred in Node Message column.  For all tables and column informations please refer the [User-Guide](./User-Guide.md).

   **Example:** CurrentNode table stores data related to current runtime node information.

3. **Expression Evaluation:** Utilize the PowerFx Engine to evaluate expressions embedded within the node message in curly braces dynamically and return the result.

  An example of what informations can be given in Node Message and what it will look like after processing.

Message:
```
The current approval requested by {=Workflow.RequestedBy} is pending for stage {=CurrentNode.StageName} on node {=CurrentNode.Name}.Please make sure to approve this request by **{=CurrentNode.DueDate}**. 
List of approvers for current node: {=CurrentNode.Approvers}
Process Name: {=Workflow.ProcessName}
Process Version: {=Workflow.ProcessVersion}
External Reference: {=Workflow.ExternalReference}
Leave Duration: {=Parameters.'Leave Duration'}
Price: {=Parameters.Price}
Additional Information: {=Workflow.AdditionalInformation}
Stage Desc: {=CurrentStage.Description}
Node Desc: {=CurrentNode.Description}
Reminders: {=First(Reminders).'ReminderDays'}
Requested By: {=Upper(Workflow.RequestedBy)}
Time:{=Now()}

Past Approvals:
{ApprovalsTable}

```

Output:
```
The current approval requested by contoso@testorg.onmicrosoft.com is pending for stage S2 on node N2 s2.Please make sure to approve this request by 6/19/2024 6:30:00 PM.
List of approvers for current node: Shrikant Singh
Process Name: Process test
Process Version: 1
External Reference: https://aka.ms/ppac.com
Leave Duration: 25
Price: 35000
Additional Information: addn info
Stage Desc: stg desc
Node Desc: n2 s2 desc
Reminders: 4
Requested By: CONTOSO@TESTORG.ONMICROSOFT.COM
Time: 6/18/2024 11:36:42 AM

Past Approvals:

ApproverUPN	Name	Node	Outcome	Stage
user1@testorg.onmicrosoft.com	20240603-001249	Node-1 S1	Approve	S1
user4@testorg.onmicrosoft.com	20240603-001250	N2 S1	Approve	S1
user7@testorg.onmicrosoft.com	20240603-001251	N1 s2	Approve	S2
```



