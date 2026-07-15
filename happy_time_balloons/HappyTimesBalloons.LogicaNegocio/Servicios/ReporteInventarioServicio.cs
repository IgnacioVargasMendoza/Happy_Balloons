using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HappyTimesBalloons.LogicaNegocio.Servicios
{
    public class ReporteInventarioServicio : IReporteInventarioServicio
    {
        private readonly IInventarioRepositorio _inventarioRepo;
        private readonly ICategoriaRepositorio _categoriaRepo;

        public ReporteInventarioServicio(
            IInventarioRepositorio inventarioRepo,
            ICategoriaRepositorio categoriaRepo)
        {
            _inventarioRepo = inventarioRepo;
            _categoriaRepo = categoriaRepo;
        }

        public async Task<ReporteInventarioDTO> ObtenerReporteAsync(
            int? categoriaId = null,
            string estadoStock = "todos")
        {
            var items = await _inventarioRepo.ObtenerTodosAsync(
                busqueda: null,
                categoriaId: categoriaId,
                estadoStock: estadoStock);

            string nombreCategoria = null;
            if (categoriaId.HasValue)
            {
                var cat = await _categoriaRepo.ObtenerPorIdAsync(categoriaId.Value);
                nombreCategoria = cat?.Nombre;
            }

            var filas = items.Select(i => new FilaInventarioReporteDTO
            {
                ProductoId = i.ProductoId,
                NombreProducto = i.ProductoNombre,
                Categoria = i.CategoriaNombre,
                StockActual = i.StockActual,
                StockMinimo = i.StockMinimo,
                EstadoStock = ResolverEstadoStock(i.StockActual, i.StockMinimo)
            }).ToList();

            // KPIs calculados sobre el subconjunto filtrado
            int sinStock = filas.Count(f => f.StockActual == 0);
            int stockBajo = filas.Count(f => f.StockActual > 0 && f.StockActual <= f.StockMinimo);

            return new ReporteInventarioDTO
            {
                CategoriaId = categoriaId,
                NombreCategoria = nombreCategoria,
                EstadoStock = estadoStock,
                TotalProductos = filas.Count,
                ProductosStockBajo = stockBajo,
                ProductosSinStock = sinStock,
                ValorTotalInventario = 0m, // sin precio unitario en InventarioDTO
                Items = filas
            };
        }

        public byte[] GenerarCsvBytes(ReporteInventarioDTO reporte)
        {
            var sb = new StringBuilder();

            sb.AppendLine("REPORTE DE INVENTARIO — Happy Times Balloons");

            if (reporte.CategoriaId.HasValue)
                sb.AppendLine($"Categoría:,{Escapar(reporte.NombreCategoria)}");
            else
                sb.AppendLine("Categoría:,Todas");

            string etiquetaEstado = reporte.EstadoStock == "todos" ? "Todos" :
                                    reporte.EstadoStock == "bajo" ? "Stock bajo" :
                                    reporte.EstadoStock == "sinStock" ? "Sin stock" : reporte.EstadoStock;
            sb.AppendLine($"Estado de stock:,{etiquetaEstado}");
            sb.AppendLine($"Total productos:,{reporte.TotalProductos}");
            sb.AppendLine($"Productos stock bajo:,{reporte.ProductosStockBajo}");
            sb.AppendLine($"Productos sin stock:,{reporte.ProductosSinStock}");
            sb.AppendLine();

            sb.AppendLine("DETALLE DE INVENTARIO");
            sb.AppendLine("Producto,Categoría,Stock Actual,Stock Mínimo,Estado");
            foreach (var f in reporte.Items)
            {
                sb.AppendLine($"{Escapar(f.NombreProducto)},{Escapar(f.Categoria)},{f.StockActual},{f.StockMinimo},{Escapar(f.EstadoStock)}");
            }

            return CombinarBom(sb.ToString());
        }

        private static string ResolverEstadoStock(int actual, int minimo)
        {
            if (actual == 0) return "Sin stock";
            if (actual <= minimo) return "Stock bajo";
            return "Normal";
        }

        private static byte[] CombinarBom(string contenido)
        {
            var bom = Encoding.UTF8.GetPreamble();
            var cuerpo = Encoding.UTF8.GetBytes(contenido);
            var resultado = new byte[bom.Length + cuerpo.Length];
            Buffer.BlockCopy(bom, 0, resultado, 0, bom.Length);
            Buffer.BlockCopy(cuerpo, 0, resultado, bom.Length, cuerpo.Length);
            return resultado;
        }

        private static string Escapar(string valor)
        {
            if (string.IsNullOrEmpty(valor)) return string.Empty;
            if (valor.Contains(",") || valor.Contains("\"") || valor.Contains("\n"))
                return $"\"{valor.Replace("\"", "\"\"")}\"";
            return valor;
        }
    }
}
