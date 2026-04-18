# provision-tenant.ps1
# Creates a new SQL Server database for a tenant and runs EF Core migrations.
#
# Usage:
#   .\provision-tenant.ps1 -TenantKey "uae-sport" -Server "localhost"
#
# Prerequisites:
#   - SQL Server running on the specified server
#   - dotnet-ef installed globally: dotnet tool install --global dotnet-ef
#   - Run from the repo root directory

param (
    [Parameter(Mandatory = $true)]
    [string]$TenantKey,

    [string]$Server = "localhost"
)

$ErrorActionPreference = "Stop"

$tenantFile = "tenants\$TenantKey.json"
$apiProject = "src\IGMS.API\IGMS.API.csproj"
$infraProject = "src\IGMS.Infrastructure\IGMS.Infrastructure.csproj"

# ── Validate ─────────────────────────────────────────────────────────────────

if (-not (Test-Path $tenantFile)) {
    Write-Error "Tenant config not found: $tenantFile"
    exit 1
}

$config = Get-Content $tenantFile | ConvertFrom-Json
$dbName = $config.database.name

Write-Host ""
Write-Host "=== IGMS Tenant Provisioning ===" -ForegroundColor Cyan
Write-Host "Tenant Key : $TenantKey"
Write-Host "Database   : $dbName"
Write-Host "Server     : $Server"
Write-Host ""

# ── Create Database ───────────────────────────────────────────────────────────

Write-Host "[1/2] Creating database '$dbName' on '$Server'..." -ForegroundColor Yellow

$createDbSql = "IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = '$dbName') CREATE DATABASE [$dbName] COLLATE Arabic_CI_AS;"

sqlcmd -S $Server -Q $createDbSql -E

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to create database."
    exit 1
}

Write-Host "      Database ready." -ForegroundColor Green

# ── Run EF Migrations ─────────────────────────────────────────────────────────

Write-Host "[2/2] Running EF Core migrations..." -ForegroundColor Yellow

$connectionString = "Server=$Server;Database=$dbName;Trusted_Connection=True;TrustServerCertificate=True;"

$env:TENANT_CONNECTION_STRING = $connectionString

dotnet ef database update `
    --project $infraProject `
    --startup-project $apiProject

if ($LASTEXITCODE -ne 0) {
    Write-Error "EF Migrations failed."
    exit 1
}

Write-Host "      Migrations applied." -ForegroundColor Green
Write-Host ""
Write-Host "=== Tenant '$TenantKey' is ready! ===" -ForegroundColor Cyan
Write-Host "Test with header: X-Tenant-Key: $TenantKey"
Write-Host ""
