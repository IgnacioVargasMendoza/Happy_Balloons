using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Enums;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using HappyTimesBalloons.LogicaNegocio.Servicios;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace HappyTimesBalloons.Tests.Sinpe
{
    [TestClass]
    public class SinpeServicioTests
    {
        private Mock<ISinpeRepositorio> _sinpeRepoMock;
        private Mock<IPedidoRepositorio> _pedidoRepoMock;
        private Mock<IAuditoriaServicio> _auditoriaMock;
        private SinpeServicio _servicio;

        private const string TokenValido = "token-test-sinpe";
        private const string IpOrigen = "127.0.0.1";

        [TestInitialize]
        public void Inicializar()
        {
            _sinpeRepoMock = new Mock<ISinpeRepositorio>();
            _pedidoRepoMock = new Mock<IPedidoRepositorio>();
            _auditoriaMock = new Mock<IAuditoriaServicio>();

            ConfigurationManager.AppSettings["Sinpe:TokenSeguridad"] = TokenValido;

            _servicio = new SinpeServicio(
                _sinpeRepoMock.Object,
                _pedidoRepoMock.Object,
                _auditoriaMock.Object);
        }

        // ── Test 1: Comprobante válido ────────────────────────────────────────

        [TestMethod]
        public async Task ProcesarWebhook_ComprobanteValido_RetornaExitoso()
        {
            var pedido = new PedidoDTO
            {
                Id = 10,
                Numero = "PED-2026-0010",
                MetodoPago = "SINPE",
                Total = 5000m,
                NumeroReferencia = "COMP-001",
                EstadoPedido = EstadoPedido.PagoPendiente
            };

            var webhook = new SinpeWebhookDTO
            {
                NumeroComprobante = "COMP-001",
                Monto = 5000m,
                NombreTitular = "Juan Pérez",
                TelefonoDestino = "8888-8888",
                TokenSeguridad = TokenValido
            };

            _sinpeRepoMock
                .Setup(r => r.ExisteComprobanteAsync("COMP-001"))
                .ReturnsAsync(false);

            _pedidoRepoMock
                .Setup(r => r.ObtenerTodosAsync(EstadoPedido.PagoPendiente, null))
                .ReturnsAsync(new List<PedidoDTO> { pedido });

            _sinpeRepoMock
                .Setup(r => r.RegistrarAsync(It.IsAny<PagoSinpeDTO>()))
                .ReturnsAsync((PagoSinpeDTO dto) => { dto.Id = 1; return dto; });

            _pedidoRepoMock
                .Setup(r => r.ActualizarEstadoAsync(10, EstadoPedido.Procesando))
                .ReturnsAsync(true);

            _auditoriaMock
                .Setup(a => a.RegistrarAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TipoOperacion>(),
                    It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var resultado = await _servicio.ProcesarWebhookAsync(webhook, IpOrigen);

            Assert.IsTrue(resultado.Exito);
            Assert.AreEqual(CodigoResultado.Exitoso, resultado.Codigo);
        }

        // ── Test 2: Comprobante duplicado ─────────────────────────────────────

        [TestMethod]
        public async Task ProcesarWebhook_ComprobanteDuplicado_RetornaDuplicado()
        {
            var webhook = new SinpeWebhookDTO
            {
                NumeroComprobante = "COMP-DUP",
                Monto = 3000m,
                NombreTitular = "María López",
                TelefonoDestino = "8888-8888",
                TokenSeguridad = TokenValido
            };

            _sinpeRepoMock
                .Setup(r => r.ExisteComprobanteAsync("COMP-DUP"))
                .ReturnsAsync(true);

            _auditoriaMock
                .Setup(a => a.RegistrarAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TipoOperacion>(),
                    It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var resultado = await _servicio.ProcesarWebhookAsync(webhook, IpOrigen);

            Assert.IsTrue(resultado.Exito);

            _auditoriaMock.Verify(a => a.RegistrarAsync(
                It.IsAny<string>(), It.IsAny<string>(),
                TipoOperacion.PagoDuplicadoSinpe,
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);
        }

        // ── Test 3: Monto incorrecto ──────────────────────────────────────────

        [TestMethod]
        public async Task ProcesarWebhook_MontoIncorrecto_RetornaRechazado()
        {
            // El pedido tiene NumeroReferencia == NumeroComprobante, así es encontrado,
            // pero el monto del webhook no coincide con pedido.Total.
            var pedido = new PedidoDTO
            {
                Id = 20,
                Numero = "PED-2026-0020",
                MetodoPago = "SINPE",
                Total = 8000m,
                NumeroReferencia = "COMP-MONTO",
                EstadoPedido = EstadoPedido.PagoPendiente
            };

            var webhook = new SinpeWebhookDTO
            {
                NumeroComprobante = "COMP-MONTO",
                Monto = 5000m,
                NombreTitular = "Carlos Mora",
                TelefonoDestino = "8888-8888",
                TokenSeguridad = TokenValido
            };

            _sinpeRepoMock
                .Setup(r => r.ExisteComprobanteAsync("COMP-MONTO"))
                .ReturnsAsync(false);

            _pedidoRepoMock
                .Setup(r => r.ObtenerTodosAsync(EstadoPedido.PagoPendiente, null))
                .ReturnsAsync(new List<PedidoDTO> { pedido });

            _sinpeRepoMock
                .Setup(r => r.RegistrarAsync(It.IsAny<PagoSinpeDTO>()))
                .ReturnsAsync((PagoSinpeDTO dto) => { dto.Id = 2; return dto; });

            _auditoriaMock
                .Setup(a => a.RegistrarAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TipoOperacion>(),
                    It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var resultado = await _servicio.ProcesarWebhookAsync(webhook, IpOrigen);

            Assert.IsTrue(resultado.Exito);

            _sinpeRepoMock.Verify(r => r.RegistrarAsync(
                It.Is<PagoSinpeDTO>(p => p.EstadoPago == EstadoPagoSinpe.Rechazado)),
                Times.Once);

            _auditoriaMock.Verify(a => a.RegistrarAsync(
                It.IsAny<string>(), It.IsAny<string>(),
                TipoOperacion.RechazarPagoSinpe,
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);
        }

        // ── Test 4: Token inválido ────────────────────────────────────────────

        [TestMethod]
        public async Task ProcesarWebhook_TokenInvalido_RetornaRechazado()
        {
            var webhook = new SinpeWebhookDTO
            {
                NumeroComprobante = "COMP-TOKEN",
                Monto = 2000m,
                NombreTitular = "Ana Castro",
                TelefonoDestino = "8888-8888",
                TokenSeguridad = "token-incorrecto"
            };

            _auditoriaMock
                .Setup(a => a.RegistrarAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TipoOperacion>(),
                    It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var resultado = await _servicio.ProcesarWebhookAsync(webhook, IpOrigen);

            Assert.IsTrue(resultado.Exito);

            _sinpeRepoMock.Verify(r => r.ExisteComprobanteAsync(It.IsAny<string>()), Times.Never);
            _pedidoRepoMock.Verify(r => r.ObtenerTodosAsync(
                It.IsAny<EstadoPedido?>(), It.IsAny<string>()), Times.Never);

            _auditoriaMock.Verify(a => a.RegistrarAsync(
                It.IsAny<string>(), It.IsAny<string>(),
                TipoOperacion.RechazarPagoSinpe,
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);
        }

        // ── Test 5: Pedido no encontrado ──────────────────────────────────────

        [TestMethod]
        public async Task ProcesarWebhook_PedidoNoEncontrado_RetornaRechazado()
        {
            var webhook = new SinpeWebhookDTO
            {
                NumeroComprobante = "COMP-NOPEDIDO",
                Monto = 9999m,
                NombreTitular = "Luis Vargas",
                TelefonoDestino = "8888-8888",
                TokenSeguridad = TokenValido
            };

            _sinpeRepoMock
                .Setup(r => r.ExisteComprobanteAsync("COMP-NOPEDIDO"))
                .ReturnsAsync(false);

            _pedidoRepoMock
                .Setup(r => r.ObtenerTodosAsync(EstadoPedido.PagoPendiente, null))
                .ReturnsAsync(new List<PedidoDTO>());

            _auditoriaMock
                .Setup(a => a.RegistrarAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TipoOperacion>(),
                    It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var resultado = await _servicio.ProcesarWebhookAsync(webhook, IpOrigen);

            Assert.IsTrue(resultado.Exito);

            _sinpeRepoMock.Verify(r => r.RegistrarAsync(It.IsAny<PagoSinpeDTO>()), Times.Never);

            _auditoriaMock.Verify(a => a.RegistrarAsync(
                It.IsAny<string>(), It.IsAny<string>(),
                TipoOperacion.RechazarPagoSinpe,
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);
        }
    }
}
