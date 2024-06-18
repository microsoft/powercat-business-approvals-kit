# Customizing Approval Messages with Dynamic Content

Messages for Approvals Kit nodes give you the ability to customize the message that you want to get sent to the approver/approvers. You can use your knowledge of PowerFx formulas similar to creating an Excel formula to enable you to customize what the messages would look like. 

Generally, you use the PowerFX and put them in curly braces and after equals to get any of the information around the approval workflow. You can also use any PowerFx expression to get evaluated.

This guide explain how to customize messages to include dynamic content using examples. We'll use the provided table mappings to demonstrate how to insert dynamic values into your messages. This guide is designed for users without technical knowledge, so each step will be explained clearly.

## Table and Column Mappings

Below are the tables and their corresponding columns, which can be used to access dynamic data from current runtime node record:

| Table                        | Description                                           | TableMapping  | ColumnsLogicalName | DisplayName      | ColumnsMapping | Example                   |
|------------------------------|-------------------------------------------------------|---------------|--------------------|------------------|----------------|---------------------------|
| Business Approval Runtime Node | To access current runtime node record information   | CurrentNode   |                    |                  |                |                           |
|                              | To access current Node Name                           |               | cat_name           | Name             | Name           | {=CurrentNode.Name}       |
|                              | To access current stage Name                          |               | cat_stage          | Stage            | StageName      | {=CurrentNode.StageName}  |
|                              | To access current stage Id(GUID)                      |               | cat_stage          | Stage            | StageId        | {=CurrentNode.StageId}    |
|                              | To access current Process Name                        |               | cat_processversion | Process Version  | ProcessName    | {=CurrentNode.ProcessName}|
|                              | To access current Process Id(GUID)                    |               | cat_processversion | Process Version  | ProcessId      | {=CurrentNode.ProcessId}  |
|                              | To access Node Type column                            |               | cat_nodetype       | Node Type        | NodeType       | {=CurrentNode.NodeType}   |
|                              | To access Approval Type column of node                |               | cat_approvaltype   | Approval Type    | ApprovalType   | {=CurrentNode.ApprovalType}|
|                              | To access Node Description                            |               | cat_description    | Description      | Description    | {=CurrentNode.Description}|
|                              | To access Node due date of current node               |               |                    |       Due Date   | DueDate        | {=CurrentNode.DueDate}    |
|                              | To Access node due date data type                     |               |cat_nodeduedatedatatype| Node Due Date Data Type        | DueDateDataType        | {=CurrentNode.DueDateDataType}    |
|                              | To access current node Id(GUID)                       |               | cat_businessapprovalruntimenodeid |       | NodeId       | {=CurrentNode.NodeId}   |
|                              | To access current node Approvers info comma separated |               |        |        | Approvers       | {=CurrentNode.Approvers}   |


## Examples and Explanations

### Business Approval Runtime Node

#### Example 1: Accessing the Current Node's Name

To include the current node's name in your message, you can use the following format:

**Example:**
```plaintext
Message:
The current node name is {=CurrentNode.Name}.

Output:
The current node name is Self-Approval.
```
**Explaination:** 
{=CurrentNode.Name}: This placeholder will be replaced by the actual name of the current node when the message is processed.

#### Example 2: Accessing the Current Process Name

To include the current process name in your message, you can use the following format:

**Example:**
```plaintext
Message:
The current process name is {=CurrentNode.ProcessName}.

Output:
The current process name is Machine Request Approvals.
```
**Explaination:** 
{=CurrentNode.ProcessName}: This placeholder will be replaced by the actual process name when the message is processed.


Similarly, other column informations from above table can also be accessed using above format.

## Business Approval Workflow Table and Column Mappings

Below are the columns from the Business Approval Workflow table, which can be used to access dynamic data from the Business Approval Workflow instance table:

| Table                       | Description                                          | ColumnsLogicalName         | DisplayName             | ColumnsMapping               | Example                        |
|-----------------------------|------------------------------------------------------|----------------------------|-------------------------|------------------------------|--------------------------------|
| Business Approval Workflow  | To access Workflow instance record information       |                            |                         |                              |                                |
|                             | To access requested by column of workflow            | cat_requestedby            | Requested By            | RequestedBy                  | {=Workflow.RequestedBy}        |
|                             | To access current Process Name                       | cat_processversion         | Process Version         | ProcessName                  | {=Workflow.ProcessName}        |
|                             | To access current Process Id                         | cat_processversion         | Process Version         | ProcessId                    | {=Workflow.ProcessId}          |
|                             | To access process version                            | cat_version                | Process Version Number  | ProcessVersion               | {=Workflow.ProcessVersion}     |
|                             | To access Additional Information column              | cat_additionalinformation  | Additional Information  | AdditionalInformation        | {=Workflow.AdditionalInformation}|
|                             | To access External Reference column                  | cat_externalreference      | External Reference      | ExternalReference            | {=Workflow.ExternalReference}  |
|                             | To access workflow Id (GUID)                         | cat_businessapprovalworkflowid | Id                   | Id                           | {=Workflow.Id}                 |


## Examples and Explanations

### Example 1: Accessing the Requested By Column

To include the "Requested By" information in your message, you can use the following format:

**Example:**
```plaintext
Message:
This request was made by {=Workflow.RequestedBy}.

Output:
This request was made by user1@testorg.onmicrosoft.com.
```
**Explanation:** 
{=Workflow.RequestedBy}: This placeholder will be replaced by the actual value from the "Requested By" column when the message is processed.

### Example 2: Accessing the External Reference Column

To include the "External Reference" information in your message, you can use the following format:

**Example:** Accessing the External Reference
To include the external reference in your message, you can use the following format:
```plaintext
Message:
The external reference is {=Workflow.ExternalReference}.

Output:
The external reference is aka.ms/ppac.com.
```
**Explanation:** 
{=Workflow.ExternalReference}: This placeholder will be replaced by the actual external reference when the message is processed.

## Business Approval Runtime Stage Table and Column Mappings

Below are the columns from the Business Approval Runtime Stage table, which can be used to access dynamic data from current runtime stage record:

| Table                        | Description                                          | ColumnsLogicalName         | DisplayName             | ColumnsMapping               | Example                        |
|------------------------------|------------------------------------------------------|----------------------------|-------------------------|------------------------------|--------------------------------|
| Business Approval Runtime Stage | To access current runtime stage record information |                           |                         |                              |                                |
|                              | To access stage name of runtime stage                | cat_name                   | Name                    | Name                         | {=CurrentStage.Name}      |
|                              | To access stage id(GUId) of runtime stage            | cat_businessapprovalruntimestageid| Stage ID         | StageId                      | {=CurrentStage.StageId}        |
|                              | To access process name of runtime stage              | cat_processversion         | Process Name            | ProcessName                  | {=CurrentStage.ProcessName}    |
|                              | To access process id of runtime stage                | cat_processversion         | Process ID              | ProcessId                    | {=CurrentStage.ProcessId}      |
|                              | To access description of runtime stage               | cat_description            | Description             | Description                  | {=CurrentStage.Description}    |
|                              | To access stage condition of runtime stage           | cat_stagecondition         | Stage Condition         | Condition                    | {=CurrentStage.Condition}      |
|                              | To access source data(GUID) of runtime stage         | cat_sourcedata             | Source Data             | SourceDataId                 | {=CurrentStage.SourceDataId}   |
|                              | To access source data type of runtime stage          | cat_sourcedatatype         | Source Data Type        | SourceDataType               | {=CurrentStage.SourceDataType} |
|                              | To access Operand of runtime stage                   | cat_operand                | Operand                 | Operand                      | {=CurrentStage.Operand}        |

## Examples and Explanations

### Example 1: Accessing the Stage Name

To include the stage name in your message, you can use the following format:

**Example:**
```plaintext
Message:
The stage name is {=CurrentStage.Name}.

Output:
The stage name is Self-Approval-Stage.
```
**Explanation:**
{=CurrentStage.Name}: This placeholder will be replaced by the actual name of the current stage when the message is processed.


## Business Approval Runtime Node Reminder Table and Column Mappings

Below are the columns from the Business Approval Runtime Stage table, which can be used to access dynamic data from current runtime stage record:

| Table                        | Description                                          | ColumnsLogicalName         | DisplayName             | ColumnsMapping               | Example                        |
|------------------------------|------------------------------------------------------|----------------------------|-------------------------|------------------------------|--------------------------------|
| Business Approval Runtime Node Reminder | To access runtime node reminder records information of current runtime node|                           |                         |                              |                                |
|                              | To access first reminder reminder days               | cat_reminderdays                   | Reminder Days                    | ReminderDays                         | {=First(Reminders).'ReminderDays'}     |
|                              | To access first reminder text                        | cat_remindertext| Reminder Text         | ReminderText                      | {=First(Reminders).'ReminderText'}        |


## Examples and Explanations

### Example 1: Accessing first reminder Reminder Days from reminders collection.

To include the stage name in your message, you can use the following format:

**Example:**
```plaintext
Message:
The reminder days is {=First(Reminders).'ReminderDays'}.

Output:
The reminder days is 4.
```
**Explanation:**
{=First(Reminders).'ReminderDays'}: This placeholder will be replaced by the actual reminder days value of the first reminder from the collection when the message is processed.



## Accessing Variables Value

Value of variables(Business Approval Data Instance) defined in a process can be accessed as shown below:

```
Syntax:{=Parameters.VariableName}
```
If a process has two variables defined 

| Name                        | Description                                          | ColumnsMapping               | Example                        |
|------------------------------|------------------------------------------------------|----------------------------|-------------------------|------------------------------|--------------------------------|
| Price |                      Variable which stores asset price                     | Price   |                     {=Parameters.Price}         |                                |
| Asset Type                   | Variable which stores asset type information       | Asset Type                         | {=Parameters.'Asset Type'}      |

## Examples and Explanations

### Example 1: Accessing Price variable value.

To include the Price variable value in your message, you can use the following format:

**Example:**
```plaintext
Message:
The asset price is {=Parameters.Price}.

Output:
The asset price is 2500.
```
**Explanation:**
{=Parameters.Price}: This placeholder will be replaced by the actual Price passed when the message is processed.

### Example 2: Accessing Asset Type variable value.

To include the 'Asset Type' variable value in your message, you can use the following format:

**Example:**
```plaintext
Message:
The asset type is {=Parameters.'Asset Type'}.

Output:
The asset type is Laptop.
```
**Explanation:**
{=Parameters.Price}: This placeholder will be replaced by the actual value of the Asset Type variable when the message is processed.


## Adding Past Approvals History to the Message: 

To add approvals history table which contains details of the past approvers and the outcomes. Add below to your message:

```
Syntax:{ApprovalsTable}
```

**Example:** 

```plaintext
Past Approvals:
{ApprovalsTable}

Output:
Past Approvals:

| ApproverUPN                                | Name           | Node     | Outcome | Stage |
|--------------------------------------------|----------------|----------|---------|-------|
| shrikants@powercattools.onmicrosoft.com    | 20240603-001249 | Node-1 S1 | Approve | S1    |
| shrikants@powercattools.onmicrosoft.com    | 20240603-001250 | N2 S1    | Approve | S1    |
| shrikants@powercattools.onmicrosoft.com    | 20240603-001251 | N1 s2    | Approve | S2    |

```

## Using some common PowerFx functions in the message

You can also pass common string, date, etc. functions in curly brace after equals.

## Examples and Explanations

### Example 1: Accessing Current date time using Now().

To include the Price variable value in your message, you can use the following format:

**Example:**
```plaintext
Message:
The current datetime is {=Now()}.

Output:
The current datetime is 07/11/2021 20:58:00.
```
**Explanation:**
{=Now()}: This placeholder will be replaced by the current time when the message is processed.

### Example 2: Accessing upper case value of string 'powerfx' using Upper().

To include the Price variable value in your message, you can use the following format:

**Example:**
```plaintext
Message:
Requested By: {=Upper(Workflow.RequestedBy)}.

Output:
Requested By: SHRIKANTS@TESTORG.ONMICROSOFT.COM.
```
**Explanation:**
{=Upper(Workflow.RequestedBy)}: This placeholder will be replaced by the upper case value of requested by when the message is processed.



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
The current approval requested by shrikants@testorg.onmicrosoft.com is pending for stage S2 on node N2 s2.Please make sure to approve this request by 6/19/2024 6:30:00 PM.
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
Requested By: SHRIKANTS@TESTORG.ONMICROSOFT.COM
Time: 6/18/2024 11:36:42 AM

Past Approvals:

ApproverUPN	Name	Node	Outcome	Stage
shrikants@powercattools.onmicrosoft.com	20240603-001249	Node-1 S1	Approve	S1
shrikants@powercattools.onmicrosoft.com	20240603-001250	N2 S1	Approve	S1
shrikants@powercattools.onmicrosoft.com	20240603-001251	N1 s2	Approve	S2
```