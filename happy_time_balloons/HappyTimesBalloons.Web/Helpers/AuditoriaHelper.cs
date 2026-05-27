using HappyTimesBalloons.Abstraccion.Enums;
using HappyTimesBalloons.AccesoADatos.Contexto;
using HappyTimesBalloons.AccesoADatos.Repositorios;
using HappyTimesBalloons.LogicaNegocio.Servicios;
using System.Threading.Tasks;
using System.Web;

namespace HappyTimesBalloons.Web.Helpers
{
    public static class AuditoriaHelper
    {
        public static async Task RegistrarAsync(
            HttpContextBase httpContext,
            TipoOperacion accion,
            string tabla,
            int? registroId = null,
            string detalle = null)
        {
            var usuarioId = httpContext.User?.Identity?.Name ?? "Anónimo";
            var nombreUsuario = usuarioId;
            var ip = httpContext.Request?.UserHostAddress;

            using (var ctx = new ApplicationDbContext())
            {
                var repo = new BitacoraRepositorio(ctx);
                var servicio = new AuditoriaServicio(repo);

                await servicio.RegistrarAsync(
                    usuarioId,
                    nombreUsuario,
                    accion,
                    tabla,
                    registroId,
                    detalle,
                    ip);
            }
        }
    }
}