using ModularNet.Business.Interfaces;
using Xunit;

namespace ModularNet.IntegrationTests.Tests;

public class EncryptManagerTests
{
    private readonly IEncryptManager _encryptManager;

    public EncryptManagerTests(IEncryptManager encryptManager)
    {
        _encryptManager = encryptManager;
    }

    public async Task RunAllTests()
    {
        await GenerateInitializationVector();
        await GenerateEmailVerificationCode();
    }

    private async Task GenerateInitializationVector()
    {
        var initializationVector = await _encryptManager.GenerateInitializationVector();

        Assert.NotNull(initializationVector);
    }

    private async Task GenerateEmailVerificationCode()
    {
        var emailVerificationCode = await _encryptManager.GenerateEmailVerificationCode("email@email.com");

        Assert.NotNull(emailVerificationCode);
    }
}