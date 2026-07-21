using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.LogicaNegocio.Servicios;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HappyTimesBalloons.Tests.ConfirmacionPedido
{
    [TestClass]
    public class NotificacionPedidoServicioTests
    {
        private NotificacionPedidoServicio _servicio;

        [TestInitialize]
        public void Inicializar()
        {
            _servicio = new NotificacionPedidoServicio();
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static PedidoDTO PedidoEjemplo() => new PedidoDTO
        {
            Id = 42,
            Numero = "PED-2026-0042",
            NombreUsuario = "María López",
            FechaPedido = new DateTime(2026, 7, 18, 10, 30, 0),
            MetodoPago = "SINPE",
            NumeroReferencia = "8888-1234",
            NombreZona = "Zona Centro",
            DireccionEntrega = "San José, Costa Rica",
            Subtotal = 5000m,
            CostoEnvio = 1000m,
            Total = 6000m,
            Detalles = new List<DetallePedidoDTO>
            {
                new DetallePedidoDTO
                {
                    NombreProducto = "Globo Rojo Grande",
                    Cantidad = 3,
                    PrecioUnitario = 1000m,
                    Subtotal = 3000m
                },
                new DetallePedidoDTO
                {
                    NombreProducto = "Globo Azul Metalico",
                    Cantidad = 2,
                    PrecioUnitario = 1000m,
                    Subtotal = 2000m
                }
            }
        };

        // ── T2/T5: Correo sin SMTP configurado retorna gracefully ─────────────

        [TestMethod]
        public async Task EnviarConfirmacion_SinSmtpConfigurado_RetornaFalloSinExcepcion()
        {
            ConfigurationManager.AppSettings["Smtp:Host"] = "";
            ConfigurationManager.AppSettings["Smtp:Usuario"] = "";

            var resultado = await _servicio.EnviarConfirmacionAsync(PedidoEjemplo(), "cliente@test.com");

            Assert.IsFalse(resultado.Exito);
            Assert.AreEqual(CodigoResultado.Error, resultado.Codigo);
        }

        // ── T1/T3: Template contiene el número de pedido ──────────────────────

        [TestMethod]
        public void GenerarHtml_ContienNumeroPedido()
        {
            var pedido = PedidoEjemplo();

            var html = NotificacionPedidoServicio.GenerarHtmlConfirmacion(pedido);

            StringAssert.Contains(html, pedido.Numero);
        }

        // ── T3: Template lista todos los productos ────────────────────────────

        [TestMethod]
        public void GenerarHtml_ContieneTodosLosProductos()
        {
            var pedido = PedidoEjemplo();

            var html = NotificacionPedidoServicio.GenerarHtmlConfirmacion(pedido);

            StringAssert.Contains(html, "Globo Rojo Grande");
            StringAssert.Contains(html, "Globo Azul Metalico");
        }

        // ── T3: Template contiene totales ─────────────────────────────────────

        [TestMethod]
        public void GenerarHtml_ContieneTotalesYZona()
        {
            var pedido = PedidoEjemplo();

            var html = NotificacionPedidoServicio.GenerarHtmlConfirmacion(pedido);

            StringAssert.Contains(html, pedido.Total.ToString("N0"));
            StringAssert.Contains(html, "Zona Centro");
            // HtmlEncode convierte é → &#233;; comprobamos la parte sin acento
            StringAssert.Contains(html, "San Jos");
            StringAssert.Contains(html, "Costa Rica");
        }

        // ── T4: Tarjeta enmascara referencia ─────────────────────────────────

        [TestMethod]
        public void EnmascararReferencia_Tarjeta_MuestraUltimos4Digitos()
        {
            var resultado = NotificacionPedidoServicio.EnmascararReferencia("Tarjeta", "4111111111111234");

            Assert.AreEqual("****-****-****-1234", resultado);
        }

        // ── T4: Tarjeta con referencia corta ──────────────────────────────────

        [TestMethod]
        public void EnmascararReferencia_TarjetaReferenciaMuyCorta_MuestraAsteriscos()
        {
            var resultado = NotificacionPedidoServicio.EnmascararReferencia("Tarjeta", "12");

            Assert.AreEqual("****", resultado);
        }

        // ── T4: SINPE no enmascara ────────────────────────────────────────────

        [TestMethod]
        public void EnmascararReferencia_Sinpe_MuestraReferenciaCompleta()
        {
            var resultado = NotificacionPedidoServicio.EnmascararReferencia("SINPE", "COMP-2026-0099");

            Assert.AreEqual("COMP-2026-0099", resultado);
        }

        // ── T4: Referencia vacía retorna guión ───────────────────────────────

        [TestMethod]
        public void EnmascararReferencia_ReferenciaVacia_RetornaGuion()
        {
            var resultado = NotificacionPedidoServicio.EnmascararReferencia("SINPE", "");

            Assert.AreEqual("—", resultado);
        }

        // ── T5: Template con pedido sin detalles no lanza excepción ───────────

        [TestMethod]
        public void GenerarHtml_PedidoSinDetalles_NoLanzaExcepcion()
        {
            var pedido = PedidoEjemplo();
            pedido.Detalles = null;

            var html = NotificacionPedidoServicio.GenerarHtmlConfirmacion(pedido);

            Assert.IsNotNull(html);
            StringAssert.Contains(html, pedido.Numero);
        }

        // ── T4: Template enmascara tarjeta en el HTML generado ────────────────

        [TestMethod]
        public void GenerarHtml_MetodoPagoTarjeta_ReferenciaSaleEnmascarada()
        {
            var pedido = PedidoEjemplo();
            pedido.MetodoPago = "Tarjeta";
            pedido.NumeroReferencia = "4111111111111111";

            var html = NotificacionPedidoServicio.GenerarHtmlConfirmacion(pedido);

            StringAssert.Contains(html, "****-****-****-1111");
            Assert.IsFalse(html.Contains("4111111111111111"),
                "El HTML no debe exponer el número completo de tarjeta.");
        }
    }
}
