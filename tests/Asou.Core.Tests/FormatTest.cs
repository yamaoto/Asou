using System.Diagnostics;
using Asou.Abstractions;
using Asou.Abstractions.ExecutionElements;
using Asou.ByteCodeEngine;
using Asou.Core.Process;
using Asou.Core.Process.Binding;
using Xunit.Abstractions;

namespace Asou.Core.Tests;

public class FormatTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public FormatTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task SimpleSequenceProcess()
    {
        // Arrange
        var stream = new MemoryStream();
        var builder = new ByteCodeWriter(stream);

        builder.DeclareScript("prepare", b =>
        {
            b.CreateComponent("start", "start"); // 13
            b.CreateComponent("doWork1", "doWork1"); // 17
            b.CreateComponent("doWork2", "doWork2"); // 17
            b.CreateComponent("end", "end"); // 9
            b.LetParameter("a"); // 3
            b.SetParameter("a", AsouTypes.String, bb => bb.Write("hello")); // 10
            // 69
        });

        builder.DeclareScript("start", b =>
        {
            b.ExecuteElement("start");
            b.CallScript("doWork1");
        });

        builder.DeclareScript("doWork1", b =>
        {
            b.ExecuteElement("doWork1");
            b.CallScript("doWork2");
        });

        builder.DeclareScript("doWork2", b =>
        {
            b.ExecuteElement("doWork2");
            b.CallScript("end");
        });

        builder.DeclareScript("end", b => { b.ExecuteElement("end"); });
        builder.CallScript("prepare");
        builder.CallScript("start");

        stream.Seek(0, 0);
        var storage = new ByteCodeStorage();
        storage.Register(nameof(FormatTest), stream.GetBuffer());
        var test = new List<string>();
        var processMachine = new ProcessRuntime(new ParameterDelegateFactory(), nameof(FormatTest))
        {
            ComponentFactory = (name, objectName) => CreateElement(name, test)
        };
        var interpreter = new ByteCodeInterpreter(false, storage, processMachine);

        var cancellationTokenSource =
            Debugger.IsAttached ? new CancellationTokenSource() : new CancellationTokenSource(1000);
        var stopWatch = Stopwatch.StartNew();

        // Act
        await interpreter.RunAsync(cancellationTokenSource.Token);

        stopWatch.Stop();
        _testOutputHelper.WriteLine("Act elapsed {0} ms", stopWatch.ElapsedMilliseconds);

        // Assert
        Assert.True(processMachine.Components.ContainsKey("start"), "processMachine.Components.ContainsKey('start')");
        Assert.True(processMachine.Components.ContainsKey("doWork1"),
            "processMachine.Components.ContainsKey('doWork1')");
        Assert.True(processMachine.Components.ContainsKey("doWork2"),
            "processMachine.Components.ContainsKey('doWork2')");
        Assert.True(processMachine.Components.ContainsKey("end"), "processMachine.Components.ContainsKey('end')");
        Assert.Equal("start", test[0]);
        Assert.Equal("doWork1", test[1]);
        Assert.Equal("doWork2", test[2]);
        Assert.Equal("end", test[3]);
        Assert.True(processMachine.Parameters.ContainsKey("a"), "processMachine.Parameters.ContainsKey('a')");
        Assert.Equal("hello", processMachine.Parameters["a"]);
    }

    private BaseElement CreateElement(string name, List<string> test)
    {
        return name switch
        {
            "start" => new Start(test),
            "doWork1" => new DoWork1(test),
            "doWork2" => new DoWork2(test),
            "end" => new End(test),
            _ => throw new ArgumentOutOfRangeException(nameof(name), name, null)
        };
    }

    private class Start : BaseElement
    {
        private readonly List<string> _test;

        public Start(List<string> test)
        {
            _test = test;
        }

        public override string ClassName { get; init; } = nameof(Start);

        public override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _test.Add("start");
            return Task.CompletedTask;
        }
    }


    private class DoWork1 : BaseElement
    {
        private readonly List<string> _test;

        public DoWork1(List<string> test)
        {
            _test = test;
        }

        public override string ClassName { get; init; } = nameof(DoWork1);


        public string? ParameterA { get; set; }
        public string? ParameterB { get; set; }

        public override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            ParameterB = ParameterA;
            _test.Add("doWork1");
            return Task.CompletedTask;
        }
    }

    private class DoWork2 : BaseElement
    {
        private readonly List<string> _test;

        public DoWork2(List<string> test)
        {
            _test = test;
        }

        public override string ClassName { get; init; } = nameof(DoWork2);


        public string? ParameterA { get; set; }
        public string? ParameterB { get; set; }

        public override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            ParameterB = ParameterA;
            _test.Add("doWork2");
            return Task.CompletedTask;
        }
    }

    private class End : BaseElement
    {
        private readonly List<string> _test;

        public End(List<string> test)
        {
            _test = test;
        }

        public override string ClassName { get; init; } = nameof(End);


        public override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _test.Add("end");
            return Task.CompletedTask;
        }
    }
}