using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.AccesoADatos.Contexto;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace HappyTimesBalloons.AccesoADatos.Repositorios
{
    public class InventarioRepositorio : IInventarioRepositorio
    {
        private readonly ApplicationDbContext _ctx;

        public InventarioRepositorio(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<List<InventarioDTO>> ObtenerTodosAsync(
            string busqueda = null,
            int? categoriaId = null,
            string estadoStock = "todos")
        {
            var query = _ctx.Inventario
                .Include(i => i.Producto)
                .Include(i => i.Producto.Categoria)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
                query = query.Where(i => i.Producto.Nombre.Contains(busqueda));

            if (categoriaId.HasValue)
                query = query.Where(i => i.Producto.CategoriaId == categoriaId.Value);

            if (estadoStock == "sin_stock")
                query = query.Where(i => i.StockActual == 0);
            else if (estadoStock == "bajo")
                query = query.Where(i => i.StockActual > 0 && i.StockActual <= i.StockMinimo);

            return await query
                .OrderBy(i => i.Producto.Nombre)
                .Select(i => new InventarioDTO
                {
                    Id = i.Id,
                    ProductoId = i.ProductoId,
                    ProductoNombre = i.Producto.Nombre,
                    CategoriaNombre = i.Producto.Categoria.Nombre,
                    StockActual = i.StockActual,
                    StockMinimo = i.StockMinimo,
                    FechaUltimaActualizacion = i.FechaUltimaActualizacion,
                    UsuarioUltimaActualizacionId = i.UsuarioUltimaActualizacionId
                })
                .ToListAsync();
        }

        public async Task<InventarioKpisDTO> ObtenerKpisAsync()
        {
            var totalProductos = await _ctx.Inventario.CountAsync();
            var productosStockBajo = await _ctx.Inventario
                .CountAsync(i => i.StockActual > 0 && i.StockActual <= i.StockMinimo);
            var productosSinStock = await _ctx.Inventario
                .CountAsync(i => i.StockActual == 0);
            var valorTotal = (decimal?)await _ctx.Inventario
                .SumAsync(i => (decimal?)((decimal)i.StockActual * i.Producto.Precio)) ?? 0m;

            var alertas = await _ctx.Inventario
                .Include(i => i.Producto)
                .Include(i => i.Producto.Categoria)
                .Where(i => i.StockActual <= i.StockMinimo)
                .OrderBy(i => i.StockActual)
                .Select(i => new InventarioDTO
                {
                    Id = i.Id,
                    ProductoId = i.ProductoId,
                    ProductoNombre = i.Producto.Nombre,
                    CategoriaNombre = i.Producto.Categoria.Nombre,
                    StockActual = i.StockActual,
                    StockMinimo = i.StockMinimo,
                    FechaUltimaActualizacion = i.FechaUltimaActualizacion,
                    UsuarioUltimaActualizacionId = i.UsuarioUltimaActualizacionId
                })
                .ToListAsync();

            return new InventarioKpisDTO
            {
                TotalProductos = totalProductos,
                ProductosStockBajo = productosStockBajo,
                ProductosSinStock = productosSinStock,
                ValorTotalInventario = valorTotal,
                AlertasReabastecimiento = alertas
            };
        }
    }
}
