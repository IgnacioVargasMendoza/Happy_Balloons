# Contexto: Configuracion de Zonas de Entrega

## Objetivo de negocio
Happy Times Balloons entrega pedidos en distintas zonas geograficas, cada una con un costo
de envio diferente. Sin un modulo de administracion de zonas, el equipo no puede registrar
nuevas zonas, actualizar tarifas, desactivar zonas sin cobertura ni auditar quien hizo cada
cambio. Este modulo permite al Administrador gestionar el catalogo completo de zonas de
entrega desde una interfaz dedicada, con validaciones de cobertura y registro de auditoria
en cada operacion critica.

## Rama
`zonas-entrega` — creada desde `develop` el 2026-07-15

## Usuarios y roles
- Rol(es): Administrador (gestion completa CRUD + auditoria)
- Requiere autenticacion: Si (todas las acciones requieren rol Administrador)

## Entidad principal
- Nombre: `ZonaEntrega`
- Tabla BD: `ZonasEntrega`
- Relaciones:
  - `Pedidos.ZonaEntregaId` → `ZonasEntrega.Id` (FK existente; una zona puede tener muchos pedidos)
- Atributos clave (modelo ya existe en AccesoADatos):
  - `Id` (int, identity, PK)
  - `Nombre` (string, max 100, required)
  - `Descripcion` (string, max 500, nullable)
  - `CostoEnvio` (decimal, required, >= 0)
  - `EsDisponible` (bool, indica si la zona esta activa para nuevos pedidos)
  - `FechaCreacion` (DateTime UTC, se asigna al crear)

## Operaciones confirmadas
| Operacion               | Incluir | Justificacion                                         |
|-------------------------|---------|-------------------------------------------------------|
| Listar                  | Si      | Tarea 1 — vista principal del modulo de configuracion |
| Ver detalle             | Si      | Tarea 1 — permite revisar zona antes de editar        |
| Crear                   | Si      | Tarea 2 — registrar zonas de entrega nuevas           |
| Editar (costo)          | Si      | Tarea 3 — actualizar costos de entrega                |
| Eliminar (logico)       | Si      | Tarea 4 — desactivar zona (EsDisponible = false)      |
| Eliminar (fisico)       | No      | Las zonas estan referenciadas por Pedidos existentes  |
| Validar cobertura       | Si      | Tarea 5 — no crear zona duplicada por nombre          |
| Auditoria               | Si      | Tarea 6 — registrar en BitacoraAuditoria cada CRUD    |
| Estadisticas            | No      | Fuera del alcance de esta historia de usuario         |

## Reglas de negocio
1. Nombre unico: no pueden existir dos zonas con el mismo `Nombre` (case-insensitive).
2. CostoEnvio debe ser >= 0; no se permiten valores negativos.
3. Al crear una zona, `FechaCreacion` se asigna en el servicio como `DateTime.UtcNow`.
4. La eliminacion es logica: se cambia `EsDisponible = false`. Nunca se borra el registro
   porque `Pedidos` existentes referencian la zona.
5. Una zona con `EsDisponible = false` NO debe aparecer en el selector del checkout
   (el repositorio ya filtra `Where(z => z.EsDisponible)` en `ObtenerTodasAsync`).
6. Toda operacion de Crear, Editar y cambio de disponibilidad registra entrada en
   `BitacoraAuditoria` via `IBitacoraRepositorio.GuardarAsync(BitacoraEntradaDTO)` con el
   usuario autenticado, tabla afectada (`ZonasEntrega`), tipo de operacion y ID del registro.
   No se usa `IAuditoriaServicio`.
7. Solo usuarios con rol `Administrador` pueden acceder al controlador de zonas de entrega.

## Dependencias
- `IBitacoraRepositorio` / `BitacoraRepositorio` — ya existe; se usa directamente para la auditoria (no via `IAuditoriaServicio`).
- `IBitacoraRepositorio` / `BitacoraRepositorio` — ya existe; no requiere cambios.
- `ResultadoOperacionDTO` — ya existe; se usa como tipo de retorno en operaciones de escritura.
- `Pedido` / `ZonaEntrega` — relacion FK ya establecida en el modelo EF6.
- `ApplicationDbContext` — ya tiene `DbSet<ZonaEntrega> ZonasEntrega` (verificado).

## Estado de implementacion

### Infraestructura base (ya implementada — alcance original de checkout)
Los siguientes artefactos fueron creados como soporte para el flujo de checkout y
estan operativos. Su alcance es de solo lectura (listar y obtener por ID).

| Paso | Descripcion                                               | Estado      |
|------|-----------------------------------------------------------|-------------|
| 1    | DTO: ZonaEntregaDTO                                       | Completo    |
| 2    | Interfaz repositorio: IZonaEntregaRepositorio             | Parcial (*) |
| 3    | Interfaz servicio: IZonaEntregaServicio                   | Parcial (*) |
| 4    | Modelo EF6: ZonaEntrega.cs                                | Completo    |
| 5    | Repositorio: ZonaEntregaRepositorio.cs                    | Parcial (*) |
| 6    | Servicio: ZonaEntregaServicio.cs                          | Parcial (*) |
| 7    | ViewModel: ZonaEntregaViewModel.cs                        | Parcial (*) |
| 8    | Controlador: ZonasEntregaController.cs                    | Pendiente   |
| 9    | Vistas Razor: Web/Views/ZonasEntrega/                     | Pendiente   |
| 10   | Registro DI en AutofacConfig.cs                          | Completo    |

(*) Parcial = solo metodos de lectura (ObtenerTodasAsync, ObtenerPorIdAsync).
    Deben ampliarse con Crear, Actualizar, CambiarDisponibilidad y ValidarNombreUnico.

### Operaciones que faltan en cada capa existente

**IZonaEntregaRepositorio / ZonaEntregaRepositorio — agregar:**
- `Task<ResultadoOperacionDTO> CrearAsync(ZonaEntregaDTO dto, string usuarioId, string nombreUsuario)`
- `Task<ResultadoOperacionDTO> ActualizarAsync(ZonaEntregaDTO dto, string usuarioId, string nombreUsuario)`
- `Task<ResultadoOperacionDTO> CambiarDisponibilidadAsync(int id, bool esDisponible, string usuarioId, string nombreUsuario)`
- `Task<bool> ExisteNombreAsync(string nombre, int? excluirId)`
- `Task<List<ZonaEntregaDTO>> ObtenerTodasIncluyendoInactivasAsync()` (para la vista admin)

**IZonaEntregaServicio / ZonaEntregaServicio — agregar:**
- `Task<ResultadoOperacionDTO> CrearAsync(ZonaEntregaDTO dto, string usuarioId, string nombreUsuario)`
- `Task<ResultadoOperacionDTO> ActualizarAsync(ZonaEntregaDTO dto, string usuarioId, string nombreUsuario)`
- `Task<ResultadoOperacionDTO> CambiarDisponibilidadAsync(int id, bool esDisponible, string usuarioId, string nombreUsuario)`
- `Task<List<ZonaEntregaDTO>> ObtenerTodasIncluyendoInactivasAsync()`

**ZonaEntregaDTO — ampliar con:**
- `FechaCreacion` (DateTime) — se incluye en el DTO para que la vista de detalle/lista la muestre

**ZonaEntregaViewModel — ampliar con:**
- `FechaCreacion` (DateTime)
- Anotaciones de validacion para la vista de formulario (Required, Range, MaxLength)

### Artefactos nuevos a crear

| Artefacto                                   | Capa        | Descripcion                                      |
|---------------------------------------------|-------------|--------------------------------------------------|
| `ZonasEntregaController.cs`                 | Web         | CRUD con [Authorize(Roles="Administrador")]       |
| `Views/ZonasEntrega/Index.cshtml`           | Web         | Lista todas las zonas (activas e inactivas)      |
| `Views/ZonasEntrega/Crear.cshtml`           | Web         | Formulario de registro de nueva zona             |
| `Views/ZonasEntrega/Editar.cshtml`          | Web         | Formulario de edicion de zona existente          |
| `Views/ZonasEntrega/Detalle.cshtml`         | Web         | Vista de detalle de una zona                     |

## Orden de implementacion recomendado

1. Ampliar `IZonaEntregaRepositorio` — agregar firmas de los 5 metodos faltantes
2. Ampliar `IZonaEntregaServicio` — agregar las mismas firmas
3. Ampliar `ZonaEntregaRepositorio` — implementar los 5 metodos con logica EF6 y auditoria
4. Ampliar `ZonaEntregaServicio` — delegar al repositorio + validar nombre unico
5. Ampliar `ZonaEntregaViewModel` — agregar FechaCreacion y DataAnnotations
6. Crear `ZonasEntregaController.cs` — acciones Index, Detalle, Crear (GET/POST),
   Editar (GET/POST), CambiarDisponibilidad (POST)
7. Crear vistas Razor — Index, Crear, Editar, Detalle

## Log de cambios
| Fecha      | Tipo           | Descripcion                                                                       | Agente          |
|------------|----------------|-----------------------------------------------------------------------------------|-----------------|
| 2026-07-15 | Inicializacion | Contexto documentado; estado de implementacion evaluado contra codigo existente   | feature-planner |
| 2026-07-15 | Correccion     | FechaCreacion agregada a ZonaEntregaDTO; firmas de escritura actualizadas con nombreUsuario; auditoria via IBitacoraRepositorio.GuardarAsync (no IAuditoriaServicio); ObtenerTodasIncluyendoInactivasAsync y ExisteNombreAsync confirmados como metodos nuevos del repositorio | data-normalizer |
