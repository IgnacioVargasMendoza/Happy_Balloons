# Happy Times Balloons

Aplicación web para la gestión de pedidos e inventario de una empresa de globos. Migrada desde un prototipo Figma Make (React + Tailwind) hacia una arquitectura ASP.NET MVC 5 con backend real.

## Stack tecnológico

| Capa | Tecnología |
|---|---|
| Lenguaje | C# / .NET Framework 4.8 |
| Framework web | ASP.NET MVC 5 |
| ORM | Entity Framework 6 (Code First + Migrations) |
| Base de datos | SQL Server Express (`Nacho\SQLEXPRESS`) |
| Frontend | Razor Views + Bootstrap 5 + jQuery |
| DI Container | Autofac 6.5.0 + Autofac.Mvc5 6.1.0 |
| Autenticación | ASP.NET Identity con roles |
| Tests | MSTest 3.1.1 + Moq 4.20.70 |
| IDE | Visual Studio 2022 |

## Arquitectura — Solución en 4 proyectos

```
HappyTimesBalloons.slnx
├── HappyTimesBalloons.Abstraccion      ← Interfaces, DTOs, Enums
├── HappyTimesBalloons.AccesoADatos     ← Repositorios EF6, DbContext, Migraciones
├── HappyTimesBalloons.LogicaNegocio    ← Servicios de negocio
├── HappyTimesBalloons.Web              ← Controladores, Vistas Razor, App_Start
└── HappyTimesBalloons.Tests            ← Tests unitarios (MSTest + Moq)
```

**Reglas de dependencia:** Web → LogicaNegocio → AccesoADatos → Abstraccion. Ninguna capa referencia hacia arriba.

## Configuración inicial

### Requisitos previos
- Visual Studio 2022
- SQL Server Express con instancia `Nacho\SQLEXPRESS`
- .NET Framework 4.8 SDK

### Cadena de conexión

La cadena de conexión vive en un archivo separado que **no se sube al repo** porque contiene el nombre del servidor local de cada desarrollador.

1. Copiar el template incluido en el repo:
```
HappyTimesBalloons.Web/connectionStrings.config.example  →  connectionStrings.config
```
2. Editar `connectionStrings.config` con tu instancia de SQL Server:
```xml
<connectionStrings>
  <add name="HappyTimesBallooonsContext"
       connectionString="Data Source=TU_SERVIDOR\SQLEXPRESS;Initial Catalog=HappyTimesBalloons;Integrated Security=True;MultipleActiveResultSets=True"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```
> `connectionStrings.config` está en `.gitignore`. Solo se commitea el `.example`.

### Ejecutar la aplicación
1. Abrir `happy_time_balloons/HappyTimesBalloons.slnx` en Visual Studio 2022.
2. Establecer `HappyTimesBalloons.Web` como proyecto de inicio.
3. Compilar la solución (`Ctrl+Shift+B`).
4. Ejecutar (`F5`). Al primer arranque, EF6 crea la base de datos y ejecuta el seeder automáticamente.

### Credenciales de administrador (seed)
| Campo | Valor |
|---|---|
| Email | `admin@happytimes.com` |
| Contraseña | `Admin@123456` |

## Módulos implementados

| Módulo | Estado | HU |
|---|---|---|
| Autenticación (login / registro) | Completo | HU-AUT-001, HU-CLI-001 |
| Recuperación de contraseña | Completo | — |
| Autenticación de doble factor (2FA) | Completo | — |
| Bitácora de auditoría | Completo | — |
| Layout base + partials compartidos | Completo | — |
| Categorías | Completo | — |
| Productos | Completo | — |
| Pedidos (checkout, mis pedidos, gestión admin) | Completo | — |
| Dashboard de administrador | Completo | — |
| Inyección de dependencias (Autofac) | Completo | — |
| Inventario (consulta, movimientos y stock mínimo) | Completo | HU-INV-001 |
| Tests unitarios (stock mínimo) | Completo | HU-INV-001 T7 |

## Módulos pendientes

- **Promociones** — HU-PRM-001
- **Configuración del sistema** — HU-CFG-001

## Roles del sistema

| Rol | Acceso |
|---|---|
| Administrador | Acceso total — gestión de productos, categorías, pedidos y dashboard |
| Operador | Gestión operativa |
| Cliente | Autoservicio: catálogo, checkout y seguimiento de pedidos |

## Base de datos

La base se genera automáticamente con EF6 Migrations (`AutomaticMigrationsEnabled = true`).  
Las tablas principales son: `Productos`, `Categorias`, `ImagenesProducto`, `Pedidos`, `DetallesPedido`, `ZonasEntrega`, `BitacoraAuditoria`, `RecuperacionTokens`, `CodigoVerificacion2FA` y las tablas de Identity (`AspNet*`).

## Configuración SMTP (2FA y recuperación de contraseña)

El envío de emails usa `System.Net.Mail.SmtpClient` con credenciales leídas desde `AppSettings`.  
Las credenciales reales **nunca se suben al repo** — van en un archivo local ignorado por git:

1. Crear `HappyTimesBalloons.Web/AppSettings.secret.config` (ya está en `.gitignore`):
```xml
<appSettings>
  <add key="Smtp:Host" value="smtp.gmail.com" />
  <add key="Smtp:Port" value="587" />
  <add key="Smtp:Usuario" value="tu-email@gmail.com" />
  <add key="Smtp:Contrasena" value="tu-app-password" />
</appSettings>
```
2. Para Gmail, usar una **Contraseña de aplicación** (no la contraseña normal): Cuenta Google → Seguridad → Verificación en 2 pasos → Contraseñas de aplicación.

Sin esta configuración la app funciona, pero los emails no se envían.

## Inyección de dependencias

Toda la configuración DI está en `Web/App_Start/AutofacConfig.cs`. Al agregar un nuevo repositorio o servicio, registrarlo ahí con `InstancePerRequest`. Los controladores reciben sus dependencias por constructor — nunca usar `new` dentro de acciones.

## Repositorio

- **GitHub:** https://github.com/IgnacioVargasMendoza/Happy_Balloons.git
- **Rama de desarrollo:** `develop`
- **Rama principal:** `main`
- **Prototipo React (referencia):** `prototipo`
