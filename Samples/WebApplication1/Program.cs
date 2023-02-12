using System.Diagnostics;
using System.Text;
using Asou.Abstractions.Process;
using Asou.Core;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.SampleProcess;

var builder = WebApplication.CreateBuilder(args);

// Add Asou services
builder.Services.AddAsou<DataContext>();

// Add EF Core database
builder.Services.AddDbContext<DataContext>(c => { c.UseInMemoryDatabase("InMemory"); });

// Register sample process and execution steps types
builder.Services.AddAsouProcess<SampleProcessDefinition>();
builder.Services.AddTransient<DoSimpleStep>();
builder.Services.AddTransient<AsynchronousResumeStep>();
builder.Services.AddTransient<ConditionalStep>();
builder.Services.AddTransient<EndStep>();

var app = builder.Build();

app.MapGet("/", () => Results.Content("<html><body><form method=\"POST\"><button>Execute</button></form></body></html>",
    "text/html",
    Encoding.UTF8));

app.MapPost("/", async (ProcessExecutionEngine processExecutionEngine, CancellationToken cancellationToken) =>
{
    var timer = Stopwatch.StartNew();
    var result = await processExecutionEngine.ExecuteAsync(new Guid(SampleProcessDefinition.ProcessId),
        new ProcessParameters { { "Parameter1", "Hello World" }, { "Parameter2", "" } }, cancellationToken);
    timer.Stop();
    result["ElapsedMilliseconds"] = timer.ElapsedMilliseconds;
    return result;
});

app.Run();

namespace WebApplication1
{
    public sealed class Program
    {
    }
}
