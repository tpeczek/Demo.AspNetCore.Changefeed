targetScope = 'subscription'

param location string = 'swedencentral'
@secure()
param azureClientId string

var projectResourceGroupName = 'rg-exposing-azure-sql-database-change-tracking' 

resource projectResourceGroup 'Microsoft.Resources/resourceGroups@2022-09-01' = {
  name: projectResourceGroupName
  location: location
}

module projectResourceGroupModule 'exposing-azure-sql-database-change-tracking-rg.bicep' = {
  name: 'exposing-azure-sql-database-change-tracking-rg'
  scope: projectResourceGroup
  params: {
    location: projectResourceGroup.location
    azureClientId: azureClientId
  }
}

output sqlServerFqdn string = projectResourceGroupModule.outputs.sqlServerFqdn
output sqlDatabaseName string = projectResourceGroupModule.outputs.sqlDatabaseName
output managedIdentityName string = projectResourceGroupModule.outputs.managedIdentityName
