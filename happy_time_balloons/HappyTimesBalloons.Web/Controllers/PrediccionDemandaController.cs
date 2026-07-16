using HappyTimesBalloons.Abstraccion.Enums;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using HappyTimesBalloons.Web.Models.ViewModels;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HappyTimesBalloons.Web.Controllers
{
    [Authorize(Roles = "Administrador,Operador")]
    public class PrediccionDemandaController : Controller
    {
        private readonly IPrediccionDemandaServicio _prediccionServicio;

        public PrediccionDemandaController(IPrediccionDemandaServicio prediccionServicio)
        {
            _prediccionServicio = prediccionServicio;
        }

        public async Task<ActionResult> Index(TipoPeriodo tipoPeriodo = TipoPeriodo.Mensual)
        {
            var predicciones = await _prediccionServicio.ObtenerPrediccionesAsync(tipoPeriodo);
            var viewModel = new PrediccionDemandaViewModel
            {
                Predicciones = predicciones,
                TipoPeriodoSeleccionado = tipoPeriodo,
                NombrePeriodo = ObtenerNombrePeriodo(tipoPeriodo)
            };
            return View(viewModel);
        }

        public async Task<ActionResult> Detalle(int id, TipoPeriodo tipoPeriodo = TipoPeriodo.Mensual)
        {
            var detalle = await _prediccionServicio.ObtenerDetallePrediccionAsync(id, tipoPeriodo);
            if (detalle == null)
            {
                return HttpNotFound();
            }

            var viewModel = new PrediccionDemandaDetalleViewModel
            {
                Detalle = detalle,
                TipoPeriodoSeleccionado = tipoPeriodo,
                NombrePeriodo = ObtenerNombrePeriodo(tipoPeriodo)
            };
            return View(viewModel);
        }

        private string ObtenerNombrePeriodo(TipoPeriodo tipo)
        {
            switch (tipo)
            {
                case TipoPeriodo.Semanal: return "Semanal";
                case TipoPeriodo.Trimestral: return "Trimestral";
                default: return "Mensual";
            }
        }
    }
}
