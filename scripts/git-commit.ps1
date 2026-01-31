<#
.SYNOPSIS
    Quick git commit script.

.DESCRIPTION
    Stages changes and creates a git commit.

.PARAMETER message
    The commit message.

.PARAMETER description
    Extended commit description.

.PARAMETER stageAll
    Whether to stage all changes.

.PARAMETER push
    Whether to push after committing.
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$message,

    [string]$description = "",

    [string]$stageAll = "$true",

    [string]$push = "$false"
)

# Change to the repository directory (can be customized)
$repoPath = $env:GIT_REPO_PATH ?? (Get-Location)
Set-Location $repoPath

try {
    # Stage changes if requested
    if ($stageAll -eq "$true" -or $stageAll -eq "True") {
        Write-Host "Staging all changes..."
        git add -A
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to stage changes"
        }
    }

    # Check if there are staged changes
    $stagedChanges = git diff --cached --name-only
    if (-not $stagedChanges) {
        Write-Warning "No staged changes to commit"
        exit 0
    }

    # Build commit message
    $fullMessage = $message
    if ($description) {
        $fullMessage = "$message`n`n$description"
    }

    # Create commit
    Write-Host "Creating commit..."
    git commit -m $fullMessage
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to create commit"
    }

    # Get the commit hash
    $commitHash = git rev-parse --short HEAD

    # Push if requested
    if ($push -eq "$true" -or $push -eq "True") {
        Write-Host "Pushing to remote..."
        git push
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to push"
        }
    }

    # Output result
    $result = @{
        success = $true
        commitHash = $commitHash
        message = "Committed: $commitHash"
    }
    $result | ConvertTo-Json

    Write-Host "Successfully committed: $commitHash"
    exit 0
}
catch {
    $errorMessage = $_.Exception.Message
    Write-Error $errorMessage

    $result = @{
        success = $false
        error = $errorMessage
    }
    $result | ConvertTo-Json
    exit 1
}
