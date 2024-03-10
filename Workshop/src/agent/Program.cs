using System.Diagnostics;
using System.Reflection;
using System.Net;
using System.Text;
using System.Web.Http;

if ( args.Length > 0 && args[0] == "kill" ) {
    var match = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location));
    var currentProcess = Process.GetCurrentProcess();
    if ( match.Length > 1 ) {
        Console.WriteLine("Closing all");
    }
    
    foreach ( var item in match ) {
        if ( item.Id != currentProcess.Id) {
            item.Kill();
        }
    }
    currentProcess.Kill();
    return;
}

var exists = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location)).Count() > 1;
if ( exists ) {
    Console.WriteLine("Already running closing");
    Process.GetCurrentProcess().Kill();
    return;
}

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

CurrentJob? current = null;
CurrentJob? validate = null;
var data = new List<String>();
var json = new StringBuilder();

app.MapPost("/status", () => {
    SetupStatus status = new(current != null,String.Join('\r',data).Replace("\r","\r\n"), current?.Started);

    return Results.Json(status); 
});

app.MapPost("/stop", () => {
    if ( current != null ) {
        current.Job?.Kill();
    }
    if ( validate != null ) {
        validate.Job?.Kill();
    }
    data.Clear();
    json.Clear();
    current = null;
    validate = null;
});

app.MapPost("/validate", async (UserSetup info) => {
    if ( validate != null ) {
        validate.Job?.Kill();
    }

    string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    json.Clear();

    var job = new Process()
    {
        StartInfo = new ProcessStartInfo() { 
            FileName = "pwsh",
            Arguments = $"./validate.ps1 {info.User}", 
            WorkingDirectory = Path.Combine(assemblyFolder,"..","..", "..","..","scripts"),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        },
        EnableRaisingEvents = true
    };
    job.OutputDataReceived += ValidationResponseOutputHandler;
    job.Exited += (sender, args) => {
        Console.WriteLine("Exit");
        validate?.Log.Dispose();
        data.Clear();
        validate = null;
    };
    
    var startDate = DateTime.Now;
    validate = new (job, startDate, info.User, new StreamWriter(startDate.ToString("yyyy-MM-dd HH-mm-ss") + ".txt"));

    job?.Start();
    
   job?.BeginOutputReadLine();
});

app.MapPost("/result", (HttpRequest request, HttpResponse response) => {
    if ( validate != null ) {
        return Results.Text("{}", "application/json");
    }

    var jsonResult = json.ToString();
    if (!string.IsNullOrEmpty(jsonResult) && jsonResult != "{}")
    {
        return Results.Text(jsonResult, "application/json");
    }

    return Results.Text("{}", "application/json");
});

app.MapPost("/start", async (UserSetup info) => {
    if ( current != null ) {
        current.Job?.Kill();
    }

    string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    var job = new Process()
    {
        StartInfo = new ProcessStartInfo() { 
            FileName = "pwsh",
            Arguments = $"./setup.ps1 {info.User}", 
            WorkingDirectory = Path.Combine(assemblyFolder,"..","..", "..","..","scripts"),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        },
        EnableRaisingEvents = true
    };
    job.OutputDataReceived += ResponseOutputHandler;
    job.Exited += (sender, args) => {
        Console.WriteLine("Exit");
        current?.Log.Dispose();
        data.Clear();
        current = null;
    };
    
    var startDate = DateTime.Now;
    current = new (job, startDate, info.User, new StreamWriter(startDate.ToString("yyyy-MM-dd HH-mm-ss") + ".txt"));

    job?.Start();
    
   job?.BeginOutputReadLine();
});

void ResponseOutputHandler(object sender, DataReceivedEventArgs e)
{
    Console.WriteLine(e.Data);
    if ( ! String.IsNullOrEmpty(e.Data)) {
        data.Add(e.Data);
        current.Log.WriteLine(e.Data);
        current.Log.Flush();
        if ( data.Count > 50 ) {
            data.RemoveAt(0);
        }
    }
}

void ValidationResponseOutputHandler(object sender, DataReceivedEventArgs e)
{
    Console.WriteLine(e.Data);
    if ( ! String.IsNullOrEmpty(e.Data)) {
        if ( e.Data.IndexOf("{") >= 0 ) {
            json.Append(e.Data);
        }
        if ( json.Length > 0 ) {
            json.Append(e.Data);
        }
        data.Add(e.Data);
        validate.Log.WriteLine(e.Data);
        validate.Log.Flush();
        if ( data.Count > 50 ) {
            data.RemoveAt(0);
        }
    }
}

app.Run("http://localhost:8000");

record UserSetup (string User);

record SetupStatus (bool Running, string Info, DateTime? Started);

record CurrentJob (Process Job, DateTime Started, string User, StreamWriter Log);
