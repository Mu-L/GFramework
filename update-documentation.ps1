#!/usr/bin/env pwsh

param(
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Phase 3: Update Documentation" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Namespace mapping (same as update-namespaces.ps1)
$namespaceMap = [ordered]@{
    # Nested namespaces first
    "\.cqrs\.behaviors" = ".CQRS.Behaviors"
    "\.cqrs\.command" = ".CQRS.Command"
    "\.cqrs\.notification" = ".CQRS.Notification"
    "\.cqrs\.query" = ".CQRS.Query"
    "\.cqrs\.request" = ".CQRS.Request"
    "\.coroutine\.extensions" = ".Coroutine.Extensions"
    "\.coroutine\.instructions" = ".Coroutine.Instructions"
    "\.functional\.async" = ".Functional.Async"
    "\.functional\.control" = ".Functional.Control"
    "\.functional\.functions" = ".Functional.Functions"
    "\.functional\.pipe" = ".Functional.Pipe"
    "\.functional\.result" = ".Functional.Result"
    "\.logging\.appenders" = ".Logging.Appenders"
    "\.logging\.filters" = ".Logging.Filters"
    "\.logging\.formatters" = ".Logging.Formatters"
    "\.services\.modules" = ".Services.Modules"
    "\.setting\.events" = ".Setting.Events"
    "\.setting\.data" = ".Setting.Data"
    "\.scene\.handler" = ".Scene.Handler"
    "\.ui\.handler" = ".UI.Handler"
    "\.extensions\.signal" = ".Extensions.Signal"

    # Single-level namespaces
    "\.cqrs" = ".CQRS"
    "\.ioc" = ".IoC"
    "\.ui" = ".UI"
    "\.ecs" = ".ECS"
    "\.architecture" = ".Architecture"
    "\.bases" = ".Bases"
    "\.command" = ".Command"
    "\.configuration" = ".Configuration"
    "\.controller" = ".Controller"
    "\.coroutine" = ".Coroutine"
    "\.data" = ".Data"
    "\.enums" = ".Enums"
    "\.environment" = ".Environment"
    "\.events" = ".Events"
    "\.extensions" = ".Extensions"
    "\.functional" = ".Functional"
    "\.internals" = ".Internals"
    "\.lifecycle" = ".Lifecycle"
    "\.logging" = ".Logging"
    "\.model" = ".Model"
    "\.pause" = ".Pause"
    "\.pool" = ".Pool"
    "\.properties" = ".Properties"
    "\.property" = ".Property"
    "\.query" = ".Query"
    "\.registries" = ".Registries"
    "\.resource" = ".Resource"
    "\.rule" = ".Rule"
    "\.serializer" = ".Serializer"
    "\.services" = ".Services"
    "\.state" = ".State"
    "\.storage" = ".Storage"
    "\.system" = ".System"
    "\.time" = ".Time"
    "\.utility" = ".Utility"
    "\.versioning" = ".Versioning"
    "\.scene" = ".Scene"
    "\.setting" = ".Setting"
    "\.diagnostics" = ".Diagnostics"
    "\.components" = ".Components"
    "\.systems" = ".Systems"
    "\.analyzers" = ".Analyzers"
}

# Get all Markdown files
Write-Host "Finding Markdown files..."
$mdFiles = @()
$mdFiles += Get-ChildItem -Path "docs" -Filter "*.md" -Recurse -ErrorAction SilentlyContinue
$mdFiles += Get-ChildItem -Path "." -Filter "README.md" -Recurse -ErrorAction SilentlyContinue | Where-Object { $_.FullName -notmatch "node_modules" -and $_.FullName -notmatch "\\bin\\" -and $_.FullName -notmatch "\\obj\\" }
Write-Host "Found $($mdFiles.Count) Markdown files`n"

$modifiedCount = 0

foreach ($file in $mdFiles) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $originalContent = $content
    $fileModified = $false

    # Update namespace references
    foreach ($mapping in $namespaceMap.GetEnumerator()) {
        $oldPattern = $mapping.Key
        $newPattern = $mapping.Value

        # Match GFramework namespace references
        if ($content -match "GFramework[^\s;,\)]*$oldPattern") {
            $content = $content -replace "(GFramework[^\s;,\)]*)$oldPattern", "`$1$newPattern"
            $fileModified = $true
        }
    }

    if ($fileModified) {
        $modifiedCount++
        $relativePath = $file.FullName.Replace((Get-Location).Path, "").TrimStart('\', '/')
        Write-Host "✓ Updated: $relativePath" -ForegroundColor Green

        if (-not $DryRun) {
            Set-Content -Path $file.FullName -Value $content -NoNewline -Encoding UTF8
        }
    }
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "  Total files scanned: $($mdFiles.Count)"
Write-Host "  Files modified: $modifiedCount"
Write-Host "========================================`n" -ForegroundColor Cyan

if (-not $DryRun -and $modifiedCount -gt 0) {
    Write-Host "Committing documentation updates..."
    git add -A
    git commit -m "docs: update namespace references to PascalCase (phase 3)"
    Write-Host "✓ Phase 3 complete" -ForegroundColor Green
} elseif ($DryRun) {
    Write-Host "[DRY RUN] No changes were made" -ForegroundColor Yellow
} else {
    Write-Host "No files needed updating" -ForegroundColor Yellow
}
