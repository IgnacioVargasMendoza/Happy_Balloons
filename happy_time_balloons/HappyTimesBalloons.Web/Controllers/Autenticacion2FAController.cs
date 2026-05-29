using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using HappyTimesBalloons.AccesoADatos.Contexto;
using HappyTimesBalloons.Web.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace HappyTimesBalloons.Web.Controllers
{
    [AllowAnonymous]
    public class Autenticacion2FAController : Controller
    {
        private readonly IAutenticacion2FAServicio _servicio2FA;

        private IAuthenticationManager AuthManager
            => HttpContext.GetOwinContext().Authentication;

        private ApplicationUserManager GetUserManager()
            => ApplicationUserManager.Create(new ApplicationDbContext());

        public Autenticacion2FAController(IAutenticacion2FAServicio servicio2FA)
        {
            _servicio2FA = servicio2FA;
        }

        [HttpGet]
        public ActionResult Verificar()
        {
            var usuarioId = TempData["UsuarioId2FA"] as string;
            if (string.IsNullOrEmpty(usuarioId))
                return RedirectToAction("Login", "Cuenta");

            // Conservar el dato para el POST
            TempData.Keep("UsuarioId2FA");

            var emailParcial = TempData["Email2FA"] as string;
            TempData.Keep("Email2FA");

            var model = new Verificar2FAViewModel
            {
                EmailParcial = emailParcial
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Verificar(Verificar2FAViewModel model)
        {
            var usuarioId = TempData["UsuarioId2FA"] as string;
            var emailParcial = TempData["Email2FA"] as string;

            if (string.IsNullOrEmpty(usuarioId))
                return RedirectToAction("Login", "Cuenta");

            model.EmailParcial = emailParcial;

            if (!ModelState.IsValid)
            {
                TempData["UsuarioId2FA"] = usuarioId;
                TempData["Email2FA"] = emailParcial;
                return View(model);
            }

            var ip = Request.UserHostAddress;
            var resultado = await _servicio2FA.ValidarCodigoAsync(usuarioId, model.Codigo, ip);

            switch (resultado)
            {
                case ResultadoVerificacion2FA.Exitoso:
                    using (var userManager = GetUserManager())
                    {
                        var usuario = await userManager.FindByIdAsync(usuarioId);
                        if (usuario == null)
                        {
                            ModelState.AddModelError("", "No se pudo completar el inicio de sesión.");
                            TempData["UsuarioId2FA"] = usuarioId;
                            TempData["Email2FA"] = emailParcial;
                            return View(model);
                        }

                        var identidad = await userManager.CreateIdentityAsync(
                            usuario, DefaultAuthenticationTypes.ApplicationCookie);
                        AuthManager.SignIn(
                            new AuthenticationProperties { IsPersistent = false },
                            identidad);
                    }

                    var esAdmin = TempData["EsAdmin2FA"] as bool? ?? false;
                    if (esAdmin)
                        return RedirectToAction("Index", "Admin");

                    return RedirectToAction("Index", "Home");

                case ResultadoVerificacion2FA.CodigoIncorrecto:
                    ModelState.AddModelError("Codigo", "Código incorrecto. Verifica el email e intenta de nuevo.");
                    TempData["UsuarioId2FA"] = usuarioId;
                    TempData["Email2FA"] = emailParcial;
                    return View(model);

                case ResultadoVerificacion2FA.MaximoIntentosAlcanzado:
                    TempData["Error2FA"] = "Has superado el número máximo de intentos. Por seguridad, inicia sesión de nuevo.";
                    return RedirectToAction("Login", "Cuenta");

                case ResultadoVerificacion2FA.CodigoExpirado:
                    TempData["UsuarioId2FA"] = usuarioId;
                    TempData["Email2FA"] = emailParcial;
                    model.MensajeError = "El código ha expirado. Solicita uno nuevo.";
                    return View(model);

                default:
                    return RedirectToAction("Login", "Cuenta");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ReenviarCodigo()
        {
            var usuarioId = TempData["UsuarioId2FA"] as string;
            var emailCompleto = TempData["EmailCompleto2FA"] as string;

            if (string.IsNullOrEmpty(usuarioId) || string.IsNullOrEmpty(emailCompleto))
                return RedirectToAction("Login", "Cuenta");

            var ip = Request.UserHostAddress;
            await _servicio2FA.GenerarYEnviarCodigoAsync(usuarioId, emailCompleto, ip);

            TempData["UsuarioId2FA"] = usuarioId;
            TempData["Email2FA"] = _servicio2FA.EnmascararEmail(emailCompleto);
            TempData["EmailCompleto2FA"] = emailCompleto;
            TempData["MensajeReenvio"] = "Se envió un nuevo código a tu correo.";

            return RedirectToAction("Verificar");
        }

    }
}
