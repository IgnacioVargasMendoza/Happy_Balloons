$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
$dir = 'C:\sqlsetup'
New-Item -ItemType Directory -Force $dir | Out-Null

function Invoke-Sql($query, $db = 'master') {
    $cn = New-Object System.Data.SqlClient.SqlConnection "Server=.\SQLEXPRESS;Database=$db;Integrated Security=True;TrustServerCertificate=True"
    $cn.Open()
    $cmd = $cn.CreateCommand()
    $cmd.CommandText = $query
    $cmd.CommandTimeout = 600
    $t = New-Object System.Data.DataTable
    $t.Load($cmd.ExecuteReader())
    $cn.Close()
    return $t
}

if (-not (Get-Service 'MSSQL$SQLEXPRESS' -ErrorAction SilentlyContinue)) {
    Write-Output 'Descargando instalador SQL Server 2025 Express...'
    Invoke-WebRequest 'https://aka.ms/sql2025express' -OutFile "$dir\ssei.exe" -UseBasicParsing
    Start-Process "$dir\ssei.exe" -ArgumentList '/Action=Download', "/MediaPath=$dir\media", '/MediaType=Core', '/Quiet' -Wait
    $pkg = Get-ChildItem "$dir\media" -Filter 'SQLEXPR*.exe' | Select-Object -First 1
    Write-Output "Medio descargado: $($pkg.Name)"
    Start-Process $pkg.FullName -ArgumentList '/q', "/x:$dir\ext" -Wait
    Write-Output 'Instalando motor...'
    $p = Start-Process "$dir\ext\setup.exe" -ArgumentList '/Q', '/ACTION=Install', '/FEATURES=SQLENGINE', '/INSTANCENAME=SQLEXPRESS', '/SQLSYSADMINACCOUNTS="BUILTIN\Administrators"', '/SQLSVCSTARTUPTYPE=Automatic', '/UPDATEENABLED=0', '/IACCEPTSQLSERVERLICENSETERMS' -Wait -PassThru
    Write-Output "setup.exe exit code: $($p.ExitCode)"
    if ($p.ExitCode -ne 0 -and $p.ExitCode -ne 3010) {
        Get-ChildItem 'C:\Program Files\Microsoft SQL Server\*\Setup Bootstrap\Log\Summary.txt' -ErrorAction SilentlyContinue | Get-Content -Tail 40
        throw 'Fallo la instalacion de SQL Server'
    }
}

Write-Output (Invoke-Sql "SELECT CAST(SERVERPROPERTY('ProductVersion') AS varchar(20)) AS Version, CAST(SERVERPROPERTY('Edition') AS varchar(60)) AS Edicion" | Format-Table | Out-String)

Invoke-Sql "EXEC sp_configure 'show advanced options', 1; RECONFIGURE; EXEC sp_configure 'max server memory (MB)', 1024; RECONFIGURE;" | Out-Null
Write-Output 'Memoria maxima de SQL limitada a 1024 MB'

Write-Output 'Descargando respaldo...'
$bak = "$dir\HappyTimesBalloons_migracion.bak"
Invoke-WebRequest '{{BAK_URL}}' -OutFile $bak -UseBasicParsing
Write-Output "Respaldo: $([math]::Round((Get-Item $bak).Length / 1MB, 1)) MB"

$data = @(Invoke-Sql "SELECT CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS nvarchar(260)) AS P")[0].P
$log = @(Invoke-Sql "SELECT CAST(SERVERPROPERTY('InstanceDefaultLogPath') AS nvarchar(260)) AS P")[0].P
Write-Output "Datos: $data | Log: $log"
Invoke-Sql "RESTORE DATABASE HappyTimesBalloons FROM DISK = N'$bak' WITH MOVE N'HappyTimesBalloons' TO N'${data}HappyTimesBalloons.mdf', MOVE N'HappyTimesBalloons_log' TO N'${log}HappyTimesBalloons_log.ldf', REPLACE, CHECKSUM" | Out-Null
Write-Output 'Base restaurada. Verificando...'
Write-Output (Invoke-Sql "SELECT (SELECT COUNT(*) FROM dbo.Pedidos) AS Pedidos, (SELECT COUNT(*) FROM dbo.AspNetUsers) AS Usuarios, (SELECT TOP 1 MigrationId FROM dbo.__MigrationHistory ORDER BY MigrationId DESC) AS UltimaMigracion" 'HappyTimesBalloons' | Format-Table | Out-String)
Remove-Item $bak
Write-Output 'LISTO'
