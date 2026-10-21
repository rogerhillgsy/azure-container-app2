# Integration hand-off

## Backend

- Project: `services/integration-worker/`
- Build: `dotnet build IntegrationWorker.sln`
- Tests: `dotnet test IntegrationWorker.sln`
- Local run: `func start --project services/integration-worker`
- Port: `7071`
- Health: `GET /api/health`
- Trigger: timer on `PhoneCallsSchedule`, daily at 02:00 in `Europe/London` (`0 0 2 * * *`); this is the user-confirmed schedule.

## Frontend

- None (background worker only); no frontend/API client seam.

## API / trigger inventory

- `GET /api/health` — 200 when worker and checkpoint storage are healthy; 503 otherwise.
- Timer — daily phone-call processing at 02:00 Europe/London; the Dataverse entity/query context and record filters/date window remain open.

## Persistence

- Type: Azure Table Storage (`phonecallcheckpoints`); local emulator setting is in ignored `services/integration-worker/local.settings.json`.
- Production endpoint: `Storage:TablesEndpoint`, authenticated by `DefaultAzureCredential` / Function App managed identity.
- Migration tool/directory: not applicable to Table Storage; table resource is declared in `infra/main.bicep`.
- Seed data: **NO seed data**.
- User-confirmed Table Storage checkpoints; checkpoint progression, retention, deduplication, and idempotency remain to be agreed.

## Types and services

- Types: `services/integration-worker/Models/`; no separate shared package or import alias.
- Essential: Azure Functions runtime; Azure Table Storage checkpoint.
- Enhancement: Application Insights (optional).
- External integration clients: `Services/DataversePhoneCallService.cs`, `Services/WebexReportingService.cs`; federated auth boundary: `Services/PendingFederatedAuthenticationProvider.cs`.

## Required decisions before live processing

- Dataverse entity/query context, filters/date window, paging and volume. Selected related contact fields are `contact/fullname`, `contact/mobilephone`, and `contact/emailaddress1`.
- Cisco Webex reporting operation, exact room identifier, message/report format, and failure/retry behavior. The destination type is a Webex room.
- Runtime federation issuer/trust and API audience/scope for each external API. GitHub-to-Azure deployment OIDC is separate.
- Checkpoint progression/retention and deduplication/idempotency policy. The approved daily run time is 02:00 Europe/London.
- Azure region, plan SKU, globally unique resource names, repository/branch/release controls, and production approvals.

Scaffold validation must not invoke external APIs. These unresolved items are retained as configuration blockers; no integration or deployment was performed as part of scaffolding.
