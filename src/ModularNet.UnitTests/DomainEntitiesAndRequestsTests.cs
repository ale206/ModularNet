using Azure.Communication.Email;
using ModularNet.Domain.Entities;
using ModularNet.Domain.Requests;

namespace ModularNet.UnitTests;

public class DomainEntitiesAndRequestsTests
{
    [Fact]
    public void BaseEntityPropertiesCanBeAssignedAndRead()
    {
        // Arrange & Act
        var now = DateTime.UtcNow;
        var id = Guid.NewGuid();
        var entity = new BaseEntity
        {
            Id = id,
            IsEnabled = true,
            Meta = "metadata",
            CreatedOn = now,
            ModifiedOn = now.AddMinutes(5),
            DeletedOn = null
        };

        // Assert
        Assert.Equal(id, entity.Id);
        Assert.True(entity.IsEnabled);
        Assert.Equal("metadata", entity.Meta);
        Assert.Equal(now, entity.CreatedOn);
        Assert.Equal(now.AddMinutes(5), entity.ModifiedOn);
        Assert.Null(entity.DeletedOn);
    }

    [Fact]
    public void ModularNetEmailPropertiesCanBeAssignedAndRead()
    {
        // Arrange & Act
        var recipients = new EmailRecipients(new List<EmailAddress> { new("to@test.com") });
        var email = new ModularNetEmail
        {
            Subject = "Test Subject",
            PlainText = "Body text",
            Html = "<p>Body html</p>",
            SenderEmail = "from@test.com",
            EmailRecipients = recipients,
            EmailRecipientsJson = "[\"to@test.com\"]",
            ImportanceValue = "High"
        };

        // Assert
        Assert.Equal("Test Subject", email.Subject);
        Assert.Equal("Body text", email.PlainText);
        Assert.Equal("<p>Body html</p>", email.Html);
        Assert.Equal("from@test.com", email.SenderEmail);
        Assert.Same(recipients, email.EmailRecipients);
        Assert.Equal("[\"to@test.com\"]", email.EmailRecipientsJson);
        Assert.Equal("High", email.ImportanceValue);
    }

    [Fact]
    public void InMemoryCacheItemPropertiesCanBeAssignedAndRead()
    {
        // Arrange & Act
        var now = DateTime.UtcNow;
        var cacheItem = new InMemoryCacheItem
        {
            ItemKey = "cache-key",
            ItemValue = "cache-val",
            ExpirationDate = now
        };

        // Assert
        Assert.Equal("cache-key", cacheItem.ItemKey);
        Assert.Equal("cache-val", (string)cacheItem.ItemValue);
        Assert.Equal(now, cacheItem.ExpirationDate);
    }

    [Fact]
    public void SignInWithExternalProviderRequestPropertiesCanBeAssignedAndRead()
    {
        // Arrange & Act
        var request = new SignInWithExternalProviderRequest
        {
            IdToken = "id-token",
            Email = "ext@test.com",
            DisplayName = "Ext User",
            PhotoUrl = "https://photo.url",
            Uid = "uid-123",
            RefreshToken = "refresh-tok",
            IsEmailVerified = true,
            ProviderId = "google.com"
        };

        // Assert
        Assert.Equal("id-token", request.IdToken);
        Assert.Equal("ext@test.com", request.Email);
        Assert.Equal("Ext User", request.DisplayName);
        Assert.Equal("https://photo.url", request.PhotoUrl);
        Assert.Equal("uid-123", request.Uid);
        Assert.Equal("refresh-tok", request.RefreshToken);
        Assert.True(request.IsEmailVerified);
        Assert.Equal("google.com", request.ProviderId);
    }

    [Fact]
    public void SignUpUserRequestPropertiesCanBeAssignedAndRead()
    {
        // Arrange & Act
        var request = new SignUpUserRequest
        {
            Email = "signup@test.com",
            Password = "Password1!",
            FirstName = "John",
            LastName = "Doe",
            TermsAccepted = true
        };

        // Assert
        Assert.Equal("signup@test.com", request.Email);
        Assert.Equal("Password1!", request.Password);
        Assert.Equal("John", request.FirstName);
        Assert.Equal("Doe", request.LastName);
        Assert.True(request.TermsAccepted);
    }

    [Fact]
    public void WriteLogRequestPropertiesCanBeAssignedAndRead()
    {
        // Arrange & Act
        var timeStamp = DateTimeOffset.UtcNow;
        var request = new WriteLogRequest
        {
            LogTimeStamp = timeStamp,
            LogLevel = "Information",
            LogMessage = "Log message content",
            LogException = "NullReferenceException",
            LogProperties = "{\"key\":\"val\"}"
        };

        // Assert
        Assert.Equal(timeStamp, request.LogTimeStamp);
        Assert.Equal("Information", request.LogLevel);
        Assert.Equal("Log message content", request.LogMessage);
        Assert.Equal("NullReferenceException", request.LogException);
        Assert.Equal("{\"key\":\"val\"}", request.LogProperties);
    }

    [Fact]
    public void RequestPocosCanBeAssignedAndRead()
    {
        // GetUserTokenRequest
        var getUserTokenRequest = new GetUserTokenRequest
        {
            Email = "user@test.com",
            OneAccessCode = "access-code-123"
        };
        Assert.Equal("user@test.com", getUserTokenRequest.Email);
        Assert.Equal("access-code-123", getUserTokenRequest.OneAccessCode);

        // ResetPasswordRequest
        var resetPasswordRequest = new ResetPasswordRequest
        {
            Email = "reset@test.com"
        };
        Assert.Equal("reset@test.com", resetPasswordRequest.Email);

        // SignInUserRequest
        var signInUserRequest = new SignInUserRequest
        {
            Email = "sign@test.com",
            Password = "Pass"
        };
        Assert.Equal("sign@test.com", signInUserRequest.Email);
        Assert.Equal("Pass", signInUserRequest.Password);

        // LogoutRequest
        var logoutRequest = new LogoutRequest
        {
            Uid = "logout-uid"
        };
        Assert.Equal("logout-uid", logoutRequest.Uid);

        // VerifyAuthenticationRequest
        var verifyAuthenticationRequest = new VerifyAuthenticationRequest
        {
            AccessToken = "tok-123"
        };
        Assert.Equal("tok-123", verifyAuthenticationRequest.AccessToken);

        // VerifyEmailRequest
        var verifyEmailRequest = new VerifyEmailRequest
        {
            EmailVerificationCode = "code-abc"
        };
        Assert.Equal("code-abc", verifyEmailRequest.EmailVerificationCode);
    }
}
