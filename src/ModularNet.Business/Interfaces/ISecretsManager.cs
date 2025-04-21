namespace ModularNet.Business.Interfaces;

public interface ISecretsManager
{
    Task<string?> GetSecretAndCacheIt(string secretName);

    //Task SetSecret(string secretName, string secretValue);
}