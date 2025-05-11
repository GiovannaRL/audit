function Get-StorageName {
    param(
        [ValidateSet("Dev", "Prod")]
        [string]$Environment = "Dev"
    )
    return ($Environment -eq "Dev") ? $script:devStorageAccount : $script:prodStorageAccount;
}

