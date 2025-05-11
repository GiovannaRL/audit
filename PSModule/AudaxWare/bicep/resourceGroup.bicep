// Define the target scope as the subscription level
targetScope = 'subscription'

// Parameters for the resource group
param resourceGroupName string
param resourceGroupLocation string

// Create the resource group
resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: resourceGroupName
  location: resourceGroupLocation
}
