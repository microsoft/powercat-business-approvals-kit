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

        var request = new MachineRequest();
        await request.Submit(values, page, logger);

        var approval = new ApprovalResponse();

        // Confirm the Self Approval
        if ( await approval.Approve(values, page, logger) ) {
            // Confirm the Manager Request
            await approval.Approve(values, page, logger);
        }
    }
}