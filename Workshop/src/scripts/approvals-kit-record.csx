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
        
        // var request = new MachineRequest();
        // await request.Submit(values, page, logger);

        //var app = new PowerApp();
        //await app.Open(values, page, logger, values["businessApprovalManager"]);

        //TODO check if record exists

        //var kit = new ApprovalKit();
        //await kit.CreateSingleStage(values, page, logger);

        //var automate = new PowerAutomate();
        //await automate.CreateContosoCoffeeApprovalsSolution(values, page, logger);

        //var request = new MachineRequest();
        //await request.Submit(values, page, logger);

        //var approval = new ApprovalResponse();

        //// Confirm the Self Approval
        //await approval.Approve(values, page, logger);

        var kit = new ApprovalKit();
        await kit.OpenWorkflow(values, page, logger);

        
        await page.GetByLabel("Process Designer").ClickAsync();

        await kit.CreateConditionalStage(page, "Manager Approval", logger);

        await page.PauseAsync();
    }
}
