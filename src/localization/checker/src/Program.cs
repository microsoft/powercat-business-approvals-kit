using System.CommandLine;
using Microsoft.PowerCAT.Localization;
using System.IO;

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

        if ( results.Count == 0 ) {
            Console.WriteLine("No string literals found");
            return;
        }

        if ( results.Count > 0 ) {
            Console.WriteLine($"Found {results.Count} string literals");

            foreach ( var match in results ) {
                var control = match.Control.Split('.');
                Console.WriteLine($"> {control[control.Length-1]}.{match.Property}");
                Console.WriteLine("  " + String.Join(',', match.Text));
            }
        }
    },
    fileOption, configOption);

return await rootCommand.InvokeAsync(args);