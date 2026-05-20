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
    public class ProductoRepositorio : IProductoRepositorio
    {
        private readonly ApplicationDbContext _ctx;

        public ProductoRepositorio(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<List<ProductoDTO>> ObtenerTodosAsync(string busqueda = null, int? categoriaId = null)
        {
            var query = _ctx.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Imagenes)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
                query = query.Where(p => p.Nombre.Contains(busqueda) ||
                                         p.Descripcion.Contains(busqueda));

            if (categoriaId.HasValue)
                query = query.Where(p => p.CategoriaId == categoriaId.Value);

            return await query
                .OrderBy(p => p.Nombre)
                .Select(p => new ProductoDTO
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    PrecioDescuento = p.PrecioDescuento,
                    Stock = p.Stock,
                    CategoriaId = p.CategoriaId,
                    CategoriaNombre = p.Categoria.Nombre,
                    EsActivo = p.EsActivo,
                    FechaCreacion = p.FechaCreacion,
                    Imagenes = p.Imagenes.OrderBy(i => i.Orden)
                        .Select(i => new ImagenProductoDTO
                        {
                            Id = i.Id,
                            ProductoId = i.ProductoId,
                            RutaImagen = i.RutaImagen,
                            EsPrincipal = i.EsPrincipal,
                            Orden = i.Orden
                        }).ToList()
                })
                .ToListAsync();
        }

        public async Task<ProductoDTO> ObtenerPorIdAsync(int id)
        {
            var p = await _ctx.Productos
                .Include(x => x.Categoria)
                .Include(x => x.Imagenes)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (p == null) return null;

            return new ProductoDTO
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Precio = p.Precio,
                PrecioDescuento = p.PrecioDescuento,
                Stock = p.Stock,
                CategoriaId = p.CategoriaId,
                CategoriaNombre = p.Categoria?.Nombre,
                EsActivo = p.EsActivo,
                FechaCreacion = p.FechaCreacion,
                Imagenes = p.Imagenes.OrderBy(i => i.Orden)
                    .Select(i => new ImagenProductoDTO
                    {
                        Id = i.Id,
                        ProductoId = i.ProductoId,
                        RutaImagen = i.RutaImagen,
                        EsPrincipal = i.EsPrincipal,
                        Orden = i.Orden
                    }).ToList()
            };
        }

        public async Task<ProductoDTO> CrearAsync(ProductoDTO dto)
        {
            var entidad = new Producto
            {
                Nombre = dto.Nombre.Trim(),
                Descripcion = dto.Descripcion?.Trim(),
                Precio = dto.Precio,
                PrecioDescuento = dto.PrecioDescuento,
                Stock = dto.Stock,
                CategoriaId = dto.CategoriaId,
                EsActivo = true,
                FechaCreacion = DateTime.UtcNow
            };

            _ctx.Productos.Add(entidad);
            await _ctx.SaveChangesAsync();

            dto.Id = entidad.Id;
            dto.EsActivo = entidad.EsActivo;
            dto.FechaCreacion = entidad.FechaCreacion;
            return dto;
        }

        public async Task<ProductoDTO> ActualizarAsync(ProductoDTO dto)
        {
            var entidad = await _ctx.Productos.FindAsync(dto.Id);
            if (entidad == null) return null;

            entidad.Nombre = dto.Nombre.Trim();
            entidad.Descripcion = dto.Descripcion?.Trim();
            entidad.Precio = dto.Precio;
            entidad.PrecioDescuento = dto.PrecioDescuento;
            entidad.Stock = dto.Stock;
            entidad.CategoriaId = dto.CategoriaId;

            await _ctx.SaveChangesAsync();
            return dto;
        }

        public async Task<bool> ToggleEstadoAsync(int id)
        {
            var entidad = await _ctx.Productos.FindAsync(id);
            if (entidad == null) return false;

            entidad.EsActivo = !entidad.EsActivo;
            await _ctx.SaveChangesAsync();
            return true;
        }

        public async Task<ImagenProductoDTO> AgregarImagenAsync(ImagenProductoDTO dto)
        {
            // Si no hay imágenes aún, la primera es principal
            bool esPrimera = !await _ctx.ImagenesProducto
                .AnyAsync(i => i.ProductoId == dto.ProductoId);

            int orden = await _ctx.ImagenesProducto
                .Where(i => i.ProductoId == dto.ProductoId)
                .Select(i => (int?)i.Orden)
                .MaxAsync() ?? 0;

            var entidad = new ImagenProducto
            {
                ProductoId = dto.ProductoId,
                RutaImagen = dto.RutaImagen,
                EsPrincipal = esPrimera || dto.EsPrincipal,
                Orden = orden + 1
            };

            _ctx.ImagenesProducto.Add(entidad);
            await _ctx.SaveChangesAsync();

            dto.Id = entidad.Id;
            dto.EsPrincipal = entidad.EsPrincipal;
            dto.Orden = entidad.Orden;
            return dto;
        }

        public async Task<bool> EliminarImagenAsync(int imagenId)
        {
            var imagen = await _ctx.ImagenesProducto.FindAsync(imagenId);
            if (imagen == null) return false;

            bool eraPrincipal = imagen.EsPrincipal;
            int productoId = imagen.ProductoId;

            _ctx.ImagenesProducto.Remove(imagen);
            await _ctx.SaveChangesAsync();

            // Si era la principal, asignar la primera restante como principal
            if (eraPrincipal)
            {
                var siguiente = await _ctx.ImagenesProducto
                    .Where(i => i.ProductoId == productoId)
                    .OrderBy(i => i.Orden)
                    .FirstOrDefaultAsync();

                if (siguiente != null)
                {
                    siguiente.EsPrincipal = true;
                    await _ctx.SaveChangesAsync();
                }
            }

            return true;
        }

        public async Task<bool> EstablecerImagenPrincipalAsync(int imagenId, int productoId)
        {
            var imagenes = await _ctx.ImagenesProducto
                .Where(i => i.ProductoId == productoId)
                .ToListAsync();

            foreach (var img in imagenes)
                img.EsPrincipal = (img.Id == imagenId);

            await _ctx.SaveChangesAsync();
            return true;
        }
    }
}
