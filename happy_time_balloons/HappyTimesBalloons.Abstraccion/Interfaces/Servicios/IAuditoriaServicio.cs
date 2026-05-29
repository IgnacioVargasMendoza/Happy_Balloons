using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Servicios
{
    public interface IAuditoriaServicio
    {
        Task RegistrarAsync(
            string usuarioId,
            string nombreUsuario,
            TipoOperacion accion,
            string tablaAfectada,
            int? registroId = null,
            string detalle = null,
            string ip = null);

        Task<List<BitacoraResumenDTO>> ObtenerActividadRecienteAsync(int cantidad);
    }
}
