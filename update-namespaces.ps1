#!/usr/bin/env pwsh

param(
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Phase 2: Update Namespace Declarations" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Namespace mapping (order matters - process longer patterns first)
$namespaceMap = [ordered]@{
    # Nested namespaces first (to avoid partial replacements)
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
    "\.events\.filters" = ".Events.Filters"
    "\.data\.events" = ".Data.Events"

    # Single-level namespaces
    "\.cqrs" = ".CQRS"
    "\.ioc" = ".IoC"
    "\.ui" = ".UI"
    "\.ecs" = ".ECS"
    "\.architecture" = ".Architecture"
    "\.bases" = ".Bases"
    "\.command" = ".Command"
    "\.configuration" = ".Configuration"
    "\.constants" = ".Constants"
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
    "\.asset" = ".Asset"
    "\.registry" = ".Registry"
    "\.diagnostics" = ".Diagnostics"
    "\.components" = ".Components"
    "\.systems" = ".Systems"
    "\.integration" = ".Integration"
    "\.tests" = ".Tests"
    "\.mediator" = ".Mediator"
    "\.analyzers" = ".Analyzers"
}

# Get all C# files
Write-Host "Finding C# files..."
$csFiles = Get-ChildItem -Path . -Filter "*.cs" -Recurse -Exclude "bin","obj" | Where-Object { $_.FullName -notmatch "\\bin\\" -and $_.FullName -notmatch "\\obj\\" }
Write-Host "Found $($csFiles.Count) C# files`n"

$modifiedCount = 0

foreach ($file in $csFiles) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $originalContent = $content
    $fileModified = $false

    # Update namespace declarations and using statements
    foreach ($mapping in $namespaceMap.GetEnumerator()) {
        $oldPattern = $mapping.Key
        $newPattern = $mapping.Value

        # Match namespace declarations: namespace GFramework.*\.old
        if ($content -match "namespace\s+GFramework[^;\{]*$oldPattern") {
            $content = $content -replace "(namespace\s+GFramework[^;\{]*)$oldPattern", "`$1$newPattern"
            $fileModified = $true
        }

        # Match using statements: using GFramework.*\.old
        if ($content -match "using\s+GFramework[^;]*$oldPattern") {
            $content = $content -replace "(using\s+GFramework[^;]*)$oldPattern", "`$1$newPattern"
            $fileModified = $true
        }

        # Match using static: using static GFramework.*\.old
        if ($content -match "using\s+static\s+GFramework[^;]*$oldPattern") {
            $content = $content -replace "(using\s+static\s+GFramework[^;]*)$oldPattern", "`$1$newPattern"
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
Write-Host "  Total files scanned: $($csFiles.Count)"
Write-Host "  Files modified: $modifiedCount"
Write-Host "========================================`n" -ForegroundColor Cyan

if (-not $DryRun -and $modifiedCount -gt 0) {
    Write-Host "Committing namespace updates..."
    git add -A
    git commit -m "refactor: update namespace declarations to PascalCase (phase 2)"
    Write-Host "✓ Phase 2 complete" -ForegroundColor Green
} elseif ($DryRun) {
    Write-Host "[DRY RUN] No changes were made" -ForegroundColor Yellow
} else {
    Write-Host "No files needed updating" -ForegroundColor Yellow
}
