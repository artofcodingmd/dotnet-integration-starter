namespace DotnetIntegrationStarter.Api.Configuration;

/// <summary>
/// Provider-agnostic configuration for the integration client.
/// </summary>
public class IntegrationClientOptions
{
    public const string SectionName = "IntegrationClient";
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetryAttempts { get; set; } = 3;
}