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
            {
                query = query.Where(c =>
                    c.Nombre.Contains(busqueda) ||
                    c.Descripcion.Contains(busqueda));
            }

            return await query
                .OrderByDescending(c => c.EsActiva)
                .ThenBy(c => c.Nombre)
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

        public async Task<CategoriaEstadisticasDTO> ObtenerEstadisticasAsync()
        {
            var categorias = await _ctx.Categorias
                .Select(c => new { c.EsActiva })
                .ToListAsync();

            return new CategoriaEstadisticasDTO
            {
                Total   = categorias.Count,
                Activas = categorias.Count(c => c.EsActiva)
            };
        }

        public async Task<CategoriaDTO> ObtenerPorIdAsync(int id)
        {
            var categoria = await _ctx.Categorias.FindAsync(id);

            if (categoria == null)
                return null;

            return new CategoriaDTO
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion,
                EsActiva = categoria.EsActiva,
                FechaCreacion = categoria.FechaCreacion
            };
        }

        public async Task<CategoriaDTO> CrearAsync(CategoriaDTO dto)
        {
            var categoria = new Categoria
            {
                Nombre = dto.Nombre.Trim(),
                Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim(),
                EsActiva = true,
                FechaCreacion = DateTime.Now
            };

            _ctx.Categorias.Add(categoria);
            await _ctx.SaveChangesAsync();

            dto.Id = categoria.Id;
            dto.EsActiva = categoria.EsActiva;
            dto.FechaCreacion = categoria.FechaCreacion;

            return dto;
        }

        public async Task<CategoriaDTO> ActualizarAsync(CategoriaDTO dto)
        {
            var categoria = await _ctx.Categorias.FindAsync(dto.Id);

            if (categoria == null)
                return null;

            categoria.Nombre = dto.Nombre.Trim();
            categoria.Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim();

            await _ctx.SaveChangesAsync();

            dto.EsActiva = categoria.EsActiva;
            dto.FechaCreacion = categoria.FechaCreacion;

            return dto;
        }

        public async Task<bool> ToggleEstadoAsync(int id)
        {
            var categoria = await _ctx.Categorias.FindAsync(id);

            if (categoria == null)
                return false;

            categoria.EsActiva = !categoria.EsActiva;

            await _ctx.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ExisteNombreAsync(string nombre, int? excluirId = null)
        {
            var nombreNormalizado = nombre.Trim().ToLower();

            var query = _ctx.Categorias
                .Where(c => c.Nombre.ToLower() == nombreNormalizado);

            if (excluirId.HasValue)
            {
                query = query.Where(c => c.Id != excluirId.Value);
            }

            return await query.AnyAsync();
        }
    }
}