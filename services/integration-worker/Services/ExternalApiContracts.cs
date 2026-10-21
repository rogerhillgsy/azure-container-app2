using IntegrationWorker.Models;

namespace IntegrationWorker.Services;

public enum ExternalApi
{
    Dataverse,
    Webex
}

public interface IExternalApiAuthenticator
{
    ValueTask<string> GetAccessTokenAsync(ExternalApi api, CancellationToken cancellationToken);
}

public interface IDataversePhoneCallService
{
    Task<IReadOnlyList<PhoneCallRecord>> GetPhoneCallsAsync(CancellationToken cancellationToken);
}

public interface IWebexReportingService
{
    Task SubmitReportAsync(
        IReadOnlyList<PhoneCallRecord> records,
        CancellationToken cancellationToken);
}

public interface IPhoneCallSyncWorkflow
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}

public interface ICheckpointService
{
    Task<WorkerCheckpoint?> GetAsync(CancellationToken cancellationToken);
    Task SaveAsync(WorkerCheckpoint checkpoint, CancellationToken cancellationToken);
    Task<bool> CheckHealthAsync(CancellationToken cancellationToken);
}
