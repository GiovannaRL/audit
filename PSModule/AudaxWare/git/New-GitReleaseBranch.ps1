function New-GitReleaseBranch {
<#
.SYNOPSIS
Creates a release branch from the 'main' branch and merges updates from 'dev'.

.DESCRIPTION
This function automates the creation of a timestamped release branch. It:
1. Checks out and updates the 'main' branch.
2. Creates a new branch named '<user>/Release/yyyyMMdd'.
3. Merges the latest 'dev' changes into the new release branch using a "theirs" strategy.
4. Pushes the new branch to the remote and sets its upstream reference.

.PARAMETER user
Optional username to prefix the branch name. Defaults to the current system username.

.EXAMPLE
New-GitReleaseBranch

Creates a branch like: yourname/Release/20250622, merges the latest dev changes into it, and pushes it.

.NOTES
- Uses Git CLI.
- Exits with an error if the branch creation fails.
- Intended for controlled release preparation workflows.
#>
    param([string] $user=$null)
    if ([string]::IsNullOrWhiteSpace($user)) {
        $user = [System.Environment]::UserName
    }
    git checkout main
    git pull
    $branch = "$user/Release/$((Get-Date).ToString('yyyyMMdd'))"
    git checkout -b $branch
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Error to create branch $branch"
        exit;
    }
    git checkout dev
    git pull
    git checkout -
    git merge dev -X theirs
    git push --set-upstream origin $branch
}
