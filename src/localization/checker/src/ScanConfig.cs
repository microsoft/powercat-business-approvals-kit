using System.Collections.Generic;

namespace Microsoft.PowerCAT.Localization;

/// <summary>
/// 
/// </summary>
public class ScanConfig {
    public ScanConfigSettings Settings {get;set;} = new ScanConfigSettings();

    public List<string> IgnoreProperties {get;set;} = new List<string>();

    public List<string> IgnoreVariableSetup {get;set;} = new List<string>();

    public List<string> IgnoreValues {get;set;} = new List<string>();

    public List<string> IgnoreFunctions {get;set;} = new List<string>();
}