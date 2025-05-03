function Get-UserStorageName {
    param()
    $group = Get-UserResourceGroupName

    # Convert the username to lowercase and remove any non-alphanumeric characters
    $cleanUserName = $group.ToLower() -replace '[^a-z0-9]', ''
    
    # Generate a hash of the cleaned username
    $hash = [System.BitConverter]::ToString([System.Security.Cryptography.MD5]::Create().ComputeHash([System.Text.Encoding]::UTF8.GetBytes($cleanUserName))).Replace("-", "").Substring(0, 8)
    
    # Combine the cleaned username and hash to form the storage account name
    $storageAccountName = "$cleanUserName$hash"
    
    # Ensure the storage account name is within the valid length (3-24 characters)
    if ($storageAccountName.Length -gt 24) {
        $storageAccountName = $storageAccountName.Substring(0, 24)
    }
    
    return $storageAccountName.ToLower()
}
