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

        public async Task<List<ProductoDTO>> ObtenerTodosAsync(string busqueda = null, int? categoriaId = null, bool? soloActivos = null)
        {
            var query = _ctx.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Imagenes)
                .Include(p => p.Inventarios)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
                query = query.Where(p => p.Nombre.Contains(busqueda) ||
                                         p.Descripcion.Contains(busqueda));

            if (categoriaId.HasValue)
                query = query.Where(p => p.CategoriaId == categoriaId.Value);

            if (soloActivos.HasValue)
                query = query.Where(p => p.EsActivo == soloActivos.Value);

            var productos = await query.OrderBy(p => p.Nombre).ToListAsync();
            var promoDict = await ObtenerPromosActivasAsync();

            return productos.Select(p =>
            {
                Promocion promo;
                promoDict.TryGetValue(p.Id, out promo);
                return new ProductoDTO
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    PrecioDescuento = promo != null
                        ? Math.Round(p.Precio * (1 - promo.DescuentoPorcentaje / 100m), 0)
                        : p.PrecioDescuento,
                    Stock = p.Inventarios.FirstOrDefault()?.StockActual ?? 0,
                    CategoriaId = p.CategoriaId,
                    CategoriaNombre = p.Categoria?.Nombre,
                    EsActivo = p.EsActivo,
                    FechaCreacion = p.FechaCreacion,
                    TienePromocion = promo != null,
                    PromocionFin = promo?.FechaFin,
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
            }).ToList();
        }

        public async Task<ProductoEstadisticasDTO> ObtenerEstadisticasAsync()
        {
            var total = await _ctx.Productos.CountAsync();
            var activos = await _ctx.Productos.CountAsync(p => p.EsActivo);
            var conBajoStock = await _ctx.Productos
                .CountAsync(p => p.EsActivo && p.Inventarios.Any(i => i.StockActual <= 5));

            return new ProductoEstadisticasDTO
            {
                Total = total,
                Activos = activos,
                ConBajoStock = conBajoStock
            };
        }

        public async Task<List<ProductoDTO>> ObtenerRelacionadosAsync(int excluirId, int categoriaId, int cantidad)
        {
            var productos = await _ctx.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Imagenes)
                .Include(p => p.Inventarios)
                .Where(p => p.CategoriaId == categoriaId && p.Id != excluirId && p.EsActivo)
                .OrderBy(p => p.Nombre)
                .Take(cantidad)
                .ToListAsync();

            var promoDict = await ObtenerPromosActivasAsync();

            return productos.Select(p =>
            {
                Promocion promo;
                promoDict.TryGetValue(p.Id, out promo);
                return new ProductoDTO
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    PrecioDescuento = promo != null
                        ? Math.Round(p.Precio * (1 - promo.DescuentoPorcentaje / 100m), 0)
                        : p.PrecioDescuento,
                    Stock = p.Inventarios.FirstOrDefault()?.StockActual ?? 0,
                    CategoriaId = p.CategoriaId,
                    CategoriaNombre = p.Categoria?.Nombre,
                    EsActivo = p.EsActivo,
                    FechaCreacion = p.FechaCreacion,
                    TienePromocion = promo != null,
                    PromocionFin = promo?.FechaFin,
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
            }).ToList();
        }

        public async Task<ImagenProductoDTO> ObtenerImagenPorIdAsync(int imagenId)
        {
            var imagen = await _ctx.ImagenesProducto.FindAsync(imagenId);
            if (imagen == null) return null;

            return new ImagenProductoDTO
            {
                Id = imagen.Id,
                ProductoId = imagen.ProductoId,
                RutaImagen = imagen.RutaImagen,
                EsPrincipal = imagen.EsPrincipal,
                Orden = imagen.Orden
            };
        }

        public async Task<ProductoDTO> ObtenerPorIdAsync(int id)
        {
            var p = await _ctx.Productos
                .Include(x => x.Categoria)
                .Include(x => x.Imagenes)
                .Include(x => x.Inventarios)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (p == null) return null;

            var now = DateTime.UtcNow;
            var promo = await _ctx.Promociones
                .Where(x => x.ProductoId == id && x.Activa && x.FechaInicio <= now && x.FechaFin >= now)
                .FirstOrDefaultAsync();

            return new ProductoDTO
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Precio = p.Precio,
                PrecioDescuento = promo != null
                    ? Math.Round(p.Precio * (1 - promo.DescuentoPorcentaje / 100m), 0)
                    : p.PrecioDescuento,
                Stock = p.Inventarios.FirstOrDefault()?.StockActual ?? 0,
                CategoriaId = p.CategoriaId,
                CategoriaNombre = p.Categoria?.Nombre,
                EsActivo = p.EsActivo,
                FechaCreacion = p.FechaCreacion,
                TienePromocion = promo != null,
                PromocionFin = promo?.FechaFin,
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
                CategoriaId = dto.CategoriaId,
                EsActivo = true,
                FechaCreacion = DateTime.UtcNow
            };

            _ctx.Productos.Add(entidad);
            await _ctx.SaveChangesAsync();

            var inventario = new Inventario
            {
                ProductoId = entidad.Id,
                StockActual = dto.Stock,
                StockMinimo = 5,
                FechaUltimaActualizacion = DateTime.UtcNow
            };

            _ctx.Inventario.Add(inventario);
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
            entidad.CategoriaId = dto.CategoriaId;

            var inventario = await _ctx.Inventario
                .FirstOrDefaultAsync(i => i.ProductoId == dto.Id);

            if (inventario != null)
            {
                inventario.StockActual = dto.Stock;
                inventario.FechaUltimaActualizacion = DateTime.UtcNow;
            }
            else
            {
                _ctx.Inventario.Add(new Inventario
                {
                    ProductoId = dto.Id,
                    StockActual = dto.Stock,
                    StockMinimo = 5,
                    FechaUltimaActualizacion = DateTime.UtcNow
                });
            }

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

        private async Task<Dictionary<int, Promocion>> ObtenerPromosActivasAsync()
        {
            var now = DateTime.UtcNow;
            var promos = await _ctx.Promociones
                .Where(p => p.Activa && p.FechaInicio <= now && p.FechaFin >= now)
                .ToListAsync();

            return promos
                .GroupBy(p => p.ProductoId)
                .ToDictionary(g => g.Key, g => g.First());
        }
    }
}
