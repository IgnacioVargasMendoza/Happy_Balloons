# Publica HappyTimesBalloons.Web en el EC2 de AWS.
# Uso (desde la raiz del repo):
#   .\.claude\deploy\aws\publicar.ps1 -InstanceId i-xxxxxxxx
# Requiere: AWS CLI v2 con sesion activa (aws login --profile <perfil>) y Visual Studio con MSBuild.
param(
    [Parameter(Mandatory = $true)][string]$InstanceId,
    [string]$Perfil = 'happy-balloons',
    [string]$Region = 'us-east-2',
    [string]$Bucket = 'happy-balloons-deploy-613852762704'
)
$ErrorActionPreference = 'Stop'
$aws = 'C:\Program Files\Amazon\AWSCLIV2\aws.exe'
$p = @('--region', $Region, '--profile', $Perfil)
$aqui = $PSScriptRoot
$raiz = (Resolve-Path "$aqui\..\..\..").Path
$proyecto = Join-Path $raiz 'HappyTimesBalloons.Web'
$temp = Join-Path $env:TEMP 'happy-balloons-publish'
$salida = Join-Path $temp 'publish'
$zip = Join-Path $temp 'happy-balloons-web.zip'
New-Item -ItemType Directory -Force $temp | Out-Null

Write-Output '1/5 Compilando en Release...'
$msbuild = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
& $msbuild (Join-Path $proyecto 'HappyTimesBalloons.Web.csproj') /restore /p:Configuration=Release /v:minimal /nologo /clp:ErrorsOnly
if ($LASTEXITCODE -ne 0) { throw 'Fallo la compilacion' }

Write-Output '2/5 Empaquetando...'
& "$aqui\empaquetar.ps1" -Proyecto $proyecto -Salida $salida
$uploads = Join-Path $proyecto 'Content\Uploads\Productos'
if (Test-Path $uploads) {
    New-Item -ItemType Directory -Force "$salida\Content\Uploads" | Out-Null
    Copy-Item -Recurse -Force $uploads "$salida\Content\Uploads\"
}
if (Test-Path $zip) { Remove-Item $zip }
Compress-Archive -Path "$salida\*" -DestinationPath $zip

Write-Output '3/5 Subiendo a S3 temporal...'
& $aws s3api create-bucket --bucket $Bucket --create-bucket-configuration "LocationConstraint=$Region" @p | Out-Null
& $aws s3 cp $zip "s3://$Bucket/happy-balloons-web.zip" @p --only-show-errors
$url = & $aws s3 presign "s3://$Bucket/happy-balloons-web.zip" --expires-in 3600 @p

try {
    Write-Output '4/5 Desplegando en IIS por SSM...'
    $script = (Get-Content "$aqui\desplegar.ps1" -Raw).Replace('{{ZIP_URL}}', $url)
    $parametros = Join-Path $temp 'ssm-params.json'
    $json = @{ commands = @($script -split "`r?`n"); executionTimeout = @('1200') } | ConvertTo-Json -Depth 3
    [IO.File]::WriteAllText($parametros, $json)
    $id = & $aws ssm send-command --instance-ids $InstanceId --document-name AWS-RunPowerShellScript --comment 'Desplegar HappyTimesBalloons' --parameters "file://$parametros" @p --query 'Command.CommandId' --output text
    Remove-Item $parametros
    do {
        Start-Sleep -Seconds 10
        $estado = & $aws ssm get-command-invocation --command-id $id --instance-id $InstanceId @p --query Status --output text 2>$null
    } while (-not $estado -or $estado -in 'Pending', 'InProgress', 'Delayed')
    & $aws ssm get-command-invocation --command-id $id --instance-id $InstanceId @p --query '[StandardOutputContent,StandardErrorContent]' --output text
    Write-Output "Estado: $estado"
}
finally {
    Write-Output '5/5 Eliminando bucket temporal...'
    & $aws s3 rb "s3://$Bucket" --force @p | Out-Null
}
