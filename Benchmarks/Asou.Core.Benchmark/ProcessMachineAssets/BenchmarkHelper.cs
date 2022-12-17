namespace Asou.Core.Benchmark.ProcessMachineAssets;

public static class BenchmarkHelper
{
    private static ByteCodeStorage? _store;

    public static ByteCodeStorage GetTestCode(int stepCount)
    {
        if (_store != null) return _store;
        var stream = new MemoryStream();
        var builder = new ByteCodeWriter(stream);
        builder.DeclareScript("prepare", b =>
        {
            b.CreateComponent("start", "start"); // 13
            for (var i = 1; i <= stepCount; i++)
            {
                var name = "doWork" + i;
                b.CreateComponent(name, name); // 17
            }

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
        for (var i = 1; i <= stepCount; i++)
        {
            var name = "doWork" + i;
            var next = i == stepCount ? "end" : "doWork" + (i + 1);
            builder.DeclareScript(name, b =>
            {
                b.ExecuteElement(name);
                b.CallScript(next);
            });
        }

        builder.DeclareScript("end", b => { b.ExecuteElement("end"); });
        builder.CallScript("prepare");
        builder.CallScript("start");
        _store = new ByteCodeStorage();
        _store.Register(nameof(ProcessMachineBenchmark), stream.GetBuffer());
        return _store;
    }
}