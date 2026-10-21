# Dataverse–Webex integration worker

An Azure Functions v4, .NET 10 isolated worker scaffold for a daily Dataverse phone-call integration with Cisco Webex. It includes a basic health endpoint, Azure Table Storage checkpoint persistence, environment-specific Bicep parameter files, and a manually dispatched GitHub Actions deployment workflow using Azure OIDC.

## Local development

Prerequisites: .NET SDK 10, Azure Functions Core Tools v4, and Azurite (for local `AzureWebJobsStorage` and Table Storage).

```text
dotnet restore IntegrationWorker.sln
dotnet build IntegrationWorker.sln
dotnet test IntegrationWorker.sln
func start --project services/integration-worker
```

The local settings file is ignored by Git. `PhoneCallsSchedule` is intentionally an unmistakable placeholder, not a selected run time. Replace it only after confirming the daily run time and NCRONTAB expression in the Europe/London time zone. The host cannot start the timer with the placeholder value.

Health endpoint: `GET http://localhost:7071/api/health`.

## Azure deployment scaffold

The GitHub workflow is `workflow_dispatch` only and offers `dev`, `qa`, and `prod`. Configure the corresponding GitHub Environments and the following environment-level **variables** (not credential values):

- `AZURE_CLIENT_ID`
- `AZURE_TENANT_ID`
- `AZURE_SUBSCRIPTION_ID`
- `AZURE_RESOURCE_GROUP` (pre-existing resource group)

The Azure federated credential must trust the actual GitHub repository and GitHub Environment subject. Repository name, branch/release policy, subscription, and resource-group values remain unconfirmed. Configure protected approvals for production in GitHub; this scaffold does not guess them.

Before dispatching, replace all `TODO_...` values in `.azure/environments/{dev,qa,prod}.bicepparam`. Select an Azure region and compatible hosting plan SKU/OS, globally unique Function App/Storage Account names, storage redundancy, and the approved local-time schedule. Verify the selected OS/SKU supports the Europe/London timer time-zone setting. The parameter files deliberately do not deploy successfully until those choices are supplied.

The Bicep template creates the Function App, system-assigned identity, Storage Account/Table checkpoint resource, and data-plane role assignments. The production Table client uses `DefaultAzureCredential`; local emulator selection occurs only when `AZURE_FUNCTIONS_ENVIRONMENT` is exactly `Development`. The optional Application Insights connection string is not required.

## Explicit integration blockers (not fabricated)

The following saved requirements are not specific enough to safely process or report records. The timer is scaffolded, but the API methods fail explicitly until these are configured:

- Dataverse entity selection, fields, filter/query, and pagination/volume policy (`Dataverse:PhoneCallsRequestUri`).
- Webex reporting operation, destination, request/payload contract, and handling of failures/retries.
- Supported Dataverse and Webex federated token issuers, audiences/scopes, trust setup, and runtime credential flow. GitHub-to-Azure deployment OIDC is separate and does not provide API tokens.
- Checkpoint progression, deduplication/idempotency, and retry semantics.
- Confirmed daily run time and schedule.
- Azure region, hosting plan SKU/cost, unique resource names, GitHub repository/branch controls, and deployment approvals.

The `PendingFederatedAuthenticationProvider` and `WebexReportingService` deliberately fail closed rather than inventing authentication or sending an assumed report. No external API calls are made by scaffold validation.

## API and service inventory

- `GET /api/health` — anonymous health response; returns `200` when checkpoint storage is reachable, otherwise `503`.
- Daily timer — `Europe/London`; schedule supplied through `PhoneCallsSchedule` and remains a placeholder.
- Essential: Azure Functions runtime; Azure Table Storage for checkpoints.
- Enhancement: Application Insights telemetry (optional).

## Integration hand-off

See `.azure/integration-plan.md` for commands, endpoint inventory, service classification, database/migration notes, and unresolved requirements. No schema migrations or seed data are included; Azure Table Storage is used, and the checkpoint table is provisioned through Bicep.
