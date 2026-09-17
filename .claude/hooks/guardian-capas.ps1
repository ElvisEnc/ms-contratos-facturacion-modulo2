# guardian-capas.ps1  ·  PreToolUse sobre Edit|Write
# Impide escribir dependencias de infraestructura dentro de la capa Domain.
# Clean Architecture (SISTEMA.md 11.1): Domain no conoce a nadie salvo Joseco.DDD.Core.
$ErrorActionPreference = 'Stop'
$payload = [Console]::In.ReadToEnd() | ConvertFrom-Json

$ruta = $payload.tool_input.file_path
if (-not $ruta) { exit 0 }

# Solo nos importa la capa Domain, y solo archivos C#
if ($ruta -notmatch '\.Domain[\\/]') { exit 0 }
if ($ruta -notmatch '\.cs$')         { exit 0 }

# Write trae 'content'; Edit trae 'new_string'
$contenido = $payload.tool_input.content
if (-not $contenido) { $contenido = $payload.tool_input.new_string }
if (-not $contenido) { exit 0 }

$prohibidos = @(
    'using\s+Microsoft\.EntityFrameworkCore',
    'using\s+Microsoft\.AspNetCore',
    'using\s+Npgsql',
    'using\s+System\.Text\.Json',
    'using\s+MediatR\s*;'          # MediatR.Contracts si llega via DomainEvent, permitido
)

foreach ($p in $prohibidos) {
    if ($contenido -match $p) {
        $ns = ($contenido | Select-String -Pattern $p).Matches[0].Value
        @{
            hookSpecificOutput = @{
                hookEventName            = 'PreToolUse'
                permissionDecision       = 'deny'
                permissionDecisionReason = "Regla de dependencias (SISTEMA.md 11.1): la capa Domain no puede referenciar infraestructura. Se intento escribir '$ns' en $ruta. Si esto de verdad hace falta, es una incoherencia: registrala en docs/INCOHERENCIAS.md y para."
            }
        } | ConvertTo-Json -Depth 5
        exit 0
    }
}

# DateTime.Now en Domain: no se bloquea, se avisa (SISTEMA.md 12.11)
if ($contenido -match 'DateTime\.Now') {
    @{ systemMessage = "Aviso: DateTime.Now en la capa Domain. SISTEMA.md 12.11 exige UtcNow, y lo deterministico es recibir la fecha por parametro." } | ConvertTo-Json
}
exit 0
