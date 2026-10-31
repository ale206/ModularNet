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

    [Fact]
    public async Task VerifyHashedTextReturnsFalseWhenPasswordIsIncorrect()
    {
        // Arrange
        var appSettings = AppSettingsHelper.GetAppSettings();
        _appSettingsManagerMock.Setup(x => x.GetAppSettings()).ReturnsAsync(appSettings);
        var encryptManager = new EncryptManager(_appSettingsManagerMock.Object);
        var hashedText = await encryptManager.HashText("CorrectPassword");

        // Act
        var isValid = await encryptManager.VerifyHashedText("WrongPassword", hashedText);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public async Task DecryptThrowsEncryptionExceptionWhenCiphertextIsCorrupted()
    {
        // Arrange
        var appSettings = AppSettingsHelper.GetAppSettings();
        _appSettingsManagerMock.Setup(x => x.GetAppSettings()).ReturnsAsync(appSettings);
        var encryptManager = new EncryptManager(_appSettingsManagerMock.Object);
        var corruptedBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var iv = await encryptManager.GenerateInitializationVector();

        // Act & Assert
        await Assert.ThrowsAsync<ModularNet.Domain.CustomExceptions.EncryptionException>(() =>
            encryptManager.Decrypt(corruptedBytes, "password", iv));
    }

    [Fact]
    public async Task GenerateEmailVerificationCodeReturnsValidBase64Code()
    {
        // Arrange
        var appSettings = AppSettingsHelper.GetAppSettings();
        appSettings.ModularNetConfig.EncryptionSalt = "salt123456789012";
        appSettings.ModularNetConfig.InitializationVector = Convert.ToBase64String(new byte[16]);
        _appSettingsManagerMock.Setup(x => x.GetAppSettings()).ReturnsAsync(appSettings);
        var encryptManager = new EncryptManager(_appSettingsManagerMock.Object);
        const string userEmail = "test@example.com";

        // Act
        var code = await encryptManager.GenerateEmailVerificationCode(userEmail);

        // Assert
        Assert.False(string.IsNullOrEmpty(code));
        var bytes = Convert.FromBase64String(code);
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public async Task IsAccessCodeValidReturnsTrueForRecentTimestamp()
    {
        // Arrange
        var appSettings = AppSettingsHelper.GetAppSettings();
        _appSettingsManagerMock.Setup(x => x.GetAppSettings()).ReturnsAsync(appSettings);
        var encryptManager = new EncryptManager(_appSettingsManagerMock.Object);
        var timestampString = DateTime.UtcNow.ToString("o");
        var encryptedAccessCode = EncryptOneAccessCodeForTest(timestampString);

        // Act
        var isValid = await encryptManager.IsAccessCodeValid(encryptedAccessCode);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public async Task IsAccessCodeValidReturnsFalseForOldTimestamp()
    {
        // Arrange
        var appSettings = AppSettingsHelper.GetAppSettings();
        _appSettingsManagerMock.Setup(x => x.GetAppSettings()).ReturnsAsync(appSettings);
        var encryptManager = new EncryptManager(_appSettingsManagerMock.Object);
        var oldTimestampString = DateTime.UtcNow.AddMinutes(-10).ToString("o");
        var encryptedAccessCode = EncryptOneAccessCodeForTest(oldTimestampString);

        // Act
        var isValid = await encryptManager.IsAccessCodeValid(encryptedAccessCode);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public async Task IsAccessCodeValidReturnsFalseWhenPlainTextIsNotDate()
    {
        // Arrange
        var appSettings = AppSettingsHelper.GetAppSettings();
        _appSettingsManagerMock.Setup(x => x.GetAppSettings()).ReturnsAsync(appSettings);
        var encryptManager = new EncryptManager(_appSettingsManagerMock.Object);
        var notADate = "invalid-date-string";
        var encryptedAccessCode = EncryptOneAccessCodeForTest(notADate);

        // Act
        var isValid = await encryptManager.IsAccessCodeValid(encryptedAccessCode);

        // Assert
        Assert.False(isValid);
    }

    private static string EncryptOneAccessCodeForTest(string plainText)
    {
        var keyString = "ModularNet.2024".PadRight(32, ' ');
        var ivString = "ModularNet.IV.2024".Substring(0, 16);
        using var aes = System.Security.Cryptography.Aes.Create();
        aes.Key = System.Text.Encoding.UTF8.GetBytes(keyString);
        aes.IV = System.Text.Encoding.UTF8.GetBytes(ivString);
        aes.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
        using var ms = new MemoryStream();
        using var cs = new System.Security.Cryptography.CryptoStream(ms, aes.CreateEncryptor(), System.Security.Cryptography.CryptoStreamMode.Write);
        var bytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        cs.Write(bytes, 0, bytes.Length);
        cs.FlushFinalBlock();
        return Convert.ToBase64String(ms.ToArray());
    }
}