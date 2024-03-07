/*
 * This class is experimental and very likely to change until the demo feature is completed.
 * Use at your own risk.
 *
 * Ideal end state: to validate the Approvals Kit custom connector
 *
 */

using System.Collections.Generic;
using Microsoft.Playwright;
using Microsoft.Extensions.Logging;

namespace Microsoft.PowerPlatform.Demo {
    public class ApprovalsKitCustomConnector {
        Dictionary<string, string> _values;
        ILogger _logger;

        public ApprovalsKitCustomConnector(Dictionary<string, string> values, ILogger logger) {
            _values = values;
            _logger = logger;
        }

        /// <summary>
        /// Update the Approvals Kit OAuth Settings
        /// </summary>
        /// <param name="page">The current authenticated page</param>
        /// <returns><c>True</c> if updated</returns>
        public async Task Update(IPage page) {
            await page.GotoAsync(_values["editUrl"]);
    
            await page.GetByPlaceholder("api.contoso.com").FillAsync(_values["host"]);

            await PowerAutomate.CloseGetStartedIfVisible(page);
    
            await OpenTab(page, "1. General", "2. Security", "Security");

            await page.GetByLabel("Edit").Nth(1).ClickAsync();

            await page.GetByPlaceholder("Client ID").FillAsync(_values["clientId"]);
            await page.GetByPlaceholder("********").FillAsync(_values["clientSecret"]);
            await page.GetByPlaceholder("Resource URL").FillAsync(_values["resourceUrl"]);

            await PowerAutomate.CloseAlertDialog(page);

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

        /// <summary>
        /// Read data from connector and return information found
        /// </summary>
        /// <param name="values"></param>
        /// <param name="page"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public async Task<IDictionary<string, Object>> Get(IPage page) {
            await page.GotoAsync(_values["editUrl"]);

            var result = new System.Dynamic.ExpandoObject() as IDictionary<string, Object>;
            
            result.Add("host", await page.GetByPlaceholder("api.contoso.com").InputValueAsync());

            await OpenTab(page, "1. General", "2. Security", "Security");
            
            result.Add("clientId", await page.GetByPlaceholder("Client ID").InputValueAsync());
            result.Add("resourceUrl", await page.GetByPlaceholder("Resource URL").InputValueAsync());

            await OpenTab(page, "2. Security", "5. Code", "Code");
            
            var operations = new System.IO.StringReader(await page.Locator("//react-dropdown").Nth(0).InnerTextAsync()).ReadLine();
            result.Add("operations", operations);

            if ( operations.Length > 0 ) {
                await OpenTab(page, "5. Code", "6. Test", "Test operation");

                int connectionCounts = int.Parse(_values["approvalsConnectionCount"]);

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
                                            var email = _values["user"].ToLower();
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

            return result;
        }

        private async Task OpenTab(IPage page, string current, string nextTab, string newHeading) {
            await page.GetByLabel(current).ClickAsync();
            await page.GetByRole(AriaRole.Menuitem, new() { Name = nextTab }).ClickAsync();
            await page.GetByRole(AriaRole.Heading, new() { Name = newHeading }).Nth(0).ClickAsync();
        }
    }
}