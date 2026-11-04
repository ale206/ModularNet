using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using ModularNet.Business.Interfaces;
using ModularNet.Domain.Entities;
using ModularNet.Domain.Requests;
using ModularNet.IntegrationTests.Infrastructure;
using Moq;
using Xunit;

namespace ModularNet.IntegrationTests.Tests;

/// <summary>
/// Demonstrates integration testing for authentication endpoints in <see cref="ModularNet.Api.Controllers.AuthController"/>.
/// Illustrates:
/// 1. Testing unauthenticated requests against protected endpoints.
/// 2. Testing validation errors on public endpoints (e.g. TermsAccepted).
/// 3. Mocking <see cref="IAuthManager"/> and <see cref="IUsersManager"/> with <see cref="TestHostWebHostBuilderExtensions.ConfigureTestServices"/>.
/// 4. Testing HTTP 200 OK, 201 Created, 401 Unauthorized, and 500 Internal Server Error scenarios.
/// </summary>
[Collection("IntegrationTests")]
public class AuthEndpointTests : IDisposable
{
    private readonly ModularNetWebApplicationFactory _factory;
    private readonly HttpClient _httpClient;

    public AuthEndpointTests(ModularNetWebApplicationFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
    }

    /// <summary>
    /// Verifies that POST /auth/verify-authentication returns 200 OK when the token is valid in Firebase.
    /// </summary>
    [Fact]
    public async Task PostVerifyAuthenticationReturnsOkWhenTokenIsValid()
    {
        // Arrange
        const string validToken = "valid-test-token";
        var authManagerMock = new Mock<IAuthManager>();
        authManagerMock
            .Setup(manager => manager.VerifyAuthenticationInFirebase(validToken))
            .ReturnsAsync(true);

        using var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped(_ => authManagerMock.Object);
            });
        }).CreateClient();

        var request = new VerifyAuthenticationRequest
        {
            AccessToken = validToken
        };

        // Act
        var response = await client.PostAsJsonAsync("/auth/verify-authentication", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        authManagerMock.Verify(manager => manager.VerifyAuthenticationInFirebase(validToken), Times.Once);
    }

    /// <summary>
    /// Verifies that POST /auth/verify-authentication returns 401 Unauthorized when the token is invalid.
    /// </summary>
    [Fact]
    public async Task PostVerifyAuthenticationReturnsUnauthorizedWhenTokenIsInvalid()
    {
        // Arrange
        const string invalidToken = "invalid-token";
        var authManagerMock = new Mock<IAuthManager>();
        authManagerMock
            .Setup(manager => manager.VerifyAuthenticationInFirebase(invalidToken))
            .ReturnsAsync(false);

        using var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped(_ => authManagerMock.Object);
            });
        }).CreateClient();

        var request = new VerifyAuthenticationRequest
        {
            AccessToken = invalidToken
        };

        // Act
        var response = await client.PostAsJsonAsync("/auth/verify-authentication", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Verifies that POST /auth/sign-in returns 200 OK with access token when credentials are valid.
    /// </summary>
    [Fact]
    public async Task PostSignInReturnsOkWithAccessTokenWhenCredentialsAreValid()
    {
        // Arrange
        const string email = "user@example.com";
        const string password = "ValidPassword123!";
        const string token = "mock-firebase-jwt-token";
        var userId = Guid.NewGuid();

        var authManagerMock = new Mock<IAuthManager>();
        authManagerMock
            .Setup(manager => manager.LoginUserInFirebase(email, password))
            .ReturnsAsync(token);

        var usersManagerMock = new Mock<IUsersManager>();
        usersManagerMock
            .Setup(manager => manager.GetUserByEmail(email))
            .ReturnsAsync(new User { Id = userId, Email = email });
        usersManagerMock
            .Setup(manager => manager.LoginUser(It.IsAny<UserSignInRequest>()))
            .Returns(Task.CompletedTask);

        using var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped(_ => authManagerMock.Object);
                services.AddScoped(_ => usersManagerMock.Object);
            });
        }).CreateClient();

        var request = new SignInUserRequest
        {
            Email = email,
            Password = password
        };

        // Act
        var response = await client.PostAsJsonAsync("/auth/sign-in", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseToken = await response.Content.ReadAsStringAsync();
        Assert.Contains(token, responseToken);
    }

    /// <summary>
    /// Verifies that POST /auth/sign-in returns 401 Unauthorized when credentials are invalid.
    /// </summary>
    [Fact]
    public async Task PostSignInReturnsUnauthorizedWhenCredentialsAreInvalid()
    {
        // Arrange
        const string email = "user@example.com";
        const string password = "WrongPassword!";

        var authManagerMock = new Mock<IAuthManager>();
        authManagerMock
            .Setup(manager => manager.LoginUserInFirebase(email, password))
            .ReturnsAsync(string.Empty);

        using var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped(_ => authManagerMock.Object);
            });
        }).CreateClient();

        var request = new SignInUserRequest
        {
            Email = email,
            Password = password
        };

        // Act
        var response = await client.PostAsJsonAsync("/auth/sign-in", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Verifies that POST /auth/sign-up creates a new user and returns 201 Created.
    /// </summary>
    [Fact]
    public async Task PostSignUpReturnsCreatedWhenUserIsNew()
    {
        // Arrange
        const string email = "newuser@example.com";
        const string password = "Password123!";
        const string firebaseUid = "fb-new-user-123";
        var createdUserId = Guid.NewGuid();

        var authManagerMock = new Mock<IAuthManager>();
        authManagerMock
            .Setup(manager => manager.RegisterUserInFirebase(email, password))
            .ReturnsAsync(firebaseUid);

        var usersManagerMock = new Mock<IUsersManager>();
        usersManagerMock
            .Setup(manager => manager.GetUserByEmail(email))
            .ReturnsAsync((User?)null);
        usersManagerMock
            .Setup(manager => manager.RegisterUserInModularNet(It.Is<RegisterUserRequest>(req => req.Email == email && req.UserOid == firebaseUid)))
            .ReturnsAsync(createdUserId);

        using var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped(_ => authManagerMock.Object);
                services.AddScoped(_ => usersManagerMock.Object);
            });
        }).CreateClient();

        var request = new SignUpUserRequest
        {
            Email = email,
            Password = password,
            FirstName = "Jane",
            LastName = "Doe",
            TermsAccepted = true
        };

        // Act
        var response = await client.PostAsJsonAsync("/auth/sign-up", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();
        Assert.Equal(createdUserId, returnedId);
    }

    /// <summary>
    /// Verifies that POST /auth/sign-up updates existing user UserOid and returns 201 Created.
    /// </summary>
    [Fact]
    public async Task PostSignUpReturnsCreatedWhenUserIsAlreadyRegistered()
    {
        // Arrange
        const string email = "existing@example.com";
        const string password = "Password123!";
        const string firebaseUid = "fb-existing-123";
        var existingUserId = Guid.NewGuid();

        var authManagerMock = new Mock<IAuthManager>();
        authManagerMock
            .Setup(manager => manager.RegisterUserInFirebase(email, password))
            .ReturnsAsync(firebaseUid);

        var existingUser = new User
        {
            Id = existingUserId,
            Email = email
        };

        var usersManagerMock = new Mock<IUsersManager>();
        usersManagerMock
            .Setup(manager => manager.GetUserByEmail(email))
            .ReturnsAsync(existingUser);
        usersManagerMock
            .Setup(manager => manager.UpdateUserOid(existingUserId, firebaseUid))
            .Returns(Task.CompletedTask);

        using var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped(_ => authManagerMock.Object);
                services.AddScoped(_ => usersManagerMock.Object);
            });
        }).CreateClient();

        var request = new SignUpUserRequest
        {
            Email = email,
            Password = password,
            FirstName = "John",
            LastName = "Doe",
            TermsAccepted = true
        };

        // Act
        var response = await client.PostAsJsonAsync("/auth/sign-up", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        usersManagerMock.Verify(manager => manager.UpdateUserOid(existingUserId, firebaseUid), Times.Once);
    }

    /// <summary>
    /// Verifies that POST /auth/sign-up returns 500 when Terms & Conditions are not accepted.
    /// </summary>
    [Fact]
    public async Task PostSignUpReturnsInternalServerErrorWhenTermsNotAccepted()
    {
        // Arrange
        var request = new SignUpUserRequest
        {
            Email = "test@example.com",
            Password = "Password123!",
            TermsAccepted = false
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/auth/sign-up", request);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
