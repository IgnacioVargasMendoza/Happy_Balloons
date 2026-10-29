using System;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using HappyTimesBalloons.Web.Models.ViewModels;

namespace HappyTimesBalloons.Web.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly IGitHubServicio _gitHubServicio;

        public FeedbackController(IGitHubServicio gitHubServicio)
        {
            _gitHubServicio = gitHubServicio;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CrearIssue(FeedbackViewModel model)
        {
            try
            {
                var tipo = string.IsNullOrWhiteSpace(model.Tipo) ? "Mejora" : model.Tipo;
                var titulo = $"[{tipo}] {model.Titulo}";

                var cuerpo = new StringBuilder();
                cuerpo.AppendLine($"## Tipo: {tipo}");
                cuerpo.AppendLine();
                cuerpo.AppendLine("### Descripción");
                cuerpo.AppendLine(model.Descripcion);

                if (!string.IsNullOrEmpty(model.ScreenshotBase64))
                {
                    cuerpo.AppendLine();
                    cuerpo.AppendLine("### Captura de pantalla");
                    cuerpo.AppendLine($"![screenshot](data:image/png;base64,{model.ScreenshotBase64})");
                }

                cuerpo.AppendLine();
                cuerpo.AppendLine("---");
                cuerpo.AppendLine($"**Usuario:** {(User.Identity.IsAuthenticated ? User.Identity.Name : "Anónimo")}");

                if (Request.UrlReferrer != null)
                {
                    cuerpo.AppendLine($"**Página:** {Request.UrlReferrer.PathAndQuery}");
                }

                var label = tipo.ToLowerInvariant();
                var issueUrl = await _gitHubServicio.CrearIssueAsync(titulo, cuerpo.ToString(), new[] { label });

                return Json(new { exito = true, url = issueUrl });
            }
            catch (Exception ex)
            {
                return Json(new { exito = false, mensaje = ex.Message });
            }
        }
    }
}
