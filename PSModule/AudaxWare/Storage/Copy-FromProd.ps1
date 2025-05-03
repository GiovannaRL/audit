function Copy-FromProd {
    param(
        [ValidateSet("User", "Dev")]
        [string]$Destination = "User",
        [int] $domainId
    )
    $containers = Get-EnterpriseContainerNames

    if ($Destination -eq "Dev") {
        $destAccount = "audaxwaredev"
    }
    else {
        $destAccount = $script:userStorageAccount
    }

    foreach ($container in $containers) {
        Write-Host "Copying container $container"
        azcopy cp "https://audaxware1.blob.core.windows.net/$container" "https://$destAccount.blob.core.windows.net/$container" --dry-run
    }
}