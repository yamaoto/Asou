#pragma warning disable CA1852
using System.Diagnostics;
using System.Text.Json.Serialization;
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
builder.Services.AddDbContext<DataContext>(c => { c.UseInMemoryDatabase("InMemory"); });

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

app.MapFallbackToFile("index.html");

app.MapPost("/SampleProcess",
    async (ProcessExecutionEngine processExecutionEngine, CancellationToken cancellationToken) =>
    {
        var timer = Stopwatch.StartNew();
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

app.Run();

namespace WebApplication1
{
    public sealed class Program
    {
    }
}
