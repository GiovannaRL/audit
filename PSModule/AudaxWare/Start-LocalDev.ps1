function Start-LocalDev{
    param()

    $env:StorageConnectionString = az account show-connection-string `
        -n $script:userStorageAccount -g $script:userResourceGroup
}
