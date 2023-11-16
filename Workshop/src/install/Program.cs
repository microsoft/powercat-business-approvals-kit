using System.CommandLine;
using System.CommandLine.Parsing;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.PowerApps.TestEngine.TestInfra;
using Microsoft.PowerPlatform.Config;
using Microsoft.PowerPlatform.Demo;

/**
 This class contains the entry point for a command-line app that manages Power Platform demos. It defines commands for managing users, flows, and connectors. 
 The app uses Microsoft.Extensions.Logging and Microsoft.PowerApps.TestEngine.TestInfra libraries to wrap Playwright. It also depends on the Microsoft.PowerPlatform.Config and Microsoft.PowerPlatform.Demo namespaces. 
 
 The Main method creates a RootCommand object that defines the available commands and their options. It then parses the command-line arguments and invokes the corresponding command handler. The CreateUserCommands, CreateFlowCommands, and CreateConnectorCommands methods define the specific commands and their options. 
 
 Note: The class assumes that
 - The app can run in a headless environment and that a timeout of 2 minutes is used by default. 
 - The user has the necessary permissions and environment variables set up. 
*/
class Program
{
    static ParseCommandOptions? commands = null;

    static async Task Main(string[] args)
    {
        var rootCommand = new RootCommand("Power Platform Demo command-line app")
        {
            CreateUserCommands(),
            CreateFlowCommands(),
            CreateConnectionCommands()
        };

        var recordOption = new Option<string>("--record", () => "", "Y to record any output as video. If missing will not record");
        rootCommand.AddGlobalOption(recordOption);

        var headlessOption = new Option<string>("--headless", () => "", "Y to make headless, N if not. If missing will assume headless");
        rootCommand.AddGlobalOption(headlessOption);

        var timeoutOption = new Option<int?>("--timeout", () => 2 * 60 * 1000, "Timeout in minutes");
        rootCommand.AddGlobalOption(timeoutOption);

        commands = new ParseCommandOptions { Commands = rootCommand.Parse(args) };
        commands.Options.Add(ParseCommand.Record, recordOption);
        commands.Options.Add(ParseCommand.Headless, headlessOption);
        commands.Options.Add(ParseCommand.Timeout, timeoutOption);

        await rootCommand.InvokeAsync(args);
    }

    private static Command CreateUserCommands()
    {
        var userCommand = new Command("user", "Manage users");

        var nameOption = new Option<string>("--upn");
        var environmentOption = new Option<string>("--env");

        var userStartCommand = new Command("start", "Start browser session for user")
        {
            nameOption, environmentOption
        };
        userStartCommand.SetHandler(UserStart, nameOption, environmentOption);
        userCommand.Add(userStartCommand);

        var fileOption = new Option<string>("--file");
        var dataOption = new Option<string>("--data");
        var userScriptCommand = new Command("script", "Start browser session")
        {
            nameOption, environmentOption, fileOption, dataOption
        };
        userScriptCommand.SetHandler(UserScriptExecute, nameOption, environmentOption, fileOption, dataOption);
        userCommand.Add(userScriptCommand);

        var pageOption = new Option<string>("--page");
        var requestOption = new Option<string>("--request");
        var userAPICommand = new Command("api", "Capture the API response for an API request when visiting a page")
        {
            nameOption, pageOption, requestOption
        };
        userAPICommand.SetHandler(UserAPIQuery, nameOption, pageOption, requestOption);
        userCommand.Add(userAPICommand);

        return userCommand;
    }

    private static Command CreateFlowCommands()
    {
        var flowCommand = new Command("flow", "Manage flows");

        var nameOption = new Option<string>("--upn");
        var environmentOption = new Option<string>("--env");
        var flowUrl = new Option<string>("--url");

        var solutionOption = new Option<string>("--solution");
        var flowIdOption = new Option<string>("--id");

        var startCommand = new Command("start", "Start an existing cloud flow")
        {
            nameOption, flowUrl
        };
        startCommand.SetHandler(CloudFlowStart, nameOption, flowUrl);
        flowCommand.Add(startCommand);

        var activateCommand = new Command("activate", "Turn on an existing cloud flow")
        {
            nameOption, environmentOption, solutionOption, flowIdOption
        };
        activateCommand.SetHandler(CloudFlowActivate, nameOption, environmentOption, solutionOption, flowIdOption);
        flowCommand.Add(activateCommand);

        return flowCommand;
    }

    private static Command CreateConnectionCommands() {
        var connectionNameOption = new Option<string>("--upn");
        var environmentOption = new Option<string>("--env");
        var connectorOption = new Option<string>("--connector");
        var signInOption = new Option<bool>("--signIn");
        signInOption.SetDefaultValue(false);
        var connectorCommand = new Command("connection");
        var connectorCreateCommand = new Command("create", "Create new connection")
        {
            connectionNameOption, environmentOption, connectorOption, signInOption
        };
        connectorCreateCommand.SetHandler(ConnectionCreate, connectionNameOption, environmentOption, connectorOption, signInOption);
        connectorCommand.Add(connectorCreateCommand);

        return connectorCommand;
    }

    /// <summary>
    /// Start a Power Automate Flow
    /// </summary>
    /// <param name="user">The user to login as</param>
    /// <param name="cloudFlowUrl">The URL of the cloud flow to start run for in the Power Automate maker portal</param>
    private static void CloudFlowStart(string user, string cloudFlowUrl) {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });
        var logger = loggerFactory.CreateLogger<Program>();

        if ( commands == null ) {
            throw new NullReferenceException("commands");
        }

        var common = new Common(logger, commands);
        var playwright = common.Login(user).Result;
        playwright.GoToUrlAsync(cloudFlowUrl).Wait();

        playwright.ClickIfAppearAsync(5, 
            new PlaywrightCondition() { Label = "Get started", Message = "Closing Get started" },
            new PlaywrightCondition() { Selector = "button.ms-TeachingBubble-primaryButton" },
            new PlaywrightCondition() { Selector = "button[name='Run']", MaxClick = 1, Message = "Started flow" },
            new PlaywrightCondition() { Label = "Continue" },
            new PlaywrightCondition() { Label = "Run flow", Final = true }
        ).Wait();

        Thread.Sleep(1000);

        logger.LogInformation("End user flow start");
    }

    public static void UserScriptExecute(string user, string environment, string script, string data) {
         using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });
        var logger = loggerFactory.CreateLogger<Program>();

        if ( commands == null ) {
            throw new NullReferenceException("commands");
        }

        var common = new Common(logger, commands);
        var playwright = common.Login(user).Result;
        playwright.ExecuteScript(data, script).Wait();
    }

     /// <summary>
    /// Activate (Turn-on) a Power Automate Flow
    /// </summary>
    /// <param name="user">The user to login as</param>
    /// <param name="environmentId">The id if the environment</param>
    private static void CloudFlowActivate(string user, string environmentId, string solutionId, string flowId) {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });
        var logger = loggerFactory.CreateLogger<Program>();

        var ids = new List<string>();

        if ( File.Exists(flowId) ) {
            ids.AddRange(File.ReadAllLines(flowId));
        } else {
            ids.AddRange(flowId.Split(","));
        }

        if ( commands == null ) {
            throw new NullReferenceException("commands");
        }

        var common = new Common(logger, commands);
        var playwright = common.Login(user).Result;
            
        foreach ( var flow in ids.Where(id => !string.IsNullOrEmpty(id))) {
            logger.LogInformation($"Start flow {flow}");
            playwright.GoToUrlAsync($"https://make.powerapps.com/environments/{environmentId}/solutions/{solutionId}/objects/cloudflows/{flow}/view").Wait();

            playwright.ClickIfAppearAsync(5,
                new PlaywrightCondition() { Label = "Get started", Message = "Close Get started" },
                new PlaywrightCondition() { Selector = "button[name='Turn on']", ClickDelay = 3000, MaxClick = 1, Message = "Turning On"},
                new PlaywrightCondition() { IsVisible = true, Selector = ".ms-MessageBar--error", Final = true, Message = "Error" },
                new PlaywrightCondition() { IsVisible = true,  Selector = "button[name='Turn off']", Final = true, Message = "Turned On"}
            ).Wait();

            Thread.Sleep(1000);
        }

        logger.LogInformation("End user flow activate");
    }

    /// <summary>
    /// Start a playwright browser session for a user
    /// </summary>
    /// <param name="user">The user to start the session for</param>
    /// <param name="environment">The environment to open</param>
    private static void UserStart(string user, string environment) {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });
        var logger = loggerFactory.CreateLogger<Program>();

        if ( commands == null ) {
            throw new NullReferenceException("commands");
        }

        var common = new Common(logger, commands);
        var instance = common.Login(user, environment).Result;

        instance.PauseAsync().Wait();

        logger.LogInformation("End user start");
    }


    /// <summary>
    /// Login and visit a page in playwright capturing responses for any matching request
    /// </summary>
    /// <param name="user">The user to login as</param>
    /// <param name="webPage">The web page to visit</param>
    /// <param name="request">The matching URL request to capture</param>
    private static void UserAPIQuery(string user, string webPage, string request) {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });
        var logger = loggerFactory.CreateLogger<Program>();

        if ( commands == null ) {
            throw new NullReferenceException("commands");
        }

        var common = new Common(logger, commands);
        var instance = common.Login(user).Result;

        instance.RecordRequest(request);
        instance.GoToUrlAsync(webPage).Wait();

        var start = DateTime.Now;
        while ( instance.Responses.Count == 0 ) {
            if ( DateTime.Now.Subtract(start).TotalMinutes > 1 ) {
                break;
            }
            Thread.Sleep(1000);
        }

        Console.WriteLine("------------");
        Console.WriteLine(instance.Responses.Count > 0 ? instance.Responses.First() : "{}" );
        Console.WriteLine("============");

        logger.LogInformation("End user start");
    }

    /// <summary>
    /// Login to a Power Platform environment and create a connection
    /// </summary>
    /// <param name="user">The user to login as</param>
    /// <param name="env">The environment to login to</param>
    /// <param name="connector">The connector name to create connection for</param>
    /// <param name="waitForSignIn">Indicate if should wait for signin</param>
    private static void ConnectionCreate(string user, string env, string connector, bool waitForSignIn) {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });
        var logger = loggerFactory.CreateLogger<Program>();

        logger.LogInformation("Connection create start");

        if ( commands == null ) {
            throw new NullReferenceException("commands");
        }

        var common = new Common(logger, commands);
        common.CreateConnection(user,env,connector, waitForSignIn).Wait();
        logger.LogInformation("End connection create");
    }
}