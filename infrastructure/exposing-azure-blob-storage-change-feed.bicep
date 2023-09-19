targetScope = 'subscription'

param location string = 'westeurope'

var projectResourceGroupName = 'rg-exposing-azure-blob-storage-change-feed' 

resource projectResourceGroup 'Microsoft.Resources/resourceGroups@2022-09-01' = {
  name: projectResourceGroupName
  location: location
}

module projectResourceGroupModule 'exposing-azure-blob-storage-change-feed-rg.bicep' = {
  name: 'exposing-azure-blob-storage-change-feed-rg'
  scope: projectResourceGroup
  params: {
    location: projectResourceGroup.location
  }
}
