targetScope = 'subscription'

param location string = 'westeurope'

var projectResourceGroupName = 'rg-exposing-rethinkdb-change-feed' 

resource projectResourceGroup 'Microsoft.Resources/resourceGroups@2022-09-01' = {
  name: projectResourceGroupName
  location: location
}

module projectResourceGroupModule 'exposing-rethinkdb-change-feed-rg.bicep' = {
  name: 'exposing-rethinkdb-change-feed-rg'
  scope: projectResourceGroup
  params: {
    location: projectResourceGroup.location
  }
}
