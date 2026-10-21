using System.Net.Http.Headers;
using System.Text.Json;
using IntegrationWorker.Models;
using Microsoft.Extensions.Options;

namespace IntegrationWorker.Services;

public sealed class DataversePhoneCallService(
    IHttpClientFactory httpClientFactory,
    IExternalApiAuthenticator authenticator,
    IOptions<DataverseOptions> options) : IDataversePhoneCallService
{
    public async Task<IReadOnlyList<PhoneCallRecord>> GetPhoneCallsAsync(
        CancellationToken cancellationToken)
    {
        var requestUri = options.Value.PhoneCallsRequestUri;
        if (!Uri.TryCreate(requestUri, UriKind.Absolute, out var uri))
        {
            throw new ExternalApiConfigurationException(
                "Dataverse:PhoneCallsRequestUri must be configured with the approved entity, " +
                "fields, and filters before the timer can process records.");
        }

        var accessToken = await authenticator.GetAccessTokenAsync(
            ExternalApi.Dataverse,
            cancellationToken);
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await httpClientFactory.CreateClient(nameof(DataversePhoneCallService))
            .SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(
            responseStream,
            cancellationToken: cancellationToken);

        if (document.RootElement.TryGetProperty("@odata.nextLink", out _))
        {
            throw new ExternalApiConfigurationException(
                "Dataverse returned another page. Pagination and volume behavior must be agreed " +
                "before processing can safely continue.");
        }

        if (!document.RootElement.TryGetProperty("value", out var values) ||
            values.ValueKind != JsonValueKind.Array)
        {
            throw new ExternalApiConfigurationException(
                "Dataverse response did not contain the expected Web API 'value' collection.");
        }

        var records = new List<PhoneCallRecord>();
        foreach (var value in values.EnumerateArray())
        {
            if (!value.TryGetProperty("activityid", out var idElement) ||
                idElement.ValueKind != JsonValueKind.String ||
                string.IsNullOrWhiteSpace(idElement.GetString()))
            {
                throw new ExternalApiConfigurationException(
                    "The configured Dataverse query must include the activityid field.");
            }

            records.Add(new PhoneCallRecord(
                idElement.GetString()!,
                value.GetRawText()));
        }

        return records;
    }
}
