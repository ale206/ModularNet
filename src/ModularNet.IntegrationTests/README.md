# ModularNet Integration Tests

This project provides integration tests for the ModularNet solution using **xUnit** and ASP.NET Core's **`Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<TEntryPoint>`**.

It serves as both the test suite and a reference template for writing integration tests against the ModularNet API and its dependencies.

---

## Architecture Overview

```
ModularNet.IntegrationTests/
├── Infrastructure/
│   ├── ModularNetWebApplicationFactory.cs  # In-memory test host configuration
│   └── IntegrationTestCollection.cs        # xUnit collection fixture for shared host lifecycle
├── Tests/
│   ├── HealthChecksEndpointTests.cs        # Controller GET endpoints & Mocking with ConfigureTestServices
│   ├── EmailVerifierEndpointTests.cs       # Controller POST endpoints with JSON payloads & error handling
│   ├── AppSettingsManagerTests.cs          # Scoped manager resolution & configuration validation
│   ├── CacheManagerTests.cs                # In-memory cache operations
│   ├── EncryptManagerTests.cs              # Encryption & initialization vector generation
│   ├── EmailServiceTests.cs                # Email service dependency resolution
│   └── ConfigurationTests.cs               # Configuration binding from appsettings.json
└── appsettings.json                        # Test-specific configuration overrides
```

---

## Key Concepts & Best Practices

### 1. `WebApplicationFactory` & Collection Fixtures
The [`ModularNetWebApplicationFactory`](Infrastructure/ModularNetWebApplicationFactory.cs) bootstraps an in-memory test server for the application.

To optimize test execution and avoid creating redundant test servers, test classes are grouped under the `[Collection("IntegrationTests")]` collection fixture defined in [`IntegrationTestCollection`](Infrastructure/IntegrationTestCollection.cs):

```csharp
[Collection("IntegrationTests")]
public class HealthChecksEndpointTests : IDisposable
{
    private readonly HttpClient _httpClient;

    public HealthChecksEndpointTests(ModularNetWebApplicationFactory factory)
    {
        _httpClient = factory.CreateClient();
    }
}
```

### 2. Testing HTTP Endpoints
Use `factory.CreateClient()` to obtain an `HttpClient` wired directly to the in-memory test server:

```csharp
[Fact]
public async Task GetHealthChecksEndpointReturnsOk()
{
    // Arrange & Act
    var response = await _httpClient.GetAsync("/healthchecks");

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
}
```

### 3. Mocking Dependencies with `ConfigureTestServices`
When testing endpoints that interact with external services (like databases, message queues, or third-party APIs), use `WithWebHostBuilder` and `ConfigureTestServices` to replace registered services with Moq mocks:

```csharp
[Fact]
public async Task GetHealthChecksDbReturnsOkWhenDbConnectionIsHealthy()
{
    // Arrange
    var healthChecksManagerMock = new Mock<IHealthChecksManager>();
    healthChecksManagerMock
        .Setup(manager => manager.CheckDbConnection())
        .Returns(Task.CompletedTask);

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
```

### 4. Resolving Scoped Services
Business managers are registered with scoped lifetimes (`AddScoped`). In integration tests, resolve scoped dependencies inside an explicit `IServiceScope`:

```csharp
[Collection("IntegrationTests")]
public class AppSettingsManagerTests : IDisposable
{
    private readonly IServiceScope _scope;
    private readonly IAppSettingsManager _appSettingsManager;

    public AppSettingsManagerTests(ModularNetWebApplicationFactory factory)
    {
        _scope = factory.Services.CreateScope();
        _appSettingsManager = _scope.ServiceProvider.GetRequiredService<IAppSettingsManager>();
    }

    public void Dispose()
    {
        _scope.Dispose();
    }
}
```

---

## Coding Standards Compliance

All tests adhere strictly to the repository's [coding standards](../../coding-standards.md):
- **Frameworks:** Exclusively use **xUnit** and **Moq**.
- **Method Naming:** Do **not** use the `Async` suffix on test methods (e.g., `GetHealthChecksEndpointReturnsOk`, not `GetHealthChecksEndpointReturnsOkAsync`).
- **Structure:** Follow the standard **Arrange, Act, Assert (AAA)** pattern with explicit comment headers.
- **Mock Naming:** Name mocks after the interface/class being replaced followed by `Mock` (e.g., `healthChecksManagerMock`).

---

## Running the Tests

To run the integration tests via command line:
```bash
dotnet test src/ModularNet.IntegrationTests/ModularNet.IntegrationTests.csproj
```

To run all tests (unit tests and integration tests) together:
```bash
dotnet test src/ModularNet.sln
```
