using System.ComponentModel.DataAnnotations;
using System.Threading.RateLimiting;
using DotnetIntegrationStarter.Api.Configuration;
using DotnetIntegrationStarter.Api.Models;
using DotnetIntegrationStarter.Api.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((hostingContext, services, configuration) => configuration
        .ReadFrom.Configuration(hostingContext.Configuration)
        .ReadFrom.Services(services)
        .WriteTo.Console());

    builder.Services.Configure<IntegrationClientOptions>(builder.Configuration.GetSection(IntegrationClientOptions.SectionName));
    builder.Services.Configure<AnthropicOptions>(builder.Configuration.GetSection(AnthropicOptions.SectionName));

    builder.Services.AddHttpClient<IIntegrationClient, AnthropicClient>().AddStandardResilienceHandler();

    builder.Services.AddEndpointsApiExplorer();

    // Rate limiting: 10 requests per minute per IP
    builder.Services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                }));
        options.RejectionStatusCode = 429; // Too Many Requests
    });

    var app = builder.Build();
    app.UseSerilogRequestLogging();
    app.UseRateLimiter();

    // POST /ask endpoint
    app.MapPost("/ask", async (AskRequest request, IIntegrationClient integrationClient, ILogger<Program> logger, CancellationToken cancellationToken) =>
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(request);
        if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
        {
            var errors = validationResults.Select(vr => vr.ErrorMessage);
            return Results.BadRequest(new { Errors = errors });
        }

        try
        {
            logger.LogInformation("Processing request of length {Length}", request.Prompt.Length);

            var response = await integrationClient.AskAsync(request.Prompt, cancellationToken);
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process request");
            return Results.Problem("An error occurred while processing your request.");
        }
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}