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
        
        var operations = new System.IO.StringReader(await page.Locator("//react-dropdown").Nth(0).InnerTextAsync()).ReadLine();
        result.Add("operations", operations);

        if ( operations.Length > 0 ) {
            await page.GetByLabel("5. Code").ClickAsync();
            await page.GetByRole(AriaRole.Menuitem, new() { Name = "6. Test" }).ClickAsync();
            await page.GetByRole(AriaRole.Heading, new() { Name = "Test operation" }).Nth(0).ClickAsync();

            int connectionCounts = int.Parse(values["approvalsConnectionCount"]);

            var connection = await page.Locator("#customApiTestTab-connections").AllAsync();
            if ( connection.Count > 0 ) {
                var inner = await connection[0].Locator(".ms-Dropdown-title").AllAsync();
                if ( inner.Count > 0 ) {
                    var connectionName = await inner[0].InnerTextAsync();
                    var started = DateTime.Now;
                    while ( String.IsNullOrEmpty(connectionName) && DateTime.Now.Subtract(started).TotalMinutes < 1 ) {
                        connectionName = await inner[0].InnerTextAsync();
                    }
                    
                    // Check if no connection exists
                    if ( connectionName == "None" && connectionCounts == 0 ) {
                        await page.GetByLabel("New connection").ClickAsync();
                        
                        started = DateTime.Now;
                        var found = false;
                        // Create Connection
                        while ( !found && DateTime.Now.Subtract(started).TotalMinutes < 1 ) {
                            try {
                                foreach ( var other in page.Context.Pages ) {
                                    var title = await other.TitleAsync();
                                    if ( title == "Sign in to your account" ) {
                                        var email = values["user"].ToLower();
                                        var match = await other.Locator($"[data-test-id=\"{email}\"]").AllAsync();
                                        if ( match.Count > 0 ) {
                                            await match[0].ClickAsync();
                                            found = true;
                                        }
                                    }
                                }
                            } catch {

                            }
                        }
                    }

                    // Wait until connection exists
                    while ( connectionName == "None" && DateTime.Now.Subtract(started).TotalMinutes < 1 ) {
                        connectionName = await inner[0].InnerTextAsync();
                    }
                }
                
                await page.GetByText("2 GetPublishedWorkflows").ClickAsync();
                await page.GetByLabel("Test operation").ClickAsync();

                await page.GetByRole(AriaRole.Tab, new() { Name = "Response" }).ClickAsync();

                var status = await page.Locator("#customApiTestTab-responseStatus").AllAsync();
                if ( status.Count > 0 ) {
                    var statusText = await status[0].InnerTextAsync();
                    result.Add("status", statusText.Trim());
                }
            }
            
        }

        string json = JsonSerializer.Serialize(result);

        Console.WriteLine(json);
    }
}
