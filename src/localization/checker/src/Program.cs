using System.CommandLine;
using Microsoft.PowerCAT.Localization;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using IHost host = Host.CreateApplicationBuilder(args).Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();

var rootCommand = new RootCommand("Localization checker");

var fileOption = new Option<string>(
            name: "--file",
            description: "The file to read");

var configOption = new Option<string>(
            name: "--config",
            description: "The config yaml file to read");

var scanCommand = new Command("scan", "Scan a file")
    {
        fileOption,
        configOption,
    };
rootCommand.AddCommand(scanCommand);

scanCommand.SetHandler((file, config) => 
    { 
        var scanner = new Scanner();

        var configStream = System.IO.File.Exists(config)? File.Open(config, FileMode.Open): null;
        var results = scanner.Scan(File.Open(file, FileMode.Open), configStream);

        var analytics = new Analytics(logger);

        if ( ! Directory.Exists("data") ) {
            Directory.CreateDirectory("data");
        }

        analytics.Generate(results, new FileStream($"data\\results-{DateTime.Now.ToString("yyyy-MM-dd hhmm")}.csv",FileMode.OpenOrCreate,FileAccess.Write));

    },
    fileOption, configOption);

return await rootCommand.InvokeAsync(args);