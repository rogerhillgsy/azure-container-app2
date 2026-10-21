using Azure.Data.Tables;
using Azure.Identity;
using IntegrationWorker.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);
builder.ConfigureFunctionsWebApplication();

var configuration = builder.Configuration;
var isDevelopment = string.Equals(
    configuration["AZURE_FUNCTIONS_ENVIRONMENT"],
    "Development",
    StringComparison.Ordinal);

builder.Services.AddHttpClient();
builder.Services.Configure<DataverseOptions>(configuration.GetSection("Dataverse"));
builder.Services.Configure<WebexOptions>(configuration.GetSection("Webex"));

builder.Services.AddSingleton<TableServiceClient>(_ =>
{
    if (isDevelopment)
    {
        var connectionString = configuration.GetConnectionString("Storage");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:Storage is required in explicit Development mode.");
        }

        return new TableServiceClient(connectionString);
    }

    var tablesEndpoint = configuration["Storage:TablesEndpoint"];
    if (!Uri.TryCreate(tablesEndpoint, UriKind.Absolute, out var endpoint))
    {
        throw new InvalidOperationException(
            "Storage:TablesEndpoint must be an absolute Azure Table Storage endpoint outside Development.");
    }

    return new TableServiceClient(endpoint, new DefaultAzureCredential());
});

builder.Services.AddSingleton<ICheckpointService>(serviceProvider =>
{
    var tableService = serviceProvider.GetRequiredService<TableServiceClient>();
    return new CheckpointService(
        tableService,
        tableService.GetTableClient(CheckpointService.TableName));
});

builder.Services.AddSingleton<IExternalApiAuthenticator, PendingFederatedAuthenticationProvider>();
builder.Services.AddTransient<IDataversePhoneCallService, DataversePhoneCallService>();
builder.Services.AddTransient<IWebexReportingService, WebexReportingService>();
builder.Services.AddTransient<IPhoneCallSyncWorkflow, PhoneCallSyncWorkflow>();

// Application Insights is an Enhancement: it is enabled only when configured.
if (!string.IsNullOrWhiteSpace(configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
{
    builder.Services.AddApplicationInsightsTelemetryWorkerService();
    builder.Services.ConfigureFunctionsApplicationInsights();
}

await builder.Build().RunAsync();
