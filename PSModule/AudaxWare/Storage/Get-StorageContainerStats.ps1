function Get-StorageContainerStats {
    param(
        [Parameter(Mandatory = $true)]
        [string] $ContainerName,
        [ValidateSet("Dev", "Prod")]
        [string]$Environment = "Dev"
    )

    $size = 0
    $count = 0
    $storage = Get-StorageName -Environment $Environment

    do {
        if ($null -ne $nextMarker) {
            $blobs = az storage blob list --account-name $storage --show-next-marker --container-name $containerName --marker $nextMarker  --auth-mode login | ConvertFrom-Json
        }
        else {
            $blobs = az storage blob list --account-name $storage --show-next-marker --container-name $containerName  --auth-mode login | ConvertFrom-Json
        }

        if ($null -eq $blobs) {
            break;
        }

        $nextMarker = $null;
        if ($blobs.Count -eq 5001) {
            $nextMarker = $blobs[5000].nextMarker
            $count += 5000
        }
        else {
            $nextMarker = $null
            $count += $blobs.Count
        }

        foreach($blob in $blobs) {
            if ($null -eq $blob.nextMarker) {
                $size += $blob.properties.contentLength
            }
        }

    } while ($null -ne $nextMarker)

    return [PSCustomObject]@{
        Size = [string]([int]($size / (1024*1024))) + "Mb"
        Count = $count
    }
}
