namespace ModularNet.Business.Interfaces;

public interface IEncryptManager
{
    Task<string> HashText(string textInClear);
    Task<bool> VerifyHashedText(string textInClear, string savedHashedText);
    Task<string> GenerateInitializationVector();
    Task<byte[]> Encrypt(string plaintext, string password, string initializationVector);
    Task<string> Decrypt(byte[] ciphertext, string password, string initializationVectorBase64);
    Task<string> GenerateEmailVerificationCode(string userEmail);
    Task<bool> IsAccessCodeValid(string accessCode);
}