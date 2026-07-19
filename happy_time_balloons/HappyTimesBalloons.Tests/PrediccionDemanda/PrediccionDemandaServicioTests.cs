using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Enums;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.LogicaNegocio.Servicios;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Tests.PrediccionDemanda
{
    [TestClass]
    public class PrediccionDemandaServicioTests
    {
        private Mock<IPrediccionDemandaRepositorio> _repoMock;
        private PrediccionDemandaServicio _servicio;

        [TestInitialize]
        public void Inicializar()
        {
            _repoMock = new Mock<IPrediccionDemandaRepositorio>();
            _servicio = new PrediccionDemandaServicio(_repoMock.Object);
        }

        // ── ObtenerPrediccionesAsync ────────────────────────────────────

        [TestMethod]
        public async Task ObtenerPrediccionesAsync_SinProductos_RetornaListaVacia()
        {
            _repoMock.Setup(r => r.ObtenerIdsProductosConVentasAsync(TipoPeriodo.Mensual))
                     .ReturnsAsync(new List<int>());

            var resultado = await _servicio.ObtenerPrediccionesAsync(TipoPeriodo.Mensual);

            Assert.AreEqual(0, resultado.Count);
        }

        [TestMethod]
        public async Task ObtenerPrediccionesAsync_TresPeriodos_DatosSuficientesTrue()
        {
            _repoMock.Setup(r => r.ObtenerIdsProductosConVentasAsync(TipoPeriodo.Mensual))
                     .ReturnsAsync(new List<int> { 1 });

            _repoMock.Setup(r => r.ObtenerHistorialVentasPorProductoAsync(1, TipoPeriodo.Mensual))
                     .ReturnsAsync(new List<PeriodoVentaDTO>
                     {
                         new PeriodoVentaDTO { Anio = 2026, NumeroPeriodo = 1, CantidadVendida = 10 },
                         new PeriodoVentaDTO { Anio = 2026, NumeroPeriodo = 2, CantidadVendida = 12 },
                         new PeriodoVentaDTO { Anio = 2026, NumeroPeriodo = 3, CantidadVendida = 14 }
                     });

            _repoMock.Setup(r => r.ObtenerNombreProductoAsync(1)).ReturnsAsync("Globo Rojo");
            _repoMock.Setup(r => r.ObtenerCategoriaProductoAsync(1)).ReturnsAsync("Globos");

            var resultado = await _servicio.ObtenerPrediccionesAsync(TipoPeriodo.Mensual);

            Assert.AreEqual(1, resultado.Count);
            Assert.IsTrue(resultado[0].DatosSuficientes);
            Assert.AreEqual("Globo Rojo", resultado[0].NombreProducto);
            Assert.AreEqual("Globos", resultado[0].Categoria);
            Assert.AreEqual(3, resultado[0].PeriodosAnalizados);
        }

        [TestMethod]
        public async Task ObtenerPrediccionesAsync_DosPeriodos_DatosSuficientesFalse()
        {
            _repoMock.Setup(r => r.ObtenerIdsProductosConVentasAsync(TipoPeriodo.Mensual))
                     .ReturnsAsync(new List<int> { 2 });

            _repoMock.Setup(r => r.ObtenerHistorialVentasPorProductoAsync(2, TipoPeriodo.Mensual))
                     .ReturnsAsync(new List<PeriodoVentaDTO>
                     {
                         new PeriodoVentaDTO { Anio = 2026, NumeroPeriodo = 1, CantidadVendida = 10 },
                         new PeriodoVentaDTO { Anio = 2026, NumeroPeriodo = 2, CantidadVendida = 15 }
                     });

            _repoMock.Setup(r => r.ObtenerNombreProductoAsync(2)).ReturnsAsync("Globo Azul");
            _repoMock.Setup(r => r.ObtenerCategoriaProductoAsync(2)).ReturnsAsync("Globos");

            var resultado = await _servicio.ObtenerPrediccionesAsync(TipoPeriodo.Mensual);

            Assert.AreEqual(1, resultado.Count);
            Assert.IsFalse(resultado[0].DatosSuficientes);
            Assert.AreEqual(0.0, resultado[0].CantidadPredichaProximoPeriodo);
        }

        [TestMethod]
        public async Task ObtenerPrediccionesAsync_TendenciaCreciente_PrediccionMayorAlPromedio()
        {
            _repoMock.Setup(r => r.ObtenerIdsProductosConVentasAsync(TipoPeriodo.Mensual))
                     .ReturnsAsync(new List<int> { 3 });

            // Serie perfectamente creciente: 5, 10, 15 → tendencia = +5
            _repoMock.Setup(r => r.ObtenerHistorialVentasPorProductoAsync(3, TipoPeriodo.Mensual))
                     .ReturnsAsync(new List<PeriodoVentaDTO>
                     {
                         new PeriodoVentaDTO { Anio = 2026, NumeroPeriodo = 1, CantidadVendida = 5 },
                         new PeriodoVentaDTO { Anio = 2026, NumeroPeriodo = 2, CantidadVendida = 10 },
                         new PeriodoVentaDTO { Anio = 2026, NumeroPeriodo = 3, CantidadVendida = 15 }
                     });

            _repoMock.Setup(r => r.ObtenerNombreProductoAsync(3)).ReturnsAsync("Globo Verde");
            _repoMock.Setup(r => r.ObtenerCategoriaProductoAsync(3)).ReturnsAsync("Globos");

            var resultado = await _servicio.ObtenerPrediccionesAsync(TipoPeriodo.Mensual);

            Assert.IsTrue(resultado[0].Tendencia > 0, "La tendencia debe ser positiva");
            Assert.IsTrue(resultado[0].CantidadPredichaProximoPeriodo > resultado[0].PromedioHistorico,
                "Con tendencia creciente la prediccion debe superar el promedio");
        }

        [TestMethod]
        public async Task ObtenerPrediccionesAsync_TendenciaDecreciente_TendenciaEsNegativa()
        {
            _repoMock.Setup(r => r.ObtenerIdsProductosConVentasAsync(TipoPeriodo.Mensual))
                     .ReturnsAsync(new List<int> { 4 });

            // Serie decreciente: 30, 20, 10
            _repoMock.Setup(r => r.ObtenerHistorialVentasPorProductoAsync(4, TipoPeriodo.Mensual))
                     .ReturnsAsync(new List<PeriodoVentaDTO>
                     {
                         new PeriodoVentaDTO { Anio = 2026, NumeroPeriodo = 1, CantidadVendida = 30 },
                         new PeriodoVentaDTO { Anio = 2026, NumeroPeriodo = 2, CantidadVendida = 20 },
                         new PeriodoVentaDTO { Anio = 2026, NumeroPeriodo = 3, CantidadVendida = 10 }
                     });

            _repoMock.Setup(r => r.ObtenerNombreProductoAsync(4)).ReturnsAsync("Globo Morado");
            _repoMock.Setup(r => r.ObtenerCategoriaProductoAsync(4)).ReturnsAsync("Globos");

            var resultado = await _servicio.ObtenerPrediccionesAsync(TipoPeriodo.Mensual);

            Assert.IsTrue(resultado[0].Tendencia < 0, "La tendencia debe ser negativa");
        }

        [TestMethod]
        public async Task ObtenerPrediccionesAsync_CaidaExtreme_PrediccionNuncaNegativa()
        {
            _repoMock.Setup(r => r.ObtenerIdsProductosConVentasAsync(TipoPeriodo.Mensual))
                     .ReturnsAsync(new List<int> { 5 });

            // Caída brutal: 100, 1, 1 → tendencia muy negativa
            _repoMock.Setup(r => r.ObtenerHistorialVentasPorProductoAsync(5, TipoPeriodo.Mensual))
                     .ReturnsAsync(new List<PeriodoVentaDTO>
                     {
                         new PeriodoVentaDTO { Anio = 2026, NumeroPeriodo = 1, CantidadVendida = 100 },
                         new PeriodoVentaDTO { Anio = 2026, NumeroPeriodo = 2, CantidadVendida = 1 },
                         new PeriodoVentaDTO { Anio = 2026, NumeroPeriodo = 3, CantidadVendida = 1 }
                     });

            _repoMock.Setup(r => r.ObtenerNombreProductoAsync(5)).ReturnsAsync("Globo Blanco");
            _repoMock.Setup(r => r.ObtenerCategoriaProductoAsync(5)).ReturnsAsync("Globos");

            var resultado = await _servicio.ObtenerPrediccionesAsync(TipoPeriodo.Mensual);

            Assert.IsTrue(resultado[0].CantidadPredichaProximoPeriodo >= 0,
                "La prediccion nunca debe ser negativa");
        }

        // ── ObtenerDetallePrediccionAsync ──────────────────────────────

        [TestMethod]
        public async Task ObtenerDetallePrediccionAsync_ProductoInexistente_RetornaNull()
        {
            _repoMock.Setup(r => r.ObtenerNombreProductoAsync(99))
                     .ReturnsAsync((string)null);

            var resultado = await _servicio.ObtenerDetallePrediccionAsync(99, TipoPeriodo.Mensual);

            Assert.IsNull(resultado);
        }

        [TestMethod]
        public async Task ObtenerDetallePrediccionAsync_DatosSuficientes_MensajeInformativo()
        {
            _repoMock.Setup(r => r.ObtenerNombreProductoAsync(1)).ReturnsAsync("Globo Rojo");
            _repoMock.Setup(r => r.ObtenerCategoriaProductoAsync(1)).ReturnsAsync("Globos");
            _repoMock.Setup(r => r.ObtenerHistorialVentasPorProductoAsync(1, TipoPeriodo.Mensual))
                     .ReturnsAsync(new List<PeriodoVentaDTO>
                     {
                         new PeriodoVentaDTO { Anio = 2026, NumeroPeriodo = 1, CantidadVendida = 10 },
                         new PeriodoVentaDTO { Anio = 2026, NumeroPeriodo = 2, CantidadVendida = 12 },
                         new PeriodoVentaDTO { Anio = 2026, NumeroPeriodo = 3, CantidadVendida = 14 }
                     });

            var resultado = await _servicio.ObtenerDetallePrediccionAsync(1, TipoPeriodo.Mensual);

            Assert.IsNotNull(resultado);
            Assert.IsTrue(resultado.DatosSuficientes);
            Assert.AreEqual(3, resultado.HistorialPeriodos.Count);
            StringAssert.Contains(resultado.MensajeValidacion, "periodos historicos");
        }

        [TestMethod]
        public async Task ObtenerDetallePrediccionAsync_DatosInsuficientes_MensajeAdvertencia()
        {
            _repoMock.Setup(r => r.ObtenerNombreProductoAsync(2)).ReturnsAsync("Globo Azul");
            _repoMock.Setup(r => r.ObtenerCategoriaProductoAsync(2)).ReturnsAsync("Globos");
            _repoMock.Setup(r => r.ObtenerHistorialVentasPorProductoAsync(2, TipoPeriodo.Mensual))
                     .ReturnsAsync(new List<PeriodoVentaDTO>
                     {
                         new PeriodoVentaDTO { Anio = 2026, NumeroPeriodo = 4, CantidadVendida = 5 }
                     });

            var resultado = await _servicio.ObtenerDetallePrediccionAsync(2, TipoPeriodo.Mensual);

            Assert.IsNotNull(resultado);
            Assert.IsFalse(resultado.DatosSuficientes);
            StringAssert.Contains(resultado.MensajeValidacion, "insuficientes");
        }

        [TestMethod]
        public async Task ObtenerDetallePrediccionAsync_EtiquetasMensuales_ContienNombreMes()
        {
            _repoMock.Setup(r => r.ObtenerNombreProductoAsync(3)).ReturnsAsync("Globo Verde");
            _repoMock.Setup(r => r.ObtenerCategoriaProductoAsync(3)).ReturnsAsync("Globos");
            _repoMock.Setup(r => r.ObtenerHistorialVentasPorProductoAsync(3, TipoPeriodo.Mensual))
                     .ReturnsAsync(new List<PeriodoVentaDTO>
                     {
                         new PeriodoVentaDTO { Anio = 2026, NumeroPeriodo = 1, CantidadVendida = 10 },
                         new PeriodoVentaDTO { Anio = 2026, NumeroPeriodo = 2, CantidadVendida = 12 },
                         new PeriodoVentaDTO { Anio = 2026, NumeroPeriodo = 3, CantidadVendida = 11 },
                         new PeriodoVentaDTO { Anio = 2026, NumeroPeriodo = 6, CantidadVendida = 14 }
                     });

            var resultado = await _servicio.ObtenerDetallePrediccionAsync(3, TipoPeriodo.Mensual);

            StringAssert.Contains(resultado.HistorialPeriodos[0].EtiquetaPeriodo, "Ene");
            StringAssert.Contains(resultado.HistorialPeriodos[3].EtiquetaPeriodo, "Jun");
        }

        [TestMethod]
        public async Task ObtenerDetallePrediccionAsync_MasDeSeisPeriodosEnRepo_SoloRetorna6()
        {
            var historial = new List<PeriodoVentaDTO>();
            for (int i = 1; i <= 10; i++)
            {
                historial.Add(new PeriodoVentaDTO { Anio = 2026, NumeroPeriodo = i, CantidadVendida = i * 5 });
            }

            _repoMock.Setup(r => r.ObtenerNombreProductoAsync(4)).ReturnsAsync("Globo Dorado");
            _repoMock.Setup(r => r.ObtenerCategoriaProductoAsync(4)).ReturnsAsync("Premium");
            _repoMock.Setup(r => r.ObtenerHistorialVentasPorProductoAsync(4, TipoPeriodo.Mensual))
                     .ReturnsAsync(historial);

            var resultado = await _servicio.ObtenerDetallePrediccionAsync(4, TipoPeriodo.Mensual);

            Assert.AreEqual(6, resultado.HistorialPeriodos.Count);
        }

        [TestMethod]
        public async Task ObtenerDetallePrediccionAsync_EtiquetaSemanal_ContieneNumSemana()
        {
            _repoMock.Setup(r => r.ObtenerNombreProductoAsync(5)).ReturnsAsync("Globo Plateado");
            _repoMock.Setup(r => r.ObtenerCategoriaProductoAsync(5)).ReturnsAsync("Premium");
            _repoMock.Setup(r => r.ObtenerHistorialVentasPorProductoAsync(5, TipoPeriodo.Semanal))
                     .ReturnsAsync(new List<PeriodoVentaDTO>
                     {
                         new PeriodoVentaDTO { Anio = 2026, NumeroPeriodo = 10, CantidadVendida = 8 },
                         new PeriodoVentaDTO { Anio = 2026, NumeroPeriodo = 11, CantidadVendida = 9 },
                         new PeriodoVentaDTO { Anio = 2026, NumeroPeriodo = 12, CantidadVendida = 7 }
                     });

            var resultado = await _servicio.ObtenerDetallePrediccionAsync(5, TipoPeriodo.Semanal);

            StringAssert.Contains(resultado.HistorialPeriodos[0].EtiquetaPeriodo, "Sem");
            StringAssert.Contains(resultado.HistorialPeriodos[0].EtiquetaPeriodo, "10");
        }

        [TestMethod]
        public async Task ObtenerDetallePrediccionAsync_EtiquetaTrimestral_ContieneTrimestre()
        {
            _repoMock.Setup(r => r.ObtenerNombreProductoAsync(6)).ReturnsAsync("Globo Coral");
            _repoMock.Setup(r => r.ObtenerCategoriaProductoAsync(6)).ReturnsAsync("Especial");
            _repoMock.Setup(r => r.ObtenerHistorialVentasPorProductoAsync(6, TipoPeriodo.Trimestral))
                     .ReturnsAsync(new List<PeriodoVentaDTO>
                     {
                         new PeriodoVentaDTO { Anio = 2025, NumeroPeriodo = 2, CantidadVendida = 50 },
                         new PeriodoVentaDTO { Anio = 2025, NumeroPeriodo = 3, CantidadVendida = 60 },
                         new PeriodoVentaDTO { Anio = 2025, NumeroPeriodo = 4, CantidadVendida = 55 }
                     });

            var resultado = await _servicio.ObtenerDetallePrediccionAsync(6, TipoPeriodo.Trimestral);

            StringAssert.Contains(resultado.HistorialPeriodos[0].EtiquetaPeriodo, "T2");
        }
    }
}
