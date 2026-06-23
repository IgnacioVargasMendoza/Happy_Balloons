using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Enums;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace HappyTimesBalloons.LogicaNegocio.Servicios
{
    public class SinpeServicio : ISinpeServicio
    {
        private readonly ISinpeRepositorio _sinpeRepo;
        private readonly IPedidoRepositorio _pedidoRepo;
        private readonly IAuditoriaServicio _auditoria;

        public SinpeServicio(
            ISinpeRepositorio sinpeRepo,
            IPedidoRepositorio pedidoRepo,
            IAuditoriaServicio auditoria)
        {
            _sinpeRepo = sinpeRepo;
            _pedidoRepo = pedidoRepo;
            _auditoria = auditoria;
        }

        public async Task<ResultadoOperacionDTO> ProcesarWebhookAsync(SinpeWebhookDTO webhook, string ipOrigen)
        {
            // 1. Verificar token de seguridad
            var tokenEsperado = ConfigurationManager.AppSettings["Sinpe:TokenSeguridad"];
            if (string.IsNullOrEmpty(tokenEsperado) || webhook.TokenSeguridad != tokenEsperado)
            {
                var pagoRechazadoToken = new PagoSinpeDTO
                {
                    PedidoId = 0,
                    NumeroComprobante = webhook.NumeroComprobante ?? string.Empty,
                    Monto = webhook.Monto,
                    NombreTitular = webhook.NombreTitular,
                    TelefonoDestino = webhook.TelefonoDestino,
                    EstadoPago = EstadoPagoSinpe.Rechazado,
                    MotivoRechazo = "Token de seguridad inválido.",
                    FechaRecepcion = DateTime.UtcNow,
                    FechaProcesamiento = DateTime.UtcNow
                };

                if (pagoRechazadoToken.PedidoId > 0)
                {
                    await _sinpeRepo.RegistrarAsync(pagoRechazadoToken);
                }

                await _auditoria.RegistrarAsync(
                    usuarioId: "sistema",
                    nombreUsuario: "Sistema",
                    accion: TipoOperacion.RechazarPagoSinpe,
                    tablaAfectada: "PagosSinpe",
                    registroId: null,
                    detalle: $"Token inválido para comprobante {webhook.NumeroComprobante}.",
                    ip: ipOrigen);

                return ResultadoOperacionDTO.Ok("Webhook recibido.");
            }

            // 2. Verificar duplicado
            bool esDuplicado = await _sinpeRepo.ExisteComprobanteAsync(webhook.NumeroComprobante);
            if (esDuplicado)
            {
                var pagoDuplicado = new PagoSinpeDTO
                {
                    PedidoId = 0,
                    NumeroComprobante = webhook.NumeroComprobante,
                    Monto = webhook.Monto,
                    NombreTitular = webhook.NombreTitular,
                    TelefonoDestino = webhook.TelefonoDestino,
                    EstadoPago = EstadoPagoSinpe.Duplicado,
                    MotivoRechazo = "Comprobante ya procesado anteriormente.",
                    FechaRecepcion = DateTime.UtcNow,
                    FechaProcesamiento = DateTime.UtcNow
                };

                await _auditoria.RegistrarAsync(
                    usuarioId: "sistema",
                    nombreUsuario: "Sistema",
                    accion: TipoOperacion.PagoDuplicadoSinpe,
                    tablaAfectada: "PagosSinpe",
                    registroId: null,
                    detalle: $"Comprobante duplicado: {webhook.NumeroComprobante}.",
                    ip: ipOrigen);

                return ResultadoOperacionDTO.Ok("Webhook recibido.");
            }

            // 3. Buscar pedido por comprobante o monto en estado PagoPendiente (query en BD)
            var pedido = await _pedidoRepo.BuscarPorSinpeAsync(webhook.NumeroComprobante, webhook.Monto);

            if (pedido == null)
            {
                await _auditoria.RegistrarAsync(
                    usuarioId: "sistema",
                    nombreUsuario: "Sistema",
                    accion: TipoOperacion.RechazarPagoSinpe,
                    tablaAfectada: "PagosSinpe",
                    registroId: null,
                    detalle: $"Pedido no encontrado para comprobante {webhook.NumeroComprobante}, monto {webhook.Monto}.",
                    ip: ipOrigen);

                return ResultadoOperacionDTO.Ok("Webhook recibido.");
            }

            // 4. Validar monto exacto
            if (pedido.Total != webhook.Monto)
            {
                var pagoMontoInvalido = new PagoSinpeDTO
                {
                    PedidoId = pedido.Id,
                    NumeroComprobante = webhook.NumeroComprobante,
                    Monto = webhook.Monto,
                    NombreTitular = webhook.NombreTitular,
                    TelefonoDestino = webhook.TelefonoDestino,
                    EstadoPago = EstadoPagoSinpe.Rechazado,
                    MotivoRechazo = $"Monto incorrecto. Esperado: {pedido.Total}, recibido: {webhook.Monto}.",
                    FechaRecepcion = DateTime.UtcNow,
                    FechaProcesamiento = DateTime.UtcNow
                };

                var pagoRegistrado = await _sinpeRepo.RegistrarAsync(pagoMontoInvalido);

                await _auditoria.RegistrarAsync(
                    usuarioId: "sistema",
                    nombreUsuario: "Sistema",
                    accion: TipoOperacion.RechazarPagoSinpe,
                    tablaAfectada: "PagosSinpe",
                    registroId: pagoRegistrado.Id,
                    detalle: pagoMontoInvalido.MotivoRechazo,
                    ip: ipOrigen);

                return ResultadoOperacionDTO.Ok("Webhook recibido.");
            }

            // 5. Pago válido — registrar como Aprobado y actualizar pedido
            var pagoAprobado = new PagoSinpeDTO
            {
                PedidoId = pedido.Id,
                NumeroComprobante = webhook.NumeroComprobante,
                Monto = webhook.Monto,
                NombreTitular = webhook.NombreTitular,
                TelefonoDestino = webhook.TelefonoDestino,
                EstadoPago = EstadoPagoSinpe.Aprobado,
                MotivoRechazo = null,
                FechaRecepcion = DateTime.UtcNow,
                FechaProcesamiento = DateTime.UtcNow
            };

            var pagoGuardado = await _sinpeRepo.RegistrarAsync(pagoAprobado);

            await _pedidoRepo.ActualizarEstadoAsync(pedido.Id, EstadoPedido.Procesando);

            await _auditoria.RegistrarAsync(
                usuarioId: "sistema",
                nombreUsuario: "Sistema",
                accion: TipoOperacion.ProcesarPagoSinpe,
                tablaAfectada: "PagosSinpe",
                registroId: pagoGuardado.Id,
                detalle: $"Pago SINPE aprobado. Comprobante: {webhook.NumeroComprobante}, pedido: {pedido.Numero}.",
                ip: ipOrigen);

            return ResultadoOperacionDTO.Ok("Pago procesado exitosamente.");
        }

        public async Task<IList<PagoSinpeDTO>> ObtenerPagosAsync()
        {
            return await _sinpeRepo.ObtenerTodosAsync();
        }

        public async Task<PagoSinpeDTO> ObtenerPorIdAsync(int id)
        {
            return await _sinpeRepo.ObtenerPorIdAsync(id);
        }
    }
}
