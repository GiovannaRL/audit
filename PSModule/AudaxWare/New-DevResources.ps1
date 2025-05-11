function New-DevResources {
    param(
        [ValidateSet("westus", "brazilsouth")]
        [string]$region
    )

    $group = Get-UserResourceGroupName
    $storageName = Get-UserStorageName
    az deployment sub create `
        --name myDeployment `
        --location $region `
        --template-file "$script:modulePath\bicep\resourceGroup.bicep" `
        --parameters resourceGroupName=$group resourceGroupLocation=$region
    
    az deployment group create `
        --resource-group $group `
        --template-file  "$script:modulePath\bicep\storageAccount.bicep" `
        --mode incremental `
        --parameters storageAccountName=$storageName storageAccountLocation=$region
}
