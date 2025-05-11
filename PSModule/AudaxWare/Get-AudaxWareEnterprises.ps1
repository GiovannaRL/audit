
function Get-AudaxWareEnterprises {
    param()
    function Get-EnterpriseId {
        param([string] $containerName) 
        $firstNumberIndex = [regex]::Match($containerName, '\d').Index
        return $firstNumberIndex -le 0 ? 1 : [int]$containerName.Substring($firstNumberIndex);
    }
    return Get-StorageContainerNames | ForEach-Object { Get-EnterpriseId $_ } | Select-Object -Unique
}