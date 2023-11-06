namespace Microsoft.PowerApps.TestEngine.TestInfra
{
    public class PlaywrightCondition {
        public string Selector { get;set;} = "";

        public string Label { get;set;} = "";

        public bool Final {get;set; }
        public bool Pause { get; set; }

        public int Click {get;set;}
        public int MaxClick {get;set;}

        public bool IsVisible {get;set;}
        
        public int ClickDelay { get; internal set; }
        public string? Message { get; internal set; }
    }
}