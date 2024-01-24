using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using YamlDotNet;
using YamlDotNet.Serialization;
using Microsoft.PowerCAT.Localization.Yaml;
using Microsoft.PowerCAT.PowerFx;
using Microsoft.PowerFx;
using Microsoft.PowerFx.Syntax;
using YamlDotNet.Serialization.NodeTypeResolvers;

namespace Microsoft.PowerCAT.Localization;

/// <summary>
/// 
/// </summary>
public class Scanner {
    public IList<ScanResult> Scan(Stream content, Stream config = null) {
        var matches =  new List<ScanResult>();

        ScanConfig scanConfig = new ScanConfig();
        if ( config != null ) {
            using ( var read = new StreamReader(config) ) {
                var configYaml = read.ReadToEnd();
                var deserializer = new DeserializerBuilder()
                    .Build();
                scanConfig = deserializer.Deserialize<ScanConfig>(configYaml);
            }
        }

        using ( var read = new StreamReader(content) ) {
            var yaml = read.ReadToEnd();
             if ( string.IsNullOrEmpty(yaml)) {
                return matches;
             }

            var deserializer = new DeserializerBuilder()
                .WithNodeTypeResolver(
                    new ExpandoNodeTypeResolver(), 
                    ls => ls.InsteadOf<DefaultContainersNodeTypeResolver>())
                .Build();

            dynamic result = null;
            try {
                result = deserializer.Deserialize(yaml);
            } catch (Exception ex){
                Console.WriteLine(yaml);
                throw ex;
            }

            var toProcess = new Queue<KeyValuePair<string,System.Collections.Generic.IDictionary<string,object>>>();

            List<string> path = new List<string>();

            if ( result is ExpandoObject ) {
                var item = result as System.Collections.Generic.IDictionary<string,object>;
                
                toProcess.Enqueue(new KeyValuePair<string,System.Collections.Generic.IDictionary<string,object>>(item.Keys.First(),item));
            }

            var fxConfig = new PowerFxConfig();
            fxConfig.EnableSetFunction();

            var engine = new RecalcEngine(fxConfig);
            var options = new ParserOptions();
            options.AllowsSideEffects = true;
            
            while ( toProcess.Count > 0 ) {
                var item = toProcess.Dequeue();

                foreach ( var key in item.Value.Keys ) {
                    var value = item.Value[key];
                    var valueKey = item.Key + "." + key;
                    if ( value is ExpandoObject ) {
                        toProcess.Enqueue(new KeyValuePair<string,System.Collections.Generic.IDictionary<string,object>>(valueKey, value as ExpandoObject));
                    }
                    if ( value is string ) {
                        var val = value as string;
                        if ( val.Trim().StartsWith("=")) {
                            val = val.Trim().Substring(1);
                        }
                        var parseResult = engine.Parse(val, options);

                        var containsLiteralText = ContainsStringLiteral(parseResult.Root, scanConfig);

                        if ( 
                            scanConfig.Settings.IgnoreTextIfNotVisible 
                            && containsLiteralText.Count > 0 
                            && item.Value.ContainsKey("Visible") 
                            && item.Value["Visible"].Equals("=false")) {
                                continue;
                        }
                        
                        if ( containsLiteralText.Count > 0 && !scanConfig.IgnoreProperties.Contains(key) ) {
                            var scanResult = new ScanResult();
                            scanResult.Control = item.Key;
                            scanResult.Property = key;

                            
                            scanResult.Text.AddRange(containsLiteralText);
                            matches.Add(scanResult);
                        }
                    }
                }
            }   
        }

        return matches;
    }

    private List<string> ContainsStringLiteral(TexlNode node, ScanConfig config) {
        var visitor = new StringLiteralVisitor(config);

        node.Accept(visitor);

        return visitor.Match;
    }
}