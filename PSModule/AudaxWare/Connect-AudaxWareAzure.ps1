function Connect-AudaxWareAzure {
    param()
    $subscriptionId = "97d3b2ca-1fbd-421b-b76b-f9f2662c76f8"
    
    $account = az account show | ConvertFrom-Json

    if (($null -ne $account) -and ($account.id -eq $subscriptionId)) {
        Write-Host "Already connected"
        return;
    }

    az config set core.login_experience_v2=off
    az login -t "196dfb84-d6bd-4c52-9214-c31f66d3f410"
    az account set --subscription $subscriptionId
}
