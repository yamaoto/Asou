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
        var testElement = new TestElement();
        var parameterDelegateFactory = new ParameterDelegateFactory();
        parameterDelegateFactory.CreateDelegates<string>(testElement, "ParameterA");
        var parameterBinder = new ParameterBinder(parameterDelegateFactory);
        var processMachine = new ProcessMachine(parameterBinder, nameof(FormatTest))
        {
            ComponentFactory = (name, objectName) => testElement
        };
        processMachine.CreateComponent("testElement", "testElement");
        var stopWatch = Stopwatch.StartNew();

        // Act
        var result = processMachine.GetElementParameter<string>("testElement", "ParameterA");

        stopWatch.Stop();
        _testOutputHelper.WriteLine("Act elapsed {0} ms", stopWatch.ElapsedMilliseconds);

        // Assert
        Assert.Equal("HELLO", result);
    }

    [Fact]
    public void SetPropertyValue()
    {
        // Arrange
        var testElement = new TestElement();
        var parameterDelegateFactory = new ParameterDelegateFactory();
        parameterDelegateFactory.CreateDelegates<string>(testElement, "ParameterA");
        var parameterBinder = new ParameterBinder(parameterDelegateFactory);
        var processMachine = new ProcessMachine(parameterBinder, nameof(FormatTest))
        {
            ComponentFactory = (name, objectName) => testElement
        };
        processMachine.CreateComponent("testElement", "testElement");
        var stopWatch = Stopwatch.StartNew();

        // Act
        processMachine.SetElementParameter("testElement", "ParameterA", "WORLD");

        stopWatch.Stop();
        _testOutputHelper.WriteLine("Act elapsed {0} ms", stopWatch.ElapsedMilliseconds);

        // Assert
        Assert.Equal("WORLD", testElement.ParameterA);
    }

    private class TestElement : BaseElement
    {
        public string ParameterA { get; } = "HELLO";

        public override string ClassName { get; init; } = nameof(TestElement);

        public override Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}