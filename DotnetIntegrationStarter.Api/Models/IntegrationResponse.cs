namespace DotnetIntegrationStarter.Api.Models;

public class IntegrationResponse
{
    public string Answer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}