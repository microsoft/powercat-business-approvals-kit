using System.Collections.Generic;
using Microsoft.Playwright;
using Microsoft.Extensions.Logging;

namespace Microsoft.PowerPlatform.Demo {
    public class ApprovalResponse {
        public async Task<bool> Approve(Dictionary<string, string> values, IPage page, ILogger logger) {
            await page.GotoAsync(values["powerAutomateApprovals"]);
            var started = DateTime.Now;
            var confirmed = false;

            while ( DateTime.Now.Subtract(started).Minutes < 5 && !confirmed ) {
                try {
                    await page.GetByRole(AriaRole.Radio).Nth(0).ClickAsync();
                    
                    await page.GetByLabel("Approve", new() { Exact = true }).Nth(0).ClickAsync();

                    await page.GetByRole(AriaRole.Button, new() { Name = "Confirm" }).ClickAsync();

                    await page.GetByRole(AriaRole.Button, new() { Name = "Done" }).ClickAsync();

                    confirmed = true;

                    logger.LogInformation("Approval Confirmed");

                    await page.ReloadAsync();
                } catch {
                    
                }

                if ( ! confirmed ) {
                    Console.WriteLine("Waiting 30 seconds");
                    System.Threading.Thread.Sleep(30 * 1000);
                    await page.ReloadAsync();
                }
            }

            if ( ! confirmed ) {
                logger.LogError("Unable to confirm Approval");
            }

            return confirmed;
        }
    }
}