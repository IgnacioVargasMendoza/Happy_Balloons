using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Enums;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HappyTimesBalloons.LogicaNegocio.Servicios
{
    public class Autenticacion2FAServicio : IAutenticacion2FAServicio
    {
        private const int TtlMinutos = 5;
        private const int MaxIntentos = 3;

        private readonly ICodigoVerificacion2FARepositorio _repo;
        private readonly IBitacoraRepositorio _bitacora;

        public Autenticacion2FAServicio(
            ICodigoVerificacion2FARepositorio repo,
            IBitacoraRepositorio bitacora)
        {
            _repo = repo;
            _bitacora = bitacora;
        }

        public async Task GenerarYEnviarCodigoAsync(string usuarioId, string email, string ip)
        {
            // Invalida cualquier código anterior antes de generar el nuevo
            await _repo.InvalidarCodigosAnterioresAsync(usuarioId);

            var codigoPlano = GenerarCodigoAleatorio();
            var codigoHash = HashearCodigo(codigoPlano);
            var ahora = DateTime.UtcNow;

            var dto = new CodigoVerificacion2FADTO
            {
                UsuarioId = usuarioId,
                FechaCreacion = ahora,
                FechaExpiracion = ahora.AddMinutes(TtlMinutos),
                Utilizado = false,
                IntentosFallidos = 0
            };

            await _repo.CrearCodigoAsync(dto, codigoHash);

            EnviarEmailCodigo(email, codigoPlano);

            await _bitacora.GuardarAsync(new BitacoraEntradaDTO
            {
                UsuarioId = usuarioId,
                NombreUsuario = email,
                Accion = TipoOperacion.Crear,
                TablaAfectada = "CodigoVerificacion2FA",
                Detalle = "Código 2FA generado y enviado",
                DireccionIp = ip
            });
        }

        public async Task<ResultadoVerificacion2FA> ValidarCodigoAsync(
            string usuarioId, string codigoIngresado, string ip)
        {
            var codigo = await _repo.ObtenerCodigoActivoAsync(usuarioId);

            if (codigo == null)
            {
                // Sin código activo: puede estar expirado o nunca generado
                await _bitacora.GuardarAsync(new BitacoraEntradaDTO
                {
                    UsuarioId = usuarioId,
                    Accion = TipoOperacion.Actualizar,
                    TablaAfectada = "CodigoVerificacion2FA",
                    Detalle = "2FA fallido - código expirado o inexistente",
                    DireccionIp = ip
                });
                return ResultadoVerificacion2FA.CodigoExpirado;
            }

            if (codigo.IntentosFallidos >= MaxIntentos)
            {
                await _repo.MarcarComoUsadoAsync(codigo.Id);
                await _bitacora.GuardarAsync(new BitacoraEntradaDTO
                {
                    UsuarioId = usuarioId,
                    Accion = TipoOperacion.Actualizar,
                    TablaAfectada = "CodigoVerificacion2FA",
                    RegistroId = codigo.Id,
                    Detalle = "2FA bloqueado - intentos máximos alcanzados",
                    DireccionIp = ip
                });
                return ResultadoVerificacion2FA.MaximoIntentosAlcanzado;
            }

            var hashIngresado = HashearCodigo(codigoIngresado?.Trim() ?? string.Empty);
            var codigoHashAlmacenado = await _repo.ObtenerHashAsync(codigo.Id);

            if (!string.Equals(hashIngresado, codigoHashAlmacenado, StringComparison.Ordinal))
            {
                await _repo.IncrementarIntentosAsync(codigo.Id);

                // Verificar si con este intento ya se supera el máximo
                var intentosActualizados = codigo.IntentosFallidos + 1;
                if (intentosActualizados >= MaxIntentos)
                {
                    await _repo.MarcarComoUsadoAsync(codigo.Id);
                    await _bitacora.GuardarAsync(new BitacoraEntradaDTO
                    {
                        UsuarioId = usuarioId,
                        Accion = TipoOperacion.Actualizar,
                        TablaAfectada = "CodigoVerificacion2FA",
                        RegistroId = codigo.Id,
                        Detalle = "2FA bloqueado - intentos máximos alcanzados",
                        DireccionIp = ip
                    });
                    return ResultadoVerificacion2FA.MaximoIntentosAlcanzado;
                }

                await _bitacora.GuardarAsync(new BitacoraEntradaDTO
                {
                    UsuarioId = usuarioId,
                    Accion = TipoOperacion.Actualizar,
                    TablaAfectada = "CodigoVerificacion2FA",
                    RegistroId = codigo.Id,
                    Detalle = $"2FA fallido - código incorrecto (intento {intentosActualizados}/{MaxIntentos})",
                    DireccionIp = ip
                });
                return ResultadoVerificacion2FA.CodigoIncorrecto;
            }

            await _repo.MarcarComoUsadoAsync(codigo.Id);
            await _bitacora.GuardarAsync(new BitacoraEntradaDTO
            {
                UsuarioId = usuarioId,
                Accion = TipoOperacion.Actualizar,
                TablaAfectada = "CodigoVerificacion2FA",
                RegistroId = codigo.Id,
                Detalle = "2FA exitoso",
                DireccionIp = ip
            });
            return ResultadoVerificacion2FA.Exitoso;
        }

        public string EnmascararEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return string.Empty;

            var arroba = email.IndexOf('@');
            if (arroba <= 0)
                return email;

            var usuario = email.Substring(0, arroba);
            var dominio = email.Substring(arroba);

            if (usuario.Length <= 2)
                return usuario + "***" + dominio;

            return usuario.Substring(0, 2) + "***" + dominio;
        }

        private static string GenerarCodigoAleatorio()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                var numero = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 1000000;
                return numero.ToString("D6");
            }
        }

        private static string HashearCodigo(string codigo)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(codigo);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        private static void EnviarEmailCodigo(string destinatario, string codigo)
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
                        Subject = "Tu código de verificación",
                        IsBodyHtml = true,
                        Body = GenerarCuerpoEmail(codigo)
                    };
                    mensaje.To.Add(destinatario);
                    cliente.Send(mensaje);
                }
            }
            catch
            {
                // El flujo continúa aunque el email falle; en desarrollo SMTP no está configurado
            }
        }

        private static string GenerarCuerpoEmail(string codigo)
        {
            return $@"
<div style='font-family:sans-serif;max-width:500px;margin:auto'>
  <h2 style='color:#e91e8c'>Happy Times Balloons</h2>
  <p>Tu código de verificación de doble factor es:</p>
  <div style='font-size:2rem;font-weight:bold;letter-spacing:0.5rem;
              background:#f3e5f5;padding:1rem;text-align:center;
              border-radius:8px;margin:1rem 0'>
    {codigo}
  </div>
  <p style='color:#666;font-size:0.85em'>
    Este código expira en {TtlMinutos} minutos.<br/>
    Si no intentaste iniciar sesión, ignora este mensaje.
  </p>
</div>";
        }
    }
}
