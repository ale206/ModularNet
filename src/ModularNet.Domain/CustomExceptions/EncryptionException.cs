namespace ModularNet.Domain.CustomExceptions;

public class EncryptionException : Exception
{
    public EncryptionException()
    {
    }

    public EncryptionException(string message) : base(message)
    {
    }

    public EncryptionException(string message, Exception innerException) : base(message, innerException)
    {
    }
}