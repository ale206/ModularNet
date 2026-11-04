using System.Net;
using System.Net.Http.Json;
using ModularNet.Domain.Requests;
using ModularNet.IntegrationTests.Infrastructure;
using Xunit;

namespace ModularNet.IntegrationTests.Tests;

/// <summary>
/// Tests that endpoints decorated with [Authorize] correctly reject unauthenticated requests
/// with HTTP 401 Unauthorized.
/// </summary>
[Collection("IntegrationTests")]
public class ProtectedEndpointsTests : IDisposable
{
    private readonly HttpClient _httpClient;

    public ProtectedEndpointsTests(ModularNetWebApplicationFactory factory)
    {
        _httpClient = factory.CreateClient();
    }

    /// <summary>
    /// Verifies that POST /logs without a valid authorization token returns 401 Unauthorized.
    /// </summary>
    [Fact]
    public async Task PostLogsWithoutAuthorizationReturnsUnauthorized()
    {
        // Arrange
        var request = new WriteLogRequest
        {
            LogLevel = "Information",
            LogMessage = "Test audit message",
            LogTimeStamp = DateTime.UtcNow
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/logs", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Verifies that GET /secrets without a valid authorization token returns 401 Unauthorized.
    /// </summary>
    [Fact]
    public async Task GetSecretsWithoutAuthorizationReturnsUnauthorized()
    {
        // Act
        var response = await _httpClient.GetAsync("/secrets?key=test-secret");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Verifies that POST /auth/logout without a valid authorization token returns 401 Unauthorized.
    /// </summary>
    [Fact]
    public async Task PostLogoutWithoutAuthorizationReturnsUnauthorized()
    {
        // Arrange
        var request = new LogoutRequest
        {
            Uid = "test-uid"
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/auth/logout", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
