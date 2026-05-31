# Contexto: Inventario — Consultar Inventario (H1)

## Objetivo de negocio
Proveer a Administradores y Operadores una vista centralizada y de solo lectura del inventario
de productos, con KPIs de negocio (stock total, productos bajo mínimo, productos sin stock,
valor total del inventario), filtros de búsqueda y alertas de reabastecimiento. La fuente
única de verdad del stock pasa a ser la tabla `Inventario`, eliminando `Producto.Stock`.

## Rama
`inventario-consulta` — creada desde `develop` el 2026-05-29

## Usuarios y roles
- Roles: `Administrador`, `Operador`
- Requiere autenticación: Sí
- Rol `Cliente`: sin acceso

## Entidad principal
- Nombre: `Inventario`
- Relaciones:
  - `Inventario.ProductoId` → `Productos.Id` (1:1, UNIQUE constraint)
  - `Inventario.UsuarioUltimaActualizacionId` → `AspNetUsers.Id` (nullable)
- Atributos clave:
  - `Id` int PK identity
  - `ProductoId` int FK UNIQUE NOT NULL
  - `StockActual` int NOT NULL DEFAULT 0
  - `StockMinimo` int NOT NULL DEFAULT 5
  - `FechaUltimaActualizacion` datetime NOT NULL
  - `UsuarioUltimaActualizacionId` varchar(128) FK nullable

## Migración de columna eliminada
`Producto.Stock` se elimina y se reemplaza por `Inventario.StockActual`.

Archivos afectados por este cambio:
- `AccesoADatos/Modelos/Producto.cs` — eliminar propiedad `Stock`
- `AccesoADatos/Repositorios/ProductoRepositorio.cs` — ajustar referencias a `Stock`
- `AccesoADatos/Repositorios/CatalogoProductoRepositorio.cs` — ajustar referencias a `Stock`
- `LogicaNegocio/Servicios/PedidoServicio.cs` — ajustar referencias a `Stock`
- `Web/Controllers/PedidoController.cs` — ajustar referencias a `Stock`
- `Web/Controllers/ProductoController.cs` — ajustar referencias a `Stock`
- `Web/Controllers/HomeController.cs` — ajustar referencias a `Stock`

## Operaciones confirmadas
| Operacion    | Incluir |
|--------------|---------|
| Listar       | SI      |
| Ver detalle  | NO      |
| Crear        | NO      |
| Editar       | NO      |
| Eliminar     | NO      |
| Estadisticas | SI      |

## Analytics y KPIs (panel superior de la vista)
- Total de productos registrados
- Productos con stock bajo: `StockActual <= StockMinimo`
- Productos sin stock: `StockActual = 0`
- Valor total del inventario: `SUM(StockActual * Precio)`
- Lista de alertas de reabastecimiento (productos bajo `StockMinimo`)

## Filtros de busqueda
- Por nombre de producto (texto libre)
- Por categoria
- Por estado de stock: Todos / Stock bajo / Sin stock

## Reglas de negocio
- Un producto tiene exactamente un registro en `Inventario` (relacion 1:1, garantizada por UNIQUE en `ProductoId`)
- `StockActual` es la unica fuente de verdad del stock; `Producto.Stock` se elimina
- Alerta de stock bajo: `StockActual <= StockMinimo`
- Sin stock: `StockActual = 0`
- La vista es de solo lectura; no se permiten modificaciones desde este modulo
- Acceso restringido a roles `Administrador` y `Operador`

## Tareas del sprint H1
| ID | Descripcion                                              |
|----|----------------------------------------------------------|
| T1 | Disenar modulo: entidades, DTOs, interfaces              |
| T2 | Mostrar stock actual de productos (tabla con inventario) |
| T3 | Implementar busqueda y filtros                           |
| T4 | Resaltar productos con stock bajo                        |
| T5 | Controlar acceso al inventario (roles Admin + Operador)  |
| T6 | Mostrar mensaje sin resultados                           |
| T7 | Analytics y KPIs en panel superior                       |
| T8 | Migrar Producto.Stock -> Inventario.StockActual          |

## Dependencias
- Modulo `Productos` ya implementado — la entidad `Producto` y su repositorio existen
- Modulo `Categorias` ya implementado — se usa para el filtro por categoria
- `AspNetUsers` (ASP.NET Identity) — FK nullable en `UsuarioUltimaActualizacionId`
- La migracion T8 afecta `PedidoServicio`, `PedidoController`, `ProductoController`, `HomeController`

## Estado de implementacion
| Paso | Descripcion                                                 | Estado      |
|------|-------------------------------------------------------------|-------------|
| 1    | DTO: `InventarioDTO`, `InventarioKpisDTO`                   | Completado  |
| 2    | Interfaz repositorio: `IInventarioRepositorio`              | Completado  |
| 3    | Interfaz servicio: `IInventarioServicio`                    | Completado  |
| 4    | Modelo EF6: `Inventario.cs` + eliminar `Producto.Stock`     | Completado  |
| 5    | Repositorio: `InventarioRepositorio`                        | Completado  |
| 6    | Servicio: `InventarioServicio`                              | Completado  |
| 7    | ViewModel: `InventarioViewModel`, `InventarioKpisViewModel` | Completado  |
| 8    | Controlador: `InventarioController` (Index con filtros)     | Completado  |
| 9    | Vista Razor: `Views/Inventario/Index.cshtml`                | Completado  |
| 10   | Registro DI en `AutofacConfig.cs`                           | Completado  |
| 11   | Migracion T8: ajustar archivos que usaban `Producto.Stock`  | Completado  |
| 12   | Migracion EF6: crear tabla `Inventario`, eliminar columna `Stock` de `Productos` | Pendiente (manual) |

## Subagentes recomendados

### module-scaffolder
Responsabilidad: crear los archivos de las 4 capas (pasos 1-9) segun el alcance confirmado.
Contexto disponible en: `.claude/features/inventario-consulta.md`
Disparar: inmediatamente, es el primer agente a ejecutar.

### di-registrar
Responsabilidad: registrar `IInventarioRepositorio`/`InventarioRepositorio` e
`IInventarioServicio`/`InventarioServicio` en `AutofacConfig.cs` (paso 10).
Disparar: despues de que module-scaffolder complete los pasos 1-9.

### convention-fixer
Responsabilidad: validar que todos los archivos .cs generados cumplan las convenciones
de espaciado, nombres y arquitectura del proyecto.
Disparar: despues de implementar los archivos .cs (pasos 1-9).

### architecture-auditor
Responsabilidad: verificar que no existan violaciones de dependencia entre capas
(Web no accede a AccesoADatos, LogicaNegocio no referencia Web, etc.).
Disparar: antes del PR final hacia `develop`.

## Log de cambios
| Fecha      | Tipo            | Descripcion                                                                                                      | Agente          |
|------------|-----------------|------------------------------------------------------------------------------------------------------------------|-----------------|
| 2026-05-29 | Inicializacion  | Rama creada, contexto documentado                                                                                | feature-planner |
| 2026-05-30 | Implementacion  | Pasos 1-11 completados: DTOs, interfaces, modelo EF6, repositorio, servicio, ViewModels, controlador, vista, migracion T8 (Producto.Stock -> Inventario.StockActual en 7 archivos) | module-scaffolder |
| 2026-05-30 | Implementacion  | Paso 10 completado: IInventarioRepositorio/InventarioRepositorio e IInventarioServicio/InventarioServicio registrados en AutofacConfig.cs | di-registrar    |
| 2026-05-30 | Correccion      | Eliminado @section Scripts con Inventario.init() en Index.cshtml — archivo .js no existia, causaba error en runtime | module-scaffolder |
