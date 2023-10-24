// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Microsoft.PowerApps.TestEngine.Config;
using Microsoft.PowerApps.TestEngine.System;
using Microsoft.PowerPlatform.Config;

namespace Microsoft.PowerApps.TestEngine.TestInfra
{
    /// <summary>
    /// Playwright implementation infrastructure function
    /// </summary>
    public class PlaywrightTestInfraFunctions
    {
        private readonly BrowserConfiguration _config;
        private readonly IFileSystem _fileSystem;
        private readonly ILogger _logger;

        public static string BrowserNotSupportedErrorMessage = "Browser not supported by Playwright, for more details check https://playwright.dev/dotnet/docs/browsers";
        private IPlaywright PlaywrightObject { get; set; }
        private IBrowser Browser { get; set; }
        private IBrowserContext BrowserContext { get; set; }
        private IPage Page { get; set; }

        private List<string> PagesOfInterest { get;set;} = new List<string>();

        public List<string> Responses { get;set;} = new List<string>();

        public PlaywrightTestInfraFunctions(BrowserConfiguration config, ILogger logger, IFileSystem fileSystem)
        {
            _config = config;
            _logger = logger;
            _fileSystem = fileSystem;
        }

        // Constructor to aid with unit testing
        public PlaywrightTestInfraFunctions(BrowserConfiguration config, ILogger logger, IFileSystem fileSystem,
            IPlaywright playwrightObject = null, IBrowserContext browserContext = null, IPage page = null) : this(config, logger, fileSystem)
        {
            PlaywrightObject = playwrightObject;
            Page = page;
            BrowserContext = browserContext;
        }

        public async Task SetupAsync()
        {
            if (_config == null)
            {
                _logger.LogError("Browser config cannot be null");
                throw new InvalidOperationException();
            }

            if (string.IsNullOrEmpty(_config.Browser))
            {
                _logger.LogError("Browser cannot be null");
                throw new InvalidOperationException();
            }

            if (PlaywrightObject == null)
            {
                PlaywrightObject = await Playwright.Playwright.CreateAsync();
            }

            var launchOptions = new BrowserTypeLaunchOptions()
            {
                Headless = _config.Commands.GetCommandValue(ParseCommand.Headless, String.Empty) != "N",
                Timeout = _config.Commands.GetCommandValue(ParseCommand.Timeout, 2 * 60 * 1000)
            };

            var browser = PlaywrightObject[_config.Browser];
            if (browser == null)
            {
                _logger.LogError("Browser not supported by Playwright, for more details check https://playwright.dev/dotnet/docs/browsers");
                throw new UserInputException(UserInputException.ErrorMapping.UserInputExceptionInvalidTestSettings.ToString());
            }

            Browser = await browser.LaunchAsync(launchOptions);
            _logger.LogInformation("Browser setup finished");

            var contextOptions = new BrowserNewContextOptions();

            if (!string.IsNullOrEmpty(_config.Device))
            {
                contextOptions = PlaywrightObject.Devices[_config.Device];
            }

            if (_config.Commands.GetCommandValue(ParseCommand.Record, String.Empty) == "Y")
            {
                var videoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"videos");
                if ( Path.Exists(videoPath)) {
                    Directory.CreateDirectory(videoPath);
                }
                contextOptions.RecordVideoDir = videoPath;
            }

            if (_config.ScreenWidth != null && _config.ScreenHeight != null)
            {
                contextOptions.ViewportSize = new ViewportSize()
                {
                    Width = _config.ScreenWidth.Value,
                    Height = _config.ScreenHeight.Value
                };
            }

            BrowserContext = await Browser.NewContextAsync(contextOptions);
            _logger.LogInformation("Browser context created");
        }

        // public async Task SetupNetworkRequestMockAsync()
        // {

        //     var mocks = _singleTestInstanceState.GetTestSuiteDefinition().NetworkRequestMocks;

        //     if (mocks == null || mocks.Count == 0)
        //     {
        //         return;
        //     }

        //     if (Page == null)
        //     {
        //         Page = await BrowserContext.NewPageAsync();
        //     }

        //     foreach (var mock in mocks)
        //     {

        //         if (string.IsNullOrEmpty(mock.RequestURL))
        //         {
        //             _logger.LogError("RequestURL cannot be null");
        //             throw new UserInputException(UserInputException.ErrorMapping.UserInputExceptionTestConfig.ToString());
        //         }

        //         if (!_fileSystem.IsValidFilePath(mock.ResponseDataFile) || !_fileSystem.FileExists(mock.ResponseDataFile))
        //         {
        //             _logger.LogError("ResponseDataFile is invalid or missing");
        //             throw new UserInputException(UserInputException.ErrorMapping.UserInputExceptionInvalidFilePath.ToString());
        //         }

        //         await Page.RouteAsync(mock.RequestURL, async route => await RouteNetworkRequest(route, mock));
        //     }
        // }

        // public async Task RouteNetworkRequest(IRoute route, NetworkRequestMock mock)
        // {
        //     // For optional properties of NetworkRequestMock, if the property is not specified, 
        //     // the routing applies to all. Ex: If Method is null, we mock response whatever the method is.
        //     bool notMatch = false;

        //     if (!string.IsNullOrEmpty(mock.Method))
        //     {
        //         notMatch = !string.Equals(mock.Method, route.Request.Method);
        //     }

        //     if (!string.IsNullOrEmpty(mock.RequestBodyFile))
        //     {
        //         notMatch = notMatch || !string.Equals(route.Request.PostData, _fileSystem.ReadAllText(mock.RequestBodyFile));
        //     }

        //     if (mock.Headers != null && mock.Headers.Count != 0)
        //     {
        //         foreach (var header in mock.Headers)
        //         {
        //             var requestHeaderValue = await route.Request.HeaderValueAsync(header.Key);
        //             notMatch = notMatch || !string.Equals(header.Value, requestHeaderValue);
        //         }
        //     }

        //     if (!notMatch)
        //     {
        //         await route.FulfillAsync(new RouteFulfillOptions { Path = mock.ResponseDataFile });
        //     }
        //     else
        //     {
        //         await route.ContinueAsync();
        //     }
        // }

        public void RecordRequest(string query) {
            PagesOfInterest.Add(query);
        }

        public async Task GoToUrlAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                _logger.LogError("Url cannot be null or empty");
                throw new InvalidOperationException();
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                _logger.LogError("Url is invalid");
                throw new InvalidOperationException();
            }

            if (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)
            {
                _logger.LogError("Url must be http/https");
                throw new InvalidOperationException();
            }

            if (Page == null)
            {
                Page = await BrowserContext.NewPageAsync();
                Page.Response += async (_, response) => {
                    if ( PagesOfInterest.Contains(response.Request.Url) || PagesOfInterest.Any(part => response.Request.Url.Contains(part) ) && response.Request.Method != "OPTIONS" ) {
                        Responses.Add(Encoding.Default.GetString(await response.BodyAsync()));
                    }
                };
            }

            var response = await Page.GotoAsync(url);

            // The response might be null because "The method either throws an error or returns a main resource response.
            // The only exceptions are navigation to about:blank or navigation to the same URL with a different hash, which would succeed and return null."
            //(From playwright https://playwright.dev/dotnet/docs/api/class-page#page-goto)
            if (response != null && !response.Ok)
            {
                _logger.LogTrace($"Page is {url}, response is {response?.Status}");
                _logger.LogError($"Error navigating to page.");
                throw new InvalidOperationException();
            }
        }

        public async Task EndTestRunAsync()
        {
            if (BrowserContext != null)
            {
                await Task.Delay(200);
                await BrowserContext.CloseAsync();
            }
        }

        public async Task DisposeAsync()
        {
            if (BrowserContext != null)
            {
                await BrowserContext.DisposeAsync();
                BrowserContext = null;
            }
            if (PlaywrightObject != null)
            {
                PlaywrightObject.Dispose();
                PlaywrightObject = null;
            }
        }

        private void ValidatePage()
        {
            if (Page == null)
            {
                throw new InvalidOperationException("Page is null, make sure to call GoToUrlAsync first");
            }
        }

        public async Task ScreenshotAsync(string screenshotFilePath)
        {
            ValidatePage();
            if (!_fileSystem.IsValidFilePath(screenshotFilePath))
            {
                throw new InvalidOperationException("screenshotFilePath must be provided");
            }

            await Page.ScreenshotAsync(new PageScreenshotOptions() { Path = $"{screenshotFilePath}" });
        }

        public async Task FillAsync(string selector, string value)
        {
            ValidatePage();
            await Page.FillAsync(selector, value);
        }

        public async Task ClickAsync(string selector)
        {
            ValidatePage();
            await Page.ClickAsync(selector);
        }

        public async Task ClickIfAppearAsync(int timeout, params PlaywrightCondition[] conditions) {
            var start = DateTime.Now;
            var complete = false;
            ValidatePage();
            while ( DateTime.Now.Subtract(start).TotalMinutes <= timeout && !complete ) {
                foreach ( var condition in conditions) {
                    if ( ! string.IsNullOrEmpty(condition.Label) ) {
                        // Collect could change save array to handle collection modification
                            var pages = Page.Context.Pages.ToArray();
                            foreach ( var page in pages ) {
                                ILocator matches = page.Locator($"button[aria-label='{condition.Label}']");

                                var pageMatch = await matches.IsVisibleAsync() && ((condition.MaxClick == 0 ) || (condition.Click < condition.MaxClick && condition.MaxClick > 0));

                                var frameMatch = false;
                                if (  !pageMatch ) {
                                    // Check if a IFrame include the selector of interest
                                    try {
                                        // No match for the button on the page
                                        var pageFrames = page.Frames;
                                        foreach ( var frame in pageFrames ) {
                                            // Search each IFrame
                                            matches = frame.Locator($"button[aria-label='{condition.Label}']");

                                            frameMatch = await matches.IsVisibleAsync() && ((condition.MaxClick == 0 ) || (condition.Click < condition.MaxClick && condition.MaxClick > 0));
                                            if ( frameMatch ) {
                                                break;
                                            } 
                                        }
                                    } catch {
                                        // Frame error ... could have been navigating on page. Will try again in the next iteration
                                    }
                                }
                                
                                if ( pageMatch || frameMatch ) {
                                    if ( !string.IsNullOrEmpty(condition.Message)) {
                                        _logger.LogInformation(condition.Message);
                                    }
                                    if ( condition.Pause ) {
                                        await page.PauseAsync();
                                    }

                                    await matches.ClickAsync();
                                    condition.Click++;

                                    if ( condition.Final ) {
                                        complete = true;
                                        break;
                                    }
                                }
                            }
                    } else {
                        if ( ! string.IsNullOrEmpty(condition.Selector) ) {
                            // Collect could change save array to handle collection modification
                            var pages = Page.Context.Pages.ToArray();
                            foreach ( var page in pages ) {
                                // Check if a IFrame include the selector of interest
                                try {
                                    var pageFrames = page.Frames;
                                    foreach ( var frame in pageFrames ) {
                                        if ( await frame.IsVisibleAsync(condition.Selector) && condition.IsVisible && condition.Final ) {
                                            complete = true;
                                            break;
                                        } 

                                        if ( await frame.IsVisibleAsync(condition.Selector) && ((condition.MaxClick == 0 ) || (condition.Click < condition.MaxClick && condition.MaxClick > 0)) ) {
                                            if ( condition.ClickDelay > 0 ) {
                                                Thread.Sleep(condition.ClickDelay);
                                            }

                                            if ( !string.IsNullOrEmpty(condition.Message)) {
                                                _logger.LogInformation(condition.Message);
                                            }

                                            await frame.ClickAsync(condition.Selector);
                                            condition.Click++;

                                            if ( condition.Pause ) {
                                                await page.PauseAsync();
                                            }
                                            if ( condition.Final ) {
                                                complete = true;
                                                break;
                                            }
                                        }
                                    }
                                } catch {
                                    // Frame error ... could have been navigating on page. Will try again in the next iteration
                                }

                                // Check if the page has the selector of interest
                                try {
                                    if ( await page.IsVisibleAsync(condition.Selector) && condition.IsVisible && condition.Final ) {
                                        complete = true;
                                        break;
                                    } 

                                    if ( await page.IsVisibleAsync(condition.Selector) && ((condition.MaxClick == 0 ) || (condition.Click < condition.MaxClick && condition.MaxClick > 0)) ) {
                                        if ( condition.ClickDelay > 0 ) {
                                            Thread.Sleep(condition.ClickDelay);
                                        }

                                        if ( !string.IsNullOrEmpty(condition.Message)) {
                                            _logger.LogInformation(condition.Message);
                                        }
                                        
                                        await page.ClickAsync(condition.Selector);
                                        condition.Click++;

                                        if ( condition.Pause ) {
                                            await page.PauseAsync();
                                        }
                                        if ( condition.Final ) {
                                            complete = true;
                                            break;
                                        }
                                    }
                                } catch {
                                    // page error ... could have been navigating on page. Will try again in the next iteration
                                }
                            }
                        }
                    }
                }
                // Wait a second and retry
                Thread.Sleep(1000);
            }

            if ( !complete ) {
                _logger.LogError("Unable to find items to click");
            }
        }

        public async Task AddScriptTagAsync(string scriptTag, string frameName)
        {
            ValidatePage();
            if (string.IsNullOrEmpty(frameName))
            {
                await Page.AddScriptTagAsync(new PageAddScriptTagOptions() { Path = scriptTag });
            }
            else
            {
                await Page.Frame(frameName).AddScriptTagAsync(new FrameAddScriptTagOptions() { Path = scriptTag });
            }
        }

        public async Task<T> RunJavascriptAsync<T>(string jsExpression)
        {
            ValidatePage();

            //if (!jsExpression.Equals(PowerAppFunctions.CheckPowerAppsTestEngineObject))
            //{
                _logger.LogDebug("Run Javascript: " + jsExpression);
            //}

            return await Page.EvaluateAsync<T>(jsExpression);
        }

        // Justification: Limited ability to run unit tests for 
        // Playwright actions on the sign-in page
        [ExcludeFromCodeCoverage]
        public async Task HandleUserEmailScreen(string selector, string value)
        {
            ValidatePage();
            await Page.Locator(selector).WaitForAsync();
            await Page.TypeAsync(selector, value, new PageTypeOptions { Delay = 50 });
            await Page.Keyboard.PressAsync("Tab", new KeyboardPressOptions { Delay = 20 });
        }

        public async Task PauseAsync() {
            ValidatePage();
            await Page.PauseAsync();
        }

        public async Task WaitForSelector(string selector) {
            ValidatePage();
            await Page.WaitForSelectorAsync(selector);
        }

        public async Task HandleUserPasswordScreen(string selector, string value, string desiredSelector)
        {
            var logger = _logger;

            ValidatePage();

            try
            {
                // Find the password box
                await Page.Locator(selector).WaitForAsync();

                // Fill in the password
                await Page.FillAsync(selector, value);

                // Submit password form
                await this.ClickAsync("input[type=\"submit\"]");

                PageWaitForSelectorOptions selectorOptions = new PageWaitForSelectorOptions
                {
                    Timeout = 10000
                };

                // For instances where there is a 'Stay signed in?' dialogue box
                try
                {
                    logger.LogDebug("Checking if asked to stay signed in.");

                    // Check if we received a 'Stay signed in?' box?
                    await Page.WaitForSelectorAsync("[id=\"KmsiCheckboxField\"]", selectorOptions);
                    logger.LogDebug("Was asked to 'stay signed in'.");

                    // Click to stay signed in
                    await Page.ClickAsync("[id=\"idBtn_Back\"]");
                }
                // If there is no 'Stay signed in?' box, an exception will throw; just catch and continue
                catch (Exception ssiException)
                {
                    logger.LogDebug("Exception encountered: " + ssiException.ToString());

                    // Keep record if passwordError was encountered
                    bool hasPasswordError = false;

                    try
                    {
                        selectorOptions.Timeout = 2000;

                        // Check if we received a password error
                        await Page.WaitForSelectorAsync("[id=\"passwordError\"]", selectorOptions);
                        hasPasswordError = true;
                    }
                    catch (Exception peException)
                    {
                        logger.LogDebug("Exception encountered: " + peException.ToString());
                    }

                    // If encountered password error, exit program
                    if (hasPasswordError)
                    {
                        logger.LogError("Incorrect password entered. Make sure you are using the correct credentials.");
                        throw new UserInputException(UserInputException.ErrorMapping.UserInputExceptionLoginCredential.ToString());
                    }
                    // If not, continue
                    else
                    {
                        logger.LogDebug("Did not encounter an invalid password error.");
                    }

                    logger.LogDebug("Was not asked to 'stay signed in'.");
                }

                await Page.WaitForSelectorAsync(desiredSelector);
            }
            catch (TimeoutException)
            {
                logger.LogError("Timed out during login attempt. In order to determine why, it may be beneficial to view the output recording. Make sure that your login credentials are correct.");
                throw new TimeoutException();
            }

            logger.LogDebug("Logged in successfully.");
        }
    }
}