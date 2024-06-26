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
    public class MachineRequest {
        
        Dictionary<string, string> _values;
        ILogger _logger;

        public MachineRequest(Dictionary<string, string> values, ILogger logger) {
            _values = values;
            _logger = logger;
        }

        /// <summary>
        /// Submit a Machine Order Request in the Contoso Coffee application
        /// </summary>
        /// <param name="values">Dictionary of values that could be used by the action</param>
        /// <param name="page">The current logged in authenticated page</param>
        /// <param name="logger">Logger to provide feedback</param>
        /// <returns></returns>
        public async Task Submit(IPage page) {
            var app = new PowerApp(_values, _logger);
            await app.Open(page, _values["contosoCoffeeApplication"]);
            
            var appFrame = page.FrameLocator("iframe[name=\"fullscreen-app-host\"]");
            
            await appFrame.Locator(".appmagic-checkbox").First.CheckAsync();

            await appFrame.GetByRole(AriaRole.Button, new() { Name = "Compare 1 item(s)" }).ClickAsync();

            await appFrame.GetByRole(AriaRole.Button, new() { Name = "Submit machine request" }).ClickAsync();

            await appFrame.GetByRole(AriaRole.Button, new() { Name = "OK" }).ClickAsync();

            _logger.LogInformation("Machine request created");
        }
    }
}
