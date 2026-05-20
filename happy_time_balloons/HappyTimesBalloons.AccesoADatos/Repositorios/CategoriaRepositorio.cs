using HappyTimesBalloons.Abstraccion.DTOs;
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
    public class CategoriaRepositorio : ICategoriaRepositorio
    {
        private readonly ApplicationDbContext _ctx;

        public CategoriaRepositorio(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<List<CategoriaDTO>> ObtenerTodasAsync(string busqueda = null)
        {
            var query = _ctx.Categorias.AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
                query = query.Where(c => c.Nombre.Contains(busqueda) ||
                                         c.Descripcion.Contains(busqueda));

            return await query
                .OrderBy(c => c.Nombre)
                .Select(c => new CategoriaDTO
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion,
                    EsActiva = c.EsActiva,
                    FechaCreacion = c.FechaCreacion
                })
                .ToListAsync();
        }

        public async Task<CategoriaDTO> ObtenerPorIdAsync(int id)
        {
            var c = await _ctx.Categorias.FindAsync(id);
            if (c == null) return null;

            return new CategoriaDTO
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion,
                EsActiva = c.EsActiva,
                FechaCreacion = c.FechaCreacion
            };
        }

        public async Task<CategoriaDTO> CrearAsync(CategoriaDTO dto)
        {
            var entidad = new Categoria
            {
                Nombre = dto.Nombre.Trim(),
                Descripcion = dto.Descripcion?.Trim(),
                EsActiva = true,
                FechaCreacion = DateTime.UtcNow
            };

            _ctx.Categorias.Add(entidad);
            await _ctx.SaveChangesAsync();

            dto.Id = entidad.Id;
            dto.EsActiva = entidad.EsActiva;
            dto.FechaCreacion = entidad.FechaCreacion;
            return dto;
        }

        public async Task<CategoriaDTO> ActualizarAsync(CategoriaDTO dto)
        {
            var entidad = await _ctx.Categorias.FindAsync(dto.Id);
            if (entidad == null) return null;

            entidad.Nombre = dto.Nombre.Trim();
            entidad.Descripcion = dto.Descripcion?.Trim();
            await _ctx.SaveChangesAsync();
            return dto;
        }

        public async Task<bool> ToggleEstadoAsync(int id)
        {
            var entidad = await _ctx.Categorias.FindAsync(id);
            if (entidad == null) return false;

            entidad.EsActiva = !entidad.EsActiva;
            await _ctx.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExisteNombreAsync(string nombre, int? excluirId = null)
        {
            var normalizado = nombre.Trim().ToLower();
            var query = _ctx.Categorias
                .Where(c => c.Nombre.ToLower() == normalizado);

            if (excluirId.HasValue)
                query = query.Where(c => c.Id != excluirId.Value);

            return await query.AnyAsync();
        }
    }
}
