/*
 * This class is experimental and very likely to change until the demo feature is completed.
 * Use at your own risk.
 *
 * Ideal end state: The ability to automate the steps of the Approval Kit in the learning module.
 *
 * Key Experimental Features:
 * - Integration with the Business Approval Manager Power App
 * - Use of Playwright and Document Object model
 * - Refactor use of Custom Page to use JavaScript Object model
 * - Integration with Dataverse to check the state of the Demo and only add required step
 *
 * Notes:
 * - The Custom Page integration is still a work in progress and will be refactored to use JavaScript Object Model (JOM).
 * - JOM is generally more robust than the previous method of using the Document Object Model, and will simplify control searching.
 * - The handling of Gallery control and state changes of Canvas controls in the custom page still needs to be addressed
 *
 */
using System.Data;
using System.Drawing;
using Microsoft.Playwright;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;

namespace Microsoft.PowerPlatform.Demo
{
    /// <summary>
    /// Temporary class to interact with Approvals Kit app.
    /// 
    /// NOTE: This class is expected to change as it makes use of DOM to automate interaction with the application
    /// </summary>
    public class ApprovalKit
    {
        public async Task OpenWorkflow(Dictionary<string, string> values, IPage page, ILogger logger)
        {
            // NOTE: Needs to be refactored to use JavaScript object model

            var businessApprovalManager = values["businessApprovalManager"];
            var workflowId = values.ContainsKey("workflowId") ? values["workflowId"] : "";

            if (string.IsNullOrEmpty(workflowId))
            {
                var client = new ServiceClient(new Uri(values["environmentUrl"]), (env) => Task.FromResult(values["token"]), true, logger);

                // Use logical names
                QueryExpression qryAccounts = new QueryExpression("cat_businessapprovalprocess")
                {
                    ColumnSet = new ColumnSet("cat_businessapprovalprocessid", "cat_name")
                };
                qryAccounts.AddOrder("createdon", OrderType.Descending);
                qryAccounts.TopCount = 1;
                qryAccounts.Criteria.AddCondition("cat_name", ConditionOperator.Equal, "Machine Requests");

                var process = await client.RetrieveMultipleAsync(qryAccounts);

                if (process.Entities.Count > 0)
                {
                    var match = process.Entities[0];
                    workflowId = match["cat_businessapprovalprocessid"]?.ToString();
                    if (!String.IsNullOrEmpty(workflowId))
                    {
                        values.Add("workflowId", workflowId);
                    }
                }
            }

            var worflowUrl = $"{businessApprovalManager}&pagetype=entityrecord&etn=cat_businessapprovalprocess&id={workflowId}";

            var app = new PowerApp();
            await app.Open(values, page, logger, worflowUrl);

            var customPage = new CustomPage();
            await customPage.Setup(page);
        }

        public async Task CreateSingleStage(Dictionary<string, string> values, IPage page, ILogger logger)
        {
            // NOTE: Needs to be refactored to use JavaScript object model

            await page.GetByRole(AriaRole.Button, new() { Name = "Configure Workflow" }).ClickAsync();

            await page.GetByText("No Approval Stage defined. Click (+) to configure your first Approval stage.").ClickAsync();

            var match = await page.Locator("input").AllAsync();
            foreach (var item in match)
            {
                var value = await item.InputValueAsync();
                if (value == "Untitled")
                {
                    await item.ClickAsync();
                    await item.FillAsync("");
                    await item.PressSequentiallyAsync("Machine Requests", new() { Delay = 100 });
                }
            }

            await AddStage(page, "Self Approval", logger, () => Task.CompletedTask);

            await AddNode(page, "Alex Wilber", logger);

            await PublishVersion(page, logger);
        }

        public async Task AddStage(IPage page, string name, ILogger logger, Func<Task> actions)
        {
            // NOTE: Needs to be refactored to use JavaScript object model

            var child = page.Locator("[data-icon-name='CircleAdditionSolid']");
            await page.Locator("button").Filter(new() { Has = child }).ClickAsync();

            await page.Locator(".ms-Panel-headerText").Nth(0).ClickAsync();

            await UpdateControl(page, "txtStageName_2", "Value", name);

            if ( actions != null ) {
                await actions();
            }

            await page.GetByText("Save").Nth(1).ClickAsync();
        }

        public async Task CreateConditionalStage(IPage page, string name, ILogger logger)
        {
            await AddStage(page, name, logger, async () =>
            {
                
                await UpdateControlStaticItems(page, "ddlStageCondition_2", "SelectedItems", "If/Else");
                
                System.Threading.Thread.Sleep(1000);

                await UpdateControlStaticItems(page, "ddlStageConditionSource_2", "SelectedItems", "Request Data");

                System.Threading.Thread.Sleep(1000);

                await UpdateControlDataSourceItems(page, "ddlRequestDataCondition_2", "SelectedItems", "cat_name=Price");

                System.Threading.Thread.Sleep(1000);

                await UpdateControlStaticItems(page, "ddlStageOperand_2", "SelectedItems", "Greater Than or Equals (>=)");

                Console.WriteLine("Complete the stage");

                // System.Threading.Thread.Sleep(1000);
                // await UpdateControlStaticItems(page, "ddlConditionTarget_2", "SelectedItems", "Static Value");

                // System.Threading.Thread.Sleep(1000);

                // await UpdateControl(page, "txtTargetStatic_2", "Value", "400");


                await page.PauseAsync();
            });
        }

        private async Task AddNode(IPage page, string approver, ILogger logger)
        {
            // NOTE: Needs to be refactored to use JavaScript object model

            await page.GetByRole(AriaRole.Button, new() { Name = "+" }).ClickAsync();

            await page.GetByText("Create Node").ClickAsync();

            var name = page.Locator("input[type=\"text\"]").Nth(3);

            await name.ClickAsync();

            await name.PressSequentiallyAsync("Submit", new() { Delay = 100 });

            await page.Locator("#react-combobox-view-0").ClickAsync();

            await page.GetByLabel("Process Designer").GetByText(approver).ClickAsync();

            await page.Locator("input[type=\"text\"]").Nth(3).ClickAsync();

            await page.Locator("button").Filter(new() { HasText = "Add" }).ClickAsync();

            System.Threading.Thread.Sleep(1000);

            await ClickButtonInArea(page, ".ms-Panel-contentInner", "Save", logger);
        }

        private async Task PublishVersion(IPage page, ILogger logger)
        {
            // NOTE: Needs to be refactored to use JavaScript object model

            logger.LogInformation("Publishing");

            await page.GetByRole(AriaRole.Menuitem, new() { Name = "Publish" }).ClickAsync();

            System.Threading.Thread.Sleep(4000);

            await ClickButtonInArea(page, ".ms-Panel-contentInner", "Publish", logger);

            logger.LogInformation("Waiting for published version");
            await WaitUntilPublished(page, logger);
            logger.LogInformation("Published");

            System.Threading.Thread.Sleep(2000);
        }

        public async Task UpdateControl(IPage page, string name, string propertyName, object value) {
            await page.EvaluateAsync<object>("object => setPropertyValueForControl({ controlName: object.control, propertyName: object.property }, object.value)", new { control = name, property = propertyName, value = value});
        }

        public async Task UpdateControlStaticItems(IPage page, string name, string propertyName, object value) {
            await UpdateControlItems(page, name, propertyName, value, StaticItems);
        }

        public async Task UpdateControlDataSourceItems(IPage page, string name, string propertyName, object value) {
            await UpdateControlItems(page, name, propertyName, value, DataSourceItems);
        }

        private List<IDictionary<string,object>> StaticItems(object items, object value) {
            var matches = new List<IDictionary<String, Object>>();
            IDictionary<String, Object> vals  = null;
            if ( items is IDictionary<String, Object>) {
                vals = (IDictionary<String, Object>)items;
            }
            if ( vals != null && vals.ContainsKey("propertyValue")) {
                dynamic valueItems = vals["propertyValue"];
                foreach ( IDictionary<String, Object> propertyValue in valueItems ) {
                    IDictionary<String, Object> itemValue = (IDictionary<String, Object>)propertyValue["Value"];
                    if ( itemValue["Value"].Equals(value) ) {
                        matches.Add(propertyValue);
                    }
                }
            }
            return matches;
        }

        private List<IDictionary<string,object>> DataSourceItems(object items, object value) {
            var parts = value.ToString()?.Split("=");
            var column = parts?[0];
            var matchValue = parts?[1];

            var matches = new List<IDictionary<String, Object>>();
            IDictionary<String, Object> vals  = null;
            if ( items is IDictionary<String, Object>) {
                vals = (IDictionary<String, Object>)items;
            }
            if ( vals != null && vals.ContainsKey("propertyValue")) {
                dynamic valueItems = vals["propertyValue"];
                foreach ( IDictionary<String, Object> propertyValue in valueItems ) {
                    if ( propertyValue[column].Equals(matchValue) ) {
                        matches.Add(propertyValue);
                    }
                }
            }
            return matches;
        }

        public async Task UpdateControlItems(IPage page, string name, string propertyName, object value, Func<object, object,List<IDictionary<String, Object>>> loadItems) {
            while ( true ) {
                var result = await page.EvaluateAsync<object>("getAppMagic().AuthoringTool.Runtime.existsOngoingAsync()");
                if ( result is bool && result.Equals(false) ) {
                    break;
                }
                System.Threading.Thread.Sleep(1000);
            }

            dynamic items = await page.EvaluateAsync<object>(@"object => getPropertyValueFromControl({ controlName: object.control, propertyName: 'Items' })" , new { control = name });

            var matches = loadItems(items, value);
           
            if ( matches != null && matches?.Count > 0 ) {
                await page.EvaluateAsync<object>("object => setPropertyValueForControl({ controlName: object.control, propertyName: object.property }, object.value)", new { control = name, property = "SelectedItems", value = matches});
            }
        }

        public async Task WaitUntilPublished(IPage page, ILogger logger)
        {
            // NOTE: Needs to be refactored to use JavaScript object model
            var published = false;
            var started = DateTime.Now;
            while (!published)
            {
                System.Threading.Thread.Sleep(30000);
                var diff = DateTime.Now.Subtract(started);
                logger.LogInformation($"Polling .... {diff.Hours.ToString("00")}:{diff.Minutes.ToString("00")}.{diff.Seconds.ToString("00")}");
                await ClickButtonInArea(page, ".ms-Panel-contentInner", "Refresh", logger);
                var table = await GetDataTable(page, ".ms-Panel-contentInner", "VersionNo,CreatedBy,Created,PubStatus,Status");
                published = (from row in table.AsEnumerable() where row.Field<string>("PubStatus") == "Published" select row).Count() > 0;
            }
        }

        public async Task<DataTable> GetDataTable(IPage page, string areaLocator, string columns)
        {
            var table = new DataTable();
            var colNames = new List<string>(columns.Split(","));

            foreach (var column in columns.Split(","))
            {
                table.Columns.Add(column, typeof(System.String));
            }

            Rectangle panelArea = await GetBoundingArea(page, areaLocator);

            var rows = await page.Locator(".virtualized-gallery-item").AllAsync();

            foreach (var row in rows)
            {
                if (await ContainedIn(row, panelArea))
                {
                    var tableColumns = await row.Locator(".appmagic-control-view").AllAsync();
                    DataRow dr = table.NewRow();
                    var added = false;
                    foreach (var column in tableColumns)
                    {
                        var name = await column.GetAttributeAsync("data-control-name");

                        foreach (var colName in colNames)
                        {
                            var parts = name?.Split('_');
                            var content = await column.InnerTextAsync();
                            if (parts?.Length == 2 && parts[0].EndsWith(colName) && !String.IsNullOrEmpty(content))
                            {
                                added = true;
                                dr[colName] = content;
                            }
                        }
                    }
                    if (added)
                    {
                        table.Rows.Add(dr);
                    }

                }
            }

            return table;
        }

        private async Task ClickButtonInArea(IPage page, string areaLocator, string button, ILogger logger)
        {
            // NOTE: Needs to be refactored to use JavaScript object model

            Rectangle panelArea = await GetBoundingArea(page, areaLocator);

            if (panelArea.X == 0)
            {
                logger.LogError($"Unable to find {areaLocator}");
                return;
            }

            var controls = await page.Locator("//button").AllAsync();
            foreach (var control in controls)
            {
                var text = await control.InnerTextAsync();
                var buttonLocation = await control.BoundingBoxAsync();
                if ((
                         text == button
                         ||
                         text.EndsWith(button)
                     )
                     && await ContainedIn(control, panelArea))
                {
                    await control.ClickAsync();
                }
            }
        }

        async Task<bool> ContainedIn(ILocator item, Rectangle area)
        {
            // NOTE: Needs to be refactored to use JavaScript object model

            var location = await item.BoundingBoxAsync();
            return location != null && location.Y > area.Top && location.Y <= area.Bottom &&
                     location.X > area.Left && location.X <= area.Right;
        }

        async Task<Rectangle> GetBoundingArea(IPage page, string locator)
        {
            // NOTE: Needs to be refactored to use JavaScript object model

            Rectangle matchArea = new Rectangle();
            var matches = await page.Locator(locator).AllAsync();
            foreach (var match in matches)
            {
                var matchLocation = await match.BoundingBoxAsync();
                matchArea = new Rectangle();
                matchArea.X = (int)matchLocation.X;
                matchArea.Y = (int)matchLocation.Y;
                matchArea.Width = (int)matchLocation.Width;
                matchArea.Height = (int)matchLocation.Height;
            }
            return matchArea;
        }
    }
}