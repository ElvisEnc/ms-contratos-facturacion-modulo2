# incoherencias-abiertas.ps1  ·  SessionStart
# Al abrir sesion, recuerda las incoherencias sin resolver.
$archivo = Join-Path $env:CLAUDE_PROJECT_DIR 'docs\INCOHERENCIAS.md'
if (Test-Path $archivo) {
    $abiertas = (Select-String -Path $archivo -Pattern 'Estado:\*\*\s*ABIERTA' -AllMatches).Count
    if ($abiertas -gt 0) {
        @{
            hookSpecificOutput = @{
                hookEventName     = 'SessionStart'
                additionalContext = "ATENCION: hay $abiertas incoherencia(s) ABIERTA(s) en docs/INCOHERENCIAS.md. Leelas antes de escribir codigo que dependa de esos puntos. No las resuelvas por tu cuenta."
            }
        } | ConvertTo-Json -Depth 5
    }
}
exit 0
