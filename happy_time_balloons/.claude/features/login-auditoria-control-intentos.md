# Contexto: Inicio de Sesion con Auditoria y Control de Intentos

## Objetivo de negocio
Permitir que los usuarios (Administrador, Operador/Staff y Cliente) inicien sesion en
Happy Times Balloons con sus credenciales. El sistema debe bloquear temporalmente las cuentas
tras 3 intentos fallidos consecutivos, registrar en BitacoraAuditoria tanto los accesos exitosos
como los fallidos, y redirigir al usuario a la pagina correspondiente segun su rol tras el login.

## Rama
`login-auditoria-control-intentos` — creada desde `develop` el 2026-05-28

## Usuarios y roles
- Roles: Administrador, Operador/Staff, Cliente
- Requiere autenticacion: No (la pantalla de login es publica; el flujo post-login si requiere rol)

## Entidad principal
- Nombre: `ApplicationUser` (ya existe — hereda de `IdentityUser`)
- Relaciones: `BitacoraAuditoria` (tabla ya existente, se escriben eventos de login)
- Atributos clave relevantes: `Id`, `UserName`, `Email`, `LockoutEnabled`,
  `LockoutEndDateUtc`, `AccessFailedCount`, `Nombre` (propiedad extendida)

## Estado del codigo al iniciar esta rama

### Lo que YA existe y NO necesita crearse

| Artefacto | Archivo | Estado |
|-----------|---------|--------|
| ViewModel | `Web/Models/ViewModels/LoginViewModel.cs` | Completo (Email, Contrasena, Recordarme) |
| Vista login | `Web/Views/Cuenta/Login.cshtml` | Completa (Bootstrap 5, validacion, cuentas de prueba) |
| Controlador | `Web/Controllers/CuentaController.cs` | Login/Logout/Registro implementados sin auditoria |
| ApplicationUserManager | `Web/App_Start/ApplicationUserManager.cs` | Completo (lockout 3 intentos / 30 min) |
| Startup.Auth.cs | `Web/App_Start/Startup.Auth.cs` | Completo (cookie OWIN configurada) |
| Modelo EF6 | `AccesoADatos/Modelos/BitacoraAuditoria.cs` | Completo |
| Repositorio auditoria | `AccesoADatos/Repositorios/BitacoraRepositorio.cs` | Completo |
| Interfaz repositorio | `Abstraccion/Interfaces/Repositorios/IBitacoraRepositorio.cs` | Completo |
| Servicio auditoria | `LogicaNegocio/Servicios/AuditoriaServicio.cs` | Completo |
| Interfaz servicio auditoria | `Abstraccion/Interfaces/Servicios/IAuditoriaServicio.cs` | Completo |
| Helper auditoria | `Web/Helpers/AuditoriaHelper.cs` | Existe pero usa `new` directo (antipatron) |
| Auth servicio | `LogicaNegocio/Servicios/AuthServicio.cs` | Solo registro; sin login ni auditoria |
| Interfaz auth | `Abstraccion/Interfaces/Servicios/IAuthServicio.cs` | Solo `RegistrarAsync` |
| Enum TipoOperacion | `Abstraccion/Enums/TipoOperacion.cs` | Crear/Leer/Actualizar/Eliminar — falta IniciarSesion/CerrarSesion/AccesoFallido |
| Registro DI auditoria | `AutofacConfig.cs` | BitacoraRepositorio e IAuditoriaServicio ya registrados |

### Gap critico identificado
`CuentaController.Login` actualmente NO registra auditoria en ningun path (exito, fallo, bloqueo).
`AuditoriaHelper` usa `new ApplicationDbContext()` y `new BitacoraRepositorio()` directamente
(viola la regla DI del proyecto). El helper debe ser eliminado o corregido despues de integrar
`IAuditoriaServicio` mediante inyeccion en el controlador.

## Operaciones confirmadas

| Operacion | Incluir | Justificacion |
|-----------|---------|---------------|
| Pantalla de login (GET) | Existo | Ya implementada, sin cambios necesarios |
| Autenticacion (POST login) | Modificar | Agregar llamadas a IAuditoriaServicio |
| Control de intentos fallidos | Existe | Ya en ApplicationUserManager (3 intentos / 30 min) |
| Registro auditoria acceso exitoso | Nuevo | Escribir en BitacoraAuditoria tras SignIn exitoso |
| Registro auditoria acceso fallido | Nuevo | Escribir en BitacoraAuditoria por cada fallo |
| Registro auditoria cuenta bloqueada | Nuevo | Escribir entrada especial cuando cuenta queda bloqueada |
| Logout con auditoria | Modificar | Registrar cierre de sesion en BitacoraAuditoria |
| Redireccion post-login por rol | Nuevo | Admin/Operador -> /Admin/Index, Cliente -> /Home/Index |

## Reglas de negocio

1. Bloqueo de cuenta: 3 intentos fallidos consecutivos bloquean la cuenta por 30 minutos.
   Configuracion ya en `ApplicationUserManager` (pendiente mover a `ConfiguracionSistema`).
2. Auditoria de acceso exitoso: registrar con `TipoOperacion.IniciarSesion` (valor a agregar al enum),
   tabla `Sesiones`, `RegistroId = null`, detalle = email del usuario.
3. Auditoria de acceso fallido: registrar con `TipoOperacion.AccesoFallido` (valor a agregar al enum),
   tabla `Sesiones`, detalle = email intentado + numero de intento.
4. Auditoria de cuenta bloqueada: registrar con `TipoOperacion.AccesoFallido`, detalle indica bloqueo.
5. Auditoria de logout: registrar con `TipoOperacion.CerrarSesion` (valor a agregar al enum),
   tabla `Sesiones`.
6. Redireccion post-login: si el usuario tiene rol `Administrador` u `Operador`, redirigir a
   `/Admin/Index`; si tiene rol `Cliente`, redirigir a `/Home/Index`. Si hay `returnUrl` valida,
   tiene prioridad sobre la redireccion por rol.
7. Usuario no encontrado: NO revelar si el email existe (mensaje generico "Credenciales incorrectas").
8. La auditoria NO debe fallar el login: si `IAuditoriaServicio.RegistrarAsync` lanza excepcion,
   capturar y loguear en Trace/Debug sin interrumpir el flujo de autenticacion.

## Dependencias
- `IAuditoriaServicio` / `IBitacoraRepositorio` / `AuditoriaServicio` / `BitacoraRepositorio`:
  ya implementados y registrados en Autofac.
- `TipoOperacion` enum: requiere agregar valores `IniciarSesion`, `CerrarSesion`, `AccesoFallido`.
- `ApplicationUserManager`: ya configurado con lockout. No requiere cambios.
- `AutofacConfig.cs`: no requiere cambios (auditoria ya registrada; `IAuthServicio` ya registrado).
- No hay dependencias en otras ramas en desarrollo.

## Plan de implementacion — archivos a modificar/crear

### Paso 1 — Enum (1 archivo modificado)
**Archivo:** `HappyTimesBalloons.Abstraccion/Enums/TipoOperacion.cs`
**Cambio:** Agregar tres valores al enum: `IniciarSesion = 4`, `CerrarSesion = 5`, `AccesoFallido = 6`
**Razon:** BitacoraAuditoria necesita categorizar eventos de sesion de forma tipada.

### Paso 2 — Interfaz IAuthServicio (1 archivo modificado)
**Archivo:** `HappyTimesBalloons.Abstraccion/Interfaces/Servicios/IAuthServicio.cs`
**Cambio:** Agregar metodo `Task<ResultadoLoginDTO> ValidarCredencialesAsync(string email, string contrasena)`
           y `Task RegistrarEventoSesionAsync(string usuarioId, string email, TipoOperacion tipo, string ip, string detalle)`
**Razon:** Centralizar la logica de validacion y auditoria de sesion en la capa de servicio, fuera del controlador.

### Paso 3 — DTO nuevo (1 archivo creado)
**Archivo:** `HappyTimesBalloons.Abstraccion/DTOs/ResultadoLoginDTO.cs`
**Contenido:**
```
ResultadoLoginDTO
  bool Exito
  string UsuarioId
  string Email
  string Nombre
  IList<string> Roles
  EstadoLogin Estado  (enum: Exitoso / CredencialesInvalidas / CuentaBloqueada / UsuarioNoEncontrado)
  int IntentosRestantes
  string MensajeError
```

### Paso 4 — Enum EstadoLogin (1 archivo creado)
**Archivo:** `HappyTimesBalloons.Abstraccion/Enums/EstadoLogin.cs`
**Valores:** `Exitoso`, `CredencialesInvalidas`, `CuentaBloqueada`, `UsuarioNoEncontrado`

### Paso 5 — AuthServicio (1 archivo modificado)
**Archivo:** `HappyTimesBalloons.LogicaNegocio/Servicios/AuthServicio.cs`
**Cambio:** Agregar `ValidarCredencialesAsync` que:
  - Busca usuario por email
  - Verifica lockout
  - Verifica password (usando `UserManager<ApplicationUser>`)
  - Llama `AccessFailedAsync` o `ResetAccessFailedCountAsync`
  - Devuelve `ResultadoLoginDTO` con estado y roles
  Agregar `RegistrarEventoSesionAsync` que delega a `IAuditoriaServicio.RegistrarAsync`
**Nueva dependencia inyectada:** `IAuditoriaServicio` (ya registrado en Autofac)

### Paso 6 — CuentaController (1 archivo modificado)
**Archivo:** `HappyTimesBalloons.Web/Controllers/CuentaController.cs`
**Cambios:**
  - Inyectar `IAuditoriaServicio` via constructor (agregar parametro)
  - En `Login POST`: despues de validar con `ApplicationUserManager`, llamar a
    `IAuditoriaServicio.RegistrarAsync` en todos los paths (exito, fallo, bloqueo)
  - En `Login POST`: implementar redireccion por rol (Admin/Operador -> /Admin/Index,
    Cliente -> /Home/Index), con try/catch en el bloque de auditoria para no interrumpir el flujo
  - En `Logout POST`: llamar a `IAuditoriaServicio.RegistrarAsync` con `TipoOperacion.CerrarSesion`
  - Eliminar dependencia de `GetUserManager()` con `new ApplicationDbContext()` si es posible
    moverlo al servicio (evaluar durante implementacion)

### Paso 7 — AuditoriaHelper (1 archivo a evaluar)
**Archivo:** `HappyTimesBalloons.Web/Helpers/AuditoriaHelper.cs`
**Accion recomendada:** Marcar como `[Obsolete]` o eliminar la clase estatica.
El helper instancia `ApplicationDbContext`, `BitacoraRepositorio` y `AuditoriaServicio` con `new`
directamente, violando la regla DI del proyecto. Si ningun otro controlador lo usa actualmente,
eliminarlo. Si otros lo usan, marcar obsoleto y migrar cada uso a inyeccion de `IAuditoriaServicio`.

### Paso 8 — Registro DI (sin cambios requeridos)
`AutofacConfig.cs` ya tiene registrados `IAuditoriaServicio` y `IAuthServicio`.
Solo se necesita verificar que el constructor actualizado de `CuentaController` no rompa la
resolucion de Autofac (agregar `IAuditoriaServicio` como parametro del constructor esta cubierto).

## Estado de implementacion

| Paso | Descripcion | Archivo(s) | Estado |
|------|-------------|------------|--------|
| 1 | Agregar valores al enum TipoOperacion | `Abstraccion/Enums/TipoOperacion.cs` | Pendiente |
| 2 | Crear enum EstadoLogin | `Abstraccion/Enums/EstadoLogin.cs` | Pendiente |
| 3 | Crear DTO ResultadoLoginDTO | `Abstraccion/DTOs/ResultadoLoginDTO.cs` | Pendiente |
| 4 | Modificar IAuthServicio | `Abstraccion/Interfaces/Servicios/IAuthServicio.cs` | Pendiente |
| 5 | Modificar AuthServicio (logica login + auditoria) | `LogicaNegocio/Servicios/AuthServicio.cs` | Pendiente |
| 6 | Modificar CuentaController (auditoria + redireccion por rol) | `Web/Controllers/CuentaController.cs` | Pendiente |
| 7 | Evaluar y limpiar AuditoriaHelper | `Web/Helpers/AuditoriaHelper.cs` | Pendiente |
| 8 | Verificar DI (sin cambios en AutofacConfig) | `Web/App_Start/AutofacConfig.cs` | Sin cambios |

## Casos de prueba requeridos (HU-AUT-001)

| Escenario | Input | Resultado esperado |
|-----------|-------|-------------------|
| Credenciales correctas (Admin) | admin@happytimes.com / Admin@123456 | Redireccion a /Admin/Index, registro BitacoraAuditoria (IniciarSesion) |
| Credenciales correctas (Cliente) | email cliente / contrasena correcta | Redireccion a /Home/Index, registro BitacoraAuditoria (IniciarSesion) |
| Contrasena incorrecta (1er intento) | email valido / pass incorrecto | Mensaje "Credenciales incorrectas. Te quedan 2 intento(s).", registro AccesoFallido |
| Contrasena incorrecta (2do intento) | email valido / pass incorrecto | Mensaje "Credenciales incorrectas. Te quedan 1 intento(s).", registro AccesoFallido |
| 3er intento fallido (bloqueo) | email valido / pass incorrecto | Mensaje "Cuenta bloqueada por multiples intentos fallidos.", registro AccesoFallido con detalle de bloqueo |
| Cuenta ya bloqueada | email bloqueado / cualquier pass | Mensaje "Cuenta bloqueada temporalmente.", registro AccesoFallido |
| Email no registrado | inexistente@test.com | Mensaje generico "Credenciales incorrectas." (sin revelar si existe), registro AccesoFallido |
| Logout | Usuario autenticado hace clic en Salir | Redireccion a /Home/Index, registro BitacoraAuditoria (CerrarSesion) |

## Log de cambios

| Fecha | Tipo | Descripcion | Agente |
|-------|------|-------------|--------|
| 2026-05-28 | Inicializacion | Rama creada, contexto documentado, analisis de codigo existente completado | feature-planner |
