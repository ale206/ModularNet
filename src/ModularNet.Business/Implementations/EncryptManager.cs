using System.Security.Cryptography;
using System.Text;
using ModularNet.Business.Interfaces;
using ModularNet.Domain.CustomExceptions;
using ModularNet.Domain.Entities;

namespace ModularNet.Business.Implementations;

public class EncryptManager : IEncryptManager
{
    private readonly AppSettings _appSettings;

    public EncryptManager(IAppSettingsManager appSettingsManager)
    {
        _appSettings = appSettingsManager.GetAppSettings().Result;
    }

    /// <summary>
    ///     Generate an Initialization Vector
    /// </summary>
    /// <returns>A Base64 string of the initialization vector created as byte[]</returns>
    public Task<string> GenerateInitializationVector()
    {
        using var rng = RandomNumberGenerator.Create();

        var initializationVector = new byte[16];
        rng.GetBytes(initializationVector);
        return Task.FromResult(Convert.ToBase64String(initializationVector));
    }

    // Use a secure symmetric encryption algorithm like AES to encrypt the user's password and mnemonic phrase using the encryption key
    public async Task<byte[]> Encrypt(string plaintext, string password, string initializationVector)
    {
        using var aes = Aes.Create();
        aes.Key = await DeriveKeyFromPassword(password);
        aes.IV = Convert.FromBase64String(initializationVector);
        aes.Padding = PaddingMode.PKCS7; // Set the same padding mode for encryption and decryption

        using var ms = new MemoryStream();
        await using var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);

        var bytes = Encoding.UTF8.GetBytes(plaintext);
        cs.Write(bytes, 0, bytes.Length);
        await cs.FlushFinalBlockAsync();
        return ms.ToArray();
    }

    // Use the decrypted encryption key to decrypt the user's password and mnemonic phrase
    public async Task<string> Decrypt(byte[] ciphertext, string password, string initializationVectorBase64)
    {
        try
        {
            using var aes = Aes.Create();
            aes.Key = await DeriveKeyFromPassword(password);
            aes.IV = Convert.FromBase64String(initializationVectorBase64);
            aes.Padding = PaddingMode.PKCS7; // Set the same padding mode for encryption and decryption

            using var ms = new MemoryStream();
            await using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write);

            cs.Write(ciphertext, 0, ciphertext.Length);
            await cs.FlushFinalBlockAsync();
            return Encoding.UTF8.GetString(ms.ToArray());
        }
        catch (Exception ex)
        {
            throw new EncryptionException("Error Decrypting", ex);
        }
    }

    public async Task<string> GenerateEmailVerificationCode(string userEmail)
    {
        var encryptionKey = _appSettings.ModularNetConfig.EncryptionSalt;
        var initializationVector = _appSettings.ModularNetConfig.InitializationVector;

        var expirationDate = DateTime.UtcNow.AddDays(7);

        // If you change this, also VerifierManager must be changed
        var verificationCode = $"{userEmail}|{expirationDate}";

        var codeEncrypted = await Encrypt(verificationCode, encryptionKey, initializationVector);

        return Convert.ToBase64String(codeEncrypted);
    }

    /// <summary>
    ///     To hash and save the password
    /// </summary>
    /// <param name="textInClear"></param>
    /// <returns>Hashed Password in Base64String</returns>
    public async Task<string> HashText(string textInClear)
    {
        // Generate a random salt
        var salt = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // Hash the password and combine with the salt
        var hash = new Rfc2898DeriveBytes(textInClear, salt, 10000, HashAlgorithmName.SHA256).GetBytes(20);
        var hashBytes = new byte[36];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 20);

        // Convert to base64-encoded string
        var base64String = Convert.ToBase64String(hashBytes);

        return base64String;
    }

    /// <summary>
    ///     To verify if the password in clear corresponds to that one hashed and stored before
    /// </summary>
    /// <param name="textInClear"></param>
    /// <param name="savedHashedText"></param>
    /// <returns>True if verified, false otherwise</returns>
    public async Task<bool> VerifyHashedText(string textInClear, string savedHashedText)
    {
        // Convert the saved password hash from base64 back to bytes
        var hashBytes = Convert.FromBase64String(savedHashedText);

        // Extract the salt from the beginning of the hash
        var salt = new byte[16];
        Array.Copy(hashBytes, 0, salt, 0, 16);

        // Hash the provided password with the same salt
        var hash = new Rfc2898DeriveBytes(textInClear, salt, 10000, HashAlgorithmName.SHA256).GetBytes(20);

        // Compare the resulting hash to the one stored in the database
        for (var i = 0; i < 20; i++)
            if (hashBytes[i + 16] != hash[i])
                return false;
        return true;
    }

    // Use a key derivation function like PBKDF2 or scrypt to generate a secure key from the user's password
    private async Task<byte[]> DeriveKeyFromPassword(string password)
    {
        var salt = Encoding.UTF8.GetBytes(password);
        const int iterations = 10000;
        const int keySize = 32;

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(keySize);
    }

    #region Methods to decrypt OneAccessCode

    public static async Task<string> DecryptOneAccessCode(string encryptedBase64)
    {
        // Define the key and IV
        // TODO: Add these tokens in a secure place
        var keyString = "ModularNet.2024".PadRight(32, ' '); // Ensure the key is 32 bytes
        var ivString = "ModularNet.IV.2024".PadRight(16, ' '); // Ensure the IV is 16 bytes

        // Convert key and IV to byte arrays
        var key = Encoding.UTF8.GetBytes(keyString);
        var iv = Encoding.UTF8.GetBytes(ivString);

        // Convert the encrypted text from Base64 to byte array
        var encryptedBytes = Convert.FromBase64String(encryptedBase64);

        // Decrypt the bytes to a string.
        return await DecryptAsync(encryptedBytes, key, iv);
    }

    private static async Task<string> DecryptAsync(byte[] ciphertext, byte[] key, byte[] iv)
    {
        // Check arguments.
        if (ciphertext == null || ciphertext.Length <= 0)
            throw new ArgumentNullException(nameof(ciphertext));
        if (key == null || key.Length <= 0)
            throw new ArgumentNullException(nameof(key));
        if (iv == null || iv.Length <= 0)
            throw new ArgumentNullException(nameof(iv));

        // Create an Aes object with the specified key and IV.
        using (var aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.IV = iv;
            aesAlg.Padding = PaddingMode.PKCS7;

            // Create a decryptor to perform the stream transform.
            var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for decryption.
            using (var msDecrypt = new MemoryStream(ciphertext))
            {
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {
                        // Read the decrypted bytes from the decrypting stream
                        // and place them in a string.
                        return await srDecrypt.ReadToEndAsync();
                    }
                }
            }
        }
    }

    public async Task<bool> IsAccessCodeValid(string accessCode)
    {
        // Step 1: Decrypt the access code
        var decryptedDateTimeString = await DecryptOneAccessCode(accessCode);

        // Step 2: Validate the decrypted date-time
        if (!DateTime.TryParse(decryptedDateTimeString, out var decryptedDateTime)) return false;
        var difference = DateTime.UtcNow - decryptedDateTime;
        return difference.TotalMinutes <= 1;
    }

    #endregion
}