# cierre-de-turno.ps1  ·  Stop
# 1) formatea  2) compila y no deja terminar el turno si falla  3) recuerda incoherencias abiertas
$ErrorActionPreference = 'Continue'
$raiz = $env:CLAUDE_PROJECT_DIR
if (-not $raiz) { exit 0 }
Set-Location $raiz

# --- Guarda anti-bucle: como maximo 2 bloqueos seguidos ---
$contador = Join-Path $raiz '.claude\.intentos-cierre'
$intentos = 0
if (Test-Path $contador) { $intentos = [int](Get-Content $contador -Raw) }

# --- 1. Formato: una vez por turno, no una por edicion ---
dotnet format --verbosity quiet 2>$null | Out-Null

# --- 2. Build ---
$salida = dotnet build --nologo -v q 2>&1 | Out-String
if ($LASTEXITCODE -ne 0) {
    if ($intentos -ge 2) {
        Set-Content $contador '0'
        @{ systemMessage = "El build sigue fallando tras 2 intentos. Te devuelvo el turno para que lo mires tu." } | ConvertTo-Json
        exit 0
    }
    Set-Content $contador ([string]($intentos + 1))
    $errores = ($salida -split "`n" | Select-String -Pattern 'error ' | Select-Object -First 15) -join "`n"
    @{
        hookSpecificOutput = @{
            hookEventName  = 'Stop'
            shouldContinue = $false
            continueReason = "El build falla. No termines el turno con la solucion rota. Errores:`n$errores"
        }
    } | ConvertTo-Json -Depth 5
    exit 0
}
Set-Content $contador '0'

# --- 2b. Tests: descomentar al cerrar la fase 2, cuando ya existan ---
$salidaTest = dotnet test --nologo -v q 2>&1 | Out-String
if ($LASTEXITCODE -ne 0) {
    $fallos = ($salidaTest -split "`n" | Select-String -Pattern 'Failed|error ' | Select-Object -First 15) -join "`n"
    @{
        hookSpecificOutput = @{
            hookEventName  = 'Stop'
            shouldContinue = $false
            continueReason = "Hay tests en rojo. No cierres la fase asi.`n$fallos"
        }
    } | ConvertTo-Json -Depth 5
    exit 0
}

# --- 3. Protocolo de incoherencias ---
$inc = Join-Path $raiz 'docs\INCOHERENCIAS.md'
if (Test-Path $inc) {
    $abiertas = (Select-String -Path $inc -Pattern 'Estado:\*\*\s*ABIERTA' -AllMatches).Count
    if ($abiertas -gt 0) {
        @{ systemMessage = "Recordatorio: $abiertas incoherencia(s) ABIERTA(s) en docs/INCOHERENCIAS.md. No se cierran solas y no las cierra Claude." } | ConvertTo-Json
        exit 0
    }
}
exit 0
