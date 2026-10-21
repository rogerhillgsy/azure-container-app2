@description('Deployment environment. Must match the selected GitHub Environment.')
@allowed([
  'dev'
  'qa'
  'prod'
])
param deploymentEnvironment string

@description('Azure region selected by the project owner.')
param location string

@description('Globally unique Function App name; replace the environment parameter placeholder.')
param functionAppName string

@description('Hosting plan SKU name selected after cost and hosting review (for example, Y1 or EP1).')
param hostingPlanSkuName string

@description('Hosting plan SKU tier matching hostingPlanSkuName.')
param hostingPlanSkuTier string

@description('Operating system selected for the Function App and hosting plan.')
@allowed([
  'Windows'
  'Linux'
  'TODO_SELECT_WINDOWS_OR_LINUX'
])
param operatingSystem string

@description('Storage redundancy SKU selected for the environment.')
param storageSkuName string

@description('Globally unique, lowercase, alphanumeric Storage Account name (3-24 characters).')
param storageAccountName string

@description('Daily NCRONTAB expression in Europe/London. Confirm the run time before deployment.')
param phoneCallsSchedule string

@description('Optional Application Insights connection string. Leave empty to disable this Enhancement.')
@secure()
param applicationInsightsConnectionString string = ''

var checkpointTableName = 'phonecallcheckpoints'
var storageBlobDataOwnerRoleId = 'b7e6dc6d-f1e8-4753-8033-0f276bb0955b'
var storageQueueDataContributorRoleId = '974c5e8b-45b9-4653-ba55-5f855dd0fb88'
var storageTableDataContributorRoleId = '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3'
var functionAppKind = operatingSystem == 'Linux' ? 'functionapp,linux' : 'functionapp'

resource storage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: storageSkuName
  }
  tags: {
    environment: deploymentEnvironment
  }
  properties: {
    allowSharedKeyAccess: false
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
}

resource tableService 'Microsoft.Storage/storageAccounts/tableServices@2023-05-01' = {
  parent: storage
  name: 'default'
}

resource checkpointTable 'Microsoft.Storage/storageAccounts/tableServices/tables@2023-05-01' = {
  parent: tableService
  name: checkpointTableName
}

resource plan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: 'plan-${functionAppName}'
  location: location
  kind: functionAppKind
  sku: {
    name: hostingPlanSkuName
    tier: hostingPlanSkuTier
  }
  properties: {
    reserved: operatingSystem == 'Linux'
  }
  tags: {
    environment: deploymentEnvironment
  }
}

resource functionApp 'Microsoft.Web/sites@2023-12-01' = {
  name: functionAppName
  location: location
  kind: functionAppKind
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    httpsOnly: true
    serverFarmId: plan.id
    siteConfig: {
      netFrameworkVersion: operatingSystem == 'Windows' ? 'v10.0' : null
      linuxFxVersion: operatingSystem == 'Linux' ? 'DOTNET-ISOLATED|10.0' : null
      use32BitWorkerProcess: false
      appSettings: [
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'AZURE_FUNCTIONS_ENVIRONMENT'
          value: 'Production'
        }
        {
          name: 'AzureWebJobsStorage__accountName'
          value: storage.name
        }
        {
          name: 'Storage__TablesEndpoint'
          value: 'https://${storage.name}.table.${az.environment().suffixes.storage}'
        }
        {
          name: 'PhoneCallsSchedule'
          value: phoneCallsSchedule
        }
        {
          name: 'WEBSITE_TIME_ZONE'
          value: 'Europe/London'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: applicationInsightsConnectionString
        }
      ]
    }
  }
  tags: {
    environment: deploymentEnvironment
  }
}

resource storageBlobRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storage.id, functionApp.id, storageBlobDataOwnerRoleId)
  scope: storage
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', storageBlobDataOwnerRoleId)
    principalId: functionApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

resource storageQueueRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storage.id, functionApp.id, storageQueueDataContributorRoleId)
  scope: storage
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', storageQueueDataContributorRoleId)
    principalId: functionApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

resource storageTableRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storage.id, functionApp.id, storageTableDataContributorRoleId)
  scope: storage
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', storageTableDataContributorRoleId)
    principalId: functionApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

output functionAppName string = functionApp.name
output storageAccountName string = storage.name
output checkpointTableName string = checkpointTable.name
