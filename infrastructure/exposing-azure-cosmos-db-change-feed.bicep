targetScope = 'subscription'

param location string = 'westeurope'

var projectResourceGroupName = 'rg-exposing-azure-cosmos-db-change-feed' 

resource projectResourceGroup 'Microsoft.Resources/resourceGroups@2022-09-01' = {
  name: projectResourceGroupName
  location: location
}

module projectResourceGroupModule 'exposing-azure-cosmos-db-change-feed-rg.bicep' = {
  name: 'exposing-azure-cosmos-db-change-feed-rg'
  scope: projectResourceGroup
  params: {
    location: projectResourceGroup.location
  }
}
