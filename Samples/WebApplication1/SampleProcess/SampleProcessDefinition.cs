using Asou.GraphEngine.CodeContractStorage;

namespace WebApplication1.SampleProcess;

public class SampleProcessDefinition : IProcessDefinition
{
    public const string ProcessId = "c380b7f4-2a76-44fc-9d5d-ecc7c105969b";
    public Guid Id => new(ProcessId);

    public Guid VersionId => new("2a8038b9-921e-4aaa-a72d-f85d6ff512e8");

    public int Version => 1;

    public string Name => "SampleProcess";

    public void Describe(GraphProcessContract builder)
    {
        builder.StartFrom<DoSimpleStep>()
            .WithParameter<DoSimpleStep, string>("Parameter1",
                instance => (string)instance.ProcessRuntime.Parameters["Parameter1"]!,
                (instance, value) => instance.ProcessRuntime.Parameters["Parameter1"] = value)
            .WithParameter<DoSimpleStep, string>("Parameter2",
                setter: (instance, value) => instance.ProcessRuntime.Parameters["Parameter2"] = value)
            .Then<AsynchronousResumeStep>()
            .Then<ConditionalStep>();

        builder.Conditional<ConditionalStep, EndStep>("ToExit");
        builder.Conditional<ConditionalStep, DoSimpleStep>("TryAgain");
    }
}
