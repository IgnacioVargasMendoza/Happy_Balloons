using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Enums;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.AccesoADatos.Contexto;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace HappyTimesBalloons.AccesoADatos.Repositorios
{
    public class HorarioEntregaRepositorio : IHorarioEntregaRepositorio
    {
        private readonly ApplicationDbContext _ctx;

        public HorarioEntregaRepositorio(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<List<HorarioEntregaDTO>> ObtenerTodosAsync()
        {
            return await _ctx.HorariosEntrega
                .Where(h => h.EsActivo)
                .OrderBy(h => h.HoraInicio)
                .Select(h => new HorarioEntregaDTO
                {
                    Id = h.Id,
                    Etiqueta = h.Etiqueta,
                    HoraInicio = h.HoraInicio,
                    HoraFin = h.HoraFin,
                    CapacidadMaxima = h.CapacidadMaxima,
                    EsActivo = h.EsActivo
                })
                .ToListAsync();
        }

        public async Task<HorarioEntregaDTO> ObtenerPorIdAsync(int id)
        {
            var h = await _ctx.HorariosEntrega.FindAsync(id);
            if (h == null) return null;

            return new HorarioEntregaDTO
            {
                Id = h.Id,
                Etiqueta = h.Etiqueta,
                HoraInicio = h.HoraInicio,
                HoraFin = h.HoraFin,
                CapacidadMaxima = h.CapacidadMaxima,
                EsActivo = h.EsActivo
            };
        }

        // Tarea 2: consulta horarios activos y agrega cupos restantes por fecha
        public async Task<List<HorarioDisponibleDTO>> ObtenerDisponiblesParaFechaAsync(DateTime fecha)
        {
            var fechaSolo = fecha.Date;

            var horarios = await _ctx.HorariosEntrega
                .Where(h => h.EsActivo)
                .OrderBy(h => h.HoraInicio)
                .ToListAsync();

            var resultado = new List<HorarioDisponibleDTO>();

            foreach (var h in horarios)
            {
                // Tarea 5: contar reservas activas para ese horario/fecha
                var reservas = await _ctx.ProgramacionesEntrega
                    .Where(p => p.HorarioEntregaId == h.Id
                        && DbFunctions.TruncateTime(p.FechaEntrega) == fechaSolo
                        && p.EstadoProgramacion != EstadoProgramacionEntrega.Cancelada)
                    .CountAsync();

                resultado.Add(new HorarioDisponibleDTO
                {
                    HorarioEntregaId = h.Id,
                    Etiqueta = h.Etiqueta,
                    HoraInicio = h.HoraInicio,
                    HoraFin = h.HoraFin,
                    CapacidadMaxima = h.CapacidadMaxima,
                    ReservasActuales = reservas
                });
            }

            return resultado;
        }
    }
}
