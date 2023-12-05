/*
 * This class is experimental and very likely to change until the demo feature is completed.
 * Use at your own risk.
 *
 * Ideal end state: Ability to record video of the end to end demo of the Automation Kit learn module
 *
 * Key ideas:
 * - Provide automation of end to end process
 * - Automate the process of video recording
 * - Could be used in the future to help with Automated test testing
 * - Could be used in future to test and record use of the Kit with localization to different languages
 *
 */

#r "Microsoft.Playwright.dll"
#r "Microsoft.Extensions.Logging.dll"
#r "install.dll"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Demo;

public class PlaywrightScript {
    public static void Run(IBrowserContext context, string json, ILogger logger) {
        RunAsync(context, json, logger).Wait();
    }

    public static async Task RunAsync(IBrowserContext context, string json, ILogger logger) {
        var page = context.Pages.First();
        var values = new Dictionary<string, string>();

        
        if ( ! String.IsNullOrEmpty(json) ) {
            values = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        }

        // TODO: Needs to be refactored to make the script reentrant so it can be run again and only run
        // required steps
        // 
        // Some ideas to consider
        // - Ability to choose which scenarios to execute (selected or all)
        // - Skip sections?
        // - Add time code index to make easier to import and edit later

        var dataverse = new ApprovalsKitDataverse(values, logger);

        var workflowId = await dataverse.GetApprovalWorkflowId("Machine Requests");

        var createWorkflow = true;
        var createRequests = true;
        var editDefinition = true;
        var editCloudFlow = true;
        var demoTwoStage = true;

        MachineRequest request = new MachineRequest(values, logger);
        ApprovalKit kit = new ApprovalKit(values, logger);
        PowerApp app = new PowerApp(values, logger);
        PowerAutomate automate = new PowerAutomate(values, logger);
        ApprovalResponse approval = new ApprovalResponse(values, logger);

        if ( createWorkflow || String.IsNullOrEmpty(workflowId) ) {
            if ( createRequests ) {
                await request.Submit(page);
            }

            await app.Open(page, values["businessApprovalManager"]);

            await kit.CreateSingleStage(page);
            workflowId = await dataverse.GetApprovalWorkflowId("Machine Requests");
        }

        await page.PauseAsync();

        var solutionId = await dataverse.GetSolutionId("Contoso Coffee Approvals");

        if ( String.IsNullOrEmpty(solutionId) ) {
            await automate.CreateContosoCoffeeApprovalsSolution(page);
            solutionId = await dataverse.GetSolutionId("Contoso Coffee Approvals");
        }

        await page.PauseAsync();

        if ( createRequests ) {
            await request.Submit(page);

            // Confirm the Self Approval (Assume single stage approval)
            await approval.Approve(page);
        }

        await page.PauseAsync();

        if ( !String.IsNullOrEmpty(workflowId) && editDefinition ) {
            await kit.OpenWorkflow(page);

            await page.GetByLabel("Process Designer").ClickAsync();

            await kit.CreateConditionalStage(page, "Manager Approval"); 
        }

        await page.PauseAsync();

        var cloudFlowId = await dataverse.GetCloudFlowId("Machine Requests", solutionId);

        if ( !String.IsNullOrEmpty(cloudFlowId) && editCloudFlow ) {
            await automate.OpenExistingCloudFlow(page);
            await automate.UpdateCloudFlow(page);
        }

        await page.PauseAsync();

        if ( await dataverse.IsTwoStageWorkflowPublishedAndCloudFlowSaved(workflowId, cloudFlowId) && demoTwoStage ) {
            await request.Submit(page);

            // Confirm the Self Approval (Assume single stage approval)
            await approval.Approve(page);

            // Confirm the Manager Approval
            await approval.Approve(page);
        }

        await page.PauseAsync();
    }
}
