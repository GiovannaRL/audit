function Get-EnterpriseContainerNames {
    params([int]$domainId)
    return @(
        "cadblock$domainId",
        "coverpo$domainId",
        "coversheet$domainId",
        "cutsheet$domainId",
        "document$domainId",
        "fullcutsheet$domainId",
        "photo$domainId",
        "project-documents$domainId"
    )
}