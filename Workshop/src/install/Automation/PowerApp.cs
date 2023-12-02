/*
 * This class is experimental and very likely to change until the demo feature is completed.
 * Use at your own risk.
 *
 * Ideal end state: Ability to automate the Canvas app using the JavaScript Model
 *
 */
 
using System.Collections.Generic;
using Microsoft.Playwright;
using Microsoft.Extensions.Logging;

namespace Microsoft.PowerPlatform.Demo {
    public class PowerApp {
        /// <summary>
        /// Open a Power Platform Power App handling optional consent dialog
        /// </summary>
        /// <param name="values">Dictionary of values that could be used by the action</param>
        /// <param name="page">The current logged in authenticated page</param>
        /// <param name="logger">Logger to provide feedback</param>
        /// <param name="url">The Power App Url to open</param>
        /// <returns></returns>
        public async Task Open(Dictionary<string, string> values, IPage page, ILogger logger, string url) {
            await page.GotoAsync(url);

            await HandleConsentDialog(page, logger);
        }

        private async Task HandleConsentDialog(IPage page, ILogger logger) {
            var started = DateTime.Now;
            var compete = false;
            while ( !compete && DateTime.Now.Subtract(started).TotalSeconds < 30 ) {
                try {
                    foreach ( var frame in page.Frames ) {
                        var found = false;

                        if ( frame.Name.StartsWith("consent") ) {
                            if ( await frame.Locator(".dialog").IsVisibleAsync() ) {
                                found = true; 
                            } else {
                                System.Threading.Thread.Sleep(1000);
                            }
                        }

                        if ( found ) {
                            // Handle consent dialog
                            await frame.GetByRole(AriaRole.Button, new() { Name = "Allow", Exact = true }).ClickAsync();
                            while ( found && DateTime.Now.Subtract(started).TotalMinutes < 1 ) {
                                if ( await frame.Locator(".dialog").IsVisibleAsync() ) {
                                    found = true; 
                                    System.Threading.Thread.Sleep(1000);
                                } else {
                                    found = false; 
                                    compete = true;
                                }
                            }
                        }
                    }
                } catch {

                }
            }
        }
    }
}