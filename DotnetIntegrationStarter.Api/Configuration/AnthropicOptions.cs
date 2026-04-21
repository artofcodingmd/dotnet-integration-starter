namespace DotnetIntegrationStarter.Api.Configuration;

/// <summary>
/// Configuration specific to Anthropic Claude API provider.
/// ApiKey must be supplied via environment variable or user secrets.
/// It must never be committed to source control.
/// </summary>
public class AnthropicOptions
{
    public const string SectionName = "Anthropic";
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "claude-sonnet-4-6";
    public int MaxTokens { get; set; } = 1000;
    public string SystemPrompt { get; set; } = string.Empty;
}