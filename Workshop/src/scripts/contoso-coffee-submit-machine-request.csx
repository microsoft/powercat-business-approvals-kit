#r "Microsoft.Playwright.dll"
#r "Microsoft.Extensions.Logging.dll"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Extensions.Logging;

public class PlaywrightScript {
    public static void Run(IBrowserContext context, string base64Data, ILogger logger) {
        RunAsync(context, base64Data, logger).Wait();
    }

    public static async Task RunAsync(IBrowserContext context, string base64Data, ILogger logger) {
        var page = context.Pages.First();
        var values = new Dictionary<string, string>();

        if ( ! String.IsNullOrEmpty(base64Data) ) {
            values = JsonSerializer.Deserialize<Dictionary<string, string>>(base64Data);
        }

        await SubmitMachineRequest(values, page, logger);

        // Confirm the Self Approval
        if ( await ConfirmApprovalRequest(values, page, logger) ) {
            // Confirm the Manager Request
            await ConfirmApprovalRequest(values, page, logger);
        }

    }

    public static async Task SubmitMachineRequest(Dictionary<string, string> values, IPage page, ILogger logger) {
        await page.GotoAsync(values["contosoCoffeeApplication"]);
        
        var appFrame = page.FrameLocator("iframe[name=\"fullscreen-app-host\"]");
        
        await appFrame.Locator(".appmagic-checkbox").First.CheckAsync();

        await appFrame.GetByRole(AriaRole.Button, new() { Name = "Compare 1 item(s)" }).ClickAsync();

        await appFrame.GetByRole(AriaRole.Button, new() { Name = "Submit machine request" }).ClickAsync();

        await appFrame.GetByRole(AriaRole.Button, new() { Name = "OK" }).ClickAsync();

        logger.LogInformation("Machine request created");
    }

    public static async Task<bool> ConfirmApprovalRequest(Dictionary<string, string> values, IPage page, ILogger logger) {
        await page.GotoAsync(values["powerAutomateApprovals"]);
        var started = DateTime.Now;
        var confirmed = false;

        while ( DateTime.Now.Subtract(started).Minutes < 5 && !confirmed ) {
            
            try {
                await page.GetByRole(AriaRole.Radio).Nth(0).ClickAsync();
                
                await page.GetByLabel("Approve", new() { Exact = true }).Nth(0).ClickAsync();

                await page.GetByRole(AriaRole.Button, new() { Name = "Confirm" }).ClickAsync();

                await page.GetByRole(AriaRole.Button, new() { Name = "Done" }).ClickAsync();

                confirmed = true;

                logger.LogInformation("Approval Confirmed");

                await page.ReloadAsync();
            } catch {
                
            }

            if ( ! confirmed ) {
                Console.WriteLine("Waiting 30 seconds");
                System.Threading.Thread.Sleep(30 * 1000);
                await page.ReloadAsync();
            }
        }

        if ( ! confirmed ) {
            logger.LogError("Unable to confirm Approval");
        }

        return confirmed;
    }
}