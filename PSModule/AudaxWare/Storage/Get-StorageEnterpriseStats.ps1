function Get-StorageEnterpriseStats {
    param([int] $domainId)

    $cadblock = Get-StorageContainerStats -containerName "cadblock$domainId"
    $coverpo = Get-StorageContainerStats -containerName "coverpo$domainId"
    $coversheet = Get-StorageContainerStats -containerName "coversheet$domainId"
    $cutsheet = Get-StorageContainerStats -containerName "cutsheet$domainId"
    $document = Get-StorageContainerStats -containerName "document$domainId"
    $fullcutsheet = Get-StorageContainerStats -containerName "fullcutsheet$domainId"
    $photo = Get-StorageContainerStats -containerName "photo$domainId"
    $projectdocuments = Get-StorageContainerStats -containerName "project-documents$domainId"
    return [PSCustomObject]@{
        CadBlocks = $cadblock
        POCovers = $coverpo
        CoverSheets = $coversheet
        CutSheets = $cutsheet
        Documents = $document
        FullCutSheets = $fullcutsheet
        Photos = $photo
        ProjectDocuments = $projectdocuments
    }
}