using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Microsoft.PowerCAT.Localization;

/// <summary>
/// 
/// </summary>
public class Analytics {
    public ILogger _logger;
    public Analytics(ILogger logger) {
        _logger = logger;
    }

    public void Generate(IList<ScanResult> results, Stream output) {
        if ( results.Count == 0 ) {
            _logger.LogWarning("No string literals found");
        }

        var csv = new StringBuilder();
        csv.AppendLine($"when;control;parent;property;text");

        if ( results.Count > 0 ) {
            _logger.LogInformation($"Found {results.Count} properties with string literals");

            SortedDictionary<string,int> usage = new SortedDictionary<string,int>();

            foreach ( var match in results ) {
                foreach ( var item in match.Text ) {
                    if ( !usage.ContainsKey(item) ) {
                        usage.Add(item,1);
                    } else {
                        usage[item]++;
                    }
                }
            }

            _logger.LogInformation($"Found unique {usage.Count} strings");

            var date = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

            foreach ( var match in results ) {
                foreach ( var text in match.Text ) {
                    var control = match.Control.Replace("'","\"");
                    var parts = SplitQuotedStringByDelimiter('.',control);
                    var parent = parts.Count > 1 ? parts[parts.Count -1] : string.Empty;
                    csv.AppendLine($"{date};{control};{parent};{match.Property};\"{text}\"");
                }
            }  
        }

        var writer = new StreamWriter(output);
        writer.Write(csv.ToString());

        // Flush not dispose as will also close base stream
        writer.Flush();
    }

    List<string> SplitQuotedStringByDelimiter(char delimiter, string text) {
        List<string> parts = new List<string>();
        if ( string.IsNullOrEmpty(text)) {
            return parts;
        }

        int start = 0;
        bool inQuotes = false;
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '\"')
            {
                inQuotes = !inQuotes;
            }
            else if (text[i] == '.' && !inQuotes)
            {
                parts.Add(text.Substring(start, i - start).Trim());
                start = i + 1;
            }
        }
        if (start < text.Length)
        {
            parts.Add(text.Substring(start).Trim());
        }
        return parts;
    }
}