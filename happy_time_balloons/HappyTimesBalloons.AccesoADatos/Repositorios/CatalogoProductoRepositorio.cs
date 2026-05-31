using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.AccesoADatos.Contexto;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace HappyTimesBalloons.AccesoADatos.Repositorios
{
    public class CatalogoProductoRepositorio : ICatalogoProductoRepositorio
    {
        private readonly ApplicationDbContext _context;

        public CatalogoProductoRepositorio(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<CatalogoProductoDTO> ObtenerCatalogo(string busqueda = "", int? categoriaId = null)
        {
            var query = _context.Productos
                .Include("Categoria")
                .Include("Imagenes")
                .Where(p => p.EsActivo);

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                query = query.Where(p =>
                    p.Nombre.Contains(busqueda) ||
                    p.Descripcion.Contains(busqueda));
            }

            if (categoriaId.HasValue)
            {
                query = query.Where(p => p.CategoriaId == categoriaId.Value);
            }

            return query
                .Select(p => new CatalogoProductoDTO
                {
                    ProductoId = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    PrecioDescuento = p.PrecioDescuento,
                    CategoriaNombre = p.Categoria.Nombre,
                    ImagenPrincipal = p.Imagenes
                        .Where(i => i.EsPrincipal)
                        .Select(i => i.RutaImagen)
                        .FirstOrDefault(),
                    EsActivo = p.EsActivo,
                    Disponible = p.Stock > 0
                })
                .ToList();
        }

        public CatalogoProductoDTO ObtenerProductoCatalogoPorId(int productoId)
        {
            return _context.Productos
                .Include("Categoria")
                .Include("Imagenes")
                .Where(p => p.Id == productoId && p.EsActivo)
                .Select(p => new CatalogoProductoDTO
                {
                    ProductoId = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    PrecioDescuento = p.PrecioDescuento,
                    CategoriaNombre = p.Categoria.Nombre,
                    ImagenPrincipal = p.Imagenes
                        .Where(i => i.EsPrincipal)
                        .Select(i => i.RutaImagen)
                        .FirstOrDefault(),
                    EsActivo = p.EsActivo,
                    Disponible = p.Stock > 0
                })
                .FirstOrDefault();
        }
    }
}