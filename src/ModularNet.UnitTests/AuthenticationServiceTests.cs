using ModularNet.Infrastructure.Implementations;
using ModularNet.Infrastructure.Interfaces;
using Moq;

namespace ModularNet.UnitTests;

public class AuthenticationServiceTests
{
    private readonly Mock<IJwtProvider> _jwtProviderMock = new();

    [Fact]
    public async Task GetJwtTokenFromFirebaseAsyncDelegatesToJwtProvider()
    {
        // Arrange
        const string email = "user@test.com";
        const string password = "Password123!";
        const string apiKey = "api-key-xyz";
        const string expectedToken = "jwt-firebase-token";

        _jwtProviderMock
            .Setup(x => x.GetJwtTokenFromFirebase(email, password, apiKey))
            .ReturnsAsync(expectedToken);

        var authenticationService = new AuthenticationService(_jwtProviderMock.Object);

        // Act
        var token = await authenticationService.GetJwtTokenFromFirebaseAsync(email, password, apiKey);

        // Assert
        Assert.Equal(expectedToken, token);
        _jwtProviderMock.Verify(x => x.GetJwtTokenFromFirebase(email, password, apiKey), Times.Once);
    }
}
