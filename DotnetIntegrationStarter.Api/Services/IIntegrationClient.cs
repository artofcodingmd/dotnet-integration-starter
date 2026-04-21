using DotnetIntegrationStarter.Api.Models;

namespace DotnetIntegrationStarter.Api.Services;

/// <summary>
/// Provider-agnostic contract for AI integration clients.
/// To add a new provider, implement this interface and register the implementation in Program.cs.
/// </summary>
public interface IIntegrationClient
{
    Task<IntegrationResponse> AskAsync(string prompt, CancellationToken cancellationToken = default);
}