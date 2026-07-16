# Contexto: Configuracion de Reglas Operativas y Logistica

## Objetivo de negocio
Happy Times Balloons opera con reglas que hoy estan hardcodeadas en el codigo fuente:
los domingos no se hacen entregas, el pedido puede programarse hasta 30 dias adelante,
y no existe un limite de tiempo para cancelar. El negocio necesita poder ajustar estas
reglas desde una pantalla de administracion sin tocar codigo. Este modulo crea una tabla
`ConfiguracionSistema` de pares clave/valor que centraliza todas las reglas operativas
y logisticas, y provee una UI para que el Administrador las consulte y modifique.

## Por que ConfiguracionSistema con pares clave/valor

Se recomienda esta arquitectura (una sola tabla, columnas `Clave` y `Valor`) en lugar de
una tabla con columnas fijas porque:
- Agregar una nueva regla en el futuro requiere solo insertar una fila, no una migracion de BD.
- El Admin puede editar cualquier valor desde la misma pantalla generica sin cambios de UI.
- Es el patron estandar en sistemas de gestion (similar a "App Settings" en la BD).
- Las claves son constantes en codigo (no magic strings en vistas), lo que mantiene seguridad de tipos.

## Rama
`configuracion-reglas-operativas` — creada desde `develop` el 2026-07-15

## Usuarios y roles
- Rol(es): Administrador (lectura y edicion de todas las reglas)
- Requiere autenticacion: Si (todas las acciones requieren rol Administrador)

## Entidad principal
- Nombre: `ConfiguracionSistema`
- Tabla BD: `ConfiguracionesSistema`
- Relaciones: ninguna FK hacia otras tablas (entidad autonoma)
- Atributos clave:
  - `Id` (int, identity, PK)
  - `Clave` (string, max 100, unique, required) — identificador de la regla
  - `Valor` (string, max 500, required) — valor serializado como string
  - `Descripcion` (string, max 300, nullable) — texto legible para el Admin en la UI
  - `FechaUltimaModificacion` (DateTime UTC)
  - `UsuarioUltimaModificacion` (string, nullable) — ID del usuario que editó por ultima vez

## Claves concretas de configuracion

| Clave                              | Valor por defecto | Descripcion para el Admin                                         |
|------------------------------------|-------------------|-------------------------------------------------------------------|
| `EntregaDiasMinimosAdelante`       | `1`               | Minimo de dias de anticipacion para programar una entrega         |
| `EntregaDiasMaximosAdelante`       | `30`              | Maximo de dias en el futuro para programar una entrega            |
| `EntregaDiasHabilitados`           | `1,2,3,4,5,6`    | Dias de la semana con entrega (1=Lunes ... 7=Domingo), separados por coma |
| `PedidoHorasMaximasCancelacion`    | `24`              | Horas desde la creacion del pedido durante las cuales el cliente puede cancelar |
| `PedidoMontoMinimo`                | `0`               | Monto minimo en colones para confirmar un pedido (0 = sin minimo) |
| `AtencionHoraCorte`                | `17:00`           | Hora limite para recibir pedidos nuevos (formato HH:mm, 24h)      |

## Modulos del sistema afectados por estas claves

| Clave                           | Modulo / archivo afectado                              | Cambio requerido                                  |
|---------------------------------|--------------------------------------------------------|---------------------------------------------------|
| `EntregaDiasMinimosAdelante`    | `ProgramacionEntregaServicio.cs` — const hardcodeada   | Reemplazar `DiasMinimosAdelante = 1` por lectura dinamica  |
| `EntregaDiasMaximosAdelante`    | `ProgramacionEntregaServicio.cs` — const hardcodeada   | Reemplazar `DiasMaximosAdelante = 30` por lectura dinamica |
| `EntregaDiasHabilitados`        | `ProgramacionEntregaServicio.cs` — `DayOfWeek.Sunday`  | Reemplazar bloqueo fijo de domingo por lista dinamica      |
| `PedidoHorasMaximasCancelacion` | No implementado aun                                    | Nueva validacion en `PedidoServicio` al intentar cancelar  |
| `PedidoMontoMinimo`             | No implementado aun                                    | Validacion en checkout antes de confirmar pedido           |
| `AtencionHoraCorte`             | No implementado aun                                    | Validacion al registrar pedido nuevo                       |

## Operaciones confirmadas
| Operacion            | Incluir | Justificacion                                                   |
|----------------------|---------|-----------------------------------------------------------------|
| Listar               | Si      | Vista principal: tabla con todas las claves y sus valores       |
| Ver detalle          | No      | La lista ya muestra descripcion; un detalle separado es exceso  |
| Crear                | No      | Las claves se crean via migracion de seed; el Admin solo edita  |
| Editar               | Si      | El Admin modifica el Valor de una clave existente               |
| Eliminar             | No      | Las claves son parte del esquema del sistema; no se eliminan    |
| Auditoria            | Si      | Cada edicion registra en BitacoraAuditoria + FechaUltimaModificacion |

## Tareas de esta historia de usuario

| # | Tarea                                     | Estado      | Nota                                              |
|---|-------------------------------------------|-------------|---------------------------------------------------|
| 1 | Registrar zonas de entrega                | Completa    | Modulo `zonas-entrega` implementado y mergeado    |
| 2 | Configurar tiempo maximo de cancelacion   | Pendiente   | Clave `PedidoHorasMaximasCancelacion` en tabla    |
| 3 | Configurar dias de entrega habilitados    | Pendiente   | Reemplaza hardcode `DayOfWeek.Sunday`             |
| 4 | Configurar dias minimos/maximos adelante  | Pendiente   | Reemplaza consts en `ProgramacionEntregaServicio` |
| 5 | Configurar monto minimo de pedido         | Pendiente   | Validacion nueva en checkout                      |
| 6 | Configurar hora de corte de pedidos       | Pendiente   | Validacion nueva al crear pedido                  |
| 7 | Pantalla de administracion (Listar/Editar)| Pendiente   | UI para que el Admin vea y edite todas las claves |

## Reglas de negocio
1. Las claves de la tabla `ConfiguracionesSistema` son fijas (definidas en seed); el Admin
   solo puede editar el campo `Valor`, no crear ni eliminar claves.
2. Cada edicion del Valor registra en `BitacoraAuditoria` (tabla, clave modificada, valor anterior, valor nuevo, usuario).
3. `FechaUltimaModificacion` y `UsuarioUltimaModificacion` se actualizan en el servicio al guardar.
4. Los valores se leen como string y se parsean en cada servicio consumidor (int, decimal, TimeSpan, List<int>).
5. El servicio de configuracion debe proveer metodos tipados: `ObtenerIntAsync(clave)`,
   `ObtenerDecimalAsync(clave)`, `ObtenerListaEnteroAsync(clave)`, `ObtenerStringAsync(clave)`.
6. Si una clave no existe en BD, el servicio retorna el valor por defecto codificado como fallback
   (no falla silenciosamente).
7. Solo usuarios con rol Administrador pueden acceder al controlador de configuracion.

## Pendientes por aclarar con el negocio
- Hora de corte: a las 17:00 el sistema deja de recibir pedidos para ese dia, o para el dia siguiente?
- Cancelacion: el tiempo maximo aplica solo a clientes, o tambien operadores pueden cancelar sin limite?
- Dias de entrega: el sistema bloquea la seleccion de fecha en el calendario, o tambien rechaza pedidos ya existentes?

## Dependencias
- `IBitacoraRepositorio` / `BitacoraRepositorio` — ya existe; se usa para auditoria de ediciones.
- `ResultadoOperacionDTO` — ya existe; retorno estandar de operaciones de escritura.
- `ProgramacionEntregaServicio` — se modificara para leer dias y limites desde `IConfiguracionServicio`.
- `ApplicationDbContext` — debe agregar `DbSet<ConfiguracionSistema> ConfiguracionesSistema`.
- `AutofacConfig.cs` — debe registrar `IConfiguracionRepositorio` e `IConfiguracionServicio`.

## Esquema de base de datos normalizado

> Validado por `data-normalizer` el 2026-07-15

### Tabla: `ConfiguracionesSistema`

| Campo | Tipo C# | Tipo SQL | Restricciones |
|---|---|---|---|
| Id | int | int IDENTITY | PK |
| Clave | string | nvarchar(100) NOT NULL | Indice unico `IX_ConfiguracionesSistema_Clave`; declarar con `[Index("IX_ConfiguracionesSistema_Clave", IsUnique = true)]` en el modelo |
| Valor | string | nvarchar(500) NOT NULL | |
| Descripcion | string | nvarchar(300) NULL | |
| FechaUltimaModificacion | DateTime | datetime2 NOT NULL | Inicializar en UTC desde el servicio, no desde el modelo |
| UsuarioUltimaModificacion | string | nvarchar(128) NULL | Almacena `ApplicationUser.Id` sin FK formal; max 128 alineado con `BitacoraAuditoria.UsuarioId` |

### Relaciones

Ninguna. Entidad autonoma sin FKs hacia otras tablas.

### Entidades adicionales detectadas

Ninguna. El esquema no requiere tablas auxiliares.

### Advertencias pendientes

- **Inconsistencia en numeracion de `EntregaDiasHabilitados`**: el contexto (seccion "Claves concretas") dice `1=Lunes...7=Domingo` (numeracion 1-7), pero la propuesta de seed usa valor `1,2,3,4,5,6` excluyendo el 7. El servicio consumidor (`ConfiguracionServicio.ObtenerListaEnteroAsync`) debe parsear con una sola convencion. Aclarar con el negocio si el rango es 1-6 (sin domingo) o 1-7 (7=Domingo excluido por defecto) antes de implementar.

- **Colision de nombre de controlador**: ya existe `ConfiguracionController.cs` para perfil de usuario. El controlador de este modulo debe llamarse `ReglasOperativasController` y las vistas deben ubicarse en `Views/ReglasOperativas/`. Los pasos 7, 8 y 9 del estado de implementacion han sido corregidos a continuacion.

## Estado de implementacion
| Paso | Descripcion                                                      | Estado      |
|------|------------------------------------------------------------------|-------------|
| 1    | DTO: ConfiguracionSistemaDTO                                     | Pendiente   |
| 2    | Interfaz repositorio: IConfiguracionRepositorio                  | Pendiente   |
| 3    | Interfaz servicio: IConfiguracionServicio                        | Pendiente   |
| 4    | Modelo EF6: ConfiguracionSistema.cs                              | Pendiente   |
| 5    | Repositorio: ConfiguracionRepositorio.cs                         | Pendiente   |
| 6    | Servicio: ConfiguracionServicio.cs                               | Pendiente   |
| 7    | ViewModel: ReglasOperativasViewModel.cs                          | Pendiente   |
| 8    | Controlador: ReglasOperativasController.cs (evita colision con ConfiguracionController de perfil) | Pendiente |
| 9    | Vistas Razor: Views/ReglasOperativas/Index.cshtml + Editar.cshtml | Pendiente  |
| 10   | Registro DI en AutofacConfig.cs                                  | Pendiente   |
| 11   | Seed inicial de claves en migracion EF6                          | Pendiente   |
| 12   | Modificar ProgramacionEntregaServicio — leer desde BD            | Pendiente   |

## Log de cambios
| Fecha      | Tipo           | Descripcion                                                                                                                                                            | Agente          |
|------------|----------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-----------------|
| 2026-07-15 | Inicializacion | Rama creada, contexto documentado, tareas deducidas, arquitectura ConfiguracionSistema propuesta                                                                       | feature-planner |
| 2026-07-15 | Normalizacion  | Esquema validado a 3FN; 0 errores criticos; 2 advertencias registradas: inconsistencia en numeracion de EntregaDiasHabilitados y colision de nombre de controlador corregida a ReglasOperativasController | data-normalizer |
