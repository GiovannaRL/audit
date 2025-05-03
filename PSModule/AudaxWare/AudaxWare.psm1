. ./ResourceGroup/Get-UserResourceGroupName.ps1
. ./Storage/Get-UserStorageName.ps1
. ./AudaxWareVariables.ps1
. ./Install-AudaxWareTools.ps1
. ./Storage/Get-StorageName.ps1
. ./Storage/Get-StorageContainerNames.ps1
. ./Storage/Get-StorageContainerStats.ps1
. ./Storage/Get-StorageEnterpriseStats.ps1
. ./Get-AudaxWareEnterprises.ps1
. ./Get-PublicIP.ps1
. ./New-DevResources.ps1
Export-ModuleMember -Function Install-AudaxWareTools
Export-ModuleMember -Function Start-StorageEmulator
Export-ModuleMember -Function Get-StorageName
Export-ModuleMember -Function Get-StorageContainerNames
Export-ModuleMember -Function Get-AudaxWareEnterprise
Export-ModuleMember -Function Get-PublicIP
Export-ModuleMember -Function Get-StorageContainerStats
Export-ModuleMember -Function Get-StorageEnterpriseStats
Export-ModuleMember -Function Get-UserStorageName
Export-ModuleMember -Function Get-UserDevResourceGroupName
Export-ModuleMember -Function New-DevResources

if ($PSVersionTable.PSVersion.Major -lt 7)
{
    Write-Warning "This module requires Powershell7, make sure you run Install-AudaxWareTools and start Powershell7"
} 
