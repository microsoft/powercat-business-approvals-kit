using System.Diagnostics;
using System.Reflection;

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
var data = new List<String>();

app.MapPost("/status", () => {
    SetupStatus status = new(current != null,String.Join('\r',data).Replace("\r","\r\n"), current?.Started);

    return Results.Json(status); 
});

app.MapPost("/stop", () => {
    if ( current != null ) {
        current.Job.Kill();
    }
    data.Clear();
    current = null;
});

app.MapPost("/start", async (UserSetup info) => {
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

app.Run("http://localhost:8000");

record UserSetup (string User);

record SetupStatus (bool Running, string Info, DateTime? Started);

record CurrentJob (Process Job, DateTime Started, string User, StreamWriter Log);
