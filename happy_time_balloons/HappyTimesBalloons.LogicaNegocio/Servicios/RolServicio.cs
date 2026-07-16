using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Enums;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.LogicaNegocio.Servicios
{
    public class RolServicio : IRolServicio
    {
        private readonly IRolRepositorio _rolRepo;
        private readonly IAuditoriaServicio _auditoria;

        public RolServicio(IRolRepositorio rolRepo, IAuditoriaServicio auditoria)
        {
            _rolRepo = rolRepo;
            _auditoria = auditoria;
        }

        public Task<List<RolDTO>> ObtenerTodosAsync()
            => _rolRepo.ObtenerTodosAsync();

        public Task<RolDTO> ObtenerPorIdAsync(string id)
            => _rolRepo.ObtenerPorIdAsync(id);

        public Task<List<UsuarioConRolDTO>> ObtenerTodosLosUsuariosAsync()
            => _rolRepo.ObtenerTodosLosUsuariosAsync();

        public async Task<ResultadoOperacionDTO<string>> CrearAsync(
            string nombre, string adminId, string adminNombre, string ip)
        {
            nombre = nombre?.Trim();
            if (string.IsNullOrEmpty(nombre))
                return ResultadoOperacionDTO<string>.Fallo("El nombre del rol es requerido.", CodigoResultado.DatosInvalidos);

            if (await _rolRepo.ExisteNombreAsync(nombre))
                return ResultadoOperacionDTO<string>.Fallo($"Ya existe un rol con el nombre '{nombre}'.", CodigoResultado.DatosInvalidos);

            var nuevoId = await _rolRepo.CrearAsync(nombre);
            if (nuevoId == null)
                return ResultadoOperacionDTO<string>.Fallo("No se pudo crear el rol.", CodigoResultado.Error);

            await _auditoria.RegistrarAsync(adminId, adminNombre, TipoOperacion.CrearRol,
                "AspNetRoles", detalle: $"Rol creado: {nombre}", ip: ip);

            return ResultadoOperacionDTO<string>.Ok($"Rol '{nombre}' creado exitosamente.", nuevoId);
        }

        public async Task<ResultadoOperacionDTO> ActualizarAsync(
            string id, string nuevoNombre, string adminId, string adminNombre, string ip)
        {
            nuevoNombre = nuevoNombre?.Trim();
            if (string.IsNullOrEmpty(nuevoNombre))
                return ResultadoOperacionDTO.Fallo("El nombre del rol es requerido.", CodigoResultado.DatosInvalidos);

            var existente = await _rolRepo.ObtenerPorIdAsync(id);
            if (existente == null)
                return ResultadoOperacionDTO.Fallo("El rol no existe.", CodigoResultado.DatosInvalidos);

            if (existente.Nombre != nuevoNombre && await _rolRepo.ExisteNombreAsync(nuevoNombre))
                return ResultadoOperacionDTO.Fallo($"Ya existe un rol con el nombre '{nuevoNombre}'.", CodigoResultado.DatosInvalidos);

            var ok = await _rolRepo.ActualizarAsync(id, nuevoNombre);
            if (!ok)
                return ResultadoOperacionDTO.Fallo("No se pudo actualizar el rol.", CodigoResultado.Error);

            await _auditoria.RegistrarAsync(adminId, adminNombre, TipoOperacion.EditarRol,
                "AspNetRoles", detalle: $"Rol '{existente.Nombre}' renombrado a '{nuevoNombre}'", ip: ip);

            return ResultadoOperacionDTO.Ok($"Rol actualizado a '{nuevoNombre}'.");
        }

        public async Task<ResultadoOperacionDTO> EliminarAsync(
            string id, string adminId, string adminNombre, string ip)
        {
            var rol = await _rolRepo.ObtenerPorIdAsync(id);
            if (rol == null)
                return ResultadoOperacionDTO.Fallo("El rol no existe.", CodigoResultado.DatosInvalidos);

            if (rol.CantidadUsuarios > 0)
                return ResultadoOperacionDTO.Fallo(
                    $"No se puede eliminar '{rol.Nombre}': tiene {rol.CantidadUsuarios} usuario(s) asignado(s).",
                    CodigoResultado.DatosInvalidos);

            var ok = await _rolRepo.EliminarAsync(id);
            if (!ok)
                return ResultadoOperacionDTO.Fallo("No se pudo eliminar el rol.", CodigoResultado.Error);

            await _auditoria.RegistrarAsync(adminId, adminNombre, TipoOperacion.EliminarRol,
                "AspNetRoles", detalle: $"Rol eliminado: {rol.Nombre}", ip: ip);

            return ResultadoOperacionDTO.Ok($"Rol '{rol.Nombre}' eliminado.");
        }

        public async Task<ResultadoOperacionDTO> AsignarRolAsync(
            string usuarioId, string rolNombre, string adminId, string adminNombre, string ip)
        {
            if (string.IsNullOrEmpty(usuarioId) || string.IsNullOrEmpty(rolNombre))
                return ResultadoOperacionDTO.Fallo("Usuario o rol no válido.", CodigoResultado.DatosInvalidos);

            if (await _rolRepo.TieneRolAsync(usuarioId, rolNombre))
                return ResultadoOperacionDTO.Fallo($"El usuario ya tiene el rol '{rolNombre}'.", CodigoResultado.DatosInvalidos);

            var ok = await _rolRepo.AsignarRolAsync(usuarioId, rolNombre);
            if (!ok)
                return ResultadoOperacionDTO.Fallo("No se pudo asignar el rol.", CodigoResultado.Error);

            await _auditoria.RegistrarAsync(adminId, adminNombre, TipoOperacion.AsignarRol,
                "AspNetUserRoles", detalle: $"Rol '{rolNombre}' asignado a usuario {usuarioId}", ip: ip);

            return ResultadoOperacionDTO.Ok($"Rol '{rolNombre}' asignado correctamente.");
        }

        public async Task<ResultadoOperacionDTO> RevocarRolAsync(
            string usuarioId, string rolNombre, string adminId, string adminNombre, string ip)
        {
            if (string.IsNullOrEmpty(usuarioId) || string.IsNullOrEmpty(rolNombre))
                return ResultadoOperacionDTO.Fallo("Usuario o rol no válido.", CodigoResultado.DatosInvalidos);

            var ok = await _rolRepo.RevocarRolAsync(usuarioId, rolNombre);
            if (!ok)
                return ResultadoOperacionDTO.Fallo("No se pudo revocar el rol.", CodigoResultado.Error);

            await _auditoria.RegistrarAsync(adminId, adminNombre, TipoOperacion.RevocarRol,
                "AspNetUserRoles", detalle: $"Rol '{rolNombre}' revocado de usuario {usuarioId}", ip: ip);

            return ResultadoOperacionDTO.Ok($"Rol '{rolNombre}' revocado correctamente.");
        }
    }
}
