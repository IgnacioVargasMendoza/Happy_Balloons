using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Enums;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using HappyTimesBalloons.AccesoADatos.Contexto;
using HappyTimesBalloons.Web.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace HappyTimesBalloons.Web.Controllers
{
    // Sign-in implementado con IAuthenticationManager (Microsoft.Owin.Security),
    // sin dependencia en Microsoft.AspNet.Identity.Owin (GHSA-25c8-p796-jg6r).
    [AllowAnonymous]
    public class CuentaController : Controller
    {
        private readonly IAuthServicio _authServicio;
        private readonly IRecuperacionPasswordServicio _recuperacionServicio;
        private readonly IAuditoriaServicio _auditoriaServicio;
        private readonly IAutenticacion2FAServicio _servicio2FA;

        private IAuthenticationManager AuthManager
            => HttpContext.GetOwinContext().Authentication;

        // ApplicationUserManager requiere el contexto OWIN en runtime,
        // por lo que se instancia por demanda en lugar de inyectarse por constructor.
        private ApplicationUserManager GetUserManager()
            => ApplicationUserManager.Create(new ApplicationDbContext());

        public CuentaController(
            IAuthServicio authServicio,
            IAuditoriaServicio auditoriaServicio,
            IRecuperacionPasswordServicio recuperacionServicio,
            IAutenticacion2FAServicio servicio2FA)
        {
            _authServicio = authServicio;
            _auditoriaServicio = auditoriaServicio;
            _recuperacionServicio = recuperacionServicio;
            _servicio2FA = servicio2FA;
        }

        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View(model);

            var ip = Request.UserHostAddress;
            var resultado = await _authServicio.ValidarCredencialesAsync(model.Email, model.Contrasena);

            var tipoAuditoria = resultado.Exito ? TipoOperacion.IniciarSesion : TipoOperacion.AccesoFallido;
            await _auditoriaServicio.RegistrarAsync(
                resultado.UsuarioId ?? model.Email,
                resultado.NombreUsuario ?? model.Email,
                tipoAuditoria, "AspNetUsers",
                detalle: resultado.Mensaje, ip: ip);

            if (!resultado.Exito)
            {
                ModelState.AddModelError("", resultado.Mensaje);
                return View(model);
            }

            // Si el usuario tiene 2FA activo, redirigir al flujo de verificación.
            // Si no, completar el SignIn directamente.
            if (resultado.TieneDobleFactor)
            {
                await _servicio2FA.GenerarYEnviarCodigoAsync(resultado.UsuarioId, model.Email, ip);

                TempData["UsuarioId2FA"] = resultado.UsuarioId;
                TempData["Email2FA"] = _servicio2FA.EnmascararEmail(model.Email);
                TempData["EmailCompleto2FA"] = model.Email;
                TempData["EsAdmin2FA"] = resultado.EsAdmin;

                return RedirectToAction("Verificar", "Autenticacion2FA");
            }

            using (var userManager = GetUserManager())
            {
                var usuario = await userManager.FindByIdAsync(resultado.UsuarioId);
                var identidad = await userManager.CreateIdentityAsync(
                    usuario, DefaultAuthenticationTypes.ApplicationCookie);
                AuthManager.SignIn(new AuthenticationProperties { IsPersistent = false }, identidad);
            }

            if (resultado.EsAdmin)
                return RedirectToAction("Index", "Admin");

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Registro(RegistroViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var resultado = await _authServicio.RegistrarAsync(new RegistroDTO
            {
                Nombre = model.Nombre,
                Email = model.Email,
                Contrasena = model.Contrasena,
                Telefono = model.Telefono,
                Direccion = model.Direccion
            });

            if (!resultado.Exito)
            {
                ModelState.AddModelError("", resultado.Mensaje);
                return View(model);
            }

            using (var userManager = GetUserManager())
            {
                var usuario = await userManager.FindByEmailAsync(model.Email);
                var identidad = await userManager.CreateIdentityAsync(
                    usuario, DefaultAuthenticationTypes.ApplicationCookie);
                AuthManager.SignIn(
                    new AuthenticationProperties { IsPersistent = false },
                    identidad);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Logout()
        {
            var usuarioId = User.Identity.GetUserId() ?? "Anónimo";
            var nombreUsuario = User.Identity.Name ?? "Anónimo";
            var ip = Request.UserHostAddress;

            await _auditoriaServicio.RegistrarAsync(
                usuarioId, nombreUsuario,
                TipoOperacion.CerrarSesion, "AspNetUsers",
                detalle: "Logout", ip: ip);

            AuthManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult OlvideContrasena()
        {
            if (TempData["Error"] != null)
                ViewBag.Error = TempData["Error"];
            return View(new OlvideContrasenaViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> OlvideContrasena(OlvideContrasenaViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var urlBase = Request.Url.GetLeftPart(UriPartial.Authority) +
                          Request.ApplicationPath.TrimEnd('/');

            var resultado = await _recuperacionServicio.SolicitarRecuperacionAsync(
                new RecuperacionPasswordDTO
                {
                    Email = model.Email,
                    UrlBase = urlBase
                });

            TempData["Email"] = model.Email;
            TempData["UrlReset"] = resultado.Datos;

            return RedirectToAction("ConfirmacionEnvio");
        }

        [HttpGet]
        public ActionResult ConfirmacionEnvio()
        {
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> RestablecerContrasena(string token)
        {
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("OlvideContrasena");

            var tokenDto = await _recuperacionServicio.ValidarTokenAsync(token);
            if (tokenDto == null)
            {
                TempData["Error"] = "El enlace de recuperación no es válido o ha expirado.";
                return RedirectToAction("OlvideContrasena");
            }

            return View(new RestablecerContrasenaViewModel { Token = token });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RestablecerContrasena(RestablecerContrasenaViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var resultado = await _recuperacionServicio.RestablecerContrasenaAsync(
                new RestablecerPasswordDTO
                {
                    Token = model.Token,
                    NuevaContrasena = model.NuevaContrasena
                });

            if (!resultado.Exito)
            {
                ModelState.AddModelError("", resultado.Mensaje);
                return View(model);
            }

            return RedirectToAction("ConfirmacionReset");
        }

        [HttpGet]
        public ActionResult ConfirmacionReset()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> ConfigurarDobleFactor()
        {
            var usuarioId = User.Identity.GetUserId();
            using (var userManager = GetUserManager())
            {
                var usuario = await userManager.FindByIdAsync(usuarioId);
                ViewBag.DobleFactorActivo = usuario.TwoFactorEnabled;
            }
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CambiarDobleFactor(bool activar)
        {
            var usuarioId = User.Identity.GetUserId();
            using (var userManager = GetUserManager())
            {
                await userManager.SetTwoFactorEnabledAsync(usuarioId, activar);
            }

            TempData["MensajeDobleFactor"] = activar
                ? "Verificación en dos pasos activada. Se pedirá un código al iniciar sesión."
                : "Verificación en dos pasos desactivada.";

            return RedirectToAction("ConfigurarDobleFactor");
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }
    }
}
