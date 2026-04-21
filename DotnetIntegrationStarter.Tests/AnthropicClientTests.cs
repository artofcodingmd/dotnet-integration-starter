using System.Net;
using System.Text;
using System.Text.Json;
using DotnetIntegrationStarter.Api.Configuration;
using DotnetIntegrationStarter.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace DotnetIntegrationStarter.Tests;

public class AnthropicClientTests
{
    private static AnthropicOptions DefaultOptions => new()
    {
        ApiKey = "test-api-key",
        Model = "claude-sonnet-4-6",
        MaxTokens = 1000,
        SystemPrompt = "You are a helpful assistant.",
        BaseUrl = "https://api.anthropic.com/v1/messages",
        ApiVersion = "2023-06-01"
    };

    private static HttpClient CreateMockHttpClient(HttpResponseMessage responseMessage)
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        return new HttpClient(handlerMock.Object);
    }

    [Fact]
    public async Task AskAsync_ReturnsAnswer_WhenResponseIsSuccessful()
    {
        // Arrange
        var anthropicResponse = new
        {
            content = new[]
            {
                new
                {
                    text = "This is a test answer.",
                    type = "text",
                }
            }
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(anthropicResponse),
                Encoding.UTF8,
                "application/json")
        };

        var httpClient = new AnthropicClient(
            CreateMockHttpClient(httpResponse),
            Options.Create(DefaultOptions),
            NullLogger<AnthropicClient>.Instance);

        // Act
        var result = await httpClient.AskAsync("What is the answer?", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("This is a test answer.", result.Answer);
        Assert.Equal("claude-sonnet-4-6", result.Model);
    }

    [Fact]
    public async Task AskAsync_ThrowsException_WhenResponseReturnsError()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);

        var httpClient = new AnthropicClient(
            CreateMockHttpClient(httpResponse),
            Options.Create(DefaultOptions),
            NullLogger<AnthropicClient>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => httpClient.AskAsync("What is the answer?", CancellationToken.None));
    }

    [Fact]
    public async Task AskAsync_IncludesCorrectHeaders_InRequest()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;

        var anthropicResponse = new
        {
            content = new[]
            {
                new
                {
                    text = "The answer.",
                    type = "text",
                }
            }
        };

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(anthropicResponse),
                    Encoding.UTF8,
                    "application/json")
            });

        var httpClient = new AnthropicClient(
            new HttpClient(handlerMock.Object),
            Options.Create(DefaultOptions),
            NullLogger<AnthropicClient>.Instance);

        // Act
        await httpClient.AskAsync("Answer?", CancellationToken.None);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.True(capturedRequest.Headers.Contains("x-api-key"));
        Assert.True(capturedRequest.Headers.Contains("anthropic-version"));
    }
}