#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Install the JSdotNet Project Guidelines Plugin to your local Copilot configuration.

.DESCRIPTION
    This script installs the plugin skills and extensions to your global Copilot CLI
    configuration so they are available across all repositories.

.PARAMETER SkillsPath
    Custom path to install skills. Defaults to ~/.copilot/skills/

.PARAMETER ExtensionsPath
    Custom path to install extensions. Defaults to ~/.copilot/extensions/

.PARAMETER PluginRoot
    Root directory of this plugin. Defaults to the script's parent directory.

.EXAMPLE
    .\install-plugin.ps1
    # Installs to default locations

.EXAMPLE
    .\install-plugin.ps1 -SkillsPath "C:\MyCustom\skills" -ExtensionsPath "C:\MyCustom\extensions"
    # Installs to custom paths
#>

param(
    [string]$SkillsPath = $null,
    [string]$ExtensionsPath = $null,
    [string]$PluginRoot = $null
)

# Resolve paths
if (-not $PluginRoot) {
    $PluginRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
}

if (-not $SkillsPath) {
    $SkillsPath = Join-Path $env:USERPROFILE ".copilot\skills"
}

if (-not $ExtensionsPath) {
    $ExtensionsPath = Join-Path $env:USERPROFILE ".copilot\extensions"
}

# Normalize paths
$SkillsPath = [System.IO.Path]::GetFullPath($SkillsPath)
$ExtensionsPath = [System.IO.Path]::GetFullPath($ExtensionsPath)
$PluginRoot = [System.IO.Path]::GetFullPath($PluginRoot)

$PluginSkillsDir = Join-Path $PluginRoot ".github\skills"
$PluginExtensionsDir = Join-Path $PluginRoot ".github\extensions"

Write-Host "📦 JSdotNet Project Guidelines Plugin Installer" -ForegroundColor Cyan
Write-Host ""
Write-Host "Installation Configuration:" -ForegroundColor Green
Write-Host "  Plugin Root:       $PluginRoot"
Write-Host "  Skills source:     $PluginSkillsDir"
Write-Host "  Skills target:     $SkillsPath"
Write-Host "  Extensions source: $PluginExtensionsDir"
Write-Host "  Extensions target: $ExtensionsPath"
Write-Host ""

# Verify source directories exist
if (-not (Test-Path $PluginSkillsDir)) {
    Write-Host "❌ Error: Skills directory not found at $PluginSkillsDir" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $PluginExtensionsDir)) {
    Write-Host "❌ Error: Extensions directory not found at $PluginExtensionsDir" -ForegroundColor Red
    exit 1
}

# Create target directories
if (-not (Test-Path $SkillsPath)) {
    New-Item -ItemType Directory -Path $SkillsPath -Force | Out-Null
    Write-Host "✓ Created skills directory" -ForegroundColor Green
}

if (-not (Test-Path $ExtensionsPath)) {
    New-Item -ItemType Directory -Path $ExtensionsPath -Force | Out-Null
    Write-Host "✓ Created extensions directory" -ForegroundColor Green
}

# Copy skills
Write-Host ""
Write-Host "Installing Skills..." -ForegroundColor Cyan
foreach ($skillFile in Get-ChildItem -Path $PluginSkillsDir -Filter "*.md") {
    $targetPath = Join-Path $SkillsPath $skillFile.Name
    Copy-Item -Path $skillFile.FullName -Destination $targetPath -Force
    Write-Host "  ✓ $($skillFile.Name)" -ForegroundColor Green
}

# Copy extensions
Write-Host ""
Write-Host "Installing Extensions..." -ForegroundColor Cyan
foreach ($extensionDir in Get-ChildItem -Path $PluginExtensionsDir -Directory) {
    $targetExtensionPath = Join-Path $ExtensionsPath $extensionDir.Name
    
    # Remove existing if present
    if (Test-Path $targetExtensionPath) {
        Remove-Item -Path $targetExtensionPath -Recurse -Force
    }
    
    # Copy new
    Copy-Item -Path $extensionDir.FullName -Destination $targetExtensionPath -Recurse -Force
    Write-Host "  ✓ $($extensionDir.Name)" -ForegroundColor Green
}

Write-Host ""
Write-Host "✅ Installation Complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "  1. Restart your Copilot CLI session"
Write-Host "  2. Run: extensions_reload"
Write-Host "  3. Invoke a skill:"
Write-Host "     • Skill: guidelines-mcp"
Write-Host "     • Skill: gap-analysis"
Write-Host "     • Skill: feedback-loop"
Write-Host ""
Write-Host "Need help? See .github\PLUGIN_README.md" -ForegroundColor Cyan
