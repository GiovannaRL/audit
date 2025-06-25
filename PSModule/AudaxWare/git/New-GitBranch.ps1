function New-GitBranch {
<#
.SYNOPSIS
Creates a new Git branch based on a conventional naming scheme and pushes it to the remote.

.DESCRIPTION
This function checks out the latest `dev` branch, pulls the most recent changes,
and creates a new branch using the format: <user>/<conventionalType>/<workItem>-<title>.
It then pushes the new branch and sets the upstream reference.

.PARAMETER conventionalType
Specifies the type of change. Must be one of: test, chore, fix, docs, feat, refactor, perf, build, ci.

.PARAMETER title
Short, descriptive branch title (used in the branch name).

.PARAMETER workItem
The associated work item or ticket number used in the branch name.

.PARAMETER user
The username to use in the branch path. If not specified, defaults to the current system user.

.EXAMPLE
New-GitBranch -conventionalType "fix" -title "login-bug" -workItem 2345

Creates and pushes a branch like: yourname/fix/2345-login-bug

.NOTES
Author: Lourenco Teodoro
Requires: Git CLI  
#>

    param(
        [Parameter(Mandatory=$true)]
        [ValidateSet("test", "chore", "fix", "docs", "feat", "refactor", "perf", "build", "ci")]
        [string] $conventionalType,
        [Parameter(Mandatory=$true)]
        [string] $title,
        [Parameter(Mandatory=$true)]
        [int] $workItem,
        [string] $user=$null)
    if ([string]::IsNullOrWhiteSpace($user)) {
        $user = [System.Environment]::UserName
    }
    Write-Host "Checking out dev branch"
    git checkout dev
    Write-Host "Retrieving latest version"
    git pull
    Write-Host "Creating branch"
    $branch = "$user/$conventionalType/$workItem-$title"
    git checkout -b $branch
    git push --set-upstream origin $branch
}
