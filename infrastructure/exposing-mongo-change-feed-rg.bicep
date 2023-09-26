targetScope = 'resourceGroup'

param location string = resourceGroup().location

var projectAppServiceName = 'app-exposing-change-feed'
var projectAppServicePlanSku = 'F1'
var projectAppServicePlanName = 'asp-exposing-change-feed'
var projectContainerInstanceName = 'ci-mongo-community'
var projectContainerDnsNameLabel = 'mongo-community-${uniqueString(resourceGroup().id)}'

var mongoCommunityImageName = 'mongodb-community-server'
var mongoCommunityImageTag = '7.0.1-ubuntu2204'

resource projectContainerInstance 'Microsoft.ContainerInstance/containerGroups@2023-05-01' = {
  name: projectContainerInstanceName
  location: location
  properties: {
    sku: 'Standard'
    osType: 'Linux'
    ipAddress: {
      type: 'Public'
      ports: [
        { 
          port: 27017
          protocol: 'TCP'
        }
      ]
      dnsNameLabel: projectContainerDnsNameLabel
    }
    containers: [
      {
        name: mongoCommunityImageName
        properties: {
          image: 'mongodb/${mongoCommunityImageName}:${mongoCommunityImageTag}'
          command: ['mongod', '--replSet', 'rs0', '--bind_ip_all']
          ports: [
            { 
              port: 27017
              protocol: 'TCP'
            }
          ]
          resources: {
            requests: {
              cpu: 2
              memoryInGB: 1
            }
          }
        }
      }
    ]
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
  properties: {
    serverFarmId: projectAppServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|7.0'
      appSettings: [
        {
          name: 'ChangefeedService'
          value: 'Mongo'
        }
        {
          name: 'Mongo__ConnectionString'
          value: 'mongodb://${projectContainerDnsNameLabel}.westeurope.azurecontainer.io:27017'
        }
      ]
    }
  }
}
