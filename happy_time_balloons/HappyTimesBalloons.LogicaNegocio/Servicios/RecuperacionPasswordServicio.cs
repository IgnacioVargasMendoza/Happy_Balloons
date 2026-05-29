using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace HappyTimesBalloons.LogicaNegocio.Servicios
{
    public class RecuperacionPasswordServicio : IRecuperacionPasswordServicio
    {
        private readonly IRecuperacionPasswordRepositorio _repo;

        public RecuperacionPasswordServicio(IRecuperacionPasswordRepositorio repo)
        {
            _repo = repo;
        }

        public async Task<ResultadoOperacionDTO<string>> SolicitarRecuperacionAsync(RecuperacionPasswordDTO solicitud)
        {
            if (!await _repo.ExisteEmailAsync(solicitud.Email))
                return ResultadoOperacionDTO<string>.Ok("Si el correo existe, recibirás un enlace de recuperación.");

            var usuarioId = await _repo.ObtenerUsuarioIdPorEmailAsync(solicitud.Email);
            var token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            var expiracion = DateTime.UtcNow.AddMinutes(ObtenerExpiracionMinutos());

            await _repo.GuardarTokenAsync(usuarioId, token, expiracion);

            var urlReset = $"{solicitud.UrlBase}/Cuenta/RestablecerContrasena?token={token}";

            EnviarEmailRecuperacion(solicitud.Email, urlReset);

            return ResultadoOperacionDTO<string>.Ok("Enlace generado.", urlReset);
        }

        public async Task<TokenRecuperacionDTO> ValidarTokenAsync(string token)
        {
            return await _repo.ObtenerTokenValidoAsync(token);
        }

        public async Task<ResultadoOperacionDTO> RestablecerContrasenaAsync(RestablecerPasswordDTO datos)
        {
            if (string.IsNullOrWhiteSpace(datos.Token) || string.IsNullOrWhiteSpace(datos.NuevaContrasena))
                return ResultadoOperacionDTO.Fallo("Datos incompletos.", CodigoResultado.DatosInvalidos);

            var tokenDto = await _repo.ObtenerTokenValidoAsync(datos.Token);
            if (tokenDto == null)
                return ResultadoOperacionDTO.Fallo(
                    "El enlace de recuperación no es válido o ha expirado.",
                    CodigoResultado.DatosInvalidos);

            var resultado = await _repo.RestablecerPasswordAsync(tokenDto.UsuarioId, datos.NuevaContrasena);
            if (!resultado.Exito)
                return resultado;

            await _repo.MarcarComoUsadoAsync(tokenDto.Id);
            return ResultadoOperacionDTO.Ok("Contraseña actualizada exitosamente.");
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
