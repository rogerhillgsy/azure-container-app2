namespace IntegrationWorker.Services;

public sealed class DataverseOptions
{
    public string? PhoneCallsRequestUri { get; set; }
}

public sealed class WebexOptions
{
    // The reporting API operation, endpoint, and payload contract remain intentionally unspecified.
}

public sealed class ExternalApiConfigurationException(string message)
    : InvalidOperationException(message);
