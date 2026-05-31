using HappyTimesBalloons.Abstraccion.DTOs;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class InventarioItemViewModel
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; }
        public string CategoriaNombre { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public DateTime FechaUltimaActualizacion { get; set; }

        public string EstadoStock
        {
            get
            {
                if (StockActual == 0) return "sin_stock";
                if (StockActual <= StockMinimo) return "bajo";
                return "normal";
            }
        }
    }

    public class InventarioKpisViewModel
    {
        public int TotalProductos { get; set; }
        public int ProductosStockBajo { get; set; }
        public int ProductosSinStock { get; set; }
        public decimal ValorTotalInventario { get; set; }
        public List<InventarioItemViewModel> AlertasReabastecimiento { get; set; }
            = new List<InventarioItemViewModel>();
    }

    public class InventarioIndexViewModel
    {
        public string Busqueda { get; set; }
        public int? CategoriaId { get; set; }
        public string EstadoStock { get; set; }
        public SelectList Categorias { get; set; }
        public InventarioKpisViewModel Kpis { get; set; }
        public List<InventarioItemViewModel> Items { get; set; }
            = new List<InventarioItemViewModel>();
    }
}
