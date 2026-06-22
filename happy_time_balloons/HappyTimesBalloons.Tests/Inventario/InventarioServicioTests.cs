using System.Threading.Tasks;
using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Enums;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.LogicaNegocio.Servicios;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace HappyTimesBalloons.Tests.Inventario
{
    [TestClass]
    public class InventarioServicioTests
    {
        private Mock<IInventarioRepositorio> _repoMock;
        private Mock<IBitacoraRepositorio> _bitacoraMock;
        private InventarioServicio _servicio;

        [TestInitialize]
        public void Inicializar()
        {
            _repoMock = new Mock<IInventarioRepositorio>();
            _bitacoraMock = new Mock<IBitacoraRepositorio>();
            _servicio = new InventarioServicio(_repoMock.Object, _bitacoraMock.Object);
        }

        // --- Validación de valores numéricos ---

        [TestMethod]
        public async Task ActualizarStockMinimo_StockMinimoNegativo_RetornaFallo()
        {
            var resultado = await _servicio.ActualizarStockMinimoAsync(1, -1, "u1", "Admin");

            Assert.IsFalse(resultado.Exito);
            Assert.AreEqual(CodigoResultado.DatosInvalidos, resultado.Codigo);
            StringAssert.Contains(resultado.Mensaje, "negativo");
        }

        [TestMethod]
        public async Task ActualizarStockMinimo_StockMinimoNegativo_NoLlamaAlRepositorio()
        {
            await _servicio.ActualizarStockMinimoAsync(1, -5, "u1", "Admin");

            _repoMock.Verify(
                r => r.ActualizarStockMinimoAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()),
                Times.Never);
        }

        [TestMethod]
        public async Task ActualizarStockMinimo_StockMinimoCero_EsValido()
        {
            _repoMock
                .Setup(r => r.ActualizarStockMinimoAsync(1, 0, "u1"))
                .ReturnsAsync(true);
            _bitacoraMock
                .Setup(b => b.GuardarAsync(It.IsAny<BitacoraEntradaDTO>()))
                .Returns(Task.CompletedTask);

            var resultado = await _servicio.ActualizarStockMinimoAsync(1, 0, "u1", "Admin");

            Assert.IsTrue(resultado.Exito);
        }

        // --- Existencia del registro en inventario ---

        [TestMethod]
        public async Task ActualizarStockMinimo_InventarioNoEncontrado_RetornaFallo()
        {
            _repoMock
                .Setup(r => r.ActualizarStockMinimoAsync(99, 10, "u1"))
                .ReturnsAsync(false);

            var resultado = await _servicio.ActualizarStockMinimoAsync(99, 10, "u1", "Admin");

            Assert.IsFalse(resultado.Exito);
            Assert.AreEqual(CodigoResultado.DatosInvalidos, resultado.Codigo);
        }

        // --- Flujo exitoso ---

        [TestMethod]
        public async Task ActualizarStockMinimo_Exitoso_RetornaOk()
        {
            _repoMock
                .Setup(r => r.ActualizarStockMinimoAsync(1, 15, "u1"))
                .ReturnsAsync(true);
            _bitacoraMock
                .Setup(b => b.GuardarAsync(It.IsAny<BitacoraEntradaDTO>()))
                .Returns(Task.CompletedTask);

            var resultado = await _servicio.ActualizarStockMinimoAsync(1, 15, "u1", "Admin");

            Assert.IsTrue(resultado.Exito);
            Assert.AreEqual(CodigoResultado.Exitoso, resultado.Codigo);
        }

        // --- Auditoría ---

        [TestMethod]
        public async Task ActualizarStockMinimo_Exitoso_RegistraBitacoraConDatosCorrectos()
        {
            _repoMock
                .Setup(r => r.ActualizarStockMinimoAsync(3, 20, "u1"))
                .ReturnsAsync(true);
            _bitacoraMock
                .Setup(b => b.GuardarAsync(It.IsAny<BitacoraEntradaDTO>()))
                .Returns(Task.CompletedTask);

            await _servicio.ActualizarStockMinimoAsync(3, 20, "u1", "Admin");

            _bitacoraMock.Verify(
                b => b.GuardarAsync(It.Is<BitacoraEntradaDTO>(e =>
                    e.RegistroId == 3 &&
                    e.TablaAfectada == "Inventario" &&
                    e.Accion == TipoOperacion.Actualizar &&
                    e.UsuarioId == "u1" &&
                    e.NombreUsuario == "Admin")),
                Times.Once);
        }

        [TestMethod]
        public async Task ActualizarStockMinimo_InventarioNoEncontrado_NoRegistraBitacora()
        {
            _repoMock
                .Setup(r => r.ActualizarStockMinimoAsync(99, 10, "u1"))
                .ReturnsAsync(false);

            await _servicio.ActualizarStockMinimoAsync(99, 10, "u1", "Admin");

            _bitacoraMock.Verify(
                b => b.GuardarAsync(It.IsAny<BitacoraEntradaDTO>()),
                Times.Never);
        }

        [TestMethod]
        public async Task ActualizarStockMinimo_StockMinimoNegativo_NoRegistraBitacora()
        {
            await _servicio.ActualizarStockMinimoAsync(1, -3, "u1", "Admin");

            _bitacoraMock.Verify(
                b => b.GuardarAsync(It.IsAny<BitacoraEntradaDTO>()),
                Times.Never);
        }
    }
}
