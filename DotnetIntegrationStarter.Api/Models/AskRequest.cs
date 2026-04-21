using System.ComponentModel.DataAnnotations;

namespace DotnetIntegrationStarter.Api.Models;

/// <summary>
/// Request model for the POST /ask endpoint.
/// </summary>
public class AskRequest
{
    [Required]
    [MinLength(1)]
    [MaxLength(2000)]
    public string Prompt { get; set; } = string.Empty;
}