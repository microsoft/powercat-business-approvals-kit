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
using System.IO;
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

        // Open the Power Automate Environment
        Uri baseUri = new Uri(values["powerAutomatePortal"]);
        await page.GotoAsync(values["powerAutomatePortal"]);

        // Add a new page for Power Apps Portal
        //var powerAppsPortal = await page.Context.NewPageAsync();
        //await powerAppsPortal.GotoAsync(values["powerAutomatePortal"].Replace("powerautomate","powerapps"));

        var contosoCoffeeApplication = await page.Context.NewPageAsync();
        await contosoCoffeeApplication.GotoAsync(values["contosoCoffeeApplication"]);
        
        // Open the Business Approval Management App
        var appPage = await page.Context.NewPageAsync();
        PowerApp app = new PowerApp(values, logger);
        await app.Open(appPage, values["businessApprovalManager"]);

        // Open the Custom Connector
        var connectorPage = await page.Context.NewPageAsync();
        await connectorPage.GotoAsync(values["customConnectorUrl"]);

        var approvalsPage = await page.Context.NewPageAsync();
        await approvalsPage.GotoAsync(values["powerAutomatePortal"] + "/approvals/received");

        var completed = false;

        page.Close += async (_, page) => {
            logger.LogInformation("Page closed");
            completed = true;
        };   


        var videoPath = Path.GetDirectoryName(await page.Video.PathAsync());

        logger.LogInformation("==============================================");
        logger.LogInformation("Manual session ready.");
        logger.LogInformation("----------------------------------------------");
        logger.LogInformation("When completed recording your session close the browser");
        logger.LogInformation($"Saved videos will be in {videoPath}");
        logger.LogInformation("----------------------------------------------");
        while ( ! completed ) {
            System.Threading.Thread.Sleep(1000);
        }

        var saveTime = DateTime.Now.ToString("yyyy-MM-dd-HH.mm");

        logger.LogInformation("Saving videos");

        logger.LogInformation("Power Automate Portal");
        await page.Video.SaveAsAsync(Path.Combine(videoPath, $"PowerAutomatePortal-{saveTime}.webm"));

        //logger.LogInformation("Power Apps Portal");
        //await powerAppsPortal.Video.SaveAsAsync(Path.Combine(videoPath, $"PowerAppsPortal-{saveTime}.webm"));

        logger.LogInformation("Contoso Coffee Application");
        await contosoCoffeeApplication.Video.SaveAsAsync(Path.Combine(videoPath, $"ContosoCoffeeApplication-{saveTime}.webm"));
        
        logger.LogInformation("Business Approval Manager");
        await appPage.Video.SaveAsAsync(Path.Combine(videoPath, $"BusinessApprovalManager-{saveTime}.webm"));
        
        logger.LogInformation("Approvals Connector");
        await connectorPage.Video.SaveAsAsync(Path.Combine(videoPath, $"ApprovalsConnector-{saveTime}.webm"));

        logger.LogInformation("Approvals Portal");
        await approvalsPage.Video.SaveAsAsync(Path.Combine(videoPath, $"ApprovalsPortal-{saveTime}.webm"));
    }
}
