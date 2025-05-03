$key = (Get-Item 'HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager\Environment')
$path = $key.GetValue('PSModulePath','','DoNotExpandEnvironmentNames')
# Removes the current path if it is already in the path
$currentPaths = $path -Split ";" | Where-Object {$_ -ne $PSScriptRoot}
$newPath = $currentPaths -Join ";"
# Adds the current path to the module loading
$newPath += ";" + $PSScriptRoot
Write-Host "Adding $PSScriptRoot to env:PSModulePath"
Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager\Environment' -Name 'PSModulePath' -Value $newPath -Type ExpandString -Force

