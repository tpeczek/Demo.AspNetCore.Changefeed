extension graph

targetScope = 'resourceGroup'

param location string = resourceGroup().location
@secure()
param azureClientId string

var projectSqlServerName = 'sql-exposing-change-tracking'
var projectSqlDatabaseName = 'sqldb-exposing-change-tracking'
var projectAppServiceName = 'app-exposing-change-tracking'
var projectAppServicePlanSku = 'F1'
var projectAppServicePlanName = 'asp-exposing-change-tracking'
var projectManagedIdentityName = 'id-exposing-change-tracking'

resource azureAdApplicationServicePrincipal 'Microsoft.Graph/servicePrincipals@v1.0' existing = {
  appId: azureClientId
}

resource projectManagedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: projectManagedIdentityName
  location: location
}

resource projectSqlServer 'Microsoft.Sql/servers@2023-08-01' = {
  name: projectSqlServerName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    administrators: {
      administratorType: 'ActiveDirectory'
      login: azureAdApplicationServicePrincipal.displayName
      sid: azureAdApplicationServicePrincipal.id
      tenantId: tenant().tenantId
      principalType: 'Application'
      azureADOnlyAuthentication: true
    }
  }

  resource projectSqlDatabase 'databases' = {
    name: projectSqlDatabaseName
    location: location
    sku: {
      name: 'Free'
      tier: 'Free'
    }
  }

  resource allowAzureServicesFirewallRule 'firewallRules' = {
    name: 'AllowAzureServicesAndResources'
    properties: {
      endIpAddress: '0.0.0.0'
      startIpAddress: '0.0.0.0'
    }
  }
}

resource msGraphApplicationServicePrincipal 'Microsoft.Graph/servicePrincipals@v1.0' existing = {
  appId: '00000003-0000-0000-c000-000000000000' // Graph Application ID
}

resource directoryReadAllRoleAssignment 'Microsoft.Graph/appRoleAssignedTo@v1.0' = {
    appRoleId: (filter(msGraphApplicationServicePrincipal.appRoles, appRole => appRole.value == 'Directory.Read.All')[0]).id
    principalId: projectSqlServer.identity.principalId
    resourceId: msGraphApplicationServicePrincipal.id
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
          value: 'AzureSqlDatabase'
        }
        {
          name: 'AzureSqlDatabase__ConnectionString'
          value: 'Server=${projectSqlServer.properties.fullyQualifiedDomainName};Authentication=Active Directory Managed Identity; Encrypt=True;User Id=${projectManagedIdentity.properties.clientId}; Database=${projectSqlDatabaseName}'
        }
      ]
    }
  }
}

output sqlServerFqdn string = projectSqlServer.properties.fullyQualifiedDomainName
output sqlDatabaseName string = projectSqlDatabaseName
output managedIdentityName string = projectManagedIdentityName
