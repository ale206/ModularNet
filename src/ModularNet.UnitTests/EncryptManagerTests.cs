using System.Text.RegularExpressions;
using ModularNet.Business.Implementations;
using ModularNet.Business.Interfaces;
using ModularNet.UnitTests.Helpers;
using Moq;

namespace ModularNet.UnitTests;

public class EncryptManagerTests
{
    private readonly Mock<IAppSettingsManager> _appSettingsManagerMock = new();

    [Fact]
    public async Task EncryptPasswordAndVerifyEncryption()
    {
        // Arrange
        var appSettings = AppSettingsHelper.GetAppSettings();
        appSettings.ModularNetConfig.EncryptionSalt = "modularNet";
        _appSettingsManagerMock.Setup(x => x.GetAppSettings()).ReturnsAsync(appSettings);

        var passwordInClear = "ModularNetPassword";
        var encryptManager = new EncryptManager(_appSettingsManagerMock.Object);

        // Act
        var encryptedPassword = await encryptManager.HashText(passwordInClear);
        var isPasswordOk = await encryptManager.VerifyHashedText(passwordInClear, encryptedPassword);

        // Assert
        Assert.True(isPasswordOk);

        var base64Pattern = @"^[A-Za-z0-9+/]*={0,2}$";
        var isBase64 = Regex.IsMatch(encryptedPassword, base64Pattern);
        Assert.True(isBase64, "Result is not a valid Base64 string.");
    }

    [Fact]
    public async Task EncryptTextAndDecryptIt()
    {
        // Arrange
        var appSettings = AppSettingsHelper.GetAppSettings();
        appSettings.ModularNetConfig.EncryptionSalt = "modularNet";
        _appSettingsManagerMock.Setup(x => x.GetAppSettings()).ReturnsAsync(appSettings);

        // Act

        // User types the password to protect the wallet
        var passwordInClear = "ModularNetPassword";
        var encryptManager = new EncryptManager(_appSettingsManagerMock.Object);

        // The password is hashed and stored
        var encryptedPassword = await encryptManager.HashText(passwordInClear);

        // We encrypt the subscriptions
        var subscriptions = "all_subscriptions";

        var iv = await encryptManager.GenerateInitializationVector();
        // WE SAVE THE IV

        var subscriptionsEncrypted = await encryptManager.Encrypt(subscriptions, encryptedPassword, iv);

        // We store the Subscriptions Encrypted

        // Now the user wants to load the subscriptions

        // He types the password, and we verify it
        var isPasswordOk = await encryptManager.VerifyHashedText(passwordInClear, encryptedPassword);
        Assert.True(isPasswordOk);

        // We decrypt the Subscriptions
        var decryptedSubscriptions = await encryptManager.Decrypt(subscriptionsEncrypted, encryptedPassword, iv);
        Assert.Equal(subscriptions, decryptedSubscriptions);
    }

    [Fact]
    public async Task EncryptEmailVerificationAndDecryptIt()
    {
        // Arrange
        var appSettings = AppSettingsHelper.GetAppSettings();
        appSettings.ModularNetConfig.EncryptionSalt = "modularNet";
        _appSettingsManagerMock.Setup(x => x.GetAppSettings()).ReturnsAsync(appSettings);

        var expirationDate = DateTime.UtcNow.AddHours(24);
        var verificationCode = $"alessio.disalvo@gmail.com|{expirationDate}";
        var password = appSettings.ModularNetConfig.EncryptionSalt;

        // Act

        // User types the password to protect the wallet
        var encryptManager = new EncryptManager(_appSettingsManagerMock.Object);
        var iv = await encryptManager.GenerateInitializationVector();

        var verificationCodeEncrypted = await encryptManager.Encrypt(verificationCode, password, iv);

        // Code sent by email
        var verificationCodeEncryptedString = Convert.ToBase64String(verificationCodeEncrypted);

        // Code received by email and converted to byte[]
        var codeInByteArray = Convert.FromBase64String(verificationCodeEncryptedString);

        var verificationCodeDecrypted = await encryptManager.Decrypt(codeInByteArray, password, iv);

        Assert.Equal(verificationCode, verificationCodeDecrypted);
    }
}