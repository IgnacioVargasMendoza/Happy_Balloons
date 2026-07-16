using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using HappyTimesBalloons.Web.Models.ViewModels;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HappyTimesBalloons.Web.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class RankingProductosController : Controller
    {
        private readonly IRankingProductosServicio _rankingServicio;
        private readonly IZonaEntregaServicio _zonaServicio;

        public RankingProductosController(
            IRankingProductosServicio rankingServicio,
            IZonaEntregaServicio zonaServicio)
        {
            _rankingServicio = rankingServicio;
            _zonaServicio = zonaServicio;
        }

        public async Task<ActionResult> Index(
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int? zonaId)
        {
            var ranking = await _rankingServicio.ObtenerRankingAsync(fechaInicio, fechaFin, zonaId);
            var zonas = await _zonaServicio.ObtenerTodasAsync();

            var vm = new RankingProductosViewModel
            {
                FechaInicio = ranking.FechaInicio,
                FechaFin = ranking.FechaFin,
                ZonaId = zonaId,
                Zonas = new SelectList(zonas, "Id", "Nombre", zonaId),
                Ranking = ranking,
                FiltroAplicado = fechaInicio.HasValue || fechaFin.HasValue || zonaId.HasValue
            };

            return View(vm);
        }

        public async Task<JsonResult> DatosGrafico(
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int? zonaId)
        {
            var ranking = await _rankingServicio.ObtenerRankingAsync(fechaInicio, fechaFin, zonaId);

            return Json(new
            {
                etiquetas = ranking.GraficoEtiquetas,
                unidades = ranking.GraficoUnidades,
                ingresos = ranking.GraficoIngresos,
                donaEtiquetas = ranking.DonaEtiquetas,
                donaValores = ranking.DonaValores
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
