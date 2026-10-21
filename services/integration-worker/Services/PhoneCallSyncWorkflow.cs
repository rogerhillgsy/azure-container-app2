using Microsoft.Extensions.Logging;

namespace IntegrationWorker.Services;

public sealed class PhoneCallSyncWorkflow(
    IDataversePhoneCallService dataverse,
    IWebexReportingService webex,
    ILogger<PhoneCallSyncWorkflow> logger) : IPhoneCallSyncWorkflow
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        // No record filters, reporting payload, pagination policy, or checkpoint/idempotency
        // semantics were approved. Do not send partial or speculative data to Webex.
        var records = await dataverse.GetPhoneCallsAsync(cancellationToken);
        await webex.SubmitReportAsync(records, cancellationToken);

        logger.LogInformation(
            "Webex accepted a report for {RecordCount} phone-call records.",
            records.Count);
    }
}
