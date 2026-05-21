using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Enums;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using System.Threading.Tasks;

namespace HappyTimesBalloons.LogicaNegocio.Servicios
{
    public class AuditoriaServicio : IAuditoriaServicio
    {
        private readonly IBitacoraRepositorio _repo;

        public AuditoriaServicio(IBitacoraRepositorio repo)
        {
            _repo = repo;
        }

        public async Task RegistrarAsync(
            string usuarioId,
            string nombreUsuario,
            TipoOperacion accion,
            string tablaAfectada,
            int? registroId = null,
            string detalle = null,
            string ip = null)
        {
            var entrada = new BitacoraEntradaDTO
            {
                UsuarioId = usuarioId,
                NombreUsuario = nombreUsuario,
                Accion = accion,
                TablaAfectada = tablaAfectada,
                RegistroId = registroId,
                Detalle = detalle,
                DireccionIp = ip
            };
            await _repo.GuardarAsync(entrada);
        }
    }
}
