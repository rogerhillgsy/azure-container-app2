using IntegrationWorker.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace IntegrationWorker.Functions;

public sealed class ProcessPhoneCallsTimer(
    IPhoneCallSyncWorkflow workflow,
    ILogger<ProcessPhoneCallsTimer> logger)
{
    [Function(nameof(ProcessPhoneCallsTimer))]
    public async Task Run(
        [TimerTrigger("%PhoneCallsSchedule%")] TimerInfo timerInfo,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Daily phone-call worker triggered at {TriggeredAtUtc}. IsPastDue={IsPastDue}.",
            DateTimeOffset.UtcNow,
            timerInfo.IsPastDue);

        await workflow.ExecuteAsync(cancellationToken);
    }
}
