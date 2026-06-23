using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Enums;
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
    public class SinpeRepositorio : ISinpeRepositorio
    {
        private readonly ApplicationDbContext _ctx;

        public SinpeRepositorio(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<PagoSinpeDTO> RegistrarAsync(PagoSinpeDTO dto)
        {
            var entidad = new PagoSinpe
            {
                PedidoId = dto.PedidoId,
                NumeroComprobante = dto.NumeroComprobante,
                Monto = dto.Monto,
                NombreTitular = dto.NombreTitular,
                TelefonoDestino = dto.TelefonoDestino,
                EstadoPago = dto.EstadoPago,
                MotivoRechazo = dto.MotivoRechazo,
                FechaRecepcion = dto.FechaRecepcion,
                FechaProcesamiento = dto.FechaProcesamiento
            };

            _ctx.PagosSinpe.Add(entidad);
            await _ctx.SaveChangesAsync();

            dto.Id = entidad.Id;
            return dto;
        }

        public async Task<bool> ExisteComprobanteAsync(string numeroComprobante)
        {
            return await _ctx.PagosSinpe
                .AnyAsync(p => p.NumeroComprobante == numeroComprobante
                            && p.EstadoPago == EstadoPagoSinpe.Aprobado);
        }

        public async Task<IList<PagoSinpeDTO>> ObtenerTodosAsync()
        {
            return await _ctx.PagosSinpe
                .Include(p => p.Pedido)
                .OrderByDescending(p => p.FechaRecepcion)
                .Select(p => new PagoSinpeDTO
                {
                    Id = p.Id,
                    PedidoId = p.PedidoId,
                    NumeroPedido = p.Pedido.Numero,
                    NumeroComprobante = p.NumeroComprobante,
                    Monto = p.Monto,
                    NombreTitular = p.NombreTitular,
                    TelefonoDestino = p.TelefonoDestino,
                    EstadoPago = p.EstadoPago,
                    MotivoRechazo = p.MotivoRechazo,
                    FechaRecepcion = p.FechaRecepcion,
                    FechaProcesamiento = p.FechaProcesamiento
                })
                .ToListAsync();
        }

        public async Task<PagoSinpeDTO> ObtenerPorIdAsync(int id)
        {
            return await _ctx.PagosSinpe
                .Include(p => p.Pedido)
                .Where(p => p.Id == id)
                .Select(p => new PagoSinpeDTO
                {
                    Id = p.Id,
                    PedidoId = p.PedidoId,
                    NumeroPedido = p.Pedido.Numero,
                    NumeroComprobante = p.NumeroComprobante,
                    Monto = p.Monto,
                    NombreTitular = p.NombreTitular,
                    TelefonoDestino = p.TelefonoDestino,
                    EstadoPago = p.EstadoPago,
                    MotivoRechazo = p.MotivoRechazo,
                    FechaRecepcion = p.FechaRecepcion,
                    FechaProcesamiento = p.FechaProcesamiento
                })
                .FirstOrDefaultAsync();
        }
    }
}
