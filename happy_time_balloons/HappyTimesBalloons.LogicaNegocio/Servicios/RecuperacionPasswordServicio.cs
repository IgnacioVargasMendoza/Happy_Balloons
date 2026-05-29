using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using HappyTimesBalloons.AccesoADatos.Contexto;
using HappyTimesBalloons.AccesoADatos.Modelos;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Configuration;
using System.Data.Entity;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace HappyTimesBalloons.LogicaNegocio.Servicios
{
    public class RecuperacionPasswordServicio : IRecuperacionPasswordServicio
    {
        private readonly ApplicationDbContext _ctx;
        private readonly IRecuperacionPasswordRepositorio _repositorio;

        public RecuperacionPasswordServicio(
            ApplicationDbContext ctx,
            IRecuperacionPasswordRepositorio repositorio)
        {
            _ctx = ctx;
            _repositorio = repositorio;
        }

        public async Task<ResultadoOperacionDTO> SolicitarRecuperacionAsync(RecuperacionPasswordDTO solicitud)
        {
            if (string.IsNullOrWhiteSpace(solicitud.Email))
                return ResultadoOperacionDTO.Fallo("El correo es requerido.", CodigoResultado.DatosInvalidos);

            var usuario = await _ctx.Users
                .FirstOrDefaultAsync(u => u.Email == solicitud.Email);

            // Respuesta genérica para no revelar si el email existe (Mensaje vacío = sin URL de dev)
            if (usuario == null)
                return ResultadoOperacionDTO.Ok(string.Empty);

            var token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            var expiracion = DateTime.UtcNow.AddMinutes(ObtenerExpiracionMinutos());

            await _repositorio.GuardarTokenAsync(usuario.Id, token, expiracion);

            var urlReset = $"{solicitud.UrlBase}/Cuenta/RestablecerContrasena?t={token}";

            EnviarEmailRecuperacion(solicitud.Email, urlReset);

            return ResultadoOperacionDTO.Ok(urlReset);
        }

        public async Task<bool> ValidarTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            var dto = await _repositorio.ObtenerTokenValidoAsync(token);
            return dto != null;
        }

        public async Task<ResultadoOperacionDTO> RestablecerContrasenaAsync(RestablecerPasswordDTO datos)
        {
            if (string.IsNullOrWhiteSpace(datos.Token) || string.IsNullOrWhiteSpace(datos.NuevaContrasena))
                return ResultadoOperacionDTO.Fallo("Datos incompletos.", CodigoResultado.DatosInvalidos);

            var tokenDto = await _repositorio.ObtenerTokenValidoAsync(datos.Token);
            if (tokenDto == null)
                return ResultadoOperacionDTO.Fallo("El enlace de recuperación es inválido o ha expirado.", CodigoResultado.DatosInvalidos);

            var userManager = CrearUserManager();
            var resultado = await userManager.RemovePasswordAsync(tokenDto.UsuarioId);
            if (!resultado.Succeeded)
                return ResultadoOperacionDTO.Fallo("No se pudo actualizar la contraseña.", CodigoResultado.Error);

            resultado = await userManager.AddPasswordAsync(tokenDto.UsuarioId, datos.NuevaContrasena);
            if (!resultado.Succeeded)
                return ResultadoOperacionDTO.Fallo(string.Join(" ", resultado.Errors), CodigoResultado.DatosInvalidos);

            await _repositorio.MarcarComoUsadoAsync(tokenDto.Id);

            return ResultadoOperacionDTO.Ok("Contraseña actualizada correctamente.");
        }

        private UserManager<ApplicationUser> CrearUserManager()
        {
            var store = new UserStore<ApplicationUser>(_ctx);
            var mgr = new UserManager<ApplicationUser>(store);
            mgr.PasswordValidator = new PasswordValidator { RequiredLength = 6 };
            return mgr;
        }

        private void EnviarEmailRecuperacion(string destinatario, string urlReset)
        {
            var host = ConfigurationManager.AppSettings["Smtp:Host"];
            var port = ConfigurationManager.AppSettings["Smtp:Port"];
            var usuario = ConfigurationManager.AppSettings["Smtp:Usuario"];
            var clave = ConfigurationManager.AppSettings["Smtp:Contrasena"];

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(usuario))
                return;

            try
            {
                using (var cliente = new SmtpClient(host, int.Parse(port ?? "587")))
                {
                    cliente.EnableSsl = true;
                    cliente.Credentials = new NetworkCredential(usuario, clave);

                    var mensaje = new MailMessage
                    {
                        From = new MailAddress(usuario, "Happy Times Balloons"),
                        Subject = "Recuperación de contraseña",
                        IsBodyHtml = true,
                        Body = GenerarCuerpoEmail(urlReset)
                    };
                    mensaje.To.Add(destinatario);

                    cliente.Send(mensaje);
                }
            }
            catch
            {
                // El enlace se muestra en la vista cuando SMTP no está configurado
            }
        }

        private static string GenerarCuerpoEmail(string urlReset)
        {
            return $@"
<div style='font-family:sans-serif;max-width:500px;margin:auto'>
  <h2 style='color:#e91e8c'>Happy Times Balloons</h2>
  <p>Recibimos una solicitud para restablecer tu contraseña.</p>
  <p>
    <a href='{urlReset}'
       style='background:#e91e8c;color:#fff;padding:10px 20px;
              border-radius:4px;text-decoration:none;display:inline-block'>
      Restablecer contraseña
    </a>
  </p>
  <p style='color:#666;font-size:0.85em'>
    Este enlace expira en {ObtenerExpiracionMinutos()} minutos.<br/>
    Si no solicitaste este cambio, ignora este mensaje.
  </p>
</div>";
        }

        private static int ObtenerExpiracionMinutos()
        {
            return int.TryParse(ConfigurationManager.AppSettings["Token:ExpiracionMinutos"], out int min)
                ? min
                : 60;
        }
    }
}
