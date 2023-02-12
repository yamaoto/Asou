using System.Dynamic;
using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;

namespace WebApplication1.FunctionalTests;

public class HelloWorldProcessTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HelloWorldProcessTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task TestHelloWorld()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsync("/", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        dynamic data = JsonConvert.DeserializeObject(json, typeof(ExpandoObject));
        Assert.Null(data.Parameter1);
        Assert.Equal("Hello World", data.Parameter2 as string);
    }
}
