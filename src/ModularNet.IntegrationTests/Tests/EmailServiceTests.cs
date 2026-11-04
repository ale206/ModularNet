using Microsoft.Extensions.DependencyInjection;
using ModularNet.Business.Interfaces;
using ModularNet.IntegrationTests.Infrastructure;
using Xunit;

namespace ModularNet.IntegrationTests.Tests;

[Collection("IntegrationTests")]
public class EmailServiceTests : IDisposable
{
    private readonly IServiceScope _scope;
    private readonly IEmailServiceManager _emailServiceManager;

    public EmailServiceTests(ModularNetWebApplicationFactory factory)
    {
        _scope = factory.Services.CreateScope();
        _emailServiceManager = _scope.ServiceProvider.GetRequiredService<IEmailServiceManager>();
    }

    [Fact]
    public void EmailServiceManagerResolvesFromHostContainer()
    {
        // Assert
        Assert.NotNull(_emailServiceManager);
    }

    public void Dispose()
    {
        _scope.Dispose();
    }
}
