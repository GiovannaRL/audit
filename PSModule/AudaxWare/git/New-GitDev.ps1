function Get-GitDev {
<#
.SYNOPSIS
Updates the current Git branch with the latest changes from the 'dev' branch.

.DESCRIPTION
This function automates syncing the current working branch with the 'dev' branch:
- It checks out 'dev' and pulls the latest changes from the remote.
- If the current branch is not 'dev' or 'main', it switches back to the original branch,
  merges 'dev' into it, and pushes the updated branch to the remote.

.PARAMETER (none)
This function does not accept any parameters.

.EXAMPLE
Get-GitDev

Fetches the latest changes on 'dev', merges them into the current branch (unless the current
branch is 'dev' or 'main'), and pushes the result to the remote repository.

.NOTES
- Intended to help keep feature branches in sync with the latest development changes.
- Prevents merging into 'dev' or 'main' directly.
- Assumes that Git CLI is installed and the current directory is a Git repository.
#>
    param()
    $currentBranch = git branch --show-current
    git checkout dev
    git pull
    if (($currentBranch -eq "dev") -or $currentBranch -eq "main") {
        Return;
    }
    git checkout $currentBranch
    git merge dev
    git push
}
