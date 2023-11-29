using System.Collections.Generic;
using System.Data;
using System.Drawing;
using Microsoft.Playwright;
using Microsoft.Extensions.Logging;

namespace Microsoft.PowerPlatform.Demo {
    public class ApprovalKit {
        public async Task CreateSingleStage(Dictionary<string, string> values, IPage page, ILogger logger) {
            await page.GetByRole(AriaRole.Button, new() { Name = "Configure Workflow" }).ClickAsync();

            await page.GetByText("No Approval Stage defined. Click (+) to configure your first Approval stage.").ClickAsync();
 
            var match = await page.Locator("input").AllAsync();
            foreach ( var item in match ) {
                var value = await item.InputValueAsync();
                if ( value == "Untitled") {
                    await item.ClickAsync();
                    await item.FillAsync("");
                    await item.PressSequentiallyAsync("Machine Requests", new() { Delay = 100 });
                }
            }

            await AddStage(page,  "Self Approval");

            await AddNode(page, "Alex Wilber", logger);            
            
            await PublishVersion(page, logger);
        }

        private async Task AddStage(IPage page, string name) {
            var buttons = await page.Locator("[data-icon-name='CircleAdditionSolid']").AllAsync();
            
            foreach ( var item in buttons ) {
                await item.ClickAsync();
            }

            await page.Locator(".ms-Panel-headerText").Nth(0).ClickAsync();

            await FillInputControl(page, "Name", name);

            System.Threading.Thread.Sleep(1000);

            await page.GetByText("Save").Nth(1).ClickAsync();
        }

        private async Task AddNode(IPage page, string approver, ILogger logger) {
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

        private async Task PublishVersion(IPage page, ILogger logger) {
            logger.LogInformation("Publishing");
            await page.GetByRole(AriaRole.Menuitem, new() { Name = "Publish" }).ClickAsync();

            System.Threading.Thread.Sleep(4000);

            await ClickButtonInArea(page, ".ms-Panel-contentInner", "Publish", logger);

            logger.LogInformation("Waiting for published version");
            await WaitUntilPublished(page, logger);
            logger.LogInformation("Published");

            System.Threading.Thread.Sleep(2000);
        }

        private async Task FillInputControl(IPage page, string name, string value) {
            var controls = await page.Locator($"input:right-of(:text(\"{name}\"))").AllAsync();
            var pressed = false;
            foreach ( var control in controls ) {
                if ( ! pressed ) {
                    await control.ClickAsync();
                    await control.PressSequentiallyAsync(value, new() { Delay = 100 });
                    pressed = true;
                }
            }
        }

        public async Task WaitUntilPublished(IPage page, ILogger logger) {
            var published = false;
            var started = DateTime.Now;
            while ( ! published ) {
                System.Threading.Thread.Sleep(30000);
                var diff = DateTime.Now.Subtract(started);
                logger.LogInformation($"Polling .... {diff.Hours.ToString("00")}:{diff.Minutes.ToString("00")}.{diff.Seconds.ToString("00")}");
                await ClickButtonInArea(page, ".ms-Panel-contentInner", "Refresh", logger);
                var table = await GetDataTable(page, ".ms-Panel-contentInner", "VersionNo,CreatedBy,Created,PubStatus,Status");
                published = (from row in table.AsEnumerable() where row.Field<string>("PubStatus") == "Published" select row ).Count() > 0;
            }
        }

        public async Task<DataTable> GetDataTable(IPage page, string areaLocator, string columns) {
            var table = new DataTable();
            var colNames = new List<string>(columns.Split(","));

            foreach ( var column in columns.Split(",") ) {
                table.Columns.Add(column, typeof(System.String));
            }

            Rectangle panelArea = await GetBoundingArea(page, areaLocator);

            var rows = await page.Locator(".virtualized-gallery-item").AllAsync();

            foreach ( var row in rows ) {
                if ( await ContainedIn(row, panelArea) ) {
                    var tableColumns = await row.Locator(".appmagic-control-view").AllAsync();
                    DataRow dr = table.NewRow();
                    var added = false;
                    foreach ( var column in tableColumns ) {
                        var name = await column.GetAttributeAsync("data-control-name");
                       
                        foreach ( var colName in colNames ) {
                            var parts = name?.Split('_');
                            var content = await column.InnerTextAsync();
                            if ( parts?.Length == 2 && parts[0].EndsWith(colName) && ! String.IsNullOrEmpty(content) ) {
                                added = true;
                                dr[colName] = content;
                            }
                        }
                    }
                    if ( added ) {
                        table.Rows.Add(dr);
                    }
                    
                }
            }

            return table;
        }

        private async Task ClickButtonInArea(IPage page, string areaLocator, string button, ILogger logger) {
            Rectangle panelArea = await GetBoundingArea(page, areaLocator);

            if ( panelArea.X == 0 ) {
                logger.LogError($"Unable to find {areaLocator}");
                return;
            }
            
            var controls = await page.Locator("//button").AllAsync();
            foreach ( var control in controls ) {
                var text = await control.InnerTextAsync();
                var buttonLocation = await control.BoundingBoxAsync();
               if ( (
                        text == button
                        ||
                        text.EndsWith(button)
                    ) 
                    && await ContainedIn(control, panelArea) ) {
                    await control.ClickAsync();
                }
            }
        }

        async Task<bool> ContainedIn(ILocator item, Rectangle area) {
            var location = await item.BoundingBoxAsync();
            return location != null && location.Y > area.Top && location.Y <= area.Bottom &&
                     location.X > area.Left && location.X <= area.Right;
        }

        async Task<Rectangle> GetBoundingArea(IPage page, string locator) {
            Rectangle matchArea = new Rectangle();
            var matches = await page.Locator(locator).AllAsync();
            foreach ( var match in matches ) {
                var matchLocation = await match.BoundingBoxAsync();
                matchArea = new Rectangle();
                matchArea.X = (int)matchLocation.X;
                matchArea.Y = (int)matchLocation.Y;
                matchArea.Width =  (int)matchLocation.Width;
                matchArea.Height = (int)matchLocation.Height;
            }
            return matchArea;
        }
    }
}