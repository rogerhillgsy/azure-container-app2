# Project Plan

**Status**: Awaiting Integration
**Created**: 2026-10-21
**Mode**: NEW

---

## 1. Project Overview

**Goal**: Build a daily scheduled integration worker that processes Dataverse phone call activity records using the related contact fields `contact/fullname`, `contact/mobilephone`, and `contact/emailaddress1`, then reports to a Webex room. The schedule is 02:00 Europe/London. The precise Dataverse entity/query context, record filters/date window, volume/paging behavior, Webex room identifier, and message/report format remain to be defined. The project is designed so that every module is independently testable.

**App Type**: Background worker

**API Login**: No

**Mode**: NEW

**Deployment Plan**: Deploy dev, qa, and prod from GitHub using Bicep. Use federated credentials for external API authentication; repository, branch, region, and release controls remain to be defined.

**External API Authentication**: Federated credentials for Dataverse and Cisco Webex, as specified in the saved requirements. Credential and trust configuration details remain to be defined.

---

## 2. Backend — Azure Functions

| Component | Technology |
|-----------|-----------|
| **Language** | C# |
| **Runtime** | .NET 10 |
| **Package Manager** | dotnet (NuGet) |
| **Test Runner** | xUnit |
| **Mocking Library** | NSubstitute |
| **Test Command** | dotnet test |
| **Orchestration** | docker-compose |

---

## 3. Services Required

| Azure Service | Role in App | Environment Variable | Default Value (Local) | Classification |
|---------------|------------|---------------------|----------------------|----------------|
| Azure Functions | Run the .NET isolated worker on a daily timer; expose a basic health check | `AzureWebJobsStorage` | `UseDevelopmentStorage=true` | Essential |
| Azure Table Storage | Persist worker checkpoints for Dataverse phone call processing; checkpoint progression, deduplication, retention, and idempotency behavior are open decisions | `ConnectionStrings:Storage` | `UseDevelopmentStorage=true` | Essential |
| Application Insights | Capture worker execution, exceptions, and dependency telemetry | `APPLICATIONINSIGHTS_CONNECTION_STRING` | — (not configured locally) | Enhancement |

---

## 4. Prerequisites

### Run

| Tool | Service(s) | Installed | Version |
|------|------------|-----------|---------|
| .NET SDK 10 | integration-worker | ❓ | `dotnet` found; installed SDKs: 5.0.416, 8.0.426, 9.0.313 (10 not detected) |
| Azure Functions Core Tools | integration-worker | ✅ | 4.3.0 |
| Azure CLI | * | ✅ | 2.91.0 |
| Bicep CLI | * | ❓ | Not confirmed as a standalone CLI |
| Git | * | ✅ | 2.51.0.windows.2 |

### Debug

| Tool | Service(s) | Installed | Version |
|------|------------|-----------|---------|
| Docker | integration-worker | ✅ | 28.4.0 |
| Docker Compose | integration-worker | ✅ | 2.39.4-desktop.1 |
| VS Code Azure Functions extension (`ms-azuretools.vscode-azurefunctions`) | integration-worker | ✅ | 1.22.2 |
| VS Code Bicep extension (`ms-azuretools.vscode-bicep`) | * | ✅ | 0.48.1 |

Double-check that all tools marked ❓ are installed and available before proceeding.

---

## 5. Project Structure

```
project-root/
├── .azure/
│   ├── project-plan.md
│   └── environments/
│       ├── dev.bicepparam
│       ├── qa.bicepparam
│       └── prod.bicepparam
├── .github/
│   └── workflows/
│       └── deploy.yml
├── infra/
│   └── main.bicep
├── services/
│   └── integration-worker/
│       ├── host.json
│       ├── local.settings.json
│       ├── IntegrationWorker.csproj
│       ├── Program.cs
│       ├── Functions/
│       │   ├── ProcessPhoneCallsTimer.cs
│       │   └── HealthCheck.cs
│       ├── Models/
│       │   └── PhoneCallRecord.cs
│       ├── Services/
│       │   ├── DataversePhoneCallService.cs
│       │   ├── WebexReportingService.cs
│       │   └── CheckpointService.cs
│       └── tests/
│           └── IntegrationWorker.Tests/
│               ├── IntegrationWorker.Tests.csproj
│               ├── Functions/
│               └── Services/
└── README.md
```

---

## 6. Route Definitions

| # | Method | Path | Description | Request Body | Response Body | Status Codes |
|---|--------|------|-------------|-------------|--------------|-------------|
| 1 | GET | `/api/health` | Basic worker health check | — | `{ status, services }` | 200, 503 |
| 2 | TIMER | Daily at 02:00 (Europe/London) | Process Dataverse phone call activity records using `contact/fullname`, `contact/mobilephone`, and `contact/emailaddress1`, then report to a Webex room. The exact entity/query context, record filters/date window, volume/pagination requirements, precise Webex room identifier, and message/report format remain open. Retry, deduplication/idempotency, and checkpoint retention behavior are also unspecified. | — | — | N/A |

---

## 7. Next Steps

1. Run **azure-project-scaffold** to execute this plan
2. Run **azure-project-integrate** to wire the frontend to live data, smoke-test the backend, and create the migrations
3. Run **azure-debug-plan** → **azure-debug-generate** for Docker emulators and VS Code debugging
4. Run the **azure-deploy** agent when ready; it uses **azure-app-onboard** for architecture, cost estimation, IaC generation, provisioning, and health verification
