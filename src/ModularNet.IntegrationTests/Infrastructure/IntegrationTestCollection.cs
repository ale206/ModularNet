using Xunit;

namespace ModularNet.IntegrationTests.Infrastructure;

[CollectionDefinition("IntegrationTests")]
public class IntegrationTestCollection : ICollectionFixture<ModularNetWebApplicationFactory>
{
}
