#r "Microsoft.Playwright.dll"
#r "Microsoft.Extensions.Logging.dll"
#r ""
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

        var connector = new ApprovalsKitCustomConnector();
        await connector.Update(page, values, logger);
        string json = JsonSerializer.Serialize(result);
    }
}
