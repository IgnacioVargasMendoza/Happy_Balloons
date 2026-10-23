param([string]$Proyecto, [string]$Salida)
$ErrorActionPreference = 'Stop'
if (Test-Path $Salida) { Remove-Item -Recurse -Force $Salida }
New-Item -ItemType Directory $Salida | Out-Null

$excluir = @('connectionStrings.config', 'AppSettings.secret.config', 'Web.Debug.config', 'Web.Release.config', 'packages.config')
[xml]$csproj = Get-Content (Join-Path $Proyecto 'HappyTimesBalloons.Web.csproj')
$contenido = $csproj.Project.ItemGroup.Content | Where-Object { $_ -and $_.Include } | ForEach-Object { $_.Include }
$copiados = 0
foreach ($rel in $contenido) {
    if ($excluir -contains (Split-Path $rel -Leaf)) { continue }
    $origen = Join-Path $Proyecto $rel
    if (-not (Test-Path $origen)) { Write-Warning "No existe: $rel"; continue }
    $destino = Join-Path $Salida $rel
    New-Item -ItemType Directory -Force (Split-Path $destino) | Out-Null
    Copy-Item $origen $destino
    $copiados++
}
Copy-Item -Recurse (Join-Path $Proyecto 'bin') (Join-Path $Salida 'bin')
Get-ChildItem (Join-Path $Salida 'bin') -Recurse -Include *.config | Where-Object { $excluir -contains $_.Name } | Remove-Item

$webConfig = Join-Path $Salida 'Web.config'
[xml]$wc = Get-Content $webConfig
$wc.configuration.'system.web'.compilation.RemoveAttribute('debug')
$ce = $wc.CreateElement('customErrors')
$ce.SetAttribute('mode', 'RemoteOnly')
$wc.configuration.'system.web'.AppendChild($ce) | Out-Null
$wc.Save($webConfig)

$total = Get-ChildItem $Salida -Recurse -File | Measure-Object Length -Sum
Write-Output "Content copiados: $copiados | Archivos totales: $($total.Count) | MB: $([math]::Round($total.Sum / 1MB, 1))"
