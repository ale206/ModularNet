using ModularNet.Domain.CustomExceptions;

namespace ModularNet.UnitTests;

public class CustomExceptionsTests
{
    [Fact]
    public void EncryptionExceptionConstructorsWorkProperly()
    {
        // Default constructor
        var exception1 = new EncryptionException();
        Assert.NotNull(exception1);

        // Message constructor
        const string message = "Encryption failed";
        var exception2 = new EncryptionException(message);
        Assert.Equal(message, exception2.Message);

        // Message and inner exception constructor
        var innerException = new InvalidOperationException("Inner error");
        var exception3 = new EncryptionException(message, innerException);
        Assert.Equal(message, exception3.Message);
        Assert.Same(innerException, exception3.InnerException);
    }

    [Fact]
    public void ValidationExceptionConstructorsWorkProperly()
    {
        // Default constructor
        var exception1 = new ValidationException();
        Assert.NotNull(exception1);

        // Message constructor
        const string message = "Validation failed";
        var exception2 = new ValidationException(message);
        Assert.Equal(message, exception2.Message);

        // Message and inner exception constructor
        var innerException = new ArgumentException("Inner arg");
        var exception3 = new ValidationException(message, innerException);
        Assert.Equal(message, exception3.Message);
        Assert.Same(innerException, exception3.InnerException);
    }
}
