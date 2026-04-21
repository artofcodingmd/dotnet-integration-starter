using System.Net.Http.Json;
using System.Text.Json.Serialization;
using DotnetIntegrationStarter.Api.Configuration;
using DotnetIntegrationStarter.Api.Models;
using Microsoft.Extensions.Options;

namespace DotnetIntegrationStarter.Api.Services;

/// <summary>
/// Anthropic Claude API implemention of IIntegrationClient.
/// Resilience is handled by the HttpClient pipeline in Program.cs.
/// </summary>
public class AnthropicClient : IIntegrationClient
{
    private readonly HttpClient _httpClient;
    private readonly AnthropicOptions _options;
    private readonly ILogger<AnthropicClient> _logger;

    public AnthropicClient(HttpClient httpClient, IOptions<AnthropicOptions> options, ILogger<AnthropicClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IntegrationResponse> AskAsync(string prompt, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending request to Anthropic API using model {Model}", _options.Model);

        var requestBody = new
        {
            model = _options.Model,
            max_tokens = _options.MaxTokens,
            system = _options.SystemPrompt,
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.BaseUrl);
        request.Headers.Add("x-api-key", _options.ApiKey);
        request.Headers.Add("anthropic-version", _options.ApiVersion);
        request.Content = JsonContent.Create(requestBody);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadFromJsonAsync<AnthropicMessagesResponse>(cancellationToken: cancellationToken);

        var answer = responseBody?.Content?.FirstOrDefault()?.Text ?? string.Empty;

        _logger.LogInformation("Received response from Anthropic API");

        return new IntegrationResponse
        {
            Answer = answer,
            Model = _options.Model,
            Timestamp = DateTimeOffset.UtcNow
        };
    }
}

internal sealed class AnthropicMessagesResponse
{
    [JsonPropertyName("content")]
    public List<AnthropicContentBlock>? Content { get; set; }
}

internal sealed class AnthropicContentBlock
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}