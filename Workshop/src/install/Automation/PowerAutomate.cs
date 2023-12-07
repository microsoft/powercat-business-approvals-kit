/*
 * This class is experimental and very likely to change until the demo feature is completed.
 * Use at your own risk.
 *
 * Ideal end state: Ability to automate the Power Automate Portal
 * to create and edit Power Automate Cloud flows for the Approvals Kit
 *
 */

using System.Collections.Generic;
using Microsoft.Playwright;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;

namespace Microsoft.PowerPlatform.Demo {
    public class PowerAutomate {
        Dictionary<string, string> _values;
        ILogger _logger;

        public PowerAutomate(Dictionary<string, string> values, ILogger logger) {
            _values = values;
            _logger = logger;
        }

        /// <summary>
        /// Open Power Automate Portal and create Contoso Coffee Approvals Solution
        /// </summary>
        /// <param name="values">Dictionary of values that could be used by the action</param>
        /// <param name="page">The current logged in authenticated page</param>
        /// <param name="logger">Logger to provide feedback</param>
        /// <returns></returns>
        public async Task CreateContosoCoffeeApprovalsSolution(IPage page) {
            Uri baseUri = new Uri(_values["powerAutomatePortal"]);
            await page.GotoAsync(Append(baseUri, "solutions").ToString());

            var solutionFrame = page.FrameLocator("iframe[name=\"widgetIFrame\"]");

            await CreateSolution(page, solutionFrame);

            await CreateNewCloudFlow(page, solutionFrame, "Machine Requests");

            var flowFrame = solutionFrame.FrameLocator("[title=\"Microsoft Flow\"]");

            await flowFrame.GetByText("Untitled").ClickAsync();

            await flowFrame.GetByPlaceholder("Untitled").PressSequentiallyAsync("Machine Requests");

            await CreateDataverseTrigger(flowFrame, "Added", "Machine Orders");

            await AddApprovalsKitCustomConnector(flowFrame);

            await SignInApprovalsKitConnector(page, flowFrame, _values["userEmail"]);

            await ConfigureApprovalsConnector(flowFrame, "Machine Requests (v1)");

            System.Threading.Thread.Sleep(1000);

            await SaveCloudFlow(flowFrame);

            System.Threading.Thread.Sleep(1000);
        }

        private async Task SaveCloudFlow(IFrameLocator flowFrame)
        {
            var saveButton = flowFrame.GetByLabel("Save").Nth(0);
            await saveButton.ClickAsync();

            var running = false;
            while ( !running ) {
                if ( ! await saveButton.IsEnabledAsync() ) {
                    running = true;
                }
                System.Threading.Thread.Sleep(1000);
            }

            while ( ! await saveButton.IsEnabledAsync() ) {
                System.Threading.Thread.Sleep(1000);
            }
        }

        private async Task SaveCloudFlow(ILocator flowFrame)
        {
            var saveButton = flowFrame.GetByLabel("Save").Nth(0);
            await saveButton.ClickAsync();

            var running = false;
            while ( !running ) {
                if ( ! await saveButton.IsEnabledAsync() ) {
                    running = true;
                }
                System.Threading.Thread.Sleep(1000);
            }

            while ( ! await saveButton.IsEnabledAsync() ) {
                System.Threading.Thread.Sleep(1000);
            }
        }

        private async Task ConfigureApprovalsConnector(IFrameLocator solutionFrame, string workflowName)
        {
            await solutionFrame.GetByLabel("Show options").ClickAsync();

            await solutionFrame.GetByLabel(workflowName).Nth(0).ClickAsync();

            var started = DateTime.Now;
            while ( true ) {
                var matches = await solutionFrame.GetByLabel("Workflow Process").AllAsync();
                foreach ( var match in matches ) {
                    var role = await match.GetAttributeAsync("role");
                    if ( role == "combobox" ) {
                        var text = await match.InnerTextAsync();
                        if ( new StringReader(text).ReadLine() == workflowName ) {
                            return;
                        }
                    }
                }
                    
                System.Threading.Thread.Sleep(1000); 
                if ( DateTime.Now.Subtract(started).TotalSeconds > 30 ) {
                    _logger.LogError("Unable to select workflow");
                    break;
                }
            }
        }

        private async Task CreateSolution(IPage page, IFrameLocator solutionFrame)
        {
            await solutionFrame.Locator("[data-test-id=\"newSolution\"]").ClickAsync();

            await CloseAutomationInADayIfVisible(page);
 
            var displayName = solutionFrame.Locator("[data-test-id=\"NewSolutionPanel\"]").GetByLabel("Display name");
            var solutionName = "Contoso Coffee Approvals";

            await displayName.PressSequentiallyAsync(solutionName, new() { Delay = 100 });
            await TypingComplete(displayName, solutionName);

            await solutionFrame.GetByRole(AriaRole.Button, new() { Name = "Publisher *" }).ClickAsync();

            await solutionFrame.GetByRole(AriaRole.Option, new() { Name = "Contoso (Contoso)" }).ClickAsync();

            System.Threading.Thread.Sleep(1000);

            await solutionFrame.Locator("[data-test-id=\"submitButton\"]").ClickAsync();
        }

        private async Task CreateNewCloudFlow(IPage page, IFrameLocator solutionFrame, string name)
        {   
            await solutionFrame.GetByLabel("Object types list").GetByText("All").ClickAsync();

            await StartCloudFlowWizard(page, solutionFrame);
        }

        public async Task OpenExistingCloudFlow(IPage page)
        {   
            var dataverse = new ApprovalsKitDataverse(_values, _logger);

            var solutionId = await dataverse.GetSolutionId("Contoso Coffee Approvals");
            var cloudFlowId = await dataverse.GetCloudFlowId("Machine Requests", solutionId);

            var powerAutomatePortal = _values["powerAutomatePortal"];

            // Open classic designer - Still need to test new modern designer
            var editCloudFlow = $"{powerAutomatePortal}/solutions/{solutionId}/flows/{cloudFlowId}?v3=false";

            await page.GotoAsync(editCloudFlow);
        }

        public async Task UpdateCloudFlow(IPage page)
        {   
            await page.GetByLabel("Start business approval process", new() { Exact = true }).GetByRole(AriaRole.Button, new() { Name = "Start business approval process" }).ClickAsync();

            await page.GetByLabel("Machine Requests (v2)").Last.ClickAsync();

            await page.GetByLabel("Name", new() { Exact = true }).Locator("div").Nth(2).ClickAsync();

            await page.GetByPlaceholder("Search dynamic content").ClickAsync();

            await page.GetByPlaceholder("Search dynamic content").FillAsync("name");

            await page.GetByRole(AriaRole.Button, new() { Name = "Machine Name" }).ClickAsync();

            await page.GetByLabel("Price").Locator("div").Nth(2).ClickAsync();

            await page.GetByPlaceholder("Search dynamic content").ClickAsync();

            await page.GetByPlaceholder("Search dynamic content").FillAsync("price");

            await page.GetByRole(AriaRole.Button, new() { Name = "Price Machine Price" }).ClickAsync();
        
            var container = await page.Locator(".main-container").AllAsync();

            if ( container.Count > 0 ) {
                await SaveCloudFlow(container[0]);
            }
        }

        private async Task SignInApprovalsKitConnector(IPage page, IFrameLocator solutionFrame, string user)
        {
            var started = DateTime.Now;
            while ( DateTime.Now.Subtract(started).TotalMinutes < 1 ) {
                var optionsVisible = await solutionFrame.GetByLabel("Show options").IsVisibleAsync();
                if ( optionsVisible ) {
                    return;
                }

                // Check if Sign in is required
                if ( await solutionFrame.GetByRole(AriaRole.Button, new() { Name = "Sign in" }).IsVisibleAsync() ) {
                    var dialogPage = await page.RunAndWaitForPopupAsync(async () =>
                    {
                        await solutionFrame.GetByRole(AriaRole.Button, new() { Name = "Sign in" }).ClickAsync();
                    });

                    await dialogPage.GotoAsync("https://login.microsoftonline.com/common/oauth2/authorize?client_id=322ba649-b53c-46ef-88bf-2f84e8c705a1&response_type=code&redirect_uri=https%3a%2f%2fglobal.consent.azure-apim.net%2fredirect%2fcat-5fapprovals-20kit-5f416fa96192e41eeb&resource=https%3a%2f%2forgdd3da207.crm.dynamics.com%2f&prompt=select_account&state=2ecfe8c8-aaea-40b4-be39-7843f1d8177b_unitedstates-002_azure-apim.net");

                    await dialogPage.Locator($"[data-test-id=\"{user.ToLower()}\"]").ClickAsync();

                    System.Threading.Thread.Sleep(1000);
                    return;
                }

                System.Threading.Thread.Sleep(1000);
            }

            _logger.LogError("Unable to find 'Sign In' or 'Show options");
        }

        private async Task AddApprovalsKitCustomConnector(IFrameLocator solutionFrame)
        {
            await solutionFrame.GetByRole(AriaRole.Button, new() { Name = "+ New step" }).ClickAsync();

            await solutionFrame.GetByRole(AriaRole.Tab, new() { Name = "Custom Custom" }).ClickAsync();

            await solutionFrame.GetByLabel("Approvals Kit").ClickAsync();

            await solutionFrame.GetByRole(AriaRole.Button, new() { Name = "Start business approval process Premium Approvals Kit" }).ClickAsync();
        }

        private async Task StartCloudFlowWizard(IPage page, IFrameLocator solutionFrame)
        {
            await solutionFrame.GetByRole(AriaRole.Menuitem, new() { Name = "New" }).ClickAsync();

            await solutionFrame.GetByRole(AriaRole.Menuitem, new() { Name = "Automation" }).ClickAsync();

            await solutionFrame.GetByRole(AriaRole.Menuitem, new() { Name = "Cloud flow" }).ClickAsync();

            await solutionFrame.GetByRole(AriaRole.Menuitem, new() { Name = "Automated" }).ClickAsync();

            // The Create new Power Automate is a sub frame, lets reference it
            var wizardFrame = solutionFrame.FrameLocator("iframe[name=\"widgetIFrame\"]");
           
            await wizardFrame.GetByRole(AriaRole.Heading, new() { Name = "Build an automated cloud flow" }).ClickAsync();

            System.Threading.Thread.Sleep(1000);

            await wizardFrame.Locator("[data-test=\"flow-modal-skip-button\"]").ClickAsync();
        }

        private async Task TypingComplete(ILocator control, string text) {
            var lastCheck = DateTime.Now;
            var last = "";
             while ( true ) {
                var newText = await control.InputValueAsync();
                if ( newText == text ) {
                    return;
                } 
                if ( DateTime.Now.Subtract(lastCheck).TotalSeconds > 15 ) {
                    if (  last == newText ) {
                        await control.FillAsync("");
                        await control.ClickAsync();
                        await control.PressSequentiallyAsync( text, new() { Delay = 100 });
                    } else {
                        last = newText;
                    }   
                }
                System.Threading.Thread.Sleep(1000);
            }
        }

        private async Task CreateDataverseTrigger(IFrameLocator solutionFrame, string operatorName, string table) {
            
            await solutionFrame.GetByPlaceholder("Search connectors and triggers").FillAsync("dataverse");

            await solutionFrame.GetByLabel("Microsoft Dataverse", new() { Exact = true }).ClickAsync();

            await solutionFrame.GetByRole(AriaRole.Button, new() { Name = "When a row is added, modified or deleted Premium Microsoft Dataverse" }).ClickAsync();
            
            await solutionFrame.GetByRole(AriaRole.Combobox, new() { Name = "Change type" }).GetByLabel("Show options").ClickAsync();

            await solutionFrame.GetByLabel(operatorName, new() { Exact = true }).ClickAsync();

            await solutionFrame.GetByRole(AriaRole.Textbox, new() { Name = "Table name" }).Locator("div").Nth(2).ClickAsync();

            await solutionFrame.GetByLabel(table).ClickAsync();

            await solutionFrame.GetByRole(AriaRole.Textbox, new() { Name = "Scope" }).Locator("div").Nth(2).ClickAsync();

            await solutionFrame.GetByLabel("Organization").ClickAsync();
        }

        public static async Task CloseAutomationInADayIfVisible(IPage page) {
            if ( await page.GetByLabel("Automation in a Day").IsVisibleAsync() ) {
                await page.GetByLabel("Automation in a Day").GetByLabel("Close").ClickAsync();
            }
            if ( await page.GetByText("Automation in a Day").IsVisibleAsync() ) {
                await page.GetByText("Automation in a Day").GetByLabel("Close").ClickAsync();
            }
        }

        public static async Task CloseAutomationInADayIfVisible(IFrameLocator page) {
            if ( await page.GetByLabel("Automation in a Day").IsVisibleAsync() ) {
                await page.GetByLabel("Automation in a Day").GetByLabel("Close").ClickAsync();
            }
            if ( await page.GetByText("Automation in a Day").IsVisibleAsync() ) {
                await page.GetByText("Automation in a Day").GetByLabel("Close").ClickAsync();
            }
        }

        public static Uri Append(Uri uri, params string[] paths)
        {
            return new Uri(paths.Aggregate(uri.AbsoluteUri, (current, path) => string.Format("{0}/{1}", current.TrimEnd('/'), path.TrimStart('/'))));
        }
    }
}