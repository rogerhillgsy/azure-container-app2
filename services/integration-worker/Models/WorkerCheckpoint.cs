namespace IntegrationWorker.Models;

public sealed record WorkerCheckpoint(string? LastProcessedRecordId, DateTimeOffset? UpdatedAt);
