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
        private readonly IAuditoriaServicio _auditoriaServicio;
        private readonly IRecuperacionPasswordServicio _recuperacionServicio;

        private IAuthenticationManager AuthManager
            => HttpContext.GetOwinContext().Authentication;

        // ApplicationUserManager requiere el contexto OWIN en runtime,
        // por lo que se instancia por demanda en lugar de inyectarse por constructor.
        private ApplicationUserManager GetUserManager()
            => ApplicationUserManager.Create(new ApplicationDbContext());

        public CuentaController(
            IAuthServicio authServicio,
            IAuditoriaServicio auditoriaServicio,
            IRecuperacionPasswordServicio recuperacionServicio)
        {
            _authServicio = authServicio;
            _auditoriaServicio = auditoriaServicio;
            _recuperacionServicio = recuperacionServicio;
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

            using (var userManager = GetUserManager())
            {
                var usuario = await userManager.FindByEmailAsync(model.Email);
                var identidad = await userManager.CreateIdentityAsync(
                    usuario, DefaultAuthenticationTypes.ApplicationCookie);
                AuthManager.SignIn(
                    new AuthenticationProperties { IsPersistent = model.Recordarme },
                    identidad);
            }

            if (resultado.EsAdmin)
                return RedirectToAction("Index", "Admin");

            return RedirectToLocal(returnUrl);
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

            var resultado = await _recuperacionServicio.SolicitarRecuperacionAsync(model.Email);

            if (resultado.Exito && resultado.Datos != null)
            {
                var baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
                var enlace = baseUrl + Url.Action("RestablecerContrasena", "Cuenta", new { token = resultado.Datos });
                TempData["EnlaceRecuperacion"] = enlace;
            }

            TempData["EmailRecuperacion"] = model.Email;
            return RedirectToAction("ConfirmacionEnvio");
        }

        [HttpGet]
        public ActionResult ConfirmacionEnvio()
        {
            ViewBag.Email = TempData["EmailRecuperacion"];
            ViewBag.Enlace = TempData["EnlaceRecuperacion"];
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

            var resultado = await _recuperacionServicio.RestablecerContrasenaAsync(model.Token, model.NuevaContrasena);
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

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }
    }
}
