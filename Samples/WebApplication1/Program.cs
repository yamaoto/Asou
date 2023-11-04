#pragma warning disable CA1852
using System.Diagnostics;
using System.Text.Json.Serialization;
using Asou.Abstractions.Events;
using Asou.Abstractions.Process.Execution;
using Asou.Abstractions.Process.Instance;
using Asou.Core;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.SampleProcess;

var builder = WebApplication.CreateBuilder(args);

// Add Asou services
builder.Services.AddAsou<DataContext>();

// Add EF Core database
var useSqlite = string.Equals(Environment.GetEnvironmentVariable("APP_USE_SQLITE"), "true",
    StringComparison.InvariantCultureIgnoreCase);
builder.Services.AddDbContext<DataContext>(c =>
{
    if (useSqlite)
    {
        c.UseSqlite("Data Source=asou.db");
    }
    else
    {
        c.UseInMemoryDatabase("InMemory");
    }
});

// Register sample process and execution steps types
builder.Services.AddAsouProcess<SampleProcessDefinition>();
builder.Services.AddTransient<DoSimpleStep>();
builder.Services.AddTransient<AsynchronousResumeStep>();
builder.Services.AddTransient<ConditionalStep>();
builder.Services.AddTransient<EndStep>();

// Serialize enums as string
builder.Services.Configure<JsonOptions>(opt =>
{
    opt.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

if (useSqlite)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
    dbContext.Database.EnsureCreated();
}

app.MapFallbackToFile("index.html");

app.MapPost("/SampleProcess",
    async (ProcessExecutionEngine processExecutionEngine, CancellationToken cancellationToken) =>
    {
        var timer = Stopwatch.StartNew();
        // Execute SampleProcess with parameters Parameter1 = "Hello World" and Parameter2 = ""
        var result = await processExecutionEngine.CreateAndExecuteAsync(new Guid(SampleProcessDefinition.ProcessId),
            new ProcessParameters { { "Parameter1", "Hello World" }, { "Parameter2", "" } },
            new ExecutionOptions(false), cancellationToken);
        timer.Stop();
        result!["ElapsedMilliseconds"] = timer.ElapsedMilliseconds;
        return result;
    });

app.MapGet("/data",
    async (IProcessInstanceRepository processInstanceRepository,
        IProcessExecutionLogRepository processExecutionLogRepository,
        IProcessExecutionPersistenceRepository processExecutionPersistenceRepository) =>
    {
        var instances = await processInstanceRepository.GetAllInstancesAsync();
        var result = new List<object>();
        foreach (var instance in instances)
        {
            var logs = await processExecutionLogRepository.GetLogForProcessInstanceAsync(instance.Id);
            var parameters = await processExecutionPersistenceRepository.GetProcessParametersAsync(instance.Id);
            result.Add(new { Instance = instance, Parameters = new ProcessParameters(parameters), Logs = logs });
        }

        return result;
    });
app.MapPost("/EmitEvent",
    async (HttpRequest req, IEventBus eventDriver, CancellationToken cancellationToken) =>
    {
        if (!req.HasFormContentType)
        {
            return Results.BadRequest(new { Results = "BadRequest" });
        }

        var form = await req.ReadFormAsync(cancellationToken);
        var eventRepresentation = new EventRepresentation(
            Guid.NewGuid().ToString(),
            "HTTP",
            form["EventType"]!,
            form["EventSubject"]!,
            DateTime.UtcNow, null);
        await eventDriver.SendAddressedAsync(eventDriver.CurrentNode, eventRepresentation, cancellationToken);
        return Results.Ok(new { Results = "OK" });
    });
app.Run();

namespace WebApplication1
{
    public sealed class Program
    {
    }
}
