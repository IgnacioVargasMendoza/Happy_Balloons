using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Enums;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HappyTimesBalloons.LogicaNegocio.Servicios
{
    public class PrediccionDemandaServicio : IPrediccionDemandaServicio
    {
        private const int MINIMO_PERIODOS = 3;
        private const int PERIODOS_HISTORIAL = 6;

        private readonly IPrediccionDemandaRepositorio _repo;

        public PrediccionDemandaServicio(IPrediccionDemandaRepositorio repo)
        {
            _repo = repo;
        }

        public async Task<List<PrediccionDemandaItemDTO>> ObtenerPrediccionesAsync(TipoPeriodo tipoPeriodo)
        {
            var ids = await _repo.ObtenerIdsProductosConVentasAsync(tipoPeriodo);
            var resultado = new List<PrediccionDemandaItemDTO>();

            foreach (var id in ids)
            {
                var historialRaw = await _repo.ObtenerHistorialVentasPorProductoAsync(id, tipoPeriodo);
                var skipItems = Math.Max(0, historialRaw.Count - PERIODOS_HISTORIAL);
                var historial = historialRaw.Skip(skipItems).ToList();
                foreach (var periodo in historial)
                {
                    periodo.EtiquetaPeriodo = FormatearEtiqueta(periodo, tipoPeriodo);
                }
                var nombre = await _repo.ObtenerNombreProductoAsync(id);
                var categoria = await _repo.ObtenerCategoriaProductoAsync(id);

                var datosSuficientes = historial.Count >= MINIMO_PERIODOS;
                var promedio = datosSuficientes ? CalcularPromedioMovil(historial) : 0.0;
                var tendencia = datosSuficientes ? CalcularTendencia(historial) : 0.0;
                var prediccion = datosSuficientes ? GenerarPrediccion(historial) : 0.0;

                resultado.Add(new PrediccionDemandaItemDTO
                {
                    ProductoId = id,
                    NombreProducto = nombre,
                    Categoria = categoria,
                    CantidadPredichaProximoPeriodo = prediccion,
                    PromedioHistorico = promedio,
                    Tendencia = tendencia,
                    PeriodosAnalizados = historial.Count,
                    DatosSuficientes = datosSuficientes
                });
            }

            return resultado
                .OrderByDescending(p => p.CantidadPredichaProximoPeriodo)
                .ToList();
        }

        public async Task<PrediccionDemandaDetalleDTO> ObtenerDetallePrediccionAsync(int productoId, TipoPeriodo tipoPeriodo)
        {
            var nombre = await _repo.ObtenerNombreProductoAsync(productoId);
            if (nombre == null)
            {
                return null;
            }

            var categoria = await _repo.ObtenerCategoriaProductoAsync(productoId);
            var historialRaw = await _repo.ObtenerHistorialVentasPorProductoAsync(productoId, tipoPeriodo);
            var skipDetalle = Math.Max(0, historialRaw.Count - PERIODOS_HISTORIAL);
            var historial = historialRaw.Skip(skipDetalle).ToList();
            foreach (var periodo in historial)
            {
                periodo.EtiquetaPeriodo = FormatearEtiqueta(periodo, tipoPeriodo);
            }

            var datosSuficientes = historial.Count >= MINIMO_PERIODOS;
            var promedio = datosSuficientes ? CalcularPromedioMovil(historial) : 0.0;
            var tendencia = datosSuficientes ? CalcularTendencia(historial) : 0.0;
            var prediccion = datosSuficientes ? GenerarPrediccion(historial) : 0.0;

            string mensaje;
            if (datosSuficientes)
            {
                mensaje = $"Prediccion basada en {historial.Count} periodos historicos. " +
                          $"Tendencia {(tendencia > 0 ? "creciente" : tendencia < 0 ? "decreciente" : "estable")} " +
                          $"de {Math.Abs(tendencia):F1} unidades por periodo.";
            }
            else
            {
                mensaje = $"Datos insuficientes: se requieren al menos {MINIMO_PERIODOS} periodos con ventas " +
                          $"y solo se encontraron {historial.Count}. La prediccion no es confiable.";
            }

            return new PrediccionDemandaDetalleDTO
            {
                ProductoId = productoId,
                NombreProducto = nombre,
                Categoria = categoria,
                HistorialPeriodos = historial,
                CantidadPredichaProximoPeriodo = prediccion,
                PromedioHistorico = promedio,
                Tendencia = tendencia,
                DatosSuficientes = datosSuficientes,
                MensajeValidacion = mensaje
            };
        }

        private static double CalcularPromedioMovil(List<PeriodoVentaDTO> historial)
        {
            if (historial == null || historial.Count == 0)
            {
                return 0.0;
            }

            return historial.Average(p => p.CantidadVendida);
        }

        private static double CalcularTendencia(List<PeriodoVentaDTO> historial)
        {
            int n = historial.Count;
            if (n < 2)
            {
                return 0.0;
            }

            double sumaX = 0;
            double sumaY = 0;
            double sumaXY = 0;
            double sumaX2 = 0;

            for (int i = 0; i < n; i++)
            {
                double x = i + 1;
                double y = historial[i].CantidadVendida;
                sumaX += x;
                sumaY += y;
                sumaXY += x * y;
                sumaX2 += x * x;
            }

            double denominador = (n * sumaX2) - (sumaX * sumaX);
            if (Math.Abs(denominador) < 1e-10)
            {
                return 0.0;
            }

            return ((n * sumaXY) - (sumaX * sumaY)) / denominador;
        }

        private static double GenerarPrediccion(List<PeriodoVentaDTO> historial)
        {
            double promedio = CalcularPromedioMovil(historial);
            double tendencia = CalcularTendencia(historial);
            double prediccion = promedio + tendencia;
            return Math.Max(0.0, prediccion);
        }

        private static string FormatearEtiqueta(PeriodoVentaDTO periodo, TipoPeriodo tipoPeriodo)
        {
            switch (tipoPeriodo)
            {
                case TipoPeriodo.Semanal:
                    return $"Sem {periodo.NumeroPeriodo} / {periodo.Anio}";
                case TipoPeriodo.Trimestral:
                    return $"T{periodo.NumeroPeriodo} {periodo.Anio}";
                default:
                    return NombreMes(periodo.NumeroPeriodo) + $" {periodo.Anio}";
            }
        }

        private static string NombreMes(int mes)
        {
            switch (mes)
            {
                case 1: return "Ene";
                case 2: return "Feb";
                case 3: return "Mar";
                case 4: return "Abr";
                case 5: return "May";
                case 6: return "Jun";
                case 7: return "Jul";
                case 8: return "Ago";
                case 9: return "Sep";
                case 10: return "Oct";
                case 11: return "Nov";
                case 12: return "Dic";
                default: return mes.ToString();
            }
        }
    }
}
