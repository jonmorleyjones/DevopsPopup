<#
.SYNOPSIS
    Creates an Azure DevOps work item.

.DESCRIPTION
    This script creates a new work item in Azure DevOps using the REST API.

.PARAMETER title
    The title of the work item.

.PARAMETER description
    The description of the work item.

.PARAMETER workItemType
    The type of work item (Task, Bug, User Story, Feature).

.PARAMETER priority
    The priority level (1-4).

.PARAMETER assignToMe
    Whether to assign the work item to the current user.
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$title,

    [string]$description = "",

    [string]$workItemType = "Task",

    [string]$priority = "2",

    [string]$assignToMe = "$true"
)

# Configuration - Update these values or use environment variables
$organization = $env:AZURE_DEVOPS_ORG ?? "your-organization"
$project = $env:AZURE_DEVOPS_PROJECT ?? "your-project"
$pat = $env:AZURE_DEVOPS_PAT ?? "your-pat"
$areaPath = $env:AZURE_DEVOPS_AREA ?? "$project"
$iterationPath = $env:AZURE_DEVOPS_ITERATION ?? "$project"

# Validate required configuration
if ($pat -eq "your-pat") {
    Write-Error "Please set the AZURE_DEVOPS_PAT environment variable"
    exit 1
}

# Build the API URL
$url = "https://dev.azure.com/$organization/$project/_apis/wit/workitems/`$$workItemType`?api-version=7.0"

# Create authorization header
$base64Auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$pat"))
$headers = @{
    "Authorization" = "Basic $base64Auth"
    "Content-Type" = "application/json-patch+json"
}

# Build the patch document
$patchDocument = @(
    @{
        op = "add"
        path = "/fields/System.Title"
        value = $title
    }
)

if ($description) {
    $patchDocument += @{
        op = "add"
        path = "/fields/System.Description"
        value = $description
    }
}

$patchDocument += @{
    op = "add"
    path = "/fields/Microsoft.VSTS.Common.Priority"
    value = [int]$priority
}

if ($areaPath) {
    $patchDocument += @{
        op = "add"
        path = "/fields/System.AreaPath"
        value = $areaPath
    }
}

if ($iterationPath) {
    $patchDocument += @{
        op = "add"
        path = "/fields/System.IterationPath"
        value = $iterationPath
    }
}

if ($assignToMe -eq "$true" -or $assignToMe -eq "True") {
    # Get current user from Azure DevOps
    try {
        $meUrl = "https://dev.azure.com/$organization/_apis/connectionData?api-version=7.0"
        $meResponse = Invoke-RestMethod -Uri $meUrl -Headers $headers -Method Get
        $currentUser = $meResponse.authenticatedUser.providerDisplayName

        $patchDocument += @{
            op = "add"
            path = "/fields/System.AssignedTo"
            value = $currentUser
        }
    } catch {
        Write-Warning "Could not get current user for assignment"
    }
}

# Convert to JSON
$body = $patchDocument | ConvertTo-Json -Depth 10

try {
    $response = Invoke-RestMethod -Uri $url -Headers $headers -Method Post -Body $body

    # Output result as JSON for parsing
    $result = @{
        success = $true
        id = $response.id
        url = $response._links.html.href
        message = "Created work item #$($response.id)"
    }

    $result | ConvertTo-Json
    Write-Host "Created work item #$($response.id): $title"
    exit 0
}
catch {
    $errorMessage = $_.Exception.Message
    Write-Error "Failed to create work item: $errorMessage"

    $result = @{
        success = $false
        error = $errorMessage
    }
    $result | ConvertTo-Json
    exit 1
}
