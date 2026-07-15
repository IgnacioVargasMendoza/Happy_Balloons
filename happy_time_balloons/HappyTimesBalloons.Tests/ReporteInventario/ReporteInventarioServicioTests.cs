using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.LogicaNegocio.Servicios;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Tests.ReporteInventario
{
    [TestClass]
    public class ReporteInventarioServicioTests
    {
        private Mock<IInventarioRepositorio> _inventarioRepoMock;
        private Mock<ICategoriaRepositorio> _categoriaRepoMock;
        private ReporteInventarioServicio _servicio;

        [TestInitialize]
        public void Inicializar()
        {
            _inventarioRepoMock = new Mock<IInventarioRepositorio>();
            _categoriaRepoMock = new Mock<ICategoriaRepositorio>();
            _servicio = new ReporteInventarioServicio(
                _inventarioRepoMock.Object,
                _categoriaRepoMock.Object);
        }

        // ── Test 1: Reporte general sin filtros ────────────────────────────────

        [TestMethod]
        public async Task ObtenerReporteAsync_SinFiltros_RetornaTodosLosProductos()
        {
            var items = new List<InventarioDTO>
            {
                new InventarioDTO { ProductoId = 1, ProductoNombre = "Globo A", CategoriaNombre = "Globos", StockActual = 10, StockMinimo = 5 },
                new InventarioDTO { ProductoId = 2, ProductoNombre = "Globo B", CategoriaNombre = "Globos", StockActual = 3, StockMinimo = 5 },
                new InventarioDTO { ProductoId = 3, ProductoNombre = "Cinta C", CategoriaNombre = "Accesorios", StockActual = 0, StockMinimo = 2 }
            };

            _inventarioRepoMock
                .Setup(r => r.ObtenerTodosAsync(null, null, "todos"))
                .ReturnsAsync(items);

            var resultado = await _servicio.ObtenerReporteAsync(null, "todos");

            Assert.AreEqual(3, resultado.TotalProductos);
            Assert.AreEqual(1, resultado.ProductosStockBajo);
            Assert.AreEqual(1, resultado.ProductosSinStock);
            Assert.AreEqual(3, resultado.Items.Count);
        }

        // ── Test 2: Filtro por categoría ──────────────────────────────────────

        [TestMethod]
        public async Task ObtenerReporteAsync_FiltroCategoria_RetornaSoloEsaCategoria()
        {
            var items = new List<InventarioDTO>
            {
                new InventarioDTO { ProductoId = 1, ProductoNombre = "Globo A", CategoriaNombre = "Globos", StockActual = 10, StockMinimo = 5 }
            };

            _inventarioRepoMock
                .Setup(r => r.ObtenerTodosAsync(null, 1, "todos"))
                .ReturnsAsync(items);

            _categoriaRepoMock
                .Setup(r => r.ObtenerPorIdAsync(1))
                .ReturnsAsync(new CategoriaDTO { Id = 1, Nombre = "Globos" });

            var resultado = await _servicio.ObtenerReporteAsync(categoriaId: 1, estadoStock: "todos");

            Assert.AreEqual(1, resultado.TotalProductos);
            Assert.AreEqual(1, resultado.CategoriaId);
            Assert.AreEqual("Globos", resultado.NombreCategoria);
        }

        // ── Test 3: Identificar productos stock bajo ───────────────────────────

        [TestMethod]
        public async Task ObtenerReporteAsync_FiltroStockBajo_KPIsCorrectos()
        {
            var items = new List<InventarioDTO>
            {
                new InventarioDTO { ProductoId = 1, ProductoNombre = "Globo A", CategoriaNombre = "Globos", StockActual = 2, StockMinimo = 5 },
                new InventarioDTO { ProductoId = 2, ProductoNombre = "Globo B", CategoriaNombre = "Globos", StockActual = 3, StockMinimo = 5 }
            };

            _inventarioRepoMock
                .Setup(r => r.ObtenerTodosAsync(null, null, "bajo"))
                .ReturnsAsync(items);

            var resultado = await _servicio.ObtenerReporteAsync(null, "bajo");

            Assert.AreEqual(2, resultado.TotalProductos);
            Assert.AreEqual(2, resultado.ProductosStockBajo);
            Assert.AreEqual(0, resultado.ProductosSinStock);
            Assert.IsTrue(resultado.Items.TrueForAll(i => i.EstadoStock == "Stock bajo"));
        }

        // ── Test 4: Reporte sin datos devuelve colección vacía ────────────────

        [TestMethod]
        public async Task ObtenerReporteAsync_SinProductos_RetornaReporteVacio()
        {
            _inventarioRepoMock
                .Setup(r => r.ObtenerTodosAsync(null, null, "todos"))
                .ReturnsAsync(new List<InventarioDTO>());

            var resultado = await _servicio.ObtenerReporteAsync();

            Assert.AreEqual(0, resultado.TotalProductos);
            Assert.AreEqual(0, resultado.ProductosStockBajo);
            Assert.AreEqual(0, resultado.ProductosSinStock);
            Assert.AreEqual(0, resultado.Items.Count);
        }

        // ── Test 5: CSV tiene encabezados y filas ─────────────────────────────

        [TestMethod]
        public void GenerarCsvBytes_ReporteConDatos_ContieneEncabezadosYFilas()
        {
            var reporte = new ReporteInventarioDTO
            {
                TotalProductos = 2,
                ProductosStockBajo = 1,
                ProductosSinStock = 0,
                Items = new List<FilaInventarioReporteDTO>
                {
                    new FilaInventarioReporteDTO { NombreProducto = "Globo A", Categoria = "Globos", StockActual = 10, StockMinimo = 5, EstadoStock = "Normal" },
                    new FilaInventarioReporteDTO { NombreProducto = "Globo B", Categoria = "Globos", StockActual = 3, StockMinimo = 5, EstadoStock = "Stock bajo" }
                }
            };

            var bytes = _servicio.GenerarCsvBytes(reporte);
            var contenido = Encoding.UTF8.GetString(bytes).TrimStart('﻿');

            Assert.IsTrue(contenido.Contains("REPORTE DE INVENTARIO"));
            Assert.IsTrue(contenido.Contains("DETALLE DE INVENTARIO"));
            Assert.IsTrue(contenido.Contains("Globo A"));
            Assert.IsTrue(contenido.Contains("Globo B"));
            Assert.IsTrue(contenido.Contains("Normal"));
            Assert.IsTrue(contenido.Contains("Stock bajo"));
        }

        // ── Test 6: CSV vacío no lanza excepción ──────────────────────────────

        [TestMethod]
        public void GenerarCsvBytes_ReporteVacio_RetornaCsvConEncabezados()
        {
            var reporte = new ReporteInventarioDTO
            {
                TotalProductos = 0,
                Items = new List<FilaInventarioReporteDTO>()
            };

            var bytes = _servicio.GenerarCsvBytes(reporte);

            Assert.IsNotNull(bytes);
            Assert.IsTrue(bytes.Length > 0);

            var contenido = Encoding.UTF8.GetString(bytes).TrimStart('﻿');
            Assert.IsTrue(contenido.Contains("DETALLE DE INVENTARIO"));
        }

        // ── Test 7: Nombre de categoría se resuelve cuando hay filtro ─────────

        [TestMethod]
        public async Task ObtenerReporteAsync_ConCategoriaId_ResuelvaNombreCategoria()
        {
            _inventarioRepoMock
                .Setup(r => r.ObtenerTodosAsync(null, 5, "todos"))
                .ReturnsAsync(new List<InventarioDTO>());

            _categoriaRepoMock
                .Setup(r => r.ObtenerPorIdAsync(5))
                .ReturnsAsync(new CategoriaDTO { Id = 5, Nombre = "Decoración" });

            var resultado = await _servicio.ObtenerReporteAsync(categoriaId: 5);

            Assert.AreEqual("Decoración", resultado.NombreCategoria);
            _categoriaRepoMock.Verify(r => r.ObtenerPorIdAsync(5), Times.Once);
        }

        // ── Test 8: Sin filtro de categoría no llama ObtenerPorIdAsync ────────

        [TestMethod]
        public async Task ObtenerReporteAsync_SinCategoriaId_NoConsultaRepositorioCategoria()
        {
            _inventarioRepoMock
                .Setup(r => r.ObtenerTodosAsync(null, null, "todos"))
                .ReturnsAsync(new List<InventarioDTO>());

            await _servicio.ObtenerReporteAsync(categoriaId: null);

            _categoriaRepoMock.Verify(r => r.ObtenerPorIdAsync(It.IsAny<int>()), Times.Never);
        }
    }
}
