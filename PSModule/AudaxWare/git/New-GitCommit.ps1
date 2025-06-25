function New-GitCommit {
<#
.SYNOPSIS
Creates and pushes a Git commit using a conventional commit message style.

.DESCRIPTION
This function ensures that developers do not commit directly to protected branches
(like `dev` or `main`). It constructs a commit message using a conventional prefix
and user-provided title, then performs `git add`, `commit`, and `push` operations.

.PARAMETER conventionalType
Specifies the commit type using the conventional commit standard.
Accepted values: test, chore, fix, docs, feat, refactor, perf, build, ci.

.PARAMETER title
Descriptive summary of the change. Must be between 1 and 45 characters long.

.EXAMPLE
New-GitCommit -conventionalType fix -title "correct null handling"

Creates a commit message like:
fix: correct null handling

.NOTES
- Blocks commits to 'main' or 'dev' branches to enforce proper workflow.
- Requires Git CLI to be available in the environment.
#>
    param(
        [Parameter(Mandatory=$true)]
        [ValidateSet("test", "chore", "fix", "docs", "feat", "refactor", "perf", "build", "ci")]
        [string] $conventionalType,
        [ValidateLength(1, 45)]
        [Parameter(Mandatory=$true)]
        [string] $title)
    $currentBranch = git branch --show-current
    if (($currentBranch -eq "dev") -or $currentBranch -eq "main") {
        Write-Error "You cannot commit changes directly to the branch '$currentBranch'. Use New-GitBranch to create a branch"
        Return;
    }
    $comment = "$conventionalType" + ": $title"
    git add *
    git commit -m $comment
    git push
}
