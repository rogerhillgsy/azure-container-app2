namespace IntegrationWorker.Models;

/// <summary>
/// Keeps the upstream activity payload intact until the required fields and filters are confirmed.
/// </summary>
public sealed record PhoneCallRecord(string Id, string RawPayload);
