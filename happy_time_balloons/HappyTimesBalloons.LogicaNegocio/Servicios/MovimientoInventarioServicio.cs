using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Enums;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;

namespace HappyTimesBalloons.LogicaNegocio.Servicios
{
    public class MovimientoInventarioServicio : IMovimientoInventarioServicio
    {
        private readonly IMovimientoInventarioRepositorio _repo;
        private readonly IInventarioRepositorio _inventarioRepo;
        private readonly IBitacoraRepositorio _bitacora;

        public MovimientoInventarioServicio(
            IMovimientoInventarioRepositorio repo,
            IInventarioRepositorio inventarioRepo,
            IBitacoraRepositorio bitacora)
        {
            _repo = repo;
            _inventarioRepo = inventarioRepo;
            _bitacora = bitacora;
        }

        public async Task<ResultadoOperacionDTO> RegistrarMovimientoAsync(
            MovimientoInventarioDTO dto,
            string usuarioId,
            string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(dto.Motivo))
            {
                return ResultadoOperacionDTO.Fallo(
                    "El motivo del movimiento es obligatorio.",
                    CodigoResultado.DatosInvalidos);
            }

            var stockActual = await _inventarioRepo.ObtenerStockActualAsync(dto.ProductoId);

            if (stockActual == null)
            {
                return ResultadoOperacionDTO.Fallo(
                    "No se encontró el registro de inventario para el producto indicado.",
                    CodigoResultado.DatosInvalidos);
            }

            int stockAnterior = stockActual.Value;
            int stockNuevo;

            if (dto.TipoMovimiento == TipoMovimiento.Entrada)
            {
                if (dto.Cantidad <= 0)
                {
                    return ResultadoOperacionDTO.Fallo(
                        "La cantidad de entrada debe ser mayor que cero.",
                        CodigoResultado.DatosInvalidos);
                }

                stockNuevo = stockAnterior + dto.Cantidad;
            }
            else if (dto.TipoMovimiento == TipoMovimiento.Salida)
            {
                if (dto.Cantidad <= 0)
                {
                    return ResultadoOperacionDTO.Fallo(
                        "La cantidad de salida debe ser mayor que cero.",
                        CodigoResultado.DatosInvalidos);
                }

                stockNuevo = stockAnterior - dto.Cantidad;

                if (stockNuevo < 0)
                {
                    return ResultadoOperacionDTO.Fallo(
                        $"Stock insuficiente. Stock actual: {stockAnterior}. Cantidad solicitada: {dto.Cantidad}.",
                        CodigoResultado.DatosInvalidos);
                }
            }
            else
            {
                // Ajuste: cantidad puede ser positiva o negativa
                stockNuevo = stockAnterior + dto.Cantidad;

                if (stockNuevo < 0)
                {
                    return ResultadoOperacionDTO.Fallo(
                        $"El ajuste resultaría en stock negativo. Stock actual: {stockAnterior}. Ajuste: {dto.Cantidad}.",
                        CodigoResultado.DatosInvalidos);
                }
            }

            dto.StockAnterior = stockAnterior;
            dto.StockNuevo = stockNuevo;
            dto.UsuarioId = usuarioId;
            dto.FechaMovimiento = DateTime.UtcNow;

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var movimientoGuardado = await _repo.RegistrarAsync(dto);

                await _inventarioRepo.ActualizarStockAsync(dto.ProductoId, stockNuevo, usuarioId);

                await _bitacora.GuardarAsync(new BitacoraEntradaDTO
                {
                    UsuarioId = usuarioId,
                    NombreUsuario = nombreUsuario,
                    Accion = TipoOperacion.Crear,
                    TablaAfectada = "MovimientosInventario",
                    RegistroId = movimientoGuardado.Id,
                    Detalle = $"{dto.TipoMovimiento} de {dto.Cantidad} unidades. Motivo: {dto.Motivo}"
                });

                scope.Complete();
            }

            return ResultadoOperacionDTO.Ok("Movimiento registrado correctamente.");
        }

        public Task<List<MovimientoInventarioDTO>> ObtenerHistorialPorProductoAsync(int productoId)
            => _repo.ObtenerPorProductoAsync(productoId);

        public Task<List<MovimientoInventarioDTO>> ObtenerUltimosMovimientosAsync(int cantidad = 20)
            => _repo.ObtenerUltimosAsync(cantidad);
    }
}
