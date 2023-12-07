/*
 * This class is experimental and very likely to change until the demo feature is completed.
 * Use at your own risk.
 *
 * Ideal end state: The ability to automate the Approval of requests.
 *
 * This class represents an ApprovalResponse and contains a method, Approve, that approves an approval
 * using Playwright and the Power Automate Portal.
 *
 * Key steps
 * - Once an approval request appears, the method clicks the first radio button 
 * - Clicks the "Approve", "Confirm", and "Done" buttons to approve the request.
 * - If the approval is successfully confirmed, the method returns true.
 * - If not, it returns false and logs an error message.
 *
 */
using System.Collections.Generic;
using Microsoft.Playwright;
using Microsoft.Extensions.Logging;

namespace Microsoft.PowerPlatform.Demo {
    public class ApprovalResponse {
        Dictionary<string, string> _values;
        ILogger _logger;

        public ApprovalResponse(Dictionary<string, string> values, ILogger logger) {
            _values = values;
            _logger = logger;
        }

        public async Task<bool> Approve(IPage page) {
            await page.GotoAsync(_values["powerAutomateApprovals"]);
            var started = DateTime.Now;
            var confirmed = false;

            while ( DateTime.Now.Subtract(started).Minutes < 5 && !confirmed ) {
                try {
                    var rows = await page.GetByRole(AriaRole.Row).AllAsync();

                    if ( rows.Count() > 0 ) {
                        await page.GetByRole(AriaRole.Radio).Nth(0).ClickAsync();
                        
                        await page.GetByLabel("Approve", new() { Exact = true }).Nth(0).ClickAsync();

                        await page.GetByRole(AriaRole.Button, new() { Name = "Confirm" }).ClickAsync();

                        await page.GetByRole(AriaRole.Button, new() { Name = "Done" }).ClickAsync();

                        confirmed = true;

                        _logger.LogInformation("Approval Confirmed");

                        await page.ReloadAsync();
                    } else {
                        await page.ReloadAsync();
                        Console.WriteLine("Waiting 30 seconds");
                        System.Threading.Thread.Sleep(30 * 1000);
                    }
                } catch {
                    
                }
            }

            if ( ! confirmed ) {
                _logger.LogError("Unable to confirm Approval");
            }

            return confirmed;
        }
    }
}