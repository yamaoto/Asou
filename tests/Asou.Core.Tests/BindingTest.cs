using System.Diagnostics;
using Asou.Abstractions.ExecutionElements;
using Asou.Core.Process;
using Asou.Core.Process.Binding;
using Xunit.Abstractions;

namespace Asou.Core.Tests;

public class BindingTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public BindingTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void GetPropertyValue()
    {
        // Arrange
        var testElement = new BindingTestElement();
        var parameterDelegateFactory = new ParameterDelegateFactory();
        parameterDelegateFactory.CreateDelegates<BindingTestElement, string>("ParameterA");
        var processMachine = new ProcessRuntime(parameterDelegateFactory, nameof(BindingTest))
        {
            ComponentFactory = _ => testElement
        };
        processMachine.CreateComponent(new Guid("266090b8-e7d4-4a2f-8b3b-ef622f137ab4"), typeof(BindingTestElement));
        var stopWatch = Stopwatch.StartNew();

        // Act
        var result = processMachine.GetElementParameter(new Guid("266090b8-e7d4-4a2f-8b3b-ef622f137ab4"), "ParameterA");

        stopWatch.Stop();
        _testOutputHelper.WriteLine("Act elapsed {0} ms", stopWatch.ElapsedMilliseconds);

        // Assert
        Assert.Equal("HELLO", result);
    }

    [Fact]
    public void SetPropertyValue()
    {
        // Arrange
        var testElement = new BindingTestElement();
        var parameterDelegateFactory = new ParameterDelegateFactory();
        parameterDelegateFactory.CreateDelegates<BindingTestElement, string>("ParameterA");
        var processMachine = new ProcessRuntime(parameterDelegateFactory, nameof(BindingTest))
        {
            ComponentFactory = _ => testElement
        };
        processMachine.CreateComponent(new Guid("266090b8-e7d4-4a2f-8b3b-ef622f137ab4"),
            typeof(BindingTestElement));
        var stopWatch = Stopwatch.StartNew();

        // Act
        processMachine.SetElementParameter(new Guid("266090b8-e7d4-4a2f-8b3b-ef622f137ab4"), "ParameterA", "WORLD");

        stopWatch.Stop();
        _testOutputHelper.WriteLine("Act elapsed {0} ms", stopWatch.ElapsedMilliseconds);

        // Assert
        Assert.Equal("WORLD", testElement.ParameterA);
    }

    public sealed class BindingTestElement : BaseElement
    {
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public string ParameterA { get; set; } = "HELLO";

        public override Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
