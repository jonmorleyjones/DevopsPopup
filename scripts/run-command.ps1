<#
.SYNOPSIS
    Runs a custom command.

.DESCRIPTION
    Executes a custom shell command with optional working directory.

.PARAMETER command
    The command to execute.

.PARAMETER workingDirectory
    Optional working directory.

.PARAMETER elevated
    Whether to run as administrator (Windows only).
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$command,

    [string]$workingDirectory = "",

    [string]$elevated = "$false"
)

try {
    # Set working directory if specified
    if ($workingDirectory -and (Test-Path $workingDirectory)) {
        Set-Location $workingDirectory
    }

    Write-Host "Executing: $command"
    Write-Host "Working directory: $(Get-Location)"

    if ($elevated -eq "$true" -or $elevated -eq "True") {
        # Run elevated (Windows only)
        if ($IsWindows) {
            Start-Process powershell -Verb RunAs -ArgumentList "-Command", $command -Wait
        } else {
            # On Unix, use sudo
            $output = Invoke-Expression "sudo $command" 2>&1
        }
    } else {
        # Run normal
        $output = Invoke-Expression $command 2>&1
    }

    $exitCode = $LASTEXITCODE

    # Output result
    $result = @{
        success = ($exitCode -eq 0)
        exitCode = $exitCode
        output = ($output | Out-String)
    }
    $result | ConvertTo-Json

    if ($exitCode -eq 0) {
        Write-Host "Command completed successfully"
        exit 0
    } else {
        Write-Error "Command failed with exit code: $exitCode"
        exit $exitCode
    }
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
