using System.Collections.Generic;
using Microsoft.Playwright;
using Microsoft.Extensions.Logging;

namespace Microsoft.PowerPlatform.Demo {
    public class PowerAutomate {
        /// <summary>
        /// Open Power Automate Portal and create Contoso Coffee Approvals Solution
        /// </summary>
        /// <param name="values">Dictionary of values that could be used by the action</param>
        /// <param name="page">The current logged in authenticated page</param>
        /// <param name="logger">Logger to provide feedback</param>
        /// <param name="url">The Power App Url to open</param>
        /// <returns></returns>
        public async Task CreateContosoCoffeeApprovalsSolution(Dictionary<string, string> values, IPage page, ILogger logger, string url) {
            Uri baseUri = new Uri(values["powerAutomateEnvironment"]);
            Uri solutions = new Uri(baseUri, "solutions");
            await page.GotoAsync(solutions.ToString());
        }
    }
}