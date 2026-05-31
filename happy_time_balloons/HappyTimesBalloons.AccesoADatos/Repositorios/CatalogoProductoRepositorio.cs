using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.AccesoADatos.Contexto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HappyTimesBalloons.AccesoADatos.Repositorios
{
    public class CatalogoProductoRepositorio : ICatalogoProductoRepositorio
    {
        private readonly ApplicationDbContext _context;

        public CatalogoProductoRepositorio(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<CatalogoProductoDTO> ObtenerCatalogo()
        {
            return _context.Productos
                .Include("Categoria")
                .Include("Imagenes")
                .Where(p => p.EsActivo)
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
                    Disponible = p.Inventarios.Any(i => i.StockActual > 0)
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
                    Disponible = p.Inventarios.Any(i => i.StockActual > 0)
                })
                .FirstOrDefault();
        }
    }
}
