using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using ModularNet.Business.Interfaces;
using ModularNet.Domain.Enums;
using ModularNet.IntegrationTests.Infrastructure;
using Moq;
using Xunit;

namespace ModularNet.IntegrationTests.Tests;

/// <summary>
/// Demonstrates integration testing for API endpoints using <see cref="ModularNetWebApplicationFactory"/>.
/// This class serves as a reference example for template users illustrating:
/// 1. Basic HTTP GET endpoint testing.
/// 2. Deserializing structured JSON responses.
/// 3. Mocking/substituting internal services via <see cref="TestHostWebHostBuilderExtensions.ConfigureTestServices"/>.
/// 4. Verifying both success (200 OK) and failure (500 Internal Server Error) status codes.
/// </summary>
[Collection("IntegrationTests")]
public class HealthChecksEndpointTests : IDisposable
{
    private readonly ModularNetWebApplicationFactory _factory;
    private readonly HttpClient _httpClient;

    public HealthChecksEndpointTests(ModularNetWebApplicationFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
    }

    /// <summary>
    /// Example 1: Basic GET request asserting standard HTTP 200 OK response.
    /// </summary>
    [Fact]
    public async Task GetHealthChecksEndpointReturnsOk()
    {
        // Arrange & Act
        var response = await _httpClient.GetAsync("/healthchecks");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Example 2: GET request executing in-memory caching pipeline and asserting deserialized JSON payload.
    /// </summary>
    [Fact]
    public async Task GetHealthChecksMemoryCacheReturnsOkWithTimestamp()
    {
        // Arrange & Act
        var response = await _httpClient.GetAsync("/healthchecks/memory-cache");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cachedTimestamp = await response.Content.ReadFromJsonAsync<DateTime>();
        Assert.NotEqual(default, cachedTimestamp);
    }

    /// <summary>
    /// Example 3: Demonstrates mocking a service (IHealthChecksManager) using ConfigureTestServices
    /// to test endpoint behavior without requiring an external database connection.
    /// </summary>
    [Fact]
    public async Task GetHealthChecksDbReturnsOkWhenDbConnectionIsHealthy()
    {
        // Arrange: Create a mock for the dependency to simulate a healthy database connection
        var healthChecksManagerMock = new Mock<IHealthChecksManager>();
        healthChecksManagerMock
            .Setup(manager => manager.CheckDbConnection())
            .Returns(Task.CompletedTask);

        // Substitute the service in the test host container
        using var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped(_ => healthChecksManagerMock.Object);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/healthchecks/db");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        healthChecksManagerMock.Verify(manager => manager.CheckDbConnection(), Times.Once);
    }

    /// <summary>
    /// Example 4: Verifies error handling (500 Internal Server Error) when a dependency throws an exception.
    /// </summary>
    [Fact]
    public async Task GetHealthChecksDbReturnsInternalServerErrorWhenDbConnectionFails()
    {
        // Arrange: Simulate a database connectivity failure
        var healthChecksManagerMock = new Mock<IHealthChecksManager>();
        healthChecksManagerMock
            .Setup(manager => manager.CheckDbConnection())
            .ThrowsAsync(new Exception("Database connection timed out."));

        using var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped(_ => healthChecksManagerMock.Object);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/healthchecks/db");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        healthChecksManagerMock.Verify(manager => manager.CheckDbConnection(), Times.Once);
    }

    /// <summary>
    /// Example 5: Demonstrates mocking cache manager to verify Redis cache healthcheck endpoint.
    /// </summary>
    [Fact]
    public async Task GetHealthChecksRedisCacheReturnsOkWhenCacheSucceeds()
    {
        // Arrange
        var testDate = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var cacheManagerMock = new Mock<ICacheManager>();
        cacheManagerMock
            .Setup(manager => manager.GetFromCache<DateTime>("cachedItem", CacheType.Redis))
            .ReturnsAsync(testDate);

        using var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped(_ => cacheManagerMock.Object);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/healthchecks/redis-cache");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var date = await response.Content.ReadFromJsonAsync<DateTime>();
        Assert.Equal(testDate, date);
    }

    /// <summary>
    /// Example 6: Verifies error handling when Redis cache throws an exception.
    /// </summary>
    [Fact]
    public async Task GetHealthChecksRedisCacheReturnsInternalServerErrorWhenCacheThrows()
    {
        // Arrange
        var cacheManagerMock = new Mock<ICacheManager>();
        cacheManagerMock
            .Setup(manager => manager.GetFromCache<DateTime>("cachedItem", CacheType.Redis))
            .ThrowsAsync(new Exception("Redis connection failed."));

        using var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped(_ => cacheManagerMock.Object);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/healthchecks/redis-cache");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
