using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace HappyTimesBalloons.LogicaNegocio.Servicios
{
    public class NotificacionPedidoServicio : INotificacionPedidoServicio
    {
        public async Task<ResultadoOperacionDTO> EnviarConfirmacionAsync(PedidoDTO pedido, string emailDestinatario)
        {
            var host = ConfigurationManager.AppSettings["Smtp:Host"];
            var usuario = ConfigurationManager.AppSettings["Smtp:Usuario"];

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(usuario))
                return ResultadoOperacionDTO.Fallo("SMTP no configurado.");

            try
            {
                var port = int.Parse(ConfigurationManager.AppSettings["Smtp:Port"] ?? "587");
                var clave = ConfigurationManager.AppSettings["Smtp:Contrasena"];

                using (var cliente = new SmtpClient(host, port))
                {
                    cliente.EnableSsl = true;
                    cliente.Credentials = new NetworkCredential(usuario, clave);

                    var mensaje = new MailMessage
                    {
                        From = new MailAddress(usuario, "Happy Times Balloons"),
                        Subject = $"Confirmación de pedido {pedido.Numero}",
                        IsBodyHtml = true,
                        Body = GenerarHtmlConfirmacion(pedido)
                    };
                    mensaje.To.Add(emailDestinatario);
                    await cliente.SendMailAsync(mensaje);
                }

                return ResultadoOperacionDTO.Ok("Correo de confirmación enviado.");
            }
            catch
            {
                return ResultadoOperacionDTO.Fallo("No se pudo enviar el correo de confirmación.");
            }
        }

        public static string GenerarHtmlConfirmacion(PedidoDTO pedido)
        {
            var sb = new StringBuilder();
            sb.Append(@"
<div style='font-family:Arial,sans-serif;max-width:600px;margin:auto;color:#333;border:1px solid #eee;border-radius:6px;overflow:hidden'>
  <div style='background:#e91e8c;padding:20px 24px;text-align:center'>
    <h1 style='color:#fff;margin:0;font-size:22px'>Happy Times Balloons</h1>
    <p style='color:#fce4ec;margin:4px 0 0'>¡Tu pedido fue confirmado!</p>
  </div>
  <div style='padding:24px'>
    <table style='width:100%;border-collapse:collapse;margin-bottom:16px'>
      <tr>
        <td style='padding:4px 0'><strong>Número de pedido:</strong></td>
        <td style='padding:4px 0;text-align:right'>" + Encode(pedido.Numero) + @"</td>
      </tr>
      <tr>
        <td style='padding:4px 0'><strong>Fecha:</strong></td>
        <td style='padding:4px 0;text-align:right'>" + pedido.FechaPedido.ToString("dd/MM/yyyy HH:mm") + @"</td>
      </tr>
    </table>
    <hr style='border:none;border-top:1px solid #eee;margin:0 0 16px'/>
    <h3 style='margin:0 0 12px;font-size:15px;color:#e91e8c'>Productos</h3>
    <table style='width:100%;border-collapse:collapse;font-size:14px'>
      <thead>
        <tr style='background:#fce4ec'>
          <th style='padding:8px;text-align:left'>Producto</th>
          <th style='padding:8px;text-align:center'>Cant.</th>
          <th style='padding:8px;text-align:right'>Precio</th>
          <th style='padding:8px;text-align:right'>Subtotal</th>
        </tr>
      </thead>
      <tbody>");

            if (pedido.Detalles != null)
            {
                foreach (var d in pedido.Detalles)
                {
                    sb.Append($@"
        <tr style='border-bottom:1px solid #f5f5f5'>
          <td style='padding:8px'>{Encode(d.NombreProducto)}</td>
          <td style='padding:8px;text-align:center'>{d.Cantidad}</td>
          <td style='padding:8px;text-align:right'>₡{d.PrecioUnitario:N0}</td>
          <td style='padding:8px;text-align:right'>₡{d.Subtotal:N0}</td>
        </tr>");
                }
            }

            sb.Append($@"
      </tbody>
    </table>
    <hr style='border:none;border-top:1px solid #eee;margin:16px 0'/>
    <table style='width:100%;border-collapse:collapse;font-size:14px'>
      <tr>
        <td style='padding:4px 0'>Subtotal</td>
        <td style='padding:4px 0;text-align:right'>₡{pedido.Subtotal:N0}</td>
      </tr>
      <tr>
        <td style='padding:4px 0'>Envío ({Encode(pedido.NombreZona)})</td>
        <td style='padding:4px 0;text-align:right'>₡{pedido.CostoEnvio:N0}</td>
      </tr>
      <tr style='font-size:16px;font-weight:bold'>
        <td style='padding:8px 0 0'>Total</td>
        <td style='padding:8px 0 0;text-align:right;color:#e91e8c'>₡{pedido.Total:N0}</td>
      </tr>
    </table>
    <hr style='border:none;border-top:1px solid #eee;margin:16px 0'/>
    <h3 style='margin:0 0 12px;font-size:15px;color:#e91e8c'>Entrega</h3>
    <table style='width:100%;border-collapse:collapse;font-size:14px'>
      <tr>
        <td style='padding:4px 0'><strong>Zona:</strong></td>
        <td style='padding:4px 0;text-align:right'>{Encode(pedido.NombreZona)}</td>
      </tr>
      <tr>
        <td style='padding:4px 0'><strong>Dirección:</strong></td>
        <td style='padding:4px 0;text-align:right'>{Encode(pedido.DireccionEntrega)}</td>
      </tr>
    </table>
    <hr style='border:none;border-top:1px solid #eee;margin:16px 0'/>
    <h3 style='margin:0 0 12px;font-size:15px;color:#e91e8c'>Pago</h3>
    <table style='width:100%;border-collapse:collapse;font-size:14px'>
      <tr>
        <td style='padding:4px 0'><strong>Método:</strong></td>
        <td style='padding:4px 0;text-align:right'>{Encode(pedido.MetodoPago)}</td>
      </tr>
      <tr>
        <td style='padding:4px 0'><strong>Referencia:</strong></td>
        <td style='padding:4px 0;text-align:right'>{Encode(EnmascararReferencia(pedido.MetodoPago, pedido.NumeroReferencia))}</td>
      </tr>
    </table>
    <div style='margin-top:24px;padding:12px;background:#f9fbe7;border-radius:4px;font-size:13px;color:#555'>
      Ante cualquier consulta comunícate con nosotros respondiendo este correo o visitando nuestra tienda.
    </div>
  </div>
  <div style='background:#f5f5f5;padding:12px 24px;text-align:center;font-size:12px;color:#999'>
    © {DateTime.Now.Year} Happy Times Balloons — Costa Rica
  </div>
</div>");

            return sb.ToString();
        }

        public static string EnmascararReferencia(string metodoPago, string referencia)
        {
            if (string.IsNullOrWhiteSpace(referencia))
                return "—";

            if (!string.IsNullOrWhiteSpace(metodoPago) &&
                metodoPago.IndexOf("Tarjeta", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return referencia.Length >= 4
                    ? "****-****-****-" + referencia.Substring(referencia.Length - 4)
                    : "****";
            }

            return referencia;
        }

        private static string Encode(string value)
        {
            if (string.IsNullOrEmpty(value)) return "—";
            return System.Net.WebUtility.HtmlEncode(value);
        }
    }
}
