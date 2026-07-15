using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.LogicaNegocio.Servicios;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Tests.ReporteVentas
{
    [TestClass]
    public class ReporteVentasServicioTests
    {
        private Mock<IReporteVentasRepositorio> _repoMock;
        private ReporteVentasServicio _servicio;

        [TestInitialize]
        public void Inicializar()
        {
            _repoMock = new Mock<IReporteVentasRepositorio>();
            _servicio = new ReporteVentasServicio(_repoMock.Object);
        }

        // ── Test 1: Reporte con datos ──────────────────────────────────────────

        [TestMethod]
        public async Task ObtenerReporteAsync_RangoConPedidos_RetornaKPIsCorrectos()
        {
            var inicio = new DateTime(2026, 1, 1);
            var fin = new DateTime(2026, 1, 31);

            var reporteEsperado = new ReporteVentasDTO
            {
                FechaInicio = inicio,
                FechaFin = fin,
                TotalPedidos = 3,
                IngresosTotales = 15000m,
                TicketPromedio = 5000m,
                UnidadesVendidas = 10,
                Pedidos = new List<FilaPedidoReporteDTO>
                {
                    new FilaPedidoReporteDTO { Id = 1, Numero = "PED-2026-0001", Total = 5000m },
                    new FilaPedidoReporteDTO { Id = 2, Numero = "PED-2026-0002", Total = 4000m },
                    new FilaPedidoReporteDTO { Id = 3, Numero = "PED-2026-0003", Total = 6000m }
                },
                ProductosMasVendidos = new List<LineaProductoReporteDTO>
                {
                    new LineaProductoReporteDTO { NombreProducto = "Globo Metálico", UnidadesVendidas = 6, Ingresos = 9000m }
                }
            };

            _repoMock.Setup(r => r.ObtenerReporteAsync(inicio, fin))
                     .ReturnsAsync(reporteEsperado);

            var resultado = await _servicio.ObtenerReporteAsync(inicio, fin);

            Assert.AreEqual(3, resultado.TotalPedidos);
            Assert.AreEqual(15000m, resultado.IngresosTotales);
            Assert.AreEqual(5000m, resultado.TicketPromedio);
            Assert.AreEqual(10, resultado.UnidadesVendidas);
            Assert.AreEqual(3, resultado.Pedidos.Count);
            Assert.AreEqual(1, resultado.ProductosMasVendidos.Count);
        }

        // ── Test 2: Rango sin resultados ──────────────────────────────────────

        [TestMethod]
        public async Task ObtenerReporteAsync_RangoSinPedidos_RetornaReporteVacio()
        {
            var inicio = new DateTime(2025, 1, 1);
            var fin = new DateTime(2025, 1, 31);

            var reporteVacio = new ReporteVentasDTO
            {
                FechaInicio = inicio,
                FechaFin = fin,
                TotalPedidos = 0,
                IngresosTotales = 0m,
                TicketPromedio = 0m,
                UnidadesVendidas = 0,
                Pedidos = new List<FilaPedidoReporteDTO>(),
                ProductosMasVendidos = new List<LineaProductoReporteDTO>()
            };

            _repoMock.Setup(r => r.ObtenerReporteAsync(inicio, fin))
                     .ReturnsAsync(reporteVacio);

            var resultado = await _servicio.ObtenerReporteAsync(inicio, fin);

            Assert.AreEqual(0, resultado.TotalPedidos);
            Assert.AreEqual(0m, resultado.IngresosTotales);
            Assert.AreEqual(0, resultado.Pedidos.Count);
            Assert.AreEqual(0, resultado.ProductosMasVendidos.Count);
        }

        // ── Test 3: Fechas invertidas lanza excepción ─────────────────────────

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ObtenerReporteAsync_FechaInicioMayorQueFin_LanzaExcepcion()
        {
            var inicio = new DateTime(2026, 3, 31);
            var fin = new DateTime(2026, 3, 1);

            await _servicio.ObtenerReporteAsync(inicio, fin);
        }

        // ── Test 4: Exportación CSV tiene contenido correcto ──────────────────

        [TestMethod]
        public void GenerarCsvBytes_ReporteConDatos_ContieneEncabezadosYFilas()
        {
            var reporte = new ReporteVentasDTO
            {
                FechaInicio = new DateTime(2026, 1, 1),
                FechaFin = new DateTime(2026, 1, 31),
                TotalPedidos = 1,
                IngresosTotales = 5000m,
                TicketPromedio = 5000m,
                UnidadesVendidas = 3,
                Pedidos = new List<FilaPedidoReporteDTO>
                {
                    new FilaPedidoReporteDTO
                    {
                        Id = 1,
                        Numero = "PED-2026-0001",
                        FechaPedido = new DateTime(2026, 1, 15),
                        NombreCliente = "Ana López",
                        MetodoPago = "SINPE",
                        Total = 5000m
                    }
                },
                ProductosMasVendidos = new List<LineaProductoReporteDTO>
                {
                    new LineaProductoReporteDTO
                    {
                        NombreProducto = "Globo Metálico",
                        Categoria = "Globos",
                        UnidadesVendidas = 3,
                        Ingresos = 5000m
                    }
                }
            };

            var bytes = _servicio.GenerarCsvBytes(reporte);
            var contenido = Encoding.UTF8.GetString(bytes).TrimStart('﻿');

            Assert.IsTrue(contenido.Contains("REPORTE DE VENTAS"));
            Assert.IsTrue(contenido.Contains("PEDIDOS ENTREGADOS"));
            Assert.IsTrue(contenido.Contains("PED-2026-0001"));
            Assert.IsTrue(contenido.Contains("Ana López"));
            Assert.IsTrue(contenido.Contains("PRODUCTOS MÁS VENDIDOS"));
            Assert.IsTrue(contenido.Contains("Globo Metálico"));
        }

        // ── Test 5: CSV con campos que contienen comas se escapan correctamente ─

        [TestMethod]
        public void GenerarCsvBytes_ProductoConComa_SeEscapaCorrectamente()
        {
            var reporte = new ReporteVentasDTO
            {
                FechaInicio = new DateTime(2026, 1, 1),
                FechaFin = new DateTime(2026, 1, 31),
                TotalPedidos = 1,
                IngresosTotales = 1000m,
                TicketPromedio = 1000m,
                UnidadesVendidas = 1,
                Pedidos = new List<FilaPedidoReporteDTO>(),
                ProductosMasVendidos = new List<LineaProductoReporteDTO>
                {
                    new LineaProductoReporteDTO
                    {
                        NombreProducto = "Globo, Grande",
                        Categoria = "Globos",
                        UnidadesVendidas = 1,
                        Ingresos = 1000m
                    }
                }
            };

            var bytes = _servicio.GenerarCsvBytes(reporte);
            var contenido = Encoding.UTF8.GetString(bytes).TrimStart('﻿');

            Assert.IsTrue(contenido.Contains("\"Globo, Grande\""),
                "Los campos con coma deben estar entre comillas dobles.");
        }

        // ── Test 6: CSV con reporte vacío no lanza excepción ─────────────────

        [TestMethod]
        public void GenerarCsvBytes_ReporteVacio_RetornaCsvConEncabezados()
        {
            var reporte = new ReporteVentasDTO
            {
                FechaInicio = new DateTime(2026, 1, 1),
                FechaFin = new DateTime(2026, 1, 31),
                TotalPedidos = 0,
                IngresosTotales = 0m,
                TicketPromedio = 0m,
                UnidadesVendidas = 0,
                Pedidos = new List<FilaPedidoReporteDTO>(),
                ProductosMasVendidos = new List<LineaProductoReporteDTO>()
            };

            var bytes = _servicio.GenerarCsvBytes(reporte);

            Assert.IsNotNull(bytes);
            Assert.IsTrue(bytes.Length > 0);

            var contenido = Encoding.UTF8.GetString(bytes).TrimStart('﻿');
            Assert.IsTrue(contenido.Contains("PEDIDOS ENTREGADOS"));
            Assert.IsTrue(contenido.Contains("PRODUCTOS MÁS VENDIDOS"));
        }
    }
}
