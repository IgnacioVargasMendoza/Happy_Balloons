# Changelog — Happy Times Balloons

Formato basado en [Keep a Changelog](https://keepachangelog.com/es/1.1.0/).
Versiones vinculadas a la rama `develop` (ASP.NET MVC 5).

---

## [Unreleased] — rama `develop`

### Añadido

#### Phase 0 — Migraciones EF6 y configuración de BD (2026-05-20)
- `AccesoADatos/Migraciones/Configuration.cs` — Nueva clase `DbMigrationsConfiguration<ApplicationDbContext>` con migraciones automáticas habilitadas. El método `Seed()` crea los roles `Administrador`, `Operador` y `Cliente`, y el usuario administrador por defecto (`admin@happytimes.com` / `Admin@123456`) si no existen.
- `AccesoADatos/App.config` — Nuevo archivo de configuración para el proyecto `AccesoADatos`: declara el proveedor EF6 (`EntityFramework.SqlServer`) y la cadena de conexión a `Nacho\SQLEXPRESS`, requerido para que EF resuelva el provider al ejecutar migraciones desde la CLI.
- `Web/Global.asax.cs` — Reemplaza `DatabaseConfig` (inicializador drop-recreate) por `MigrateDatabaseToLatestVersion<ApplicationDbContext, Configuration>` con `force: true`, de modo que la BD se actualiza incrementalmente en cada arranque en lugar de recrearse.

#### Phase 1 — Infraestructura de layout (2026-05-20)
- `Views/Shared/_Layout.cshtml` — Se activa `@Html.Partial("_NavBar")` que estaba comentado; se elimina el `@Html.ActionLink` huérfano que reemplazaba al navbar. Todas las páginas ahora renderizan la barra de navegación correctamente.
- `Views/Shared/_NavBar.cshtml` — Se elimina el bloque `@Html.ActionLink` duplicado (workaround anterior). El ícono del carrito ahora apunta a `Pedido/Carrito` y muestra un badge rojo con el conteo de ítems leído desde `Session["CarritoCount"]`.

#### Phase 2 — Catálogo público (2026-05-20)
- `Models/ViewModels/CatalogoIndexViewModel.cs` — Nuevo ViewModel con `List<ProductoViewModel>`, `IEnumerable<SelectListItem>` de categorías, filtros de búsqueda y referencia a `PaginacionViewModel`.
- `Controllers/HomeController.cs` — Acción `Index(busqueda, categoriaId)` que consulta productos activos vía `ProductoServicio` y categorías activas directamente desde el contexto. Helper privado `MapearProducto(ProductoDTO)` reutilizado por ambas acciones.
- `Views/Shared/_TarjetaProducto.cshtml` — Partial reutilizable tipado a `ProductoViewModel`. Incluye imagen con placeholder, badges de oferta y agotado, precio con/sin descuento y formulario POST a `Pedido/AgregarAlCarrito`.
- `Views/Home/Index.cshtml` — Vista completa del catálogo público: hero con gradiente, formulario de filtros (búsqueda + categoría), grid responsivo `row-cols-xl-4` usando `_TarjetaProducto`, estado vacío y partial `_Paginacion`.

#### Phase 3 — Detalle de producto (2026-05-20)
- `Models/ViewModels/ProductoDetalleViewModel.cs` — Nuevo ViewModel con el producto principal y `List<ProductoViewModel>` de relacionados (misma categoría, hasta 4).
- `Controllers/HomeController.cs` — Acción `Detalle(int id)` agregada: retorna 404 si el producto no existe o está inactivo; carga hasta 4 productos relacionados de la misma categoría.
- `Views/Home/Detalle.cshtml` — Vista de detalle con breadcrumb, Bootstrap Carousel para galería de imágenes, miniaturas clicables, selector de cantidad con botones +/-, formulario "Agregar al carrito" (deshabilitado si stock = 0), sección de productos relacionados y aviso de stock bajo (< 5 unidades).

#### Phase 4 — Carrito de compras (sesión) (2026-05-20)
- `Models/ViewModels/CarritoItemViewModel.cs` — Nuevo ViewModel `[Serializable]` para ítems en sesión. Propiedades calculadas: `PrecioEfectivo` (aplica descuento si existe) y `Subtotal`.
- `Models/ViewModels/CarritoViewModel.cs` — Agrega la lista de ítems con `Total`, `CantidadTotal` y `EstaVacio`.
- `Controllers/PedidoController.cs` — Nuevo controlador con acciones: `Carrito` (GET, anónimo), `AgregarAlCarrito` (POST, valida stock vía `ProductoServicio`), `ActualizarCantidad` (POST), `QuitarDelCarrito` (POST), `Index` (placeholder hasta Phase 5). El carrito se persiste en `Session["Carrito"]` (`List<CarritoItemViewModel>`) y el conteo en `Session["CarritoCount"]`.
- `Views/Pedido/Carrito.cshtml` — Vista de dos columnas: tabla de ítems con formularios inline +/- y botón de eliminar por fila; columna de resumen con subtotal, total y botón de pago (deshabilitado hasta Phase 5). Estado vacío con enlace al catálogo.

#### Pago SINPE — Módulo completo con webhook automático (2026-06-22)
- `Abstraccion/DTOs/SinpeWebhookDTO.cs` — Nuevo DTO para el payload recibido del banco: `NumeroComprobante`, `Monto`, `NombreTitular`, `TelefonoDestino`, `TokenSeguridad`.
- `Abstraccion/DTOs/PagoSinpeDTO.cs` — Nuevo DTO de registro interno con `EstadoPago`, `MotivoRechazo`, `FechaRecepcion` y `FechaProcesamiento`.
- `Abstraccion/Enums/EstadoPagoSinpe.cs` — Nuevo enum: `Pendiente=1`, `Aprobado=2`, `Rechazado=3`, `Duplicado=4`.
- `Abstraccion/Enums/EstadoPedido.cs` — Agrega `PagoPendiente = 6`; estado inicial de pedidos creados con MetodoPago = "SINPE".
- `Abstraccion/Enums/TipoOperacion.cs` — Agrega `ProcesarPagoSinpe`, `RechazarPagoSinpe`, `PagoDuplicadoSinpe` para auditoría granular de pagos.
- `Abstraccion/Interfaces/Repositorios/ISinpeRepositorio.cs` — Interfaz con `RegistrarAsync`, `ExisteComprobanteAsync`, `ObtenerTodosAsync`, `ObtenerPorIdAsync`.
- `Abstraccion/Interfaces/Repositorios/IPedidoRepositorio.cs` — Agrega `BuscarPorSinpeAsync(comprobante, monto)`: query SQL que busca pedidos en estado `PagoPendiente` con MetodoPago = "SINPE" por número de comprobante o monto, evitando carga en memoria.
- `Abstraccion/Interfaces/Servicios/ISinpeServicio.cs` — Interfaz con `ProcesarWebhookAsync`, `ObtenerPagosAsync`, `ObtenerPorIdAsync`.
- `AccesoADatos/Modelos/PagoSinpe.cs` — Modelo EF6 para tabla `[PagosSinpe]` con FK a `Pedidos` (no-cascade).
- `AccesoADatos/Migraciones/202506220002_AgregarTablaPagosSinpe.cs` — Migración EF6: crea tabla `PagosSinpe` con índice ÚNICO en `NumeroComprobante` (barrera de duplicados en BD) e índice en `PedidoId`.
- `AccesoADatos/Repositorios/SinpeRepositorio.cs` — Implementación de `ISinpeRepositorio` con EF6. `ExisteComprobanteAsync` filtra solo comprobantes `Aprobado` para no bloquear reintentos de rechazados.
- `AccesoADatos/Repositorios/PedidoRepositorio.cs` — Implementa `BuscarPorSinpeAsync`; corrige `CrearAsync` para asignar `EstadoPedido.PagoPendiente` cuando `MetodoPago == "SINPE"` en lugar de `Pendiente`.
- `AccesoADatos/Contexto/ApplicationDbContext.cs` — Agrega `DbSet<PagoSinpe> PagosSinpe`.
- `LogicaNegocio/Servicios/SinpeServicio.cs` — Implementa `ProcesarWebhookAsync` con flujo completo: (1) validación de token via `AppSettings["Sinpe:TokenSeguridad"]`, (2) detección de duplicados, (3) búsqueda de pedido en BD con `BuscarPorSinpeAsync`, (4) validación de monto exacto, (5) aprobación con actualización de estado a `Procesando`; cada paso registra en `BitacoraAuditoria`. Siempre retorna HTTP 200 al banco.
- `Web/Controllers/SinpeController.cs` — Endpoint público `[HttpPost] RecibirWebhook` para recibir webhooks del banco; vistas admin `Index` y `Detalle` con `[Authorize(Roles = "Administrador,Operador")]`.
- `Web/Models/ViewModels/SinpeViewModel.cs` — `SinpeViewModel` (lista) y `SinpeDetalleViewModel` (detalle individual).
- `Web/Views/Sinpe/Index.cshtml` — Vista admin con tabla de pagos SINPE, badges de estado por color y modal Bootstrap para ver detalle sin salir de la lista.
- `Web/Views/Sinpe/_DetallePago.cshtml` — Partial cargado vía AJAX en el modal de detalle.
- `Web/Content/Sinpe.css` — Estilos del módulo SINPE y checkout (badges de estado, panel SINPE, layout de cards de pago).
- `Web/Scripts/Sinpe.js` — Lógica de checkout: mostrar/ocultar panel de instrucciones SINPE al seleccionar método de pago; carga de detalle de pago en modal via `$.get()`.
- `Web/Views/Pedido/Checkout.cshtml` — Agrega opción "SINPE Móvil" en selector de método de pago con panel de instrucciones (teléfono configurable via `AppSettings["Sinpe:TelefonoNegocio"]`). CSS y JS extraídos a archivos externos.
- `Web/App_Start/AutofacConfig.cs` — Registra `SinpeRepositorio → ISinpeRepositorio` y `SinpeServicio → ISinpeServicio` con `InstancePerRequest`.
- `HappyTimesBalloons.Tests/Sinpe/SinpeServicioTests.cs` — 5 tests unitarios (MSTest + Moq): token válido aprobado, comprobante duplicado, monto incorrecto, token inválido, pedido no encontrado.
- `connectionStrings.config` — Eliminado del seguimiento de git (`git rm --cached`); el archivo es local por máquina y ya estaba en `.gitignore`.

#### Inventario — Configurar stock mínimo (2026-06-22)
- `Abstraccion/DTOs/InventarioDTO.cs` — Agrega `[Required]` y `[Range(0, int.MaxValue)]` sobre `StockActual` y `StockMinimo`; agrega referencia a `System.ComponentModel.DataAnnotations` en el `.csproj` de Abstraccion.
- `Abstraccion/Interfaces/Repositorios/IInventarioRepositorio.cs` — Nuevo método `ActualizarStockMinimoAsync(int inventarioId, int nuevoStockMinimo, string usuarioId)`.
- `Abstraccion/Interfaces/Servicios/IInventarioServicio.cs` — Nuevo método `ActualizarStockMinimoAsync(int inventarioId, int nuevoStockMinimo, string usuarioId, string nombreUsuario)`.
- `AccesoADatos/Repositorios/InventarioRepositorio.cs` — Implementación de `ActualizarStockMinimoAsync`: actualiza `StockMinimo`, `FechaUltimaActualizacion` y `UsuarioUltimaActualizacionId`; retorna `false` si el registro no existe.
- `LogicaNegocio/Servicios/InventarioServicio.cs` — Implementación de `ActualizarStockMinimoAsync`: valida `nuevoStockMinimo >= 0`, llama al repositorio y registra en bitácora con `TipoOperacion.Actualizar` si la operación fue exitosa.
- `Web/Controllers/InventarioController.cs` — Nueva acción `[HttpPost] EditarStockMinimo` con `[ValidateAntiForgeryToken]`; lee `usuarioId` con `User.Identity.GetUserId()` y redirige a `Index` con `TempData` de éxito o error.
- `Web/Views/Inventario/Index.cshtml` — Modal "Editar stock mínimo" con formulario POST, input numérico `min="0"` y token CSRF; JavaScript que carga el `inventarioId` y valor actual al abrir el modal.
- `HappyTimesBalloons.Tests/` — Nuevo proyecto de tests unitarios (MSTest 3.1.1 + Moq 4.20.70) con 15 tests: `InventarioServicioTests` (8 tests: validación numérica, flujo exitoso, auditoría y cortocircuito) y `AlertaStockTests` (7 tests: estados `sin_stock`, `bajo`, `normal` con casos borde). 15/15 pasan.

### Pendiente (próximas fases)
- **Phase 5** — Checkout y Mis Pedidos: requiere entidades `Pedido`, `DetallePedido`, `ZonaEntrega` en backend.
- **Phase 6** — Gestión de pedidos (admin/operador): `PedidoController.Index` con tabla filtrable y modal de cambio de estado.
- **Phase 7** — Dashboard administrativo: `AdminController` con KPIs, últimos pedidos, configuración del sistema y gestión de promociones.

---

## Historial de commits previos

| Commit | Descripción |
|---|---|
| `51163a4` | Actualización diseño |
| `de7b5b1` | Commit inicial |
| `3763467` | Initialize README with project details |
| `66ae311` | Initial commit |
