#!/usr/bin/env pwsh

param(
    [switch]$DryRun,
    [switch]$SkipTests,
    [string]$Phase = "all"
)

$ErrorActionPreference = "Stop"

function Write-Phase {
    param([string]$Message)
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host $Message -ForegroundColor Cyan
    Write-Host "========================================`n" -ForegroundColor Cyan
}

function Invoke-GitCommand {
    param([string]$Command)
    if ($DryRun) {
        Write-Host "[DRY RUN] git $Command" -ForegroundColor Yellow
    } else {
        Invoke-Expression "git $Command"
        if ($LASTEXITCODE -ne 0) {
            throw "Git command failed: git $Command"
        }
    }
}

# Phase 0: Preparation
if ($Phase -eq "all" -or $Phase -eq "0") {
    Write-Phase "Phase 0: Preparation"

    # Check clean working directory
    $status = git status --porcelain
    if ($status) {
        Write-Error "Working directory is not clean. Please commit or stash changes first."
        exit 1
    }

    # Create backup tag
    Write-Host "Creating backup tag..."
    Invoke-GitCommand "tag backup-before-namespace-refactor-$(Get-Date -Format 'yyyyMMdd-HHmmss')"

    # Run baseline tests
    if (-not $SkipTests) {
        Write-Host "Running baseline tests..."
        dotnet test
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Baseline tests failed. Please fix before proceeding."
            exit 1
        }
    }

    Write-Host "✓ Preparation complete" -ForegroundColor Green
}

# Phase 1: Rename folders
if ($Phase -eq "all" -or $Phase -eq "1") {
    Write-Phase "Phase 1: Rename Folders"

    # Load mappings
    $mappingsJson = Get-Content "folder-mappings.json" -Raw | ConvertFrom-Json

    foreach ($project in $mappingsJson.projects) {
        Write-Host "`nProcessing project: $($project.name)" -ForegroundColor Yellow

        # Sort folders by depth (deepest first) to avoid conflicts
        $sortedFolders = $project.folders | Sort-Object { ($_.old -split '/').Count } -Descending

        foreach ($mapping in $sortedFolders) {
            $oldPath = Join-Path $project.path $mapping.old
            $newPath = Join-Path $project.path $mapping.new

            if (Test-Path $oldPath) {
                # Windows case-insensitive workaround
                if ($mapping.old.ToLower() -eq $mapping.new.ToLower()) {
                    $tempPath = "$oldPath`_temp"
                    Write-Host "  Renaming (2-step): $($mapping.old) → $($mapping.new)"
                    Invoke-GitCommand "mv `"$oldPath`" `"$tempPath`""
                    Invoke-GitCommand "mv `"$tempPath`" `"$newPath`""
                } else {
                    Write-Host "  Renaming: $($mapping.old) → $($mapping.new)"
                    Invoke-GitCommand "mv `"$oldPath`" `"$newPath`""
                }
            } else {
                Write-Host "  Skipping (not found): $($mapping.old)" -ForegroundColor Gray
            }
        }
    }

    if (-not $DryRun) {
        Write-Host "`nCommitting folder renames..."
        git commit -m "refactor: rename folders to PascalCase (phase 1)"
    }

    Write-Host "✓ Phase 1 complete" -ForegroundColor Green
}

Write-Host "`n✓ Script execution complete!" -ForegroundColor Green
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Run: ./update-namespaces.ps1" -ForegroundColor White
Write-Host "  2. Run: ./update-documentation.ps1" -ForegroundColor White
Write-Host "  3. Run: ./verify-namespaces.ps1" -ForegroundColor White
