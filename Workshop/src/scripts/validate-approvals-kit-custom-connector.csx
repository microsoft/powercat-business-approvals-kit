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
    public static void Run(IBrowserContext context, string base64Data, ILogger logger) {
        RunAsync(context, base64Data, logger).Wait();
    }

    public static async Task RunAsync(IBrowserContext context, string json, ILogger logger) {
        var page = context.Pages.First();
        var values = new Dictionary<string, string>();

        if ( ! String.IsNullOrEmpty(json) ) {
            values = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        }

        var connector = new ApprovalsKitCustomConnector();
        var result = await connector.Get(page, values, logger);
        string json = JsonSerializer.Serialize(result);

        Console.WriteLine(json);
    }
}
