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

        await page.GotoAsync(values["editUrl"]);
    
        await page.GetByPlaceholder("api.contoso.com").FillAsync(values["host"]);

        await page.GetByLabel("1. General").ClickAsync();
        await page.GetByRole(AriaRole.Menuitem, new() { Name = "2. Security" }).ClickAsync();
        await page.GetByRole(AriaRole.Heading, new() { Name = "Security" }).ClickAsync();
        
        await page.GetByLabel("Edit").Nth(1).ClickAsync();

        await page.GetByPlaceholder("Client ID").FillAsync(values["clientId"]);
        await page.GetByPlaceholder("********").FillAsync(values["clientSecret"]);
        await page.GetByPlaceholder("Resource URL").FillAsync(values["resourceUrl"]);

        if ( await page.GetByLabel("Automation in a Day").IsVisibleAsync() ) {
            await page.GetByLabel("Automation in a Day").GetByLabel("Close").ClickAsync();
        }

        await page.GetByLabel("Update connector").ClickAsync(new() { Force = true });

        var start = DateTime.Now;
        var found = false;
        var complete = false;
        while ( DateTime.Now.Subtract(start).TotalMinutes <= 5 && !complete ) {
            ILocator matches = page.Locator(".ms-Spinner-circle").Nth(0);

            var pageMatch = await matches.IsVisibleAsync();

            if ( !found && pageMatch ) {
                found = true;
            }

            if (  !pageMatch && found ) { 
                complete = true;
            } else {
                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}
