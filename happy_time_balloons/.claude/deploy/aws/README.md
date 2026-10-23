# Deploy en AWS — Happy Times Balloons

Cómo está publicada la aplicación y cómo volver a publicarla. Desplegado el 2026-10-19.

## Arquitectura

```
Internet ──HTTP 80──► EC2 Windows Server 2022 (t3.small, us-east-2c)
                      ├── IIS: sitio "HappyTimesBalloons" → C:\inetpub\HappyTimesBalloons
                      │        Application Pool "HappyTimesBalloons" (.NET v4.0, Integrated,
                      │        ApplicationPoolIdentity)
                      └── SQL Server 2025 Express (.\SQLEXPRESS), base HappyTimesBalloons
```

- **Una sola instancia EC2** con IIS y SQL Server Express. No hay RDS.
- **Región `us-east-2` (Ohio).** El servidor tiene una Elastic IP fija.
- **Créditos de CPU en modo `standard`.** Nunca cobra CPU extra: si se acaban los créditos, solo baja el rendimiento.
- **Administración remota por SSM Run Command.** El rol IAM `happy-balloons-ec2-role` tiene `AmazonSSMManagedInstanceCore`; no hace falta RDP para desplegar.
- **Security Group `happy-balloons-web-sg`:** RDP 3389 solo desde la IP del desarrollador, y 80/443 abiertos. El puerto 1433 de SQL **no** está expuesto.

### Por qué no RDS
Se probó RDS SQL Server Express en `db.t3.micro`. Con 1 GB de RAM la instancia consumía ~42% de CPU sin ninguna conexión. Como RDS T3 siempre está en modo *unlimited*, eso generaba ~US$2.25/día en "CPU Credits", unos US$85/mes con la base vacía. Se eliminó y quedó el snapshot final `db-happy-times-balloons-final-2026-10-19`.

## Configuración en el servidor (no está en git)

| Archivo | Contenido |
|---|---|
| `C:\inetpub\HappyTimesBalloons\connectionStrings.config` | `Data Source=.\SQLEXPRESS;Initial Catalog=HappyTimesBalloons;Integrated Security=True;MultipleActiveResultSets=True` (nombre `HappyTimesBallooonsContext`) |
| `AppSettings.secret.config` | **Pendiente.** Sin él no se envían correos SMTP. |

- **Login de SQL del pool:** `IIS APPPOOL\HappyTimesBalloons`, con rol `db_owner` en la base. Necesita `db_owner` porque `Global.asax` usa `MigrateDatabaseToLatestVersion` y aplica migraciones al arrancar.
- **Límite de memoria de SQL Server:** `max server memory` = 1024 MB, para dejar RAM a IIS.
- **Permisos de carpetas para el pool:** lectura en el sitio, y modificación en `Content\Uploads` y `C:\AspNetTemp`. `Web.config` usa `tempDirectory="C:\AspNetTemp\HappyTimesBalloons"`.
- **Zona horaria:** Central America Standard Time, porque la app usa hora local para `FechaPedido`.

## Volver a publicar

Desde la raíz del repo, con sesión AWS activa:

```powershell
aws login --region us-east-2 --profile happy-balloons
.\.claude\deploy\aws\publicar.ps1 -InstanceId <id-de-la-instancia>
```

`publicar.ps1` hace lo siguiente:
1. Compila en Release con MSBuild.
2. Arma el paquete con `empaquetar.ps1`:
   - Incluye `bin` y los ítems `<Content>` del `.csproj`, más `Content\Uploads\Productos`.
   - Aplica a mano la transformación Release del `Web.config`: quita `debug` y pone `customErrors="RemoteOnly"`.
   - Excluye `connectionStrings.config` y `AppSettings.secret.config`.
3. Sube el zip a un bucket S3 temporal y genera un enlace firmado de 1 hora.
4. Ejecuta `desplegar.ps1` en el EC2 por SSM:
   - Reemplaza los archivos sin tocar `Content\` (las imágenes subidas desde la app se conservan).
   - Recrea `connectionStrings.config`.
   - Asegura el sitio, el pool, los permisos y el login de SQL.
   - Prueba `/` y `/Cuenta/Login` en el propio servidor.
5. Borra el bucket temporal.

**Por qué el empaquetado es manual:** el `.csproj` importa `Microsoft.WebApplication.targets` con `Condition="false"`. Por eso `DeployOnBuild`/`publishUrl` compila pero no publica.

Después de publicar, prueba en el navegador con la IP pública.

## Otros scripts

| Script | Uso |
|---|---|
| `userdata.ps1` | User-data al crear la instancia: instala IIS + ASP.NET 4.8, crea `C:\AspNetTemp` y configura la zona horaria. |
| `instalar-sql.ps1` | Por SSM: instala SQL Server 2025 Express desde `aka.ms/sql2025express` y restaura un `.bak`. `{{BAK_URL}}` debe ser un enlace firmado de S3. |

**Importante:** la base local es SQL Server 2025 (v17). Un `.bak` de 2025 solo se puede restaurar en 2025 o superior.

## Pendientes
- **HTTPS:** requiere un dominio apuntando a la Elastic IP y win-acme (Let's Encrypt) en IIS. Lo necesitan el login y el webhook SINPE.
- **SMTP:** crear `AppSettings.secret.config` en el servidor con las credenciales (contraseña de aplicación de Gmail).
- **SINPE:** agregar sus claves de configuración en el servidor.
- **Respaldos:** no hay respaldo automático de SQL Server. Programar un `BACKUP DATABASE` o snapshots EBS.
- **Costo:** el EC2 cuesta ~US$0.04/h con licencia Windows. Detenerlo cuando no se use; la Elastic IP se mantiene.
