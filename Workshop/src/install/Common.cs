
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.PowerApps.TestEngine.Config;
using Microsoft.PowerApps.TestEngine.TestInfra;
using Microsoft.PowerPlatform.Config;
using NeoSmart.SecureStore;

namespace Microsoft.PowerPlatform.Demo
{
    public class Common {
        private readonly ILogger _logger;
        private ParseCommandOptions _commands;

        private PlaywrightTestInfraFunctions _playwright = null;

        private const string EmailSelector = "input[type=\"email\"]";
        private const string PasswordSelector = "input[type=\"password\"]";
        private const string SubmitButtonSelector = "input[type=\"submit\"]";
        private const string KeepMeSignedInNoSelector = "[id=\"idBtn_Back\"]";

        private const string PowerAppsTitle = "span:text('Power Apps')";

        public Func<string, string?> GetSecureValue = (string name) => {
            // Presuming a secrets file has already been created with the CLI utility:
            string storeFile = File.Exists("secure/secrets.json") ? "secure/secrets.json" : "secrets.json";
            string secretsKey = File.Exists("secure/secret.key") ? "secure/secret.key" : "secret.key";

            using var sman = SecretsManager.LoadStore(storeFile);
            sman.LoadKeyFromFile(secretsKey);

            var secretValue = sman.Get(name);

            return secretValue;
        };

        public Common(ILogger logger, ParseCommandOptions commands)  {
            _logger = logger;
            _commands = commands;
        }
        
        public Func<string,string,string> CommandRunner = (command, args) => RunCommand(command, args);

        /// <summary>
        /// Create a Power Platform connection via the maker portal using Playwright browser automation
        /// </summary>
        /// <param name="user">The user to login as</param>cd
        /// <param name="environment">The environment id to login to</param>
        /// <param name="connector">The connector to create</param>
        /// <returns></returns>        
        public async Task CreateConnection(string user, string environment, string connector, bool waitForSignIn) {
            await Login(user);
            await _playwright.GoToUrlAsync($"https://make.powerapps.com/environments/{environment}/connections/available?apiName={connector}");
            await _playwright.ClickAsync("button.add");
            if ( waitForSignIn ) {
                await _playwright.ClickIfAppearAsync(10000, new PlaywrightCondition { Selector = $".table[data-test-id='{user.ToLower()}']", Final = true, MaxClick = 1 } );
            }
            await _playwright.PauseAsync();
        }

        public async Task GetEnvironments() {
        }

        /// <summary>
        /// Login to an environment assuming password is in environment variable DEMO_PASSWORD
        /// </summary>
        /// <param name="user">The user to login as</param>
        /// <param name="environment">The environment id to login to</param>
        /// <returns></returns>
        /// <summary>
        public async Task<PlaywrightTestInfraFunctions> Login(string user, string environment = "") {
            if ( _playwright == null ) {
                var config = new BrowserConfiguration() { Browser = "Chromium", Commands = _commands };
                var playwright = new PlaywrightTestInfraFunctions(config, _logger, null);
                await playwright.SetupAsync();

                var desiredUrl = string.IsNullOrEmpty(environment) ? "https://make.powerapps.com" : $"https://make.powerapps.com/environments/{environment}";
                if ( string.IsNullOrEmpty(environment) && environment.StartsWith("https://") ) {
                    desiredUrl = environment;
                }

                await playwright.GoToUrlAsync(desiredUrl);
                await playwright.HandleUserEmailScreen(EmailSelector, user);

                await playwright.ClickAsync(SubmitButtonSelector);

                // Wait for the sliding animation to finish
                await Task.Delay(1000);

                var passwordKey = _commands.GetCommandValue(ParseCommand.Admin, String.Empty) == "Y" ?
                    "ADMIN_PASSWORD" : "DEMO_PASSWORD";
                var password = GetSecureValue(passwordKey);

                await playwright.HandleUserPasswordScreen(PasswordSelector, password, PowerAppsTitle);
                _playwright = playwright;
            }

            return _playwright;
            
            // Adapted from https://github.com/microsoft/PowerApps-TestEngine/blob/480a9a3b3cc237470e45310f6cfd348db8448e21/src/Microsoft.PowerApps.TestEngine/TestInfra/PlaywrightTestInfraFunctions.cs
        }

        /// <summary>
        /// Run an operating system command and wait until it exit returning the text output by the command
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <param name="args">The command line arguments</param>
        /// <returns>The standard output of the command</returns>
        public static string RunCommand(string command, string args) {
            var startInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = command,
                Arguments = args
            };

            StringBuilder output = new StringBuilder();
            using (Process osProcess = Process.Start(startInfo))
            {
                osProcess.OutputDataReceived += (object sender, DataReceivedEventArgs e) => output.Append(e.Data);
                osProcess.WaitForExit();
                return output.ToString();
            }
        }
    }
}

