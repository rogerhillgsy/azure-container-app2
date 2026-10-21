using IntegrationWorker.Functions;
using IntegrationWorker.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace IntegrationWorker.Tests.Functions;

public sealed class HealthCheckTests
{
    [Fact]
    public async Task Run_ReturnsHealthy_WhenCheckpointStorageIsReachable()
    {
        var checkpoints = Substitute.For<ICheckpointService>();
        checkpoints.CheckHealthAsync(Arg.Any<CancellationToken>()).Returns(true);
        var function = new HealthCheck(checkpoints, Substitute.For<ILogger<HealthCheck>>());

        var result = await function.Run(
            Substitute.For<HttpRequest>(),
            CancellationToken.None);

        Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.Ok<HealthCheck.HealthResponse>>(result);
    }

    [Fact]
    public async Task Run_ReturnsServiceUnavailable_WhenCheckpointStorageIsUnreachable()
    {
        var checkpoints = Substitute.For<ICheckpointService>();
        checkpoints.CheckHealthAsync(Arg.Any<CancellationToken>()).Returns(false);
        var function = new HealthCheck(checkpoints, Substitute.For<ILogger<HealthCheck>>());

        var result = await function.Run(
            Substitute.For<HttpRequest>(),
            CancellationToken.None);

        var jsonResult = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.JsonHttpResult<HealthCheck.HealthResponse>>(result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, jsonResult.StatusCode);
        Assert.Equal("unhealthy", jsonResult.Value?.Status);
    }
}
