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

        await page.GotoAsync(values["editUrl"]);

        var result = new System.Dynamic.ExpandoObject() as IDictionary<string, Object>;
        
        result.Add("host", await page.GetByPlaceholder("api.contoso.com").InputValueAsync());

        await page.GetByLabel("1. General").ClickAsync();
        await page.GetByRole(AriaRole.Menuitem, new() { Name = "2. Security" }).ClickAsync();
        await page.GetByRole(AriaRole.Heading, new() { Name = "Security" }).ClickAsync();
        
        result.Add("clientId", await page.GetByPlaceholder("Client ID").InputValueAsync());
        result.Add("resourceUrl", await page.GetByPlaceholder("Resource URL").InputValueAsync());

        await page.GetByLabel("2. Security").ClickAsync();
        await page.GetByRole(AriaRole.Menuitem, new() { Name = "5. Code" }).ClickAsync();
        await page.GetByRole(AriaRole.Heading, new() { Name = "Code" }).Nth(0).ClickAsync();
        
        await page.PauseAsync();
        result.Add("operations", new System.IO.StringReader(await page.Locator("//react-dropdown").Nth(0).InnerTextAsync()).ReadLine());

        string json = JsonSerializer.Serialize(result);

        Console.WriteLine(json);
    }
}
