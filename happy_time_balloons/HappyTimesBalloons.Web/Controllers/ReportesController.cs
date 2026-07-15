using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using HappyTimesBalloons.Web.Models.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HappyTimesBalloons.Web.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ReportesController : Controller
    {
        private readonly IReporteVentasServicio _reporteVentasServicio;
        private readonly IReporteInventarioServicio _reporteInventarioServicio;
        private readonly ICategoriaServicio _categoriaServicio;

        public ReportesController(
            IReporteVentasServicio reporteVentasServicio,
            IReporteInventarioServicio reporteInventarioServicio,
            ICategoriaServicio categoriaServicio)
        {
            _reporteVentasServicio = reporteVentasServicio;
            _reporteInventarioServicio = reporteInventarioServicio;
            _categoriaServicio = categoriaServicio;
        }

        // ── Ventas ────────────────────────────────────────────────────────────

        public async Task<ActionResult> Ventas(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var inicio = fechaInicio ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var fin = fechaFin ?? DateTime.Today;

            if (inicio > fin)
            {
                ModelState.AddModelError("", "La fecha de inicio no puede ser posterior a la fecha fin.");
                inicio = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                fin = DateTime.Today;
            }

            var reporte = await _reporteVentasServicio.ObtenerReporteAsync(inicio, fin);

            var vm = new ReporteVentasViewModel
            {
                FechaInicio = inicio,
                FechaFin = fin,
                Reporte = reporte,
                FiltroAplicado = fechaInicio.HasValue || fechaFin.HasValue
            };

            return View(vm);
        }

        public async Task<ActionResult> ExportarCsv(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var inicio = fechaInicio ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var fin = fechaFin ?? DateTime.Today;

            var reporte = await _reporteVentasServicio.ObtenerReporteAsync(inicio, fin);
            var bytes = _reporteVentasServicio.GenerarCsvBytes(reporte);

            string nombreArchivo = $"reporte-ventas-{inicio:yyyy-MM-dd}-{fin:yyyy-MM-dd}.csv";
            return File(bytes, "text/csv", nombreArchivo);
        }

        // ── Inventario ────────────────────────────────────────────────────────

        public async Task<ActionResult> Inventario(int? categoriaId, string estadoStock)
        {
            if (string.IsNullOrEmpty(estadoStock))
                estadoStock = "todos";

            var reporte = await _reporteInventarioServicio.ObtenerReporteAsync(categoriaId, estadoStock);

            var categorias = (await _categoriaServicio.ObtenerTodasAsync(soloActivas: true))
                .OrderBy(c => c.Nombre).ToList();

            var vm = new ReporteInventarioViewModel
            {
                CategoriaId = categoriaId,
                EstadoStock = estadoStock,
                Categorias = new SelectList(categorias, "Id", "Nombre", categoriaId),
                Reporte = reporte,
                FiltroAplicado = categoriaId.HasValue || (!string.IsNullOrEmpty(estadoStock) && estadoStock != "todos")
            };

            return View(vm);
        }

        public async Task<ActionResult> ExportarCsvInventario(int? categoriaId, string estadoStock)
        {
            if (string.IsNullOrEmpty(estadoStock))
                estadoStock = "todos";

            var reporte = await _reporteInventarioServicio.ObtenerReporteAsync(categoriaId, estadoStock);
            var bytes = _reporteInventarioServicio.GenerarCsvBytes(reporte);

            string fecha = DateTime.Today.ToString("yyyy-MM-dd");
            string nombreArchivo = $"reporte-inventario-{fecha}.csv";
            return File(bytes, "text/csv", nombreArchivo);
        }
    }
}
