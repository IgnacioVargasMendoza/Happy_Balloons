using System;
using System.Collections.Generic;

namespace HappyTimesBalloons.Abstraccion.DTOs
{
    public class RankingProductosDTO
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int? ZonaId { get; set; }
        public string NombreZona { get; set; }

        public int TotalUnidadesVendidas { get; set; }
        public decimal IngresosTotales { get; set; }
        public int TotalProductosActivos { get; set; }

        public List<RankingProductoItemDTO> Items { get; set; }
            = new List<RankingProductoItemDTO>();

        public List<string> GraficoEtiquetas { get; set; } = new List<string>();
        public List<int> GraficoUnidades { get; set; } = new List<int>();
        public List<decimal> GraficoIngresos { get; set; } = new List<decimal>();

        public List<string> DonaEtiquetas { get; set; } = new List<string>();
        public List<int> DonaValores { get; set; } = new List<int>();
    }
}
