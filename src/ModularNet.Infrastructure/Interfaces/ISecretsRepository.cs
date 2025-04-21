namespace ModularNet.Infrastructure.Interfaces;

public interface ISecretsRepository
{
    Task<string?> GetSecret(string secretName);

    //Task SetSecret(string secretName, string secretValue);
    Task<string> GetSecretAndCacheIt(string secretName);
}