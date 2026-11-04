using Microsoft.Extensions.DependencyInjection;
using ModularNet.Business.Interfaces;
using ModularNet.IntegrationTests.Infrastructure;
using Xunit;

namespace ModularNet.IntegrationTests.Tests;

[Collection("IntegrationTests")]
public class EncryptManagerTests : IDisposable
{
    private readonly IServiceScope _scope;
    private readonly IEncryptManager _encryptManager;

    public EncryptManagerTests(ModularNetWebApplicationFactory factory)
    {
        _scope = factory.Services.CreateScope();
        _encryptManager = _scope.ServiceProvider.GetRequiredService<IEncryptManager>();
    }

    [Fact]
    public async Task GenerateInitializationVectorReturnsValidBase64()
    {
        // Arrange & Act
        var initializationVector = await _encryptManager.GenerateInitializationVector();

        // Assert
        Assert.NotNull(initializationVector);
        Assert.NotEmpty(initializationVector);
        var bytes = Convert.FromBase64String(initializationVector);
        Assert.Equal(16, bytes.Length);
    }

    [Fact]
    public async Task GenerateEmailVerificationCodeReturnsValidCode()
    {
        // Arrange & Act
        var emailVerificationCode = await _encryptManager.GenerateEmailVerificationCode("email@example.com");

        // Assert
        Assert.NotNull(emailVerificationCode);
        Assert.NotEmpty(emailVerificationCode);
    }

    public void Dispose()
    {
        _scope.Dispose();
    }
}
