function Get-PublicIP {
    param()
    $publicIP = (Invoke-WebRequest -uri "http://ifconfig.me/ip").Content
    return $publicIP
}