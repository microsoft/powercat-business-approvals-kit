#r "Microsoft.Playwright.dll"
#r "Microsoft.Extensions.Logging.dll"
using System.Linq;
using Microsoft.Playwright;
using Microsoft.Extensions.Logging;

public class PlaywrightScript {
    public static void Run(IBrowserContext context, string base64Data, ILogger logger) {
        var page = context.Pages.First();
        System.Console.WriteLine(base64Data);
        // Record your script with Invoke-OpenBrowser "first.last@contoso.onmicrosoft.com" and add C# script here
        page.PauseAsync().Wait();
    }
}