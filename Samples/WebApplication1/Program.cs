using System.Diagnostics;
using System.Text;
using Asou.Abstractions;
using Asou.Core;
using Asou.GraphEngine;
using Asou.GraphEngine.CodeContractStorage;
using WebApplication1;
using WebApplication1.SampleProcess;

var builder = WebApplication.CreateBuilder(args);
builder.Services.RegisterAsouGraphEngine();
builder.Services.AddSingleton<IProcessInstanceRepository, ProcessInstanceRepository>();
builder.Services.AddSingleton<IExecutionPersistence, ExecutionPersistence>();

builder.Services.AddTransient<DoSimpleStep>();
builder.Services.AddTransient<AwaiterStep>();
builder.Services.AddTransient<ConditionalStep>();
builder.Services.AddTransient<EndStep>();

var app = builder.Build();

var flowBuilder = GraphProcessContract
    .Create(new Guid("c380b7f4-2a76-44fc-9d5d-ecc7c105969b"),
        new Guid("2a8038b9-921e-4aaa-a72d-f85d6ff512e8"), 1, "SampleProcess")
    .StartFrom<DoSimpleStep>()
    .WithParameter<DoSimpleStep, string>("Parameter1",
        instance => (string)instance.ProcessRuntime.Parameters["Parameter1"]!,
        (instance, value) => instance.ProcessRuntime.Parameters["Parameter1"] = value)
    .WithParameter<DoSimpleStep, string>("Parameter2",
        setter: (instance, value) => instance.ProcessRuntime.Parameters["Parameter2"] = value)
    .Then<AwaiterStep>()
    .Then<ConditionalStep>();

flowBuilder.Conditional<ConditionalStep, EndStep>("ToExit");
flowBuilder.Conditional<ConditionalStep, DoSimpleStep>("TryAgain");

var processRegistration = app.Services.GetRequiredService<IGraphProcessRegistration>();
processRegistration.RegisterFlow(flowBuilder);

app.MapGet("/", () => Results.Content("<html><body><form method=\"POST\"><button>Execute</button></form></body></html>",
    "text/html",
    Encoding.UTF8));

app.MapPost("/", async (ProcessExecutionEngine processExecutionEngine, CancellationToken cancellationToken) =>
{
    var timer = Stopwatch.StartNew();
    var result = await processExecutionEngine.ExecuteAsync(new Guid("c380b7f4-2a76-44fc-9d5d-ecc7c105969b"),
        new ProcessParameters { { "Parameter1", "Hello World" }, { "Parameter2", "" } }, cancellationToken);
    timer.Stop();
    result["ElapsedMilliseconds"] = timer.ElapsedMilliseconds;
    return result;
});

app.Run();