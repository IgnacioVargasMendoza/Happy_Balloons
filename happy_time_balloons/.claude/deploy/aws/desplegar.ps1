$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
Import-Module WebAdministration
$sitio = 'HappyTimesBalloons'
$ruta = 'C:\inetpub\HappyTimesBalloons'
$zip = 'C:\sqlsetup\happy-balloons-web.zip'

Write-Output 'Descargando paquete...'
Invoke-WebRequest '{{ZIP_URL}}' -OutFile $zip -UseBasicParsing
if (Test-Path "IIS:\Sites\$sitio") { Stop-Website $sitio }
if (Test-Path $ruta) { Get-ChildItem $ruta | Where-Object { $_.Name -ne 'Content' } | Remove-Item -Recurse -Force }
Expand-Archive $zip -DestinationPath $ruta -Force
Remove-Item $zip

@'
<connectionStrings>
  <add name="HappyTimesBallooonsContext"
       connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=HappyTimesBalloons;Integrated Security=True;MultipleActiveResultSets=True"
       providerName="System.Data.SqlClient" />
</connectionStrings>
'@ | Set-Content "$ruta\connectionStrings.config" -Encoding UTF8
Write-Output 'connectionStrings.config creado'

if (-not (Test-Path "IIS:\AppPools\$sitio")) { New-WebAppPool $sitio | Out-Null }
Set-ItemProperty "IIS:\AppPools\$sitio" -Name managedRuntimeVersion -Value 'v4.0'
Set-ItemProperty "IIS:\AppPools\$sitio" -Name managedPipelineMode -Value 'Integrated'
Set-ItemProperty "IIS:\AppPools\$sitio" -Name processModel.identityType -Value 'ApplicationPoolIdentity'

if (Get-Website 'Default Web Site' -ErrorAction SilentlyContinue) { Stop-Website 'Default Web Site'; Set-ItemProperty 'IIS:\Sites\Default Web Site' -Name serverAutoStart -Value $false }
if (-not (Test-Path "IIS:\Sites\$sitio")) { New-Website -Name $sitio -PhysicalPath $ruta -ApplicationPool $sitio -Port 80 | Out-Null }
Write-Output 'Sitio y Application Pool configurados'

New-Item -ItemType Directory -Force "$ruta\Content\Uploads\Productos" | Out-Null
icacls $ruta /grant "IIS AppPool\${sitio}:(OI)(CI)RX" | Out-Null
icacls "$ruta\Content\Uploads" /grant "IIS AppPool\${sitio}:(OI)(CI)M" | Out-Null
icacls 'C:\AspNetTemp' /grant "IIS AppPool\${sitio}:(OI)(CI)M" | Out-Null
Write-Output 'Permisos de carpetas asignados'

$cn = New-Object System.Data.SqlClient.SqlConnection 'Server=.\SQLEXPRESS;Database=master;Integrated Security=True'
$cn.Open()
$cmd = $cn.CreateCommand()
$cmd.CommandText = @"
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'IIS APPPOOL\$sitio')
    CREATE LOGIN [IIS APPPOOL\$sitio] FROM WINDOWS WITH DEFAULT_DATABASE = HappyTimesBalloons;
USE HappyTimesBalloons;
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'IIS APPPOOL\$sitio')
    CREATE USER [IIS APPPOOL\$sitio] FOR LOGIN [IIS APPPOOL\$sitio];
ALTER ROLE db_owner ADD MEMBER [IIS APPPOOL\$sitio];
"@
$cmd.ExecuteNonQuery() | Out-Null
$cn.Close()
Write-Output 'Login SQL del pool creado con db_owner'

Start-WebAppPool $sitio -ErrorAction SilentlyContinue
Start-Website $sitio
foreach ($url in 'http://localhost/', 'http://localhost/Cuenta/Login') {
    try {
        $r = Invoke-WebRequest $url -UseBasicParsing -TimeoutSec 180
        $titulo = [regex]::Match($r.Content, '<title>(.*?)</title>').Groups[1].Value
        Write-Output "$url -> HTTP $($r.StatusCode) | $titulo"
    } catch {
        $resp = $_.Exception.Response
        Write-Output "$url -> ERROR $([int]$resp.StatusCode)"
        if ($resp) { $sr = New-Object IO.StreamReader($resp.GetResponseStream()); $t = [regex]::Replace($sr.ReadToEnd(), '<[^>]+>', ' ') -replace '\s+', ' '; Write-Output $t.Substring(0, [Math]::Min(1500, $t.Length)) }
    }
}
Write-Output 'LISTO'
