using DotnetIntegrationStarter.Api.Configuration;
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

    var app = builder.Build();
    app.UseSerilogRequestLogging();

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