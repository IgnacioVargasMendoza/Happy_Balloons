using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Enums;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.AccesoADatos.Contexto;
using HappyTimesBalloons.AccesoADatos.Modelos;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace HappyTimesBalloons.AccesoADatos.Repositorios
{
    public class ProgramacionEntregaRepositorio : IProgramacionEntregaRepositorio
    {
        private readonly ApplicationDbContext _ctx;
        private readonly IBitacoraRepositorio _bitacora;

        public ProgramacionEntregaRepositorio(ApplicationDbContext ctx, IBitacoraRepositorio bitacora)
        {
            _ctx = ctx;
            _bitacora = bitacora;
        }

        public async Task<List<ProgramacionEntregaDTO>> ObtenerTodasAsync()
        {
            return await _ctx.ProgramacionesEntrega
                .Include(p => p.Pedido)
                .Include(p => p.HorarioEntrega)
                .Include(p => p.Usuario)
                .OrderByDescending(p => p.FechaEntrega)
                .Select(p => new ProgramacionEntregaDTO
                {
                    Id = p.Id,
                    PedidoId = p.PedidoId,
                    NumeroPedido = p.Pedido.Numero,
                    HorarioEntregaId = p.HorarioEntregaId,
                    EtiquetaHorario = p.HorarioEntrega.Etiqueta,
                    FechaEntrega = p.FechaEntrega,
                    EstadoProgramacion = p.EstadoProgramacion,
                    FechaProgramacion = p.FechaProgramacion,
                    Notas = p.Notas,
                    UsuarioId = p.UsuarioId,
                    NombreUsuario = p.Usuario.Nombre
                })
                .ToListAsync();
        }

        public async Task<ProgramacionEntregaDTO> ObtenerPorIdAsync(int id)
        {
            var p = await _ctx.ProgramacionesEntrega
                .Include(x => x.Pedido)
                .Include(x => x.HorarioEntrega)
                .Include(x => x.Usuario)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (p == null) return null;

            return new ProgramacionEntregaDTO
            {
                Id = p.Id,
                PedidoId = p.PedidoId,
                NumeroPedido = p.Pedido.Numero,
                HorarioEntregaId = p.HorarioEntregaId,
                EtiquetaHorario = p.HorarioEntrega.Etiqueta,
                FechaEntrega = p.FechaEntrega,
                EstadoProgramacion = p.EstadoProgramacion,
                FechaProgramacion = p.FechaProgramacion,
                Notas = p.Notas,
                UsuarioId = p.UsuarioId,
                NombreUsuario = p.Usuario.Nombre
            };
        }

        public async Task<ProgramacionEntregaDTO> ObtenerPorPedidoIdAsync(int pedidoId)
        {
            var p = await _ctx.ProgramacionesEntrega
                .Include(x => x.Pedido)
                .Include(x => x.HorarioEntrega)
                .Include(x => x.Usuario)
                .FirstOrDefaultAsync(x => x.PedidoId == pedidoId
                    && x.EstadoProgramacion != EstadoProgramacionEntrega.Cancelada);

            if (p == null) return null;

            return new ProgramacionEntregaDTO
            {
                Id = p.Id,
                PedidoId = p.PedidoId,
                NumeroPedido = p.Pedido.Numero,
                HorarioEntregaId = p.HorarioEntregaId,
                EtiquetaHorario = p.HorarioEntrega.Etiqueta,
                FechaEntrega = p.FechaEntrega,
                EstadoProgramacion = p.EstadoProgramacion,
                FechaProgramacion = p.FechaProgramacion,
                Notas = p.Notas,
                UsuarioId = p.UsuarioId,
                NombreUsuario = p.Usuario.Nombre
            };
        }

        // Tarea 5: conteo activo de reservas para calcular disponibilidad en tiempo real
        public async Task<int> ContarReservasPorHorarioYFechaAsync(int horarioEntregaId, DateTime fecha)
        {
            var fechaSolo = fecha.Date;
            return await _ctx.ProgramacionesEntrega
                .Where(p => p.HorarioEntregaId == horarioEntregaId
                    && DbFunctions.TruncateTime(p.FechaEntrega) == fechaSolo
                    && p.EstadoProgramacion != EstadoProgramacionEntrega.Cancelada)
                .CountAsync();
        }

        // Tarea 3: registrar programación de entrega
        public async Task<ResultadoOperacionDTO> RegistrarAsync(ProgramacionEntregaDTO dto, string usuarioId, string nombreUsuario)
        {
            var programacion = new ProgramacionEntrega
            {
                PedidoId = dto.PedidoId,
                HorarioEntregaId = dto.HorarioEntregaId,
                FechaEntrega = dto.FechaEntrega.Date,
                EstadoProgramacion = EstadoProgramacionEntrega.Pendiente,
                FechaProgramacion = DateTime.UtcNow,
                Notas = dto.Notas,
                UsuarioId = usuarioId
            };

            _ctx.ProgramacionesEntrega.Add(programacion);
            await _ctx.SaveChangesAsync();

            await _bitacora.GuardarAsync(new BitacoraEntradaDTO
            {
                UsuarioId = usuarioId,
                NombreUsuario = nombreUsuario,
                Accion = TipoOperacion.Crear,
                TablaAfectada = "ProgramacionesEntrega",
                RegistroId = programacion.Id,
                Detalle = $"Pedido {dto.PedidoId} para {programacion.FechaEntrega:dd/MM/yyyy}"
            });

            return ResultadoOperacionDTO.Ok("Entrega programada correctamente.");
        }

        // Tarea 5: cancelar libera el cupo automáticamente (el conteo baja)
        public async Task<ResultadoOperacionDTO> CancelarAsync(int id, string usuarioId, string nombreUsuario)
        {
            var programacion = await _ctx.ProgramacionesEntrega.FindAsync(id);
            if (programacion == null)
            {
                return ResultadoOperacionDTO.Fallo("La programación de entrega no existe.");
            }

            if (programacion.EstadoProgramacion == EstadoProgramacionEntrega.Cancelada)
            {
                return ResultadoOperacionDTO.Fallo("La programación ya está cancelada.");
            }

            programacion.EstadoProgramacion = EstadoProgramacionEntrega.Cancelada;
            await _ctx.SaveChangesAsync();

            await _bitacora.GuardarAsync(new BitacoraEntradaDTO
            {
                UsuarioId = usuarioId,
                NombreUsuario = nombreUsuario,
                Accion = TipoOperacion.Actualizar,
                TablaAfectada = "ProgramacionesEntrega",
                RegistroId = programacion.Id,
                Detalle = $"Cancelada: Pedido {programacion.PedidoId}"
            });

            return ResultadoOperacionDTO.Ok("Programación de entrega cancelada. El cupo quedó disponible.");
        }
    }
}
