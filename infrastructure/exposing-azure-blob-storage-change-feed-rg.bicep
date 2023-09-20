targetScope = 'resourceGroup'

param location string = resourceGroup().location

var projectAppServiceName = 'app-exposing-change-feed'
var projectAppServicePlanSku = 'F1'
var projectAppServicePlanName = 'asp-exposing-change-feed'
var projectStorageAccountName = 'st${uniqueString(resourceGroup().id)}'
var projectManagedIdentityName = 'id-exposing-change-feed'
var projectStorageAccountSkuName = 'Standard_LRS'

resource projectManagedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: projectManagedIdentityName
  location: location
}

resource projectStorageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: projectStorageAccountName
  location: location
  sku: {
    name: projectStorageAccountSkuName
  }
  kind: 'StorageV2'
  properties: {
  }

  resource blobService 'blobServices' = {
    name: 'default'
    properties: {
      changeFeed: {
        enabled: true
        retentionInDays: 1
      }
    }
  }
}

resource storageBlobDataContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  name: 'ba92f5b4-2d11-453d-a403-e96b0029c9fe' // Storage Blob Data Contributor
  scope: subscription()
}

resource storageBlobDataContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: projectStorageAccount
  name: guid(projectStorageAccount.id, projectManagedIdentity.id, storageBlobDataContributorRoleDefinition.id)
  properties: {
    roleDefinitionId: storageBlobDataContributorRoleDefinition.id
    principalId: projectManagedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource projectAppServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: projectAppServicePlanName
  location: location
  sku: {
    name: projectAppServicePlanSku
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

resource projectAppService 'Microsoft.Web/sites@2022-09-01' = {
  name: projectAppServiceName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${projectManagedIdentity.id}': {}
    }
  }
  properties: {
    serverFarmId: projectAppServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|7.0'
      appSettings: [
        {
          name: 'AZURE_CLIENT_ID'
          value: projectManagedIdentity.properties.clientId
        }
        {
          name: 'AzureStorageBlobs__ServiceUri'
          value: 'https://${projectStorageAccount.name}.blob.${environment().suffixes.storage}'
        }
      ]
    }
  }
}
