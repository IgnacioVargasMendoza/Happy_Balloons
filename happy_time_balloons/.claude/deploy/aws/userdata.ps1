<powershell>
Start-Transcript -Path C:\setup-happy-balloons.log
Install-WindowsFeature Web-Server, Web-Asp-Net45, Web-Net-Ext45, Web-ISAPI-Ext, Web-ISAPI-Filter, Web-Http-Redirect, Web-Mgmt-Console
New-Item -ItemType Directory -Force C:\AspNetTemp\HappyTimesBalloons | Out-Null
New-Item -ItemType Directory -Force C:\inetpub\HappyTimesBalloons | Out-Null
icacls C:\AspNetTemp /grant "IIS_IUSRS:(OI)(CI)M"
Set-TimeZone -Id "Central America Standard Time"
Stop-Transcript
</powershell>
