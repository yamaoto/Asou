using Asou.Core.Process.Binding;
using Asou.GraphEngine.CodeContractStorage;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace WebApplication1.UnitTests;

public class GraphProcessRegistrationValidation
{
    [Fact]
    public void IsValidCase1()
    {
        // Arrange
        var validator = new GraphProcessRegistration(new Mock<IGraphProcessContractRepository>().Object,
            new Mock<IParameterDelegateFactory>().Object, new NullLogger<GraphProcessRegistration>());
        var graph = GraphProcessContract.Create(Guid.NewGuid(), Guid.NewGuid(), 1, "test",
            new NullLogger<GraphProcessContract>());

        graph.StartFrom<TestElement>("Start1")
            .Then<TestElement>("Step11")
            .Then<TestElement>("Step12")
            .Then<TestElement>("Step13");

        graph.StartFrom<TestElement>("Start2")
            .Then<TestElement>("Step21")
            .Then<TestElement>("Step22")
            .Then<TestElement>("Step23");

        // Act
        var result = validator.ValidateGraph(graph, true);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidCase2Cycling()
    {
        // Arrange
        var validator = new GraphProcessRegistration(new Mock<IGraphProcessContractRepository>().Object,
            new Mock<IParameterDelegateFactory>().Object, new NullLogger<GraphProcessRegistration>());
        var graph = GraphProcessContract.Create(Guid.NewGuid(), Guid.NewGuid(), 1, "test",
            new NullLogger<GraphProcessContract>());

        graph.StartFrom<TestElement>("Start1")
            .Then<TestElement>("Step11")
            .Then<TestElement>("Step12")
            .Then<TestElement>("Step13");

        graph.Sequence<TestElement, TestElement>("Step11", "Step13");
        graph.Sequence<TestElement, TestElement>("Step13", "Step11");

        // Act
        var result = validator.ValidateGraph(graph, true);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsInvalidCase1()
    {
        // Arrange
        var validator = new GraphProcessRegistration(new Mock<IGraphProcessContractRepository>().Object,
            new Mock<IParameterDelegateFactory>().Object, new NullLogger<GraphProcessRegistration>());
        var graph = GraphProcessContract.Create(Guid.NewGuid(), Guid.NewGuid(), 1, "test",
            new NullLogger<GraphProcessContract>());

        graph.StartFrom<TestElement>("Start1")
            .Then<TestElement>("Step11")
            .Then<TestElement>("Step12")
            .Then<TestElement>("Step13");


        graph.StartFrom<TestElement>("Start2")
            .Then<TestElement>("Step21")
            .Then<TestElement>("Step22")
            .Then<TestElement>("Step23");


        graph.Sequence<TestElement, TestElement>("Isolated1", "Isolated2");

        // Act
        Assert.Throws<Exception>(() => validator.ValidateGraph(graph, true));
    }


    [Theory]
    [InlineData(10, 5, 50)]
    public void IsValidCase2Dynamic(int startElements, int depth, int count)
    {
        // Arrange
        var validator = new GraphProcessRegistration(new Mock<IGraphProcessContractRepository>().Object,
            new Mock<IParameterDelegateFactory>().Object, new NullLogger<GraphProcessRegistration>());
        var graph = GraphProcessContract.Create(Guid.NewGuid(), Guid.NewGuid(), 1, "test",
            new NullLogger<GraphProcessContract>());

        for (var i = 0; i < startElements; i++)
        {
            var current = $"Start{i}";
            graph.StartFrom<TestElement>(current);
            for (var j = 0; j < depth; j++)
            {
                var name = $"Step{i}{j}";
                graph.Sequence<TestElement, TestElement>(current, name);
                current = name;
                for (var k = 0; k < count; k++)
                {
                    graph.Sequence<TestElement, TestElement>(current, $"Step{i}{j}{k}");
                }
            }
        }

        // Act
        var result = validator.ValidateGraph(graph, true);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(10, 5, 50)]
    public void IsInvalidCase2Dynamic(int startElements, int depth, int count)
    {
        // Arrange
        var validator = new GraphProcessRegistration(new Mock<IGraphProcessContractRepository>().Object,
            new Mock<IParameterDelegateFactory>().Object, new NullLogger<GraphProcessRegistration>());
        var graph = GraphProcessContract.Create(Guid.NewGuid(), Guid.NewGuid(), 1, "test",
            new NullLogger<GraphProcessContract>());

        for (var i = 0; i < startElements; i++)
        {
            var current = $"Start{i}";
            graph.StartFrom<TestElement>(current);
            for (var j = 0; j < depth; j++)
            {
                var name = $"Step{i}{j}";
                graph.Sequence<TestElement, TestElement>(current, name);
                current = name;
                for (var k = 0; k < count; k++)
                {
                    graph.Sequence<TestElement, TestElement>(current, $"Step{i}{j}{k}");
                }
            }
        }

        graph.Sequence<TestElement, TestElement>("Isolated1", "Isolated2");

        // Act
        Assert.Throws<Exception>(() => validator.ValidateGraph(graph, true));
    }
}
