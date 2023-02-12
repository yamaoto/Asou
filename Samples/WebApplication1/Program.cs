using System.Diagnostics;
using System.Text;
using Asou.Abstractions;
using Asou.Core;
using Asou.EfCore;
using Asou.GraphEngine.CodeContractStorage;
using Microsoft.EntityFrameworkCore;
using WebApplication1;
using WebApplication1.Data;
using WebApplication1.SampleProcess;


var builder = WebApplication.CreateBuilder(args);

// Add EF Core database
builder.Services.AddDbContext<DataContext>(c => { c.UseInMemoryDatabase("InMemory"); });

// Add ASOU engine
builder.Services.RegisterAsouGraphEngine();

// Use EF Core persistence provider
builder.Services.RegisterAsouEfCorePersistence<DataContext>();

// TODO: Move registration to ASOU
builder.Services.AddHostedService<BackgroundServices>();

// Register local execution steps
builder.Services.AddTransient<DoSimpleStep>();
builder.Services.AddTransient<AsynchronousResumeStep>();
builder.Services.AddTransient<ConditionalStep>();
builder.Services.AddTransient<EndStep>();

var app = builder.Build();

// Describe execution flow
var processId = new Guid("c380b7f4-2a76-44fc-9d5d-ecc7c105969b");
var versionId = new Guid("2a8038b9-921e-4aaa-a72d-f85d6ff512e8");
var flowBuilder = GraphProcessContract
    .Create(processId, versionId, 1, "SampleProcess")
    .StartFrom<DoSimpleStep>()
    .WithParameter<DoSimpleStep, string>("Parameter1",
        instance => (string)instance.ProcessRuntime.Parameters["Parameter1"]!,
        (instance, value) => instance.ProcessRuntime.Parameters["Parameter1"] = value)
    .WithParameter<DoSimpleStep, string>("Parameter2",
        setter: (instance, value) => instance.ProcessRuntime.Parameters["Parameter2"] = value)
    .Then<AsynchronousResumeStep>()
    .Then<ConditionalStep>();

flowBuilder.Conditional<ConditionalStep, EndStep>("ToExit");
flowBuilder.Conditional<ConditionalStep, DoSimpleStep>("TryAgain");

var processRegistration = app.Services.GetRequiredService<IGraphProcessRegistration>();

var sw = Stopwatch.StartNew();
processRegistration.RegisterFlow(flowBuilder);
sw.Stop();
Console.WriteLine("Delegates creation: {0}ms", sw.ElapsedMilliseconds);


using var scope = app.Services.CreateScope();
var subsRepository = scope.ServiceProvider.GetRequiredService<ISubscriptionPersistantRepository>();
sw.Start();
// Fetch data to prepare EF Core InMemory database initialization to prevent performance measurements impact
await subsRepository.GetActiveSubscriptionsAsync();
Console.WriteLine("Fetching subscriptions: {0}ms", sw.ElapsedMilliseconds);


app.MapGet("/", () => Results.Content("<html><body><form method=\"POST\"><button>Execute</button></form></body></html>",
    "text/html",
    Encoding.UTF8));

app.MapPost("/", async (ProcessExecutionEngine processExecutionEngine, CancellationToken cancellationToken) =>
{
    var timer = Stopwatch.StartNew();
    var result = await processExecutionEngine.ExecuteAsync(processId,
        new ProcessParameters { { "Parameter1", "Hello World" }, { "Parameter2", "" } }, cancellationToken);
    timer.Stop();
    result["ElapsedMilliseconds"] = timer.ElapsedMilliseconds;
    return result;
});

app.Run();

public sealed partial class Program
{
}
