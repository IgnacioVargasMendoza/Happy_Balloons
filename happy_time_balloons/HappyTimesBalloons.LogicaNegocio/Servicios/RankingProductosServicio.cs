using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HappyTimesBalloons.LogicaNegocio.Servicios
{
    public class RankingProductosServicio : IRankingProductosServicio
    {
        private readonly IRankingProductosRepositorio _repo;
        private readonly IZonaEntregaServicio _zonaServicio;

        public RankingProductosServicio(
            IRankingProductosRepositorio repo,
            IZonaEntregaServicio zonaServicio)
        {
            _repo = repo;
            _zonaServicio = zonaServicio;
        }

        public async Task<RankingProductosDTO> ObtenerRankingAsync(
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int? zonaId)
        {
            var inicio = fechaInicio ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var fin = fechaFin ?? DateTime.Today;

            if (inicio > fin)
            {
                inicio = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                fin = DateTime.Today;
            }

            var items = await _repo.ObtenerItemsAsync(inicio, fin, zonaId);

            int total = items.Sum(i => i.UnidadesVendidas);
            for (int i = 0; i < items.Count; i++)
            {
                items[i].Posicion = i + 1;
                items[i].PorcentajeUnidades = total > 0
                    ? Math.Round((decimal)items[i].UnidadesVendidas / total * 100, 1)
                    : 0m;
            }

            string nombreZona = null;
            if (zonaId.HasValue)
            {
                var zona = await _zonaServicio.ObtenerPorIdAsync(zonaId.Value);
                nombreZona = zona?.Nombre;
            }

            var top10 = items.Take(10).ToList();

            var porCategoria = items
                .GroupBy(i => i.Categoria)
                .Select(g => new { Categoria = g.Key, Unidades = g.Sum(i => i.UnidadesVendidas) })
                .OrderByDescending(g => g.Unidades)
                .ToList();

            return new RankingProductosDTO
            {
                FechaInicio = inicio.Date,
                FechaFin = fin.Date,
                ZonaId = zonaId,
                NombreZona = nombreZona,
                TotalUnidadesVendidas = items.Sum(i => i.UnidadesVendidas),
                IngresosTotales = items.Sum(i => i.Ingresos),
                TotalProductosActivos = items.Count,
                Items = items,
                GraficoEtiquetas = top10.Select(i => i.NombreProducto).ToList(),
                GraficoUnidades = top10.Select(i => i.UnidadesVendidas).ToList(),
                GraficoIngresos = top10.Select(i => i.Ingresos).ToList(),
                DonaEtiquetas = porCategoria.Select(g => g.Categoria).ToList(),
                DonaValores = porCategoria.Select(g => g.Unidades).ToList()
            };
        }
    }
}
