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

        var request = new MachineRequest(values, logger);
        await request.Submit(page);

        var approval = new ApprovalResponse(values, logger);

        // Confirm the Self Approval
        if ( await approval.Approve(page) ) {
            // Confirm the Manager Request
            await approval.Approve(page);
        }
    }
}