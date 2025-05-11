targetScope = 'resourceGroup'

// Parameters for the storage account
param storageAccountName string
param storageAccountLocation string

// Create the storage account within the resource group
resource storageAccount 'Microsoft.Storage/storageAccounts@2021-04-01' = {
  name: storageAccountName
  location: storageAccountLocation
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: false
  }
} 
