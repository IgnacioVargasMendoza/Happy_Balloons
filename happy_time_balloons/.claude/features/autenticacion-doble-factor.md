# Contexto: Autenticacion de Doble Factor (2FA)

## Objetivo de negocio
Reforzar la seguridad del acceso a la aplicacion exigiendo un segundo factor de verificacion
despues del login con contrasena. Al iniciar sesion, el sistema genera un codigo numerico de
6 digitos, lo envia al correo electronico del usuario y le da 5 minutos para ingresarlo.
Solo si el codigo es valido y no ha expirado, el usuario obtiene acceso. Cada intento de
verificacion queda registrado en BitacoraAuditoria para trazabilidad completa.

## Rama
`autenticacion-doble-factor` — creada desde `develop` el 2026-05-28

## Usuarios y roles
- Roles: Administrador, Operador, Cliente (todos los usuarios autenticados)
- Requiere autenticacion: Si — el 2FA es el segundo paso del login existente

## Entidades afectadas

### Nueva entidad: `CodigoVerificacion2FA`
| Propiedad        | Tipo          | Descripcion                                      |
|------------------|---------------|--------------------------------------------------|
| Id               | int, PK       | Identidad autoincremental                        |
| UsuarioId        | string, FK    | FK hacia AspNetUsers.Id (ASP.NET Identity)       |
| Codigo           | string(6)     | Codigo numerico de 6 digitos                     |
| FechaCreacion    | DateTime      | Timestamp UTC de generacion del codigo           |
| FechaExpiracion  | DateTime      | FechaCreacion + 5 minutos (TTL)                  |
| Utilizado        | bool          | true si ya fue validado exitosamente             |
| IntentosFallidos | int           | Contador de intentos incorrectos (max 3)         |

### Entidad existente: `BitacoraAuditoria`
Se registra en esta tabla para cada evento 2FA:
- Codigo generado y enviado
- Validacion exitosa
- Validacion fallida (codigo incorrecto)
- Codigo expirado al intentar validar
- Bloqueo por intentos maximos superados

## Flujo de usuario completo

```
[1] Usuario ingresa email + contrasena en /Account/Login
        |
        v
[2] ASP.NET Identity valida credenciales
        |
        v (credenciales correctas)
[3] Sistema genera codigo de 6 digitos aleatorio
    Sistema guarda CodigoVerificacion2FA en BD (TTL 5 min)
    Sistema envia codigo al email del usuario
    Sistema registra en BitacoraAuditoria: "Codigo 2FA generado"
        |
        v
[4] Usuario es redirigido a /Account/Verificar2FA
    (NO recibe sesion todavia — sesion parcial con flag 2FA pendiente)
        |
        v
[5] Usuario ingresa el codigo de 6 digitos
        |
        +--- Codigo valido y no expirado y no utilizado
        |           |
        |           v
        |    [6a] Marca CodigoVerificacion2FA.Utilizado = true
        |         Completa la sesion del usuario (SignIn definitivo)
        |         Registra en BitacoraAuditoria: "2FA exitoso"
        |         Redirige a /Home/Index o returnUrl
        |
        +--- Codigo incorrecto (IntentosFallidos < 3)
        |           |
        |           v
        |    [6b] Incrementa IntentosFallidos
        |         Muestra error "Codigo incorrecto, X intentos restantes"
        |         Registra en BitacoraAuditoria: "2FA fallido - codigo incorrecto"
        |
        +--- IntentosFallidos >= 3
        |           |
        |           v
        |    [6c] Invalida el codigo (Utilizado = true)
        |         Redirige a Login con mensaje de error
        |         Registra en BitacoraAuditoria: "2FA bloqueado - intentos maximos"
        |
        +--- Codigo expirado
                    |
                    v
             [6d] Muestra error "Codigo expirado"
                  Ofrece enlace para reenviar codigo
                  Registra en BitacoraAuditoria: "2FA fallido - codigo expirado"
```

## Operaciones confirmadas
| Operacion              | Incluir | Justificacion                                     |
|------------------------|---------|---------------------------------------------------|
| Generar codigo         | Si      | Nucleo de la funcionalidad                        |
| Enviar por email       | Si      | Canal de entrega del segundo factor               |
| Mostrar pantalla 2FA   | Si      | Vista /Account/Verificar2FA                       |
| Validar codigo         | Si      | Accion POST que completa o rechaza el acceso      |
| Reenviar codigo        | Si      | Permite al usuario pedir un nuevo codigo          |
| Expirar codigos        | Si      | TTL de 5 minutos, controlado al momento de validar|
| Registrar auditoria    | Si      | Todo evento 2FA va a BitacoraAuditoria            |
| Listar codigos (admin) | No      | Fuera del alcance de esta HU                      |

## Reglas de negocio
1. Un codigo expira exactamente 5 minutos despues de su generacion (FechaExpiracion = FechaCreacion + 5 min).
2. Un codigo ya utilizado (Utilizado = true) no puede reutilizarse, aunque no haya expirado.
3. Maximo 3 intentos fallidos por codigo. Al superar el limite, el codigo queda invalidado.
4. Solo el codigo mas reciente del usuario es valido. Codigos anteriores no utilizados quedan obsoletos.
5. El usuario NO tiene sesion activa entre el paso de login y la validacion 2FA exitosa.
6. El envio de email usa SmtpClient con configuracion en Web.config (sin paquetes externos nuevos).
7. El codigo se genera con System.Security.Cryptography.RandomNumberGenerator para garantizar aleatoriedad segura.
8. Todo evento de generacion, validacion exitosa, fallo y bloqueo se registra en BitacoraAuditoria.
9. Al reenviar un codigo, el anterior queda marcado como Utilizado = true antes de generar el nuevo.

## Dependencias
- ASP.NET Identity ya implementado (tabla AspNetUsers, AccountController existente)
- BitacoraAuditoria ya existe en BD y en el proyecto
- SmtpClient / configuracion SMTP debe estar en Web.config (nueva configuracion si no existe)
- No depende de features en desarrollo paralelo

## Capas afectadas
| Capa              | Cambio                                                                 |
|-------------------|------------------------------------------------------------------------|
| Abstraccion       | DTO nuevo, interfaz repositorio nueva, interfaz servicio nueva         |
| AccesoADatos      | Modelo EF6 nuevo, repositorio nuevo, registro en ApplicationDbContext  |
| LogicaNegocio     | Servicio nuevo (genera, envia, valida, audita)                         |
| Web               | Modificacion AccountController, ViewModel nuevo, Vista nueva           |
| ApplicationDbContext | Nuevo DbSet<CodigoVerificacion2FA>                                  |
| AutofacConfig.cs  | Registro del repositorio e interfaz de servicio nuevos                 |

## Criterios de aceptacion
- [ ] Al hacer login correcto, el usuario es redirigido a /Account/Verificar2FA y NO obtiene sesion.
- [ ] El usuario recibe un email con un codigo numerico de exactamente 6 digitos.
- [ ] Un codigo valido ingresado en menos de 5 minutos completa el login exitosamente.
- [ ] Un codigo incorrecto muestra el mensaje de error con los intentos restantes.
- [ ] Tres intentos fallidos invalidan el codigo y redirigen al Login.
- [ ] Un codigo ingresado despues de 5 minutos muestra "Codigo expirado".
- [ ] El enlace "Reenviar codigo" genera un nuevo codigo e invalida el anterior.
- [ ] Cada evento 2FA aparece registrado en BitacoraAuditoria con usuario, timestamp y tipo.
- [ ] El codigo ya usado no puede reutilizarse aunque no haya expirado.

## Estado de implementacion
| Paso | Descripcion                                      | Estado       |
|------|--------------------------------------------------|--------------|
| 1    | DTO (CodigoVerificacion2FADTO)                   | Pendiente    |
| 2    | Interfaz repositorio (ICodigoVerificacion2FARepo)| Pendiente    |
| 3    | Interfaz servicio (IAutenticacion2FAServicio)    | Pendiente    |
| 4    | Modelo EF6 (CodigoVerificacion2FA)               | Pendiente    |
| 5    | Repositorio (CodigoVerificacion2FARepositorio)   | Pendiente    |
| 6    | Servicio (Autenticacion2FAServicio)              | Pendiente    |
| 7    | ViewModel (Verificar2FAViewModel)                | Pendiente    |
| 8    | Controlador (modificar AccountController)        | Pendiente    |
| 9    | Vista Razor (Verificar2FA.cshtml)                | Pendiente    |
| 10   | Registro DI en AutofacConfig.cs                  | Pendiente    |

## Notas tecnicas para el implementador
- El servicio de 2FA (paso 6) es el mas complejo: contiene la logica de generacion con
  RandomNumberGenerator, envio via SmtpClient, validacion con todas las reglas de negocio,
  y la escritura a BitacoraAuditoria.
- AccountController (paso 8) debe modificarse en la accion Login POST para interceptar el
  flujo despues de ValidateUser exitoso y redirigir a Verificar2FA en lugar de hacer SignIn.
- Se recomienda usar TempData o Session para pasar el UsuarioId entre el Login y el paso 2FA,
  sin exponer informacion en la URL.
- El envio de email en ASP.NET MVC 5 usa System.Net.Mail.SmtpClient con configuracion
  en la seccion <system.net><mailSettings> del Web.config. No instalar MailKit ni SendGrid
  sin verificar compatibilidad con .NET Framework 4.8 primero.

## Log de cambios
| Fecha      | Tipo           | Descripcion                                                   | Agente          |
|------------|----------------|---------------------------------------------------------------|-----------------|
| 2026-05-28 | Inicializacion | Rama creada, contexto documentado, alcance confirmado         | feature-planner |
