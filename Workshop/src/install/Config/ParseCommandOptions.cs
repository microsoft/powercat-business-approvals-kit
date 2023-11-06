using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.Json.Nodes;

namespace Microsoft.PowerPlatform.Config
{
    public class ParseCommandOptions {
        public Dictionary<ParseCommand,System.CommandLine.Option> Options = new Dictionary<ParseCommand, System.CommandLine.Option>();
        public ParseResult? Commands { get;set; }

        public T GetCommandValue<T>(ParseCommand commandOption, T defaultValue) {
            // Use any options from command line first
            if ( Options.ContainsKey(commandOption) ) {
                Option? match = null;
                if ( Options.TryGetValue(commandOption, out match) && Commands != null ) {
                    var result = Commands.GetValueForOption(match);
                    if ( result != null || ( result != null && !String.IsNullOrEmpty(result.ToString()) ) ) {
                        return (T)result;
                    }
                }
            }

            // Try any configured value
            var configFile = Path.Combine(Directory.GetCurrentDirectory(), "config.json");
            if ( File.Exists(configFile) )
            {
                var values = JsonValue.Parse(File.ReadAllText(configFile)) as JsonObject;
                if ( values != null && values.ContainsKey(commandOption.ToString()) ) {
                    T? value = default;
                    if ( values[commandOption.ToString()].AsValue().TryGetValue(out value) ) {
                        return value;
                    } 
                }
            }
           
            return defaultValue;
        }
    }
}