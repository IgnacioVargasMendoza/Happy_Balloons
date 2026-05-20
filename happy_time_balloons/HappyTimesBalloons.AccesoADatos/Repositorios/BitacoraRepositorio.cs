using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.AccesoADatos.Contexto;
using HappyTimesBalloons.AccesoADatos.Modelos;
using System;
using System.Threading.Tasks;

namespace HappyTimesBalloons.AccesoADatos.Repositorios
{
    public class BitacoraRepositorio : IBitacoraRepositorio
    {
        private readonly ApplicationDbContext _ctx;

        public BitacoraRepositorio(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task GuardarAsync(BitacoraEntradaDTO entrada)
        {
            var registro = new BitacoraAuditoria
            {
                UsuarioId = entrada.UsuarioId,
                NombreUsuario = entrada.NombreUsuario,
                Accion = entrada.Accion,
                TablaAfectada = entrada.TablaAfectada,
                RegistroId = entrada.RegistroId,
                FechaHoraUtc = DateTime.UtcNow,
                DireccionIp = entrada.DireccionIp,
                Detalle = entrada.Detalle
            };
            _ctx.BitacoraAuditoria.Add(registro);
            await _ctx.SaveChangesAsync();
        }
    }
}
