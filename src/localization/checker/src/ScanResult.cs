using System.Collections.Generic;

namespace Microsoft.PowerCAT.Localization {
    public class ScanResult {
        public string Control {get;set;}
        public string Property { get;set;}
        public List<string> Text { get; private set; } = new List<string>();
    }
}