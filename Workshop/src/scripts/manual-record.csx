/*
 * This class is experimental and very likely to change until the demo feature is completed.
 * Use at your own risk.
 *
 * Ideal end state: Ability to record video of the end to end demo of the Automation Kit learn module
 *
 * Key ideas:
 * - Provide automation of end to end process
 * - Automate the process of video recording
 * - Could be used in the future to help with Automated test testing
 * - Could be used in future to test and record use of the Kit with localization to different languages
 *
 */

#r "Microsoft.Playwright.dll"
#r "Microsoft.Extensions.Logging.dll"
#r "install.dll"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Demo;

public class PlaywrightScript {
    public static void Run(IBrowserContext context, string json, ILogger logger) {
        RunAsync(context, json, logger).Wait();
    }

    public static async Task RunAsync(IBrowserContext context, string json, ILogger logger) {
        var page = context.Pages.First();

        await page.PauseAsync();
    }
}
