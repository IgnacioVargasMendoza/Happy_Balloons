# Contexto: Movimientos de Inventario

## Objetivo de negocio
Registrar y auditar todos los cambios de stock de manera trazable. Hoy el inventario solo
muestra lectura (KPIs y tabla de stock) y los ajustes manuales se hacen sobrescribiendo
`Inventario.StockActual` desde el modal de edición de Productos sin dejar rastro. Con este
módulo cada entrada, salida o ajuste manual queda registrado con usuario, fecha, motivo y
los valores de stock antes y después del cambio. Además se elimina el campo Stock del modal
de edición de Productos para que ningún cambio pueda bypassear la auditoría.

## Rama
`inventario-movimientos` — creada desde `segundo-sprint` el 2026-06-22

## Usuarios y roles
- Roles: Administrador, Operador
- Requiere autenticación: Sí

## Entidad principal
- Nombre: `MovimientoInventario`
- Relaciones:
  - `ProductoId` → `Productos.Id` (FK)
  - `UsuarioId` → `AspNetUsers.Id` (FK, usuario que registró el movimiento)
- Atributos clave:
  - `Id` (int, identity, PK)
  - `ProductoId` (int, FK)
  - `TipoMovimiento` (enum: Entrada / Salida / Ajuste)
  - `Cantidad` (int, > 0)
  - `StockAnterior` (int)
  - `StockNuevo` (int)
  - `Motivo` (string, obligatorio)
  - `UsuarioId` (string, FK a AspNetUsers)
  - `FechaMovimiento` (DateTime UTC)

## Operaciones confirmadas
| Operacion       | Incluir |
|-----------------|---------|
| Listar          | Si      |
| Ver detalle     | Si      |
| Crear           | Si      |
| Editar          | No      |
| Eliminar        | No      |
| Historial por producto | Si |

## Reglas de negocio
- `StockAnterior` se calcula automáticamente en el servicio leyendo `Inventario.StockActual`
  antes de aplicar el movimiento; no lo ingresa el usuario.
- `StockNuevo` = `StockAnterior + Cantidad` (Entrada/Ajuste positivo) o
  `StockAnterior - Cantidad` (Salida/Ajuste negativo).
- `Cantidad` debe ser > 0; la direccion la define `TipoMovimiento`.
- Un movimiento de tipo Salida no puede dejar `StockNuevo` < 0.
- `Inventario.StockActual` se actualiza en la misma transaccion en que se inserta el
  `MovimientoInventario`.
- Todo movimiento registra una entrada en `BitacoraAuditoria`.
- El campo Stock en el modal de edicion de Productos se elimina para forzar que todo
  cambio de stock pase por este modulo.
- Los movimientos no se pueden editar ni eliminar (inmutabilidad del historial).

## Dependencias
- Modulo de Inventario existente (rama `segundo-sprint`, ya mergeado): proporciona la
  entidad `Inventario` cuyo campo `StockActual` se actualiza transaccionalmente.
- Modulo de Productos existente: se elimina el campo Stock del modal de edicion.
- Trigger SQL `trg_DescontarStock`: ya descuenta stock al insertar `DetallesPedido`;
  este modulo cubre los movimientos manuales que el trigger no registra.

## Estado de implementacion
| Paso | Descripcion                              | Estado       |
|------|------------------------------------------|--------------|
| 1    | DTO (MovimientoInventarioDTO)            | Pendiente    |
| 2    | Interfaz repositorio                     | Pendiente    |
| 3    | Interfaz servicio                        | Pendiente    |
| 4    | Modelo EF6 (MovimientoInventario)        | Pendiente    |
| 5    | Repositorio                              | Pendiente    |
| 6    | Servicio                                 | Pendiente    |
| 7    | ViewModel                                | Pendiente    |
| 8    | Controlador                              | Pendiente    |
| 9    | Vista Razor (Historial + Formulario)     | Pendiente    |
| 10   | Registro DI en AutofacConfig.cs          | Pendiente    |
| 11   | Eliminar campo Stock del modal Productos | Pendiente    |

## Log de cambios
| Fecha      | Tipo            | Descripcion                                              | Agente          |
|------------|-----------------|----------------------------------------------------------|-----------------|
| 2026-06-22 | Inicializacion  | Rama creada, contexto documentado                        | feature-planner |
