using IntegrationWorker.Models;
using Microsoft.Extensions.Options;

namespace IntegrationWorker.Services;

public sealed class WebexReportingService(
    IExternalApiAuthenticator authenticator,
    IOptions<WebexOptions> options) : IWebexReportingService
{
    public Task SubmitReportAsync(
        IReadOnlyList<PhoneCallRecord> records,
        CancellationToken cancellationToken)
    {
        _ = authenticator;
        _ = options;
        _ = records;
        _ = cancellationToken;
        throw new ExternalApiConfigurationException(
            "The Webex reporting operation, destination, request schema, and federation audience " +
            "must be confirmed before any report can be sent.");
    }
}
