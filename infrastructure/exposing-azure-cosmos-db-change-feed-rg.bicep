targetScope = 'resourceGroup'

param location string = resourceGroup().location

var projectAppServiceName = 'app-exposing-change-feed'
var projectAppServicePlanSku = 'F1'
var projectCosmosAccountName = 'cosmos-${uniqueString(resourceGroup().id)}'
var projectAppServicePlanName = 'asp-exposing-change-feed'
var projectManagedIdentityName = 'id-exposing-change-feed'

var changeFeedDatabaseName = 'Demo_AspNetCore_Changefeed_CosmosDB'
var changeFeedContainerName = 'ThreadStats'

resource projectManagedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: projectManagedIdentityName
  location: location
}

resource projectCosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2023-04-15' = {
  name: projectCosmosAccountName
  location: location
  properties: {
    enableFreeTier: true
    databaseAccountOfferType: 'Standard'
    locations: [
      {
        locationName: location
      }
    ]
  }

  resource contributorRoleDefinition 'sqlRoleDefinitions' = {
    name: guid('sql-role-definition-custom-contributor', projectCosmosAccount.id)
    properties: {
      roleName: 'Cosmos DB Custom Data Contributor'
      type: 'CustomRole'
      assignableScopes: [
        projectCosmosAccount.id
      ]
      permissions: [
        {
          dataActions: [
            'Microsoft.DocumentDB/databaseAccounts/readMetadata'
            'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/*'
            'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/items/*'
          ] 
        }
      ]
    }
  }

  resource contributorRoleAssignment 'sqlRoleAssignments' = {
    name: guid(projectCosmosAccount.id, projectManagedIdentity.id, contributorRoleDefinition.id)
    properties: {
      roleDefinitionId: contributorRoleDefinition.id
      principalId: projectManagedIdentity.properties.principalId
      scope: projectCosmosAccount.id
    }
  }

  resource changeFeedDatabase 'sqlDatabases' = {
    name: changeFeedDatabaseName
    properties: {
      resource: {
        id: changeFeedDatabaseName
      }
      options: {
        throughput: 400
      }
    }

    resource changeFeedContainer 'containers' = {
      name: changeFeedContainerName
      properties: {
        resource: {
          id: changeFeedContainerName
          partitionKey: {
            paths: [
              '/partionKey'
            ]
            kind: 'Hash'
          }
          indexingPolicy: {
            automatic: false
            indexingMode: 'none'
          }
        }
      }
    }
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
      linuxFxVersion: 'DOTNETCORE|8.0'
      appSettings: [
        {
          name: 'AZURE_CLIENT_ID'
          value: projectManagedIdentity.properties.clientId
        }
        {
          name: 'ChangefeedService'
          value: 'AzureCosmos'
        }
        {
          name: 'AzureCosmos__DocumentEndpoint'
          value: projectCosmosAccount.properties.documentEndpoint
        }
      ]
    }
  }
}
