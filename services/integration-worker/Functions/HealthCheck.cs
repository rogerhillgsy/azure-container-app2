using IntegrationWorker.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IntegrationWorker.Functions;

public sealed class HealthCheck(
    ICheckpointService checkpointService,
    ILogger<HealthCheck> logger)
{
    [Function(nameof(HealthCheck))]
    public async Task<IResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequest request,
        CancellationToken cancellationToken)
    {
        _ = request;
        bool checkpointStorageHealthy;

        try
        {
            checkpointStorageHealthy = await checkpointService.CheckHealthAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Checkpoint storage health probe failed.");
            checkpointStorageHealthy = false;
        }

        var services = new Dictionary<string, string>
        {
            ["worker"] = "healthy",
            ["checkpointStorage"] = checkpointStorageHealthy ? "healthy" : "unhealthy"
        };
        var healthy = checkpointStorageHealthy;
        var response = new HealthResponse(healthy ? "healthy" : "unhealthy", services);

        return healthy
            ? Results.Ok(response)
            : Results.Json(response, statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    public sealed record HealthResponse(string Status, IReadOnlyDictionary<string, string> Services);
}
