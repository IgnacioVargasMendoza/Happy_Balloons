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
    public class ZonaEntregaRepositorio : IZonaEntregaRepositorio
    {
        private readonly ApplicationDbContext _ctx;
        private readonly IBitacoraRepositorio _bitacora;

        public ZonaEntregaRepositorio(ApplicationDbContext ctx, IBitacoraRepositorio bitacora)
        {
            _ctx = ctx;
            _bitacora = bitacora;
        }

        public async Task<List<ZonaEntregaDTO>> ObtenerTodasAsync()
        {
            return await _ctx.ZonasEntrega
                .Where(z => z.EsDisponible)
                .OrderBy(z => z.CostoEnvio)
                .Select(z => new ZonaEntregaDTO
                {
                    Id = z.Id,
                    Nombre = z.Nombre,
                    Descripcion = z.Descripcion,
                    CostoEnvio = z.CostoEnvio,
                    EsDisponible = z.EsDisponible,
                    FechaCreacion = z.FechaCreacion
                })
                .ToListAsync();
        }

        public async Task<ZonaEntregaDTO> ObtenerPorIdAsync(int id)
        {
            var z = await _ctx.ZonasEntrega.FindAsync(id);
            if (z == null) return null;

            return new ZonaEntregaDTO
            {
                Id = z.Id,
                Nombre = z.Nombre,
                Descripcion = z.Descripcion,
                CostoEnvio = z.CostoEnvio,
                EsDisponible = z.EsDisponible,
                FechaCreacion = z.FechaCreacion
            };
        }

        public async Task<List<ZonaEntregaDTO>> ObtenerTodasIncluyendoInactivasAsync()
        {
            return await _ctx.ZonasEntrega
                .OrderBy(z => z.Nombre)
                .Select(z => new ZonaEntregaDTO
                {
                    Id = z.Id,
                    Nombre = z.Nombre,
                    Descripcion = z.Descripcion,
                    CostoEnvio = z.CostoEnvio,
                    EsDisponible = z.EsDisponible,
                    FechaCreacion = z.FechaCreacion
                })
                .ToListAsync();
        }

        public async Task<ResultadoOperacionDTO> CrearAsync(ZonaEntregaDTO dto, string usuarioId, string nombreUsuario)
        {
            var zona = new ZonaEntrega
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                CostoEnvio = dto.CostoEnvio,
                EsDisponible = dto.EsDisponible,
                FechaCreacion = DateTime.UtcNow
            };

            _ctx.ZonasEntrega.Add(zona);
            await _ctx.SaveChangesAsync();

            await _bitacora.GuardarAsync(new BitacoraEntradaDTO
            {
                UsuarioId = usuarioId,
                NombreUsuario = nombreUsuario,
                Accion = TipoOperacion.Crear,
                TablaAfectada = "ZonasEntrega",
                RegistroId = zona.Id,
                Detalle = zona.Nombre
            });

            return ResultadoOperacionDTO.Ok("Zona de entrega creada correctamente.");
        }

        public async Task<ResultadoOperacionDTO> ActualizarAsync(ZonaEntregaDTO dto, string usuarioId, string nombreUsuario)
        {
            var zona = await _ctx.ZonasEntrega.FindAsync(dto.Id);
            if (zona == null)
            {
                return ResultadoOperacionDTO.Fallo("La zona de entrega no existe.");
            }

            zona.Nombre = dto.Nombre;
            zona.Descripcion = dto.Descripcion;
            zona.CostoEnvio = dto.CostoEnvio;
            zona.EsDisponible = dto.EsDisponible;

            await _ctx.SaveChangesAsync();

            await _bitacora.GuardarAsync(new BitacoraEntradaDTO
            {
                UsuarioId = usuarioId,
                NombreUsuario = nombreUsuario,
                Accion = TipoOperacion.Actualizar,
                TablaAfectada = "ZonasEntrega",
                RegistroId = zona.Id,
                Detalle = zona.Nombre
            });

            return ResultadoOperacionDTO.Ok("Zona de entrega actualizada correctamente.");
        }

        public async Task<ResultadoOperacionDTO> CambiarDisponibilidadAsync(int id, bool esDisponible, string usuarioId, string nombreUsuario)
        {
            var zona = await _ctx.ZonasEntrega.FindAsync(id);
            if (zona == null)
            {
                return ResultadoOperacionDTO.Fallo("La zona de entrega no existe.");
            }

            zona.EsDisponible = esDisponible;
            await _ctx.SaveChangesAsync();

            var accionDetalle = esDisponible ? "Activada" : "Desactivada";

            await _bitacora.GuardarAsync(new BitacoraEntradaDTO
            {
                UsuarioId = usuarioId,
                NombreUsuario = nombreUsuario,
                Accion = TipoOperacion.Actualizar,
                TablaAfectada = "ZonasEntrega",
                RegistroId = zona.Id,
                Detalle = accionDetalle + ": " + zona.Nombre
            });

            return ResultadoOperacionDTO.Ok("Disponibilidad de la zona actualizada correctamente.");
        }

        public async Task<bool> ExisteNombreAsync(string nombre, int? excluirId = null)
        {
            var nombreNormalizado = nombre.Trim().ToLower();

            return await _ctx.ZonasEntrega
                .Where(z => z.Nombre.ToLower() == nombreNormalizado
                    && (excluirId == null || z.Id != excluirId.Value))
                .AnyAsync();
        }
    }
}
