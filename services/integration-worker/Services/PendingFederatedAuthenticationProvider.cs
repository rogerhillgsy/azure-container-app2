namespace IntegrationWorker.Services;

/// <summary>
/// Safe scaffold boundary. Bind the organization-approved Dataverse and Webex federation
/// arrangements here once their issuers, audiences/scopes, and runtime trust are confirmed.
/// GitHub-to-Azure deployment OIDC is configured separately in the workflow.
/// </summary>
public sealed class PendingFederatedAuthenticationProvider : IExternalApiAuthenticator
{
    public ValueTask<string> GetAccessTokenAsync(
        ExternalApi api,
        CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        throw new ExternalApiConfigurationException(
            $"Federated runtime authentication for {api} is a required configuration placeholder. " +
            "No token is issued until the provider trust and API audience/scope are confirmed.");
    }
}
