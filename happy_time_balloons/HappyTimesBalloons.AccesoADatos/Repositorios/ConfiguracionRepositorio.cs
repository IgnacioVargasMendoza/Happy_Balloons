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
    public class ConfiguracionRepositorio : IConfiguracionRepositorio
    {
        private readonly ApplicationDbContext _ctx;
        private readonly IBitacoraRepositorio _bitacora;

        public ConfiguracionRepositorio(ApplicationDbContext ctx, IBitacoraRepositorio bitacora)
        {
            _ctx = ctx;
            _bitacora = bitacora;
        }

        public async Task<List<ConfiguracionSistemaDTO>> ObtenerTodasAsync()
        {
            return await _ctx.ConfiguracionesSistema
                .OrderBy(c => c.Clave)
                .Select(c => new ConfiguracionSistemaDTO
                {
                    Id = c.Id,
                    Clave = c.Clave,
                    Valor = c.Valor,
                    Descripcion = c.Descripcion,
                    FechaUltimaModificacion = c.FechaUltimaModificacion,
                    UsuarioUltimaModificacion = c.UsuarioUltimaModificacion
                })
                .ToListAsync();
        }

        public async Task<ConfiguracionSistemaDTO> ObtenerPorIdAsync(int id)
        {
            var c = await _ctx.ConfiguracionesSistema.FindAsync(id);
            if (c == null) return null;

            return new ConfiguracionSistemaDTO
            {
                Id = c.Id,
                Clave = c.Clave,
                Valor = c.Valor,
                Descripcion = c.Descripcion,
                FechaUltimaModificacion = c.FechaUltimaModificacion,
                UsuarioUltimaModificacion = c.UsuarioUltimaModificacion
            };
        }

        public async Task<ConfiguracionSistemaDTO> ObtenerPorClaveAsync(string clave)
        {
            return await _ctx.ConfiguracionesSistema
                .Where(c => c.Clave == clave)
                .Select(c => new ConfiguracionSistemaDTO
                {
                    Id = c.Id,
                    Clave = c.Clave,
                    Valor = c.Valor,
                    Descripcion = c.Descripcion,
                    FechaUltimaModificacion = c.FechaUltimaModificacion,
                    UsuarioUltimaModificacion = c.UsuarioUltimaModificacion
                })
                .FirstOrDefaultAsync();
        }

        public async Task<ResultadoOperacionDTO> ActualizarAsync(ConfiguracionSistemaDTO dto, string usuarioId, string nombreUsuario)
        {
            var entidad = await _ctx.ConfiguracionesSistema.FindAsync(dto.Id);
            if (entidad == null)
            {
                return ResultadoOperacionDTO.Fallo("La configuración no existe.");
            }

            entidad.Valor = dto.Valor;
            entidad.FechaUltimaModificacion = DateTime.UtcNow;
            entidad.UsuarioUltimaModificacion = nombreUsuario;

            await _ctx.SaveChangesAsync();

            await _bitacora.GuardarAsync(new BitacoraEntradaDTO
            {
                UsuarioId = usuarioId,
                NombreUsuario = nombreUsuario,
                Accion = TipoOperacion.Actualizar,
                TablaAfectada = "ConfiguracionesSistema",
                RegistroId = entidad.Id,
                Detalle = entidad.Clave + " = " + entidad.Valor
            });

            return ResultadoOperacionDTO.Ok("Configuración actualizada correctamente.");
        }
    }
}
