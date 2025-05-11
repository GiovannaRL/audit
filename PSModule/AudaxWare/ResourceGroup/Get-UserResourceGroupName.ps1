function Get-UserResourceGroupName {
    param()

    $user = az ad signed-in-user show --query userPrincipalName --output tsv
    $emailSuffix = $user.IndexOf('_audaxware');
    if ($null -eq $emailSuffix) {
        Write-Error "If you are an AudaxWare developer, you must login with your audaxware e-mail"
        return;
    }

    $rgName = $user.Substring(0, $emailSuffix)
    return $rgName.Replace('.', '-');
}
