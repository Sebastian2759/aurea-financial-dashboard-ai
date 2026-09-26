[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) { throw 'Instala Docker Desktop, activa los contenedores Linux y vuelve a ejecutar .\start.ps1.' }
docker info --format '{{.OSType}}' | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'Docker no está listo. Inicia Docker Desktop y vuelve a ejecutar el comando.' }
docker compose version | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'Se necesita Docker Compose v2 con soporte para --wait.' }
function New-DemoSecret {
    $bytes = New-Object byte[] 32
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try { $rng.GetBytes($bytes) } finally { $rng.Dispose() }
    return [Convert]::ToBase64String($bytes)
}
if (-not (Test-Path -LiteralPath '.env')) {
    $config = @('DASHBOARD_PORT=8080', ('SQL_PASSWORD=Aa1!' + (New-DemoSecret)), ('JWT_SECRET=' + (New-DemoSecret)), ('API_KEY=' + (New-DemoSecret)), ('COINGECKO_API_KEY=' + $env:COINGECKO_API_KEY))
    [IO.File]::WriteAllLines((Join-Path $PSScriptRoot '.env'), $config, (New-Object Text.UTF8Encoding($false)))
    Write-Host 'Configuración demo generada en .env (excluida de Git).'
}
docker compose up --build --detach --wait --wait-timeout 300
if ($LASTEXITCODE -ne 0) { throw 'El arranque no terminó correctamente. Revisa docker compose logs --tail 100. No se eliminó el volumen de datos.' }
$portLine = Get-Content -LiteralPath '.env' | Where-Object { $_ -match '^DASHBOARD_PORT=' } | Select-Object -First 1
$dashboardPort = if ($portLine) { $portLine.Split('=', 2)[1] } else { '8080' }
Write-Host "Dashboard listo: http://localhost:$dashboardPort"
