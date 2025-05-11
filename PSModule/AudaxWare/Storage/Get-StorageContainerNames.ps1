function Get-StorageContainerNames {
    param(
        [ValidateSet("Dev", "Prod")]
        [string]$Environment = "Dev"
    )
    $storage = Get-StorageName -Environment $Environment
    return az storage container list --account-name $storage --query "[].name" --auth-mode login | ConvertFrom-Json
}
