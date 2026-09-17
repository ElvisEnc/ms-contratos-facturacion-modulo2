# statusline.ps1  ·  statusLine de tipo command
$ErrorActionPreference = 'SilentlyContinue'
$d = [Console]::In.ReadToEnd() | ConvertFrom-Json

$modelo = $d.model.display_name
$pct    = [int]($d.context_window.used_percentage)
$rama   = (git rev-parse --abbrev-ref HEAD 2>$null)
$sucio  = if ((git status --porcelain 2>$null)) { '*' } else { '' }

$llenos = [math]::Floor($pct / 10)
$barra  = ('#' * $llenos) + ('.' * (10 - $llenos))

$aviso = ''
if ($pct -ge 70) { $aviso = '  <- /clear entre fases' }

Write-Output "[$modelo]  $rama$sucio  [$barra] $pct% contexto$aviso"
