using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using ModularNet.Business.Interfaces;
using ModularNet.Domain.Requests;
using ModularNet.IntegrationTests.Infrastructure;
using Moq;
using Xunit;

namespace ModularNet.IntegrationTests.Tests;

/// <summary>
/// Demonstrates integration testing for HTTP POST endpoints accepting JSON request bodies.
/// Serves as a reference example for template users illustrating:
/// 1. Sending POST requests with JSON payloads via <see cref="HttpClientJsonExtensions.PostAsJsonAsync{T}"/>.
/// 2. Mocking business managers via <see cref="TestHostWebHostBuilderExtensions.ConfigureTestServices"/>.
/// 3. Asserting response status codes and deserialized response values.
/// </summary>
[Collection("IntegrationTests")]
public class EmailVerifierEndpointTests
{
    private readonly ModularNetWebApplicationFactory _factory;

    public EmailVerifierEndpointTests(ModularNetWebApplicationFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Verifies that POST /emailverifier/verify returns 200 OK and true when verification succeeds.
    /// </summary>
    [Fact]
    public async Task PostVerifyEmailReturnsOkWhenVerificationSucceeds()
    {
        // Arrange
        const string verificationCode = "dGVzdC12ZXJpZmljYXRpb24tY29kZQ==";
        var emailVerifierManagerMock = new Mock<IEmailVerifierManager>();
        emailVerifierManagerMock
            .Setup(manager => manager.VerifyEmail(verificationCode))
            .ReturnsAsync(true);

        using var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped(_ => emailVerifierManagerMock.Object);
            });
        }).CreateClient();

        var request = new VerifyEmailRequest
        {
            EmailVerificationCode = verificationCode
        };

        // Act
        var response = await client.PostAsJsonAsync("/emailverifier/verify", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var isVerified = await response.Content.ReadFromJsonAsync<bool>();
        Assert.True(isVerified);
        emailVerifierManagerMock.Verify(manager => manager.VerifyEmail(verificationCode), Times.Once);
    }

    /// <summary>
    /// Verifies that POST /emailverifier/verify returns 500 Internal Server Error when verification fails with an exception.
    /// </summary>
    [Fact]
    public async Task PostVerifyEmailReturnsInternalServerErrorWhenVerificationThrows()
    {
        // Arrange
        const string verificationCode = "invalid-code";
        var emailVerifierManagerMock = new Mock<IEmailVerifierManager>();
        emailVerifierManagerMock
            .Setup(manager => manager.VerifyEmail(verificationCode))
            .ThrowsAsync(new Exception("Invalid verification token"));

        using var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped(_ => emailVerifierManagerMock.Object);
            });
        }).CreateClient();

        var request = new VerifyEmailRequest
        {
            EmailVerificationCode = verificationCode
        };

        // Act
        var response = await client.PostAsJsonAsync("/emailverifier/verify", request);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        emailVerifierManagerMock.Verify(manager => manager.VerifyEmail(verificationCode), Times.Once);
    }
}
