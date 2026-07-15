using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using HappyTimesBalloons.Web.Models.ViewModels;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HappyTimesBalloons.Web.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ReportesController : Controller
    {
        private readonly IReporteVentasServicio _reporteServicio;

        public ReportesController(IReporteVentasServicio reporteServicio)
        {
            _reporteServicio = reporteServicio;
        }

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

            var reporte = await _reporteServicio.ObtenerReporteAsync(inicio, fin);

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

            var reporte = await _reporteServicio.ObtenerReporteAsync(inicio, fin);
            var bytes = _reporteServicio.GenerarCsvBytes(reporte);

            string nombreArchivo = $"reporte-ventas-{inicio:yyyy-MM-dd}-{fin:yyyy-MM-dd}.csv";
            return File(bytes, "text/csv", nombreArchivo);
        }
    }
}
