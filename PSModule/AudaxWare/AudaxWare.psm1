$folders = Get-ChildItem -Path $PSScriptRoot -Directory

foreach ($folder in $folders) {
    #$fullPath = Join-Path $moduleRoot $folder

    if (Test-Path $folder) {
        Get-ChildItem -Path $folder -Filter '*.ps1' -Recurse -File | ForEach-Object {
            Write-Verbose "Loading script: $($_.FullName)"
            . $_.FullName
        }
    }
    else {
        Write-Warning "Folder not found: $fullPath"
    }
}

Export-ModuleMember -Function New-GitBranch
Export-ModuleMember -Function New-GitCommit
Export-ModuleMember -Function New-GitReleaseBranch
Export-ModuleMember -Function Get-GitDev
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
