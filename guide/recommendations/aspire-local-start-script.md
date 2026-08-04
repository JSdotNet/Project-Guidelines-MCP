---
title: "Aspire Local Start Script"
date: 2026-08-04
status: Accepted
tags: [aspire, scripts, developer-experience, orchestration, tooling, recommendations]
---
# Recommendation: Aspire Local Start Script

## Purpose

Every repository that adopts .NET Aspire (see ADR 0003) should ship a single, cross-team-consistent entry point for starting the AppHost locally. Without it, developers invent ad-hoc `dotnet run` invocations, skip dev-certificate trust, forget to restore/build first, and get inconsistent dashboard behavior. A canonical start script removes that guesswork and gives Copilot (and humans) one predictable command to run per repo.

## Recommendation

- Every repo with an `Aspire/*.AppHost` project MUST include a `scripts/Start-Aspire.ps1` script at the repository root's `scripts/` folder.
- The script is the **only supported way** to start the local Aspire orchestration for day-to-day development; do not hand-roll `dotnet run` invocations in READMEs or ad-hoc notes.
- Provide a POSIX equivalent (`scripts/start-aspire.sh`) only if the team actively develops on macOS/Linux; otherwise the PowerShell script alone (PowerShell 7+ runs cross-platform) is sufficient.
- Reference the script from the repo's root `README.md` under a "Running locally" section.
- Keep the script idempotent and safe to re-run: it should not fail if certs are already trusted or the build is already up to date.

## Script Responsibilities

The script must, in order:

1. Verify the required .NET SDK is installed (`dotnet --version`) and fail fast with a clear message if missing.
2. Trust local HTTPS dev certificates once per machine (`dotnet dev-certs https --trust`), skipping silently if already trusted.
3. Restore and build the solution (skip via a `-NoBuild` switch for faster inner-loop iteration).
4. Locate the AppHost project by convention (`src/Aspire/*.AppHost/*.csproj`) rather than a hardcoded path, so the script keeps working as the product name changes.
5. Run the AppHost with `ASPNETCORE_ENVIRONMENT=Development` (or `DOTNET_ENVIRONMENT`), optionally under `dotnet watch` when a `-Watch` switch is passed.
6. Print the Aspire dashboard URL prominently once the host reports it is listening.

## Canonical Script

Place this at `scripts/Start-Aspire.ps1`. Replace the AppHost glob pattern only if the repo deviates from ADR 0005's `src/Aspire/` convention.

```powershell
<#
.SYNOPSIS
    Starts the .NET Aspire AppHost for local development.
.DESCRIPTION
    Canonical entry point for running this repo's Aspire orchestration locally.
    See guideline: aspire-local-start-script (JSdotNet Project Guidelines).
.PARAMETER Watch
    Run the AppHost under `dotnet watch` for hot reload.
.PARAMETER NoBuild
    Skip restore/build and run directly (fastest inner loop once already built).
.PARAMETER Configuration
    Build configuration to use. Defaults to Debug.
#>
[CmdletBinding()]
param(
    [switch]$Watch,
    [switch]$NoBuild,
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"

function Write-Step($message) {
    Write-Host "==> $message" -ForegroundColor Cyan
}

# 1. Verify SDK availability
Write-Step "Checking .NET SDK"
$dotnetVersion = dotnet --version 2>$null
if (-not $dotnetVersion) {
    Write-Error "dotnet SDK not found on PATH. Install the .NET SDK before running this script."
    exit 1
}
Write-Host "Using .NET SDK $dotnetVersion"

# 2. Trust local HTTPS dev certificates (safe to re-run)
Write-Step "Ensuring local HTTPS dev certificates are trusted"
dotnet dev-certs https --trust | Out-Null

# 3. Locate the AppHost project by convention (ADR 0005: src/Aspire/*.AppHost)
Write-Step "Locating Aspire AppHost project"
$repoRoot = Split-Path -Parent $PSScriptRoot
$appHostProject = Get-ChildItem -Path (Join-Path $repoRoot "src\Aspire") -Recurse -Filter "*.AppHost.csproj" -ErrorAction SilentlyContinue |
    Select-Object -First 1

if (-not $appHostProject) {
    Write-Error "No *.AppHost.csproj found under src\Aspire. Confirm the repo follows ADR 0005's project layout."
    exit 1
}
Write-Host "Found AppHost: $($appHostProject.FullName)"

# 4. Restore/build unless skipped
if (-not $NoBuild) {
    Write-Step "Restoring and building ($Configuration)"
    dotnet build $appHostProject.FullName -c $Configuration
}
else {
    Write-Step "Skipping build (-NoBuild specified)"
}

# 5. Run the AppHost
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:DOTNET_ENVIRONMENT = "Development"

Write-Step "Starting Aspire AppHost (watch: $($Watch.IsPresent))"
if ($Watch) {
    dotnet watch --project $appHostProject.FullName run -c $Configuration
}
else {
    dotnet run --project $appHostProject.FullName -c $Configuration --no-build:$NoBuild
}
```

## Usage

```powershell
# Standard start
.\scripts\Start-Aspire.ps1

# Hot-reload inner loop
.\scripts\Start-Aspire.ps1 -Watch

# Fast restart after an already-successful build
.\scripts\Start-Aspire.ps1 -NoBuild
```

## Design rules

- Discover the AppHost project by glob/convention, never a hardcoded solution-specific path — this keeps the script copy-paste portable across repos.
- Fail fast with actionable messages (missing SDK, missing AppHost project) rather than letting `dotnet` produce a raw stack trace.
- Do not embed secrets, connection strings, or environment-specific values in the script; rely on user secrets / `appsettings.Development.json` / Aspire parameters instead.
- Keep the script dependency-free (no external PowerShell modules) so it runs on a fresh clone without extra setup.
- Extend the script (e.g., adding `-OpenDashboard` to launch the browser) via additive switches; do not fork per-repo variants that drift from this canonical version.

## References

- ADR 0003: Strong Recommendation to Adopt .NET Aspire for ASP.NET Web Services
- ADR 0005: Modular Monolith Project Structure (`src/Aspire/` convention)
- Config: MCP Server Configuration (.mcp.json) — Aspire MCP server entry used alongside local dev tooling
