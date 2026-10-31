using System.Net;
using System.Text;
using ModularNet.Infrastructure.Implementations;

namespace ModularNet.UnitTests;

public class JwtProviderTests
{
    private class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _responseContent;
        private readonly HttpStatusCode _statusCode;

        public FakeHttpMessageHandler(string responseContent, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _responseContent = responseContent;
            _statusCode = statusCode;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_responseContent, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }

    [Fact]
    public async Task GetJwtTokenFromFirebaseReturnsTokenWhenResponseSuccessful()
    {
        // Arrange
        const string jsonResponse = "{\"idToken\":\"test-firebase-id-token-12345\"}";
        var handler = new FakeHttpMessageHandler(jsonResponse);
        var httpClient = new HttpClient(handler);
        var jwtProvider = new JwtProvider(httpClient);

        // Act
        var token = await jwtProvider.GetJwtTokenFromFirebase("user@test.com", "Password123!", "api-key-xyz");

        // Assert
        Assert.Equal("test-firebase-id-token-12345", token);
    }

    [Fact]
    public void AuthTokenPropertiesCanBeSetAndRead()
    {
        // Arrange & Act
        var authToken = new JwtProvider.AuthToken
        {
            Kind = "identitytoolkit#VerifyPasswordResponse",
            LocalId = "user-id-123",
            Email = "user@test.com",
            DisplayName = "Test User",
            IdToken = "jwt-token-value",
            Registered = "true",
            RefreshToken = "refresh-token-value",
            ExpiresIn = "3600"
        };

        // Assert
        Assert.Equal("identitytoolkit#VerifyPasswordResponse", authToken.Kind);
        Assert.Equal("user-id-123", authToken.LocalId);
        Assert.Equal("user@test.com", authToken.Email);
        Assert.Equal("Test User", authToken.DisplayName);
        Assert.Equal("jwt-token-value", authToken.IdToken);
        Assert.Equal("true", authToken.Registered);
        Assert.Equal("refresh-token-value", authToken.RefreshToken);
        Assert.Equal("3600", authToken.ExpiresIn);
    }
}
