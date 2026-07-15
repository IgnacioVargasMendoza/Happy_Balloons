using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Enums;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.LogicaNegocio.Servicios;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Tests.ProgramacionEntrega
{
    [TestClass]
    public class ProgramacionEntregaServicioTests
    {
        private Mock<IProgramacionEntregaRepositorio> _repoMock;
        private Mock<IHorarioEntregaRepositorio> _horarioRepoMock;
        private ProgramacionEntregaServicio _servicio;

        [TestInitialize]
        public void Inicializar()
        {
            _repoMock = new Mock<IProgramacionEntregaRepositorio>();
            _horarioRepoMock = new Mock<IHorarioEntregaRepositorio>();
            _servicio = new ProgramacionEntregaServicio(_repoMock.Object, _horarioRepoMock.Object);
        }

        // ── Tarea 4: Validación de fechas ─────────────────────────────────────

        [TestMethod]
        public void EsFechaPermitida_FechaHoy_RetornaFalse()
        {
            var resultado = _servicio.EsFechaPermitida(DateTime.Today, out var error);

            Assert.IsFalse(resultado);
            Assert.IsNotNull(error);
            StringAssert.Contains(error, "mañana");
        }

        [TestMethod]
        public void EsFechaPermitida_FechaPasada_RetornaFalse()
        {
            var resultado = _servicio.EsFechaPermitida(DateTime.Today.AddDays(-3), out var error);

            Assert.IsFalse(resultado);
            Assert.IsNotNull(error);
        }

        [TestMethod]
        public void EsFechaPermitida_Domingo_RetornaFalse()
        {
            var domingo = ObtenerProximoDomingo();

            var resultado = _servicio.EsFechaPermitida(domingo, out var error);

            Assert.IsFalse(resultado);
            Assert.IsNotNull(error);
            StringAssert.Contains(error, "domingo");
        }

        [TestMethod]
        public void EsFechaPermitida_MasDe30Dias_RetornaFalse()
        {
            var fecha = DateTime.Today.AddDays(31);

            var resultado = _servicio.EsFechaPermitida(fecha, out var error);

            Assert.IsFalse(resultado);
            Assert.IsNotNull(error);
            StringAssert.Contains(error, "30");
        }

        [TestMethod]
        public void EsFechaPermitida_MananaNoEsDomingo_RetornaTrue()
        {
            var manana = DateTime.Today.AddDays(1);
            if (manana.DayOfWeek == DayOfWeek.Sunday)
            {
                manana = manana.AddDays(1);
            }

            var resultado = _servicio.EsFechaPermitida(manana, out var error);

            Assert.IsTrue(resultado);
            Assert.IsNull(error);
        }

        // ── Tarea 3: Registro de programación ────────────────────────────────

        [TestMethod]
        public async Task RegistrarAsync_PedidoYaProgramado_RetornaFallo()
        {
            var fecha = DateTime.Today.AddDays(2);
            var dto = new ProgramacionEntregaDTO
            {
                PedidoId = 1,
                HorarioEntregaId = 1,
                FechaEntrega = fecha
            };

            _repoMock.Setup(r => r.ObtenerPorPedidoIdAsync(1))
                     .ReturnsAsync(new ProgramacionEntregaDTO { Id = 99 });

            var resultado = await _servicio.RegistrarAsync(dto, "u1", "Admin");

            Assert.IsFalse(resultado.Exito);
            StringAssert.Contains(resultado.Mensaje, "ya tiene una entrega programada");
        }

        [TestMethod]
        public async Task RegistrarAsync_SinCuposDisponibles_RetornaFallo()
        {
            var fecha = DateTime.Today.AddDays(2);
            if (fecha.DayOfWeek == DayOfWeek.Sunday) fecha = fecha.AddDays(1);

            var dto = new ProgramacionEntregaDTO
            {
                PedidoId = 2,
                HorarioEntregaId = 1,
                FechaEntrega = fecha
            };

            _repoMock.Setup(r => r.ObtenerPorPedidoIdAsync(2))
                     .ReturnsAsync((ProgramacionEntregaDTO)null);

            _repoMock.Setup(r => r.ContarReservasPorHorarioYFechaAsync(1, fecha.Date))
                     .ReturnsAsync(10);

            _horarioRepoMock.Setup(r => r.ObtenerPorIdAsync(1))
                            .ReturnsAsync(new HorarioEntregaDTO
                            {
                                Id = 1,
                                Etiqueta = "Mañana",
                                CapacidadMaxima = 10
                            });

            var resultado = await _servicio.RegistrarAsync(dto, "u1", "Admin");

            Assert.IsFalse(resultado.Exito);
            StringAssert.Contains(resultado.Mensaje, "cupos disponibles");
        }

        [TestMethod]
        public async Task RegistrarAsync_DatosValidos_RetornaExito()
        {
            var fecha = DateTime.Today.AddDays(2);
            if (fecha.DayOfWeek == DayOfWeek.Sunday) fecha = fecha.AddDays(1);

            var dto = new ProgramacionEntregaDTO
            {
                PedidoId = 3,
                HorarioEntregaId = 1,
                FechaEntrega = fecha
            };

            _repoMock.Setup(r => r.ObtenerPorPedidoIdAsync(3))
                     .ReturnsAsync((ProgramacionEntregaDTO)null);

            _repoMock.Setup(r => r.ContarReservasPorHorarioYFechaAsync(1, fecha.Date))
                     .ReturnsAsync(5);

            _horarioRepoMock.Setup(r => r.ObtenerPorIdAsync(1))
                            .ReturnsAsync(new HorarioEntregaDTO
                            {
                                Id = 1,
                                Etiqueta = "Mañana",
                                CapacidadMaxima = 10
                            });

            _repoMock.Setup(r => r.RegistrarAsync(dto, "u1", "Admin"))
                     .ReturnsAsync(ResultadoOperacionDTO.Ok("Entrega programada correctamente."));

            var resultado = await _servicio.RegistrarAsync(dto, "u1", "Admin");

            Assert.IsTrue(resultado.Exito);
            StringAssert.Contains(resultado.Mensaje, "correctamente");
        }

        // ── Tarea 2: Horarios disponibles ────────────────────────────────────

        [TestMethod]
        public async Task ObtenerHorariosDisponiblesAsync_FechaValida_RetornaListaDeHorarios()
        {
            var fecha = DateTime.Today.AddDays(1);
            var horariosMock = new List<HorarioDisponibleDTO>
            {
                new HorarioDisponibleDTO { HorarioEntregaId = 1, Etiqueta = "Mañana",  CapacidadMaxima = 10, ReservasActuales = 3 },
                new HorarioDisponibleDTO { HorarioEntregaId = 2, Etiqueta = "Tarde",   CapacidadMaxima = 10, ReservasActuales = 10 },
                new HorarioDisponibleDTO { HorarioEntregaId = 3, Etiqueta = "Noche",   CapacidadMaxima = 5,  ReservasActuales = 0 }
            };

            _horarioRepoMock.Setup(r => r.ObtenerDisponiblesParaFechaAsync(fecha))
                            .ReturnsAsync(horariosMock);

            var resultado = await _servicio.ObtenerHorariosDisponiblesAsync(fecha);

            Assert.AreEqual(3, resultado.Count);
            Assert.AreEqual(7, resultado[0].CuposRestantes);
            Assert.AreEqual(0, resultado[1].CuposRestantes);
            Assert.IsFalse(resultado[1].TieneCupo);
            Assert.IsTrue(resultado[2].TieneCupo);
        }

        // ── Tarea 5: Cancelación y actualización de disponibilidad ────────────

        [TestMethod]
        public async Task CancelarAsync_ProgramacionExistente_LlamaAlRepositorioYRetornaExito()
        {
            _repoMock.Setup(r => r.CancelarAsync(7, "u1", "Admin"))
                     .ReturnsAsync(ResultadoOperacionDTO.Ok("Programación de entrega cancelada. El cupo quedó disponible."));

            var resultado = await _servicio.CancelarAsync(7, "u1", "Admin");

            Assert.IsTrue(resultado.Exito);
            StringAssert.Contains(resultado.Mensaje, "cancelada");
            _repoMock.Verify(r => r.CancelarAsync(7, "u1", "Admin"), Times.Once);
        }

        // ── Helper ──────────────────────────────────────────────────────────

        private static DateTime ObtenerProximoDomingo()
        {
            var fecha = DateTime.Today.AddDays(1);
            while (fecha.DayOfWeek != DayOfWeek.Sunday)
            {
                fecha = fecha.AddDays(1);
            }
            return fecha;
        }
    }
}
