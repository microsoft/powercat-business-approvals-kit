using System.Collections.Generic;

namespace Microsoft.PowerCAT.Localization;

/// <summary>
/// 
/// </summary>
public class ScanConfig {
    public List<string> IgnoreProperties {get;set;} = new List<string>();

    public List<string> IgnoreFunctions {get;set;} = new List<string>();
}