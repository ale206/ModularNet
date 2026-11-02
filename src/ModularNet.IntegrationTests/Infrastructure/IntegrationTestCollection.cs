using Xunit;

namespace ModularNet.IntegrationTests.Infrastructure;

/// <summary>
/// Defines an xUnit test collection to share a single <see cref="ModularNetWebApplicationFactory"/> instance
/// across all integration test classes, avoiding redundant host startups and concurrent port/resource conflicts.
/// </summary>
[CollectionDefinition("IntegrationTests")]
public class IntegrationTestCollection : ICollectionFixture<ModularNetWebApplicationFactory>
{
}
