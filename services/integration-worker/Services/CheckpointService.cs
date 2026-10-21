using Azure;
using Azure.Data.Tables;
using IntegrationWorker.Models;

namespace IntegrationWorker.Services;

public sealed class CheckpointService(
    TableServiceClient tableServiceClient,
    TableClient tableClient) : ICheckpointService
{
    public const string TableName = "phonecallcheckpoints";
    private const string PartitionKey = "phoneCalls";
    private const string RowKey = "worker";

    public async Task<WorkerCheckpoint?> GetAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await tableClient.GetEntityAsync<TableEntity>(
                PartitionKey,
                RowKey,
                cancellationToken: cancellationToken);
            var entity = response.Value;

            return new WorkerCheckpoint(
                entity.TryGetValue("LastProcessedRecordId", out var id) ? id?.ToString() : null,
                entity.TryGetValue("UpdatedAt", out var updatedAt) && updatedAt is DateTimeOffset timestamp
                    ? timestamp
                    : null);
        }
        catch (RequestFailedException exception) when (exception.Status == 404)
        {
            return null;
        }
    }

    public async Task SaveAsync(
        WorkerCheckpoint checkpoint,
        CancellationToken cancellationToken)
    {
        var entity = new TableEntity(PartitionKey, RowKey)
        {
            ["LastProcessedRecordId"] = checkpoint.LastProcessedRecordId,
            ["UpdatedAt"] = checkpoint.UpdatedAt ?? DateTimeOffset.UtcNow
        };

        await tableClient.UpsertEntityAsync(
            entity,
            TableUpdateMode.Replace,
            cancellationToken);
    }

    public async Task<bool> CheckHealthAsync(CancellationToken cancellationToken)
    {
        _ = await tableServiceClient.GetPropertiesAsync(cancellationToken);
        return true;
    }
}
