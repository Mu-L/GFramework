#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Phase 4: Verify Namespace Consistency" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Patterns to detect lowercase namespaces (should not exist after refactoring)
$lowercasePatterns = @(
    "\.architecture\b",
    "\.bases\b",
    "\.command\b",
    "\.configuration\b",
    "\.controller\b",
    "\.coroutine\b",
    "\.cqrs\b",
    "\.data\b",
    "\.enums\b",
    "\.environment\b",
    "\.events\b",
    "\.extensions\b",
    "\.functional\b",
    "\.internals\b",
    "\.ioc\b",
    "\.lifecycle\b",
    "\.logging\b",
    "\.model\b",
    "\.pause\b",
    "\.pool\b",
    "\.properties\b",
    "\.property\b",
    "\.query\b",
    "\.registries\b",
    "\.resource\b",
    "\.rule\b",
    "\.serializer\b",
    "\.services\b",
    "\.state\b",
    "\.storage\b",
    "\.system\b",
    "\.time\b",
    "\.utility\b",
    "\.versioning\b",
    "\.ui\b",
    "\.ecs\b",
    "\.scene\b",
    "\.setting\b",
    "\.diagnostics\b",
    "\.components\b",
    "\.systems\b",
    "\.analyzers\b"
)

Write-Host "Searching for lowercase namespaces in C# files..."
$csFiles = Get-ChildItem -Path . -Filter "*.cs" -Recurse -Exclude "bin","obj" | Where-Object { $_.FullName -notmatch "\\bin\\" -and $_.FullName -notmatch "\\obj\\" }

$issues = @()

foreach ($file in $csFiles) {
    $content = Get-Content $file.FullName -Raw
    $lineNumber = 0

    foreach ($line in (Get-Content $file.FullName)) {
        $lineNumber++

        # Check if line contains namespace or using statement
        if ($line -match "^\s*(namespace|using)\s+GFramework") {
            foreach ($pattern in $lowercasePatterns) {
                if ($line -match $pattern) {
                    $relativePath = $file.FullName.Replace((Get-Location).Path, "").TrimStart('\', '/')
                    $issues += [PSCustomObject]@{
                        File = $relativePath
                        Line = $lineNumber
                        Content = $line.Trim()
                    }
                    break
                }
            }
        }
    }
}

if ($issues.Count -gt 0) {
    Write-Host "`n❌ Found $($issues.Count) lowercase namespace(s):`n" -ForegroundColor Red

    foreach ($issue in $issues) {
        Write-Host "  File: $($issue.File):$($issue.Line)" -ForegroundColor Yellow
        Write-Host "    $($issue.Content)" -ForegroundColor Gray
        Write-Host ""
    }

    Write-Host "Please review and fix these issues manually.`n" -ForegroundColor Red
    exit 1
} else {
    Write-Host "✓ All namespaces are PascalCase!" -ForegroundColor Green
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Verification Summary:" -ForegroundColor Cyan
Write-Host "  Files scanned: $($csFiles.Count)"
Write-Host "  Issues found: $($issues.Count)"
Write-Host "========================================`n" -ForegroundColor Cyan

if ($issues.Count -eq 0) {
    Write-Host "✓ Namespace verification passed!" -ForegroundColor Green
    exit 0
}
