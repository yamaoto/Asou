using System.Diagnostics;
using System.Dynamic;
using System.Net;
using Asou.Abstractions.Events;
using Asou.Abstractions.Process.Instance;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Xunit.Abstractions;

namespace WebApplication1.FunctionalTests;

public class SampleProcessTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _testOutputHelper;

    public SampleProcessTest(
        WebApplicationFactory<Program> factory,
        ITestOutputHelper testOutputHelper)
    {
        _factory = factory;
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task TestExecution()
    {
        // Arrange
        var stopWatch = Stopwatch.StartNew();
        _testOutputHelper.WriteLine($"[{stopWatch.ElapsedMilliseconds:0000}ms] Arrange start");
        var stepLength = Stopwatch.GetTimestamp();

        var client = _factory.CreateClient();

        // Enforce EF Core initialization with simple request
        using var scope = _factory.Services.CreateScope();
        var processInstanceRepository = scope.ServiceProvider.GetRequiredService<IProcessInstanceRepository>();
        await processInstanceRepository.GetAllInstancesAsync();

        _testOutputHelper.WriteLine(
            $"[{stopWatch.ElapsedMilliseconds:0000}ms] Arrange end. Elapsed {Stopwatch.GetElapsedTime(stepLength).Milliseconds:0000}ms");

        // Act
        _testOutputHelper.WriteLine($"[{stopWatch.ElapsedMilliseconds:0000}ms] Act start");
        stepLength = Stopwatch.GetTimestamp();

        var responseTask = client.PostAsync("/SampleProcess", null);
        await Task.WhenAll(responseTask, EmitEvent());
        var response = responseTask.Result;
        _testOutputHelper.WriteLine(
            $"[{stopWatch.ElapsedMilliseconds:0000}ms] Act end. Elapsed {Stopwatch.GetElapsedTime(stepLength).Milliseconds:0000}ms");
        stepLength = Stopwatch.GetTimestamp();

        // Assert
        _testOutputHelper.WriteLine($"[{stopWatch.ElapsedMilliseconds:0000}ms] Assert start");
        stepLength = Stopwatch.GetTimestamp();


        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        dynamic data = JsonConvert.DeserializeObject(json, typeof(ExpandoObject));
        Assert.Null(data.Parameter1);
        Assert.Equal("Hello World", data.Parameter2 as string);
        _testOutputHelper.WriteLine(
            $"[{stopWatch.ElapsedMilliseconds:0000}ms] Assert end. Elapsed {Stopwatch.GetElapsedTime(stepLength).Milliseconds:0000}ms");
    }

    private async Task EmitEvent()
    {
        using var scope = _factory.Services.CreateScope();
        var eventDriver = scope.ServiceProvider.GetRequiredService<IEventDriver>();
        await Task.Delay(500);
        await eventDriver.PublishAsync(
            new EventRepresentation("test", "test", "MyEventType", "MyEventSubject", DateTime.UtcNow, ""));
    }
}
