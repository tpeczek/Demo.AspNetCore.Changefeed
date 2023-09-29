targetScope = 'resourceGroup'

param location string = resourceGroup().location

var projectAppServiceName = 'app-exposing-change-feed'
var projectAppServicePlanSku = 'F1'
var projectAppServicePlanName = 'asp-exposing-change-feed'
var projectContainerInstanceName = 'ci-rethinkdb'
var projectContainerDnsNameLabel = 'rethinkdb-${uniqueString(resourceGroup().id)}'

var rethinkDbImageName = 'rethinkdb'
var rethinkDbImageTag = '2.4.2'

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
          port: 28015
          protocol: 'TCP'
        }
        { 
          port: 8080
          protocol: 'TCP'
        }
      ]
      dnsNameLabel: projectContainerDnsNameLabel
    }
    containers: [
      {
        name: rethinkDbImageName
        properties: {
          image: '${rethinkDbImageName}:${rethinkDbImageTag}'
          ports: [
            // Client driver
            { 
              port: 28015
              protocol: 'TCP'
            }
            // Administrative HTTP
            { 
              port: 8080
              protocol: 'TCP'
            }
          ]
          resources: {
            requests: {
              cpu: 1
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
          value: 'RethinkDb'
        }
        {
          name: 'RethinkDb__Hostname'
          value: '${projectContainerDnsNameLabel}.${location}.azurecontainer.io'
        }
      ]
    }
  }
}
