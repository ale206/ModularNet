using System.Net;
using ModularNet.IntegrationTests.Infrastructure;
using Xunit;

namespace ModularNet.IntegrationTests.Tests;

[Collection("IntegrationTests")]
public class HealthChecksEndpointTests : IDisposable
{
    private readonly HttpClient _httpClient;

    public HealthChecksEndpointTests(ModularNetWebApplicationFactory factory)
    {
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task GetHealthChecksEndpointReturnsOk()
    {
        // Arrange & Act
        var response = await _httpClient.GetAsync("/healthchecks");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
