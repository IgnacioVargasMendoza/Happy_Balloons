using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using System;
using System.Text;
using System.Threading.Tasks;

namespace HappyTimesBalloons.LogicaNegocio.Servicios
{
    public class ReporteVentasServicio : IReporteVentasServicio
    {
        private readonly IReporteVentasRepositorio _repo;

        public ReporteVentasServicio(IReporteVentasRepositorio repo)
        {
            _repo = repo;
        }

        public async Task<ReporteVentasDTO> ObtenerReporteAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            if (fechaInicio > fechaFin)
                throw new ArgumentException("La fecha de inicio no puede ser posterior a la fecha fin.");

            return await _repo.ObtenerReporteAsync(fechaInicio, fechaFin);
        }

        public byte[] GenerarCsvBytes(ReporteVentasDTO reporte)
        {
            var sb = new StringBuilder();

            // BOM UTF-8 para compatibilidad con Excel
            sb.AppendLine("REPORTE DE VENTAS — Happy Times Balloons");
            sb.AppendLine($"Período:,{reporte.FechaInicio:dd/MM/yyyy},{reporte.FechaFin:dd/MM/yyyy}");
            sb.AppendLine($"Total pedidos:,{reporte.TotalPedidos}");
            sb.AppendLine($"Ingresos totales:,{reporte.IngresosTotales:F2}");
            sb.AppendLine($"Ticket promedio:,{reporte.TicketPromedio:F2}");
            sb.AppendLine($"Unidades vendidas:,{reporte.UnidadesVendidas}");
            sb.AppendLine();

            sb.AppendLine("PEDIDOS ENTREGADOS");
            sb.AppendLine("Número,Fecha,Cliente,Método de Pago,Total");
            foreach (var p in reporte.Pedidos)
            {
                sb.AppendLine($"{Escapar(p.Numero)},{p.FechaPedido:dd/MM/yyyy},{Escapar(p.NombreCliente)},{Escapar(p.MetodoPago)},{p.Total:F2}");
            }

            sb.AppendLine();
            sb.AppendLine("PRODUCTOS MÁS VENDIDOS");
            sb.AppendLine("Producto,Categoría,Unidades Vendidas,Ingresos");
            foreach (var pr in reporte.ProductosMasVendidos)
            {
                sb.AppendLine($"{Escapar(pr.NombreProducto)},{Escapar(pr.Categoria)},{pr.UnidadesVendidas},{pr.Ingresos:F2}");
            }

            // UTF-8 con BOM para que Excel lo abra correctamente
            return Encoding.UTF8.GetPreamble().Length > 0
                ? CombinarBom(sb.ToString())
                : Encoding.UTF8.GetBytes(sb.ToString());
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
