# bloquear-destructivos.ps1  ·  PreToolUse sobre Bash|PowerShell
# Bloquea comandos irreversibles antes de que se ejecuten.
$ErrorActionPreference = 'Stop'
$payload = [Console]::In.ReadToEnd() | ConvertFrom-Json
$cmd = $payload.tool_input.command
if (-not $cmd) { exit 0 }

$patrones = @(
    'ef\s+database\s+drop',
    'ef\s+migrations\s+remove',
    'DROP\s+(TABLE|DATABASE|SCHEMA)',
    'TRUNCATE\s+',
    'git\s+push',
    'git\s+reset\s+--hard',
    'git\s+clean\s+-[a-zA-Z]*f',
    'docker\s+compose\s+down',
    'Remove-Item.*-Recurse.*-Force',
    'rm\s+-rf'
)

foreach ($p in $patrones) {
    if ($cmd -match $p) {
        @{
            hookSpecificOutput = @{
                hookEventName            = 'PreToolUse'
                permissionDecision       = 'deny'
                permissionDecisionReason = "Comando destructivo bloqueado por el hook del proyecto (patron: $p). Si hace falta de verdad, lo ejecuto yo a mano."
            }
        } | ConvertTo-Json -Depth 5
        exit 0
    }
}
exit 0
