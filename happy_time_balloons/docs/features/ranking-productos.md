# Contexto: Ranking de Productos Mas Vendidos

## Objetivo de negocio

Proporcionar al administrador una vista dedicada que muestre, para cualquier rango de fechas
y opcionalmente filtrada por zona de entrega, cuales productos generan mas ventas en cantidad
de unidades y en ingresos. Esta informacion permite decidir que productos promover, ajustar
inventario anticipadamente, e identificar zonas geograficas con mayor demanda.

## Rama

`ranking-productos` — creada desde `develop` el 2026-07-15

## Usuarios y roles

- Rol(es): Administrador
- Requiere autenticacion: Si (atributo `[Authorize(Roles = "Administrador")]`)

---

## Contexto de datos existente

### Tablas involucradas (sin crear tablas nuevas)

| Tabla | Alias | Proposito |
|-------|-------|-----------|
| `DetallesPedido` | `dp` | Fuente de cantidad y subtotal por producto |
| `Pedidos` | `p` | Permite filtrar por fecha, zona y excluir cancelados |
| `Productos` | `prod` | Nombre e imagen del producto |
| `Categorias` | `cat` | Agrupacion por categoria para el grafico |
| `ZonasEntrega` | `z` | Filtro por zona geografica |

### Consulta SQL conceptual del ranking

```sql
SELECT
    prod.Id              AS ProductoId,
    prod.Nombre          AS NombreProducto,
    cat.Nombre           AS Categoria,
    z.Nombre             AS Zona,
    SUM(dp.Cantidad)     AS UnidadesVendidas,
    SUM(dp.Subtotal)     AS Ingresos,
    COUNT(DISTINCT p.Id) AS NumeroPedidos
FROM DetallesPedido dp
INNER JOIN Pedidos     p    ON dp.PedidoId    = p.Id
INNER JOIN Productos   prod ON dp.ProductoId  = prod.Id
INNER JOIN Categorias  cat  ON prod.CategoriaId = cat.Id
INNER JOIN ZonasEntrega z   ON p.ZonaEntregaId  = z.Id
WHERE
    p.EstadoPedido != 5           -- excluir Cancelado
    AND p.FechaPedido >= @FechaInicio
    AND p.FechaPedido <  @FechaFinExclusiva   -- fechaFin.Date.AddDays(1)
    AND (@ZonaId IS NULL OR p.ZonaEntregaId = @ZonaId)
GROUP BY prod.Id, prod.Nombre, cat.Nombre, z.Nombre
ORDER BY SUM(dp.Cantidad) DESC
```

**Nota:** La consulta se ejecuta en memoria via LINQ sobre EF6, aplicando el mismo patron
que usa `ReporteVentasRepositorio`. No se crea ninguna vista SQL ni procedimiento almacenado.

---

## Esquema de DTOs (capa Abstraccion)

### 1. `RankingProductoItemDTO`

Representa una fila del ranking.

```csharp
namespace HappyTimesBalloons.Abstraccion.DTOs
{
    public class RankingProductoItemDTO
    {
        public int    Posicion         { get; set; }
        public int    ProductoId       { get; set; }
        public string NombreProducto   { get; set; }
        public string Categoria        { get; set; }
        public int    UnidadesVendidas { get; set; }
        public decimal Ingresos        { get; set; }
        public int    NumeroPedidos    { get; set; }
        // Porcentaje sobre el total de unidades del periodo (calculado en servicio)
        public decimal PorcentajeUnidades { get; set; }
    }
}
```

### 2. `RankingProductosDTO`

Envelope que agrupa el ranking completo con los KPIs del periodo y los datos para graficos.

```csharp
namespace HappyTimesBalloons.Abstraccion.DTOs
{
    public class RankingProductosDTO
    {
        public DateTime FechaInicio       { get; set; }
        public DateTime FechaFin          { get; set; }
        public int?     ZonaId            { get; set; }
        public string   NombreZona        { get; set; }

        // KPIs globales del periodo
        public int      TotalUnidadesVendidas { get; set; }
        public decimal  IngresosTotales       { get; set; }
        public int      TotalProductosActivos { get; set; }

        // Ranking principal (ordenado por UnidadesVendidas DESC)
        public List<RankingProductoItemDTO> Items { get; set; }
            = new List<RankingProductoItemDTO>();

        // Datos para grafico de barras: top-10 por unidades
        public List<string>  GraficoEtiquetas  { get; set; } = new List<string>();
        public List<int>     GraficoUnidades   { get; set; } = new List<int>();
        public List<decimal> GraficoIngresos   { get; set; } = new List<decimal>();

        // Datos para grafico de dona: distribucion por categoria
        public List<string>  DonaEtiquetas     { get; set; } = new List<string>();
        public List<int>     DonaValores       { get; set; } = new List<int>();
    }
}
```

### 3. `ZonaEntregaDTO` (ya existe en Abstraccion/DTOs)

Se reutiliza sin modificacion para poblar el selector de zona.

---

## Interfaces (capa Abstraccion)

### Repositorio — `IRankingProductosRepositorio`

Archivo: `HappyTimesBalloons.Abstraccion/Interfaces/Repositorios/IRankingProductosRepositorio.cs`

```csharp
using HappyTimesBalloons.Abstraccion.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Repositorios
{
    public interface IRankingProductosRepositorio
    {
        /// Devuelve la lista base de items ordenada por UnidadesVendidas DESC.
        /// zonaId = null significa "todas las zonas".
        Task<List<RankingProductoItemDTO>> ObtenerItemsAsync(
            DateTime fechaInicio,
            DateTime fechaFin,
            int?     zonaId);
    }
}
```

### Servicio — `IRankingProductosServicio`

Archivo: `HappyTimesBalloons.Abstraccion/Interfaces/Servicios/IRankingProductosServicio.cs`

```csharp
using HappyTimesBalloons.Abstraccion.DTOs;
using System;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Servicios
{
    public interface IRankingProductosServicio
    {
        Task<RankingProductosDTO> ObtenerRankingAsync(
            DateTime fechaInicio,
            DateTime fechaFin,
            int?     zonaId);
    }
}
```

---

## Implementacion del repositorio (AccesoADatos)

Archivo: `HappyTimesBalloons.AccesoADatos/Repositorios/RankingProductosRepositorio.cs`

Patron de implementacion (guia para el scaffolder):

```csharp
public class RankingProductosRepositorio : IRankingProductosRepositorio
{
    private readonly ApplicationDbContext _ctx;

    public RankingProductosRepositorio(ApplicationDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<List<RankingProductoItemDTO>> ObtenerItemsAsync(
        DateTime fechaInicio, DateTime fechaFin, int? zonaId)
    {
        var hasta = fechaFin.Date.AddDays(1);

        // Partimos de DetallesPedido y navegamos hacia el pedido para aplicar filtros
        var query = _ctx.DetallesPedido
            .Include(dp => dp.Pedido)
            .Include(dp => dp.Producto.Categoria)
            .Where(dp =>
                dp.Pedido.EstadoPedido != EstadoPedido.Cancelado &&
                dp.Pedido.FechaPedido >= fechaInicio.Date &&
                dp.Pedido.FechaPedido <  hasta &&
                (zonaId == null || dp.Pedido.ZonaEntregaId == zonaId));

        // Agrupar en memoria para aprovechar la carga eager
        var detalles = await query.ToListAsync();

        var items = detalles
            .GroupBy(dp => new
            {
                dp.ProductoId,
                NombreProducto = dp.Producto.Nombre,
                Categoria      = dp.Producto.Categoria.Nombre
            })
            .Select(g => new RankingProductoItemDTO
            {
                ProductoId       = g.Key.ProductoId,
                NombreProducto   = g.Key.NombreProducto,
                Categoria        = g.Key.Categoria,
                UnidadesVendidas = g.Sum(d => d.Cantidad),
                Ingresos         = g.Sum(d => d.Subtotal),
                NumeroPedidos    = g.Select(d => d.PedidoId).Distinct().Count()
            })
            .OrderByDescending(x => x.UnidadesVendidas)
            .ToList();

        // Asignar posicion y porcentaje en memoria
        int total = items.Sum(i => i.UnidadesVendidas);
        for (int i = 0; i < items.Count; i++)
        {
            items[i].Posicion = i + 1;
            items[i].PorcentajeUnidades = total > 0
                ? Math.Round((decimal)items[i].UnidadesVendidas / total * 100, 1)
                : 0m;
        }

        return items;
    }
}
```

---

## Implementacion del servicio (LogicaNegocio)

Archivo: `HappyTimesBalloons.LogicaNegocio/Servicios/RankingProductosServicio.cs`

Responsabilidades:
1. Llamar al repositorio con los parametros validados.
2. Calcular los KPIs globales (`TotalUnidadesVendidas`, `IngresosTotales`,
   `TotalProductosActivos`).
3. Construir los arrays de datos para los graficos Chart.js:
   - Grafico de barras: top-10 productos por unidades.
   - Grafico de dona: agrupar por categoria sumando unidades.
4. Si `zonaId` es no nulo, resolver `NombreZona` desde `IZonaEntregaServicio`
   (dependencia inyectada).

Patron de implementacion:

```csharp
public class RankingProductosServicio : IRankingProductosServicio
{
    private readonly IRankingProductosRepositorio _repo;
    private readonly IZonaEntregaServicio         _zonaServicio;

    public RankingProductosServicio(
        IRankingProductosRepositorio repo,
        IZonaEntregaServicio         zonaServicio)
    {
        _repo         = repo;
        _zonaServicio = zonaServicio;
    }

    public async Task<RankingProductosDTO> ObtenerRankingAsync(
        DateTime fechaInicio, DateTime fechaFin, int? zonaId)
    {
        var items = await _repo.ObtenerItemsAsync(fechaInicio, fechaFin, zonaId);

        string nombreZona = null;
        if (zonaId.HasValue)
        {
            var zona = await _zonaServicio.ObtenerPorIdAsync(zonaId.Value);
            nombreZona = zona?.Nombre;
        }

        // Top-10 para el grafico de barras
        var top10 = items.Take(10).ToList();

        // Agrupacion por categoria para el grafico de dona
        var porCategoria = items
            .GroupBy(i => i.Categoria)
            .Select(g => new { Categoria = g.Key, Unidades = g.Sum(i => i.UnidadesVendidas) })
            .OrderByDescending(g => g.Unidades)
            .ToList();

        return new RankingProductosDTO
        {
            FechaInicio           = fechaInicio.Date,
            FechaFin              = fechaFin.Date,
            ZonaId                = zonaId,
            NombreZona            = nombreZona,
            TotalUnidadesVendidas = items.Sum(i => i.UnidadesVendidas),
            IngresosTotales       = items.Sum(i => i.Ingresos),
            TotalProductosActivos = items.Count,
            Items                 = items,
            GraficoEtiquetas      = top10.Select(i => i.NombreProducto).ToList(),
            GraficoUnidades       = top10.Select(i => i.UnidadesVendidas).ToList(),
            GraficoIngresos       = top10.Select(i => i.Ingresos).ToList(),
            DonaEtiquetas         = porCategoria.Select(g => g.Categoria).ToList(),
            DonaValores           = porCategoria.Select(g => g.Unidades).ToList()
        };
    }
}
```

**Dependencia adicional:** `IZonaEntregaServicio` ya existe y ya esta registrada en Autofac.

---

## ViewModel (capa Web)

Archivo: `HappyTimesBalloons.Web/Models/ViewModels/RankingProductosViewModel.cs`

```csharp
using HappyTimesBalloons.Abstraccion.DTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class RankingProductosViewModel
    {
        [Display(Name = "Fecha inicio")]
        [DataType(DataType.Date)]
        public DateTime FechaInicio { get; set; }

        [Display(Name = "Fecha fin")]
        [DataType(DataType.Date)]
        public DateTime FechaFin { get; set; }

        public int? ZonaId { get; set; }

        // Lista para el <select> de zonas (incluye opcion "Todas las zonas")
        public SelectList Zonas { get; set; }

        public RankingProductosDTO Ranking { get; set; }

        public bool FiltroAplicado { get; set; }

        public bool TieneDatos => Ranking != null && Ranking.Items.Count > 0;
    }
}
```

---

## Controlador (capa Web)

Archivo: `HappyTimesBalloons.Web/Controllers/RankingProductosController.cs`

### Endpoints

| Verbo | Ruta (Action) | Parametros QS | Descripcion |
|-------|---------------|---------------|-------------|
| GET | `RankingProductos/Index` | `fechaInicio`, `fechaFin`, `zonaId` | Vista principal con tabla y graficos |
| GET | `RankingProductos/DatosGrafico` | `fechaInicio`, `fechaFin`, `zonaId` | JSON para actualizacion dinamica de Chart.js (opcional, AJAX) |

Patron de implementacion:

```csharp
[Authorize(Roles = "Administrador")]
public class RankingProductosController : Controller
{
    private readonly IRankingProductosServicio _rankingServicio;
    private readonly IZonaEntregaServicio      _zonaServicio;

    public RankingProductosController(
        IRankingProductosServicio rankingServicio,
        IZonaEntregaServicio      zonaServicio)
    {
        _rankingServicio = rankingServicio;
        _zonaServicio    = zonaServicio;
    }

    public async Task<ActionResult> Index(
        DateTime? fechaInicio,
        DateTime? fechaFin,
        int?      zonaId)
    {
        var inicio = fechaInicio ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var fin    = fechaFin    ?? DateTime.Today;

        if (inicio > fin)
        {
            ModelState.AddModelError("", "La fecha de inicio no puede ser posterior a la fecha fin.");
            inicio = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            fin    = DateTime.Today;
        }

        var ranking = await _rankingServicio.ObtenerRankingAsync(inicio, fin, zonaId);
        var zonas   = await _zonaServicio.ObtenerTodasAsync();   // metodo existente

        var vm = new RankingProductosViewModel
        {
            FechaInicio    = inicio,
            FechaFin       = fin,
            ZonaId         = zonaId,
            Zonas          = new SelectList(zonas, "Id", "Nombre", zonaId),
            Ranking        = ranking,
            FiltroAplicado = fechaInicio.HasValue || fechaFin.HasValue || zonaId.HasValue
        };

        return View(vm);
    }

    // Endpoint AJAX: devuelve JSON con los arrays de Chart.js
    public async Task<JsonResult> DatosGrafico(
        DateTime? fechaInicio,
        DateTime? fechaFin,
        int?      zonaId)
    {
        var inicio  = fechaInicio ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var fin     = fechaFin    ?? DateTime.Today;
        var ranking = await _rankingServicio.ObtenerRankingAsync(inicio, fin, zonaId);

        return Json(new
        {
            etiquetas       = ranking.GraficoEtiquetas,
            unidades        = ranking.GraficoUnidades,
            ingresos        = ranking.GraficoIngresos,
            donaEtiquetas   = ranking.DonaEtiquetas,
            donaValores     = ranking.DonaValores
        }, JsonRequestBehavior.AllowGet);
    }
}
```

---

## Vista Razor (capa Web)

Archivo: `HappyTimesBalloons.Web/Views/RankingProductos/Index.cshtml`

### Estructura de la vista

```
Index.cshtml
├── @model RankingProductosViewModel
├── Panel de filtros (form GET)
│   ├── input[type=date] fechaInicio
│   ├── input[type=date] fechaFin
│   ├── <select> zonaId  (con opcion "Todas las zonas" value="")
│   └── boton Filtrar + enlace Limpiar (si FiltroAplicado)
│
├── [Si !TieneDatos]  Partial _SinResultados.cshtml (reutilizar de Reportes/)
│
├── [Si TieneDatos]
│   ├── Fila KPIs (3 tarjetas)
│   │   ├── Total unidades vendidas  (bg-primary)
│   │   ├── Ingresos totales         (bg-success)
│   │   └── Productos con ventas     (bg-info)
│   │
│   ├── Fila de graficos (2 columnas)
│   │   ├── col-md-8: Grafico de barras — Top 10 por unidades (Chart.js, id="chartBarras")
│   │   └── col-md-4: Grafico de dona  — Distribucion por categoria (Chart.js, id="chartDona")
│   │
│   └── Tabla de ranking completo
│       ├── Columnas: # | Producto | Categoria | Unidades | % del total | Ingresos | N Pedidos
│       ├── Posiciones 1-3 con badge de medalla (oro/plata/bronce usando Bootstrap badges)
│       └── Barra de progreso Bootstrap en columna "% del total" (width = PorcentajeUnidades%)
│
└── @section Scripts (al final)
    ├── <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    └── <script> bloque que inicializa chartBarras y chartDona con datos de @Json.Encode(Model.Ranking.*)
```

### Patron de inicializacion Chart.js

```javascript
// En @section Scripts dentro de Index.cshtml
var etiquetas = @Html.Raw(Newtonsoft.Json.JsonConvert.SerializeObject(Model.Ranking.GraficoEtiquetas));
var unidades  = @Html.Raw(Newtonsoft.Json.JsonConvert.SerializeObject(Model.Ranking.GraficoUnidades));
var ingresos  = @Html.Raw(Newtonsoft.Json.JsonConvert.SerializeObject(Model.Ranking.GraficoIngresos));

var ctxBarras = document.getElementById('chartBarras').getContext('2d');
new Chart(ctxBarras, {
    type: 'bar',
    data: {
        labels: etiquetas,
        datasets: [{
            label: 'Unidades vendidas',
            data: unidades,
            backgroundColor: 'rgba(13, 110, 253, 0.7)'
        }]
    },
    options: {
        responsive: true,
        plugins: { legend: { display: false } },
        scales: { y: { beginAtZero: true, precision: 0 } }
    }
});

var donaEtiquetas = @Html.Raw(Newtonsoft.Json.JsonConvert.SerializeObject(Model.Ranking.DonaEtiquetas));
var donaValores   = @Html.Raw(Newtonsoft.Json.JsonConvert.SerializeObject(Model.Ranking.DonaValores));

var ctxDona = document.getElementById('chartDona').getContext('2d');
new Chart(ctxDona, {
    type: 'doughnut',
    data: {
        labels: donaEtiquetas,
        datasets: [{ data: donaValores }]
    },
    options: { responsive: true }
});
```

**Nota:** Newtonsoft.Json ya esta disponible como dependencia transitiva de EF6 y MVC5.

---

## Registro en AutofacConfig.cs

Lineas a agregar en `HappyTimesBalloons.Web/App_Start/AutofacConfig.cs` dentro de `Register()`:

```csharp
// Repositorios
builder.RegisterType<RankingProductosRepositorio>()
       .As<IRankingProductosRepositorio>()
       .InstancePerRequest();

// Servicios
builder.RegisterType<RankingProductosServicio>()
       .As<IRankingProductosServicio>()
       .InstancePerRequest();
```

---

## Registro en Web.csproj

Todo archivo nuevo en el proyecto Web debe declararse en
`HappyTimesBalloons.Web/HappyTimesBalloons.Web.csproj`:

- `RankingProductosViewModel.cs`     → `<Compile Include="Models\ViewModels\RankingProductosViewModel.cs" />`
- `RankingProductosController.cs`    → `<Compile Include="Controllers\RankingProductosController.cs" />`
- `Index.cshtml`                     → `<Content Include="Views\RankingProductos\Index.cshtml" />`

Los proyectos Abstraccion, AccesoADatos y LogicaNegocio son SDK-style y auto-incluyen archivos.

---

## Reglas de negocio

1. Se excluyen pedidos con `EstadoPedido == Cancelado` (valor 5 en el enum).
2. El rango de fechas es inclusivo en ambos extremos: la fecha fin se extiende a `fechaFin.Date.AddDays(1)` para capturar todo el dia.
3. Si `zonaId` es nulo o 0, se consultan todas las zonas.
4. El porcentaje de unidades se calcula sobre el total de unidades del periodo filtrado, no sobre el historico completo.
5. El grafico de barras muestra como maximo los 10 primeros productos; la tabla muestra el ranking completo.
6. Si no hay pedidos en el periodo seleccionado, se muestra el parcial `_SinResultados.cshtml`.
7. La vista es de solo lectura: no hay operaciones de escritura.

---

## Dependencias

- `IZonaEntregaServicio` — ya implementada, solo se inyecta.
- `IReporteVentasRepositorio` — no se reutiliza su logica (tiene firma distinta), pero sirve como referencia de patron.
- Chart.js — cargado desde CDN (no requiere paquete NuGet).
- `Newtonsoft.Json` — ya disponible en el proyecto, usado para serializar arrays a JavaScript.

---

## Operaciones confirmadas

| Operacion      | Incluir |
|----------------|---------|
| Listar/Ranking | Si      |
| Ver detalle    | No      |
| Crear          | No      |
| Editar         | No      |
| Eliminar       | No      |
| Estadisticas   | Si (KPIs + graficos) |
| Exportar CSV   | No (fuera de alcance de este sprint) |

---

## Estado de implementacion

| Paso | Descripcion | Estado |
|------|-------------|--------|
| 1 | `RankingProductoItemDTO` y `RankingProductosDTO` | Pendiente |
| 2 | `IRankingProductosRepositorio` | Pendiente |
| 3 | `IRankingProductosServicio` | Pendiente |
| 4 | No aplica (sin modelo EF nuevo) | N/A |
| 5 | `RankingProductosRepositorio` | Pendiente |
| 6 | `RankingProductosServicio` | Pendiente |
| 7 | `RankingProductosViewModel` | Pendiente |
| 8 | `RankingProductosController` | Pendiente |
| 9 | `Views/RankingProductos/Index.cshtml` | Pendiente |
| 10 | Registro en AutofacConfig.cs + Web.csproj | Pendiente |

---

## Log de cambios

| Fecha | Tipo | Descripcion | Agente |
|-------|------|-------------|--------|
| 2026-07-15 | Inicializacion | Rama creada, contexto documentado con esquema de datos, DTOs, interfaces, servicio, controlador y estructura de vista | feature-planner |
