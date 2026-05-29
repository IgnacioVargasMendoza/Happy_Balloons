using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.AccesoADatos.Contexto;
using HappyTimesBalloons.AccesoADatos.Modelos;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Threading.Tasks;

namespace HappyTimesBalloons.AccesoADatos.Repositorios
{
    public class AuthRepositorio : IAuthRepositorio, IDisposable
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthRepositorio(ApplicationDbContext ctx)
        {
            _userManager = CrearUserManager(ctx);
        }

        private static UserManager<ApplicationUser> CrearUserManager(ApplicationDbContext ctx)
        {
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(ctx));
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };
            manager.PasswordValidator = new PasswordValidator { RequiredLength = 6 };
            manager.UserLockoutEnabledByDefault = true;
            // TODO: leer desde ConfiguracionSistema (módulo CFG)
            manager.MaxFailedAccessAttemptsBeforeLockout = 3;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(30);
            return manager;
        }

        public async Task<UsuarioDTO> BuscarPorEmailAsync(string email)
        {
            var usuario = await _userManager.FindByEmailAsync(email);
            if (usuario == null) return null;
            return new UsuarioDTO
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                Telefono = usuario.PhoneNumber,
                Direccion = usuario.Direccion,
                TieneDobleFactor = usuario.TwoFactorEnabled
            };
        }

        public async Task<ResultadoOperacionDTO> CrearCuentaAsync(RegistroDTO registro)
        {
            var nuevoUsuario = new ApplicationUser
            {
                UserName = registro.Email,
                Email = registro.Email,
                Nombre = registro.Nombre.Trim(),
                PhoneNumber = registro.Telefono,
                Direccion = registro.Direccion,
                EmailConfirmed = true,
                LockoutEnabled = true
            };

            var resultado = await _userManager.CreateAsync(nuevoUsuario, registro.Contrasena);

            if (!resultado.Succeeded)
            {
                string errores = string.Join(" ", resultado.Errors);
                return ResultadoOperacionDTO.Fallo(errores, CodigoResultado.DatosInvalidos);
            }

            var dto = new UsuarioDTO
            {
                Id = nuevoUsuario.Id,
                Nombre = nuevoUsuario.Nombre,
                Email = nuevoUsuario.Email,
                Telefono = nuevoUsuario.PhoneNumber,
                Direccion = nuevoUsuario.Direccion,
                Rol = "Cliente"
            };

            return ResultadoOperacionDTO.Ok("Cuenta creada exitosamente.", dto);
        }

        public async Task AsignarRolAsync(string userId, string rol)
        {
            await _userManager.AddToRoleAsync(userId, rol);
        }

        public async Task<bool> EstaBloquedoAsync(string userId)
        {
            return await _userManager.IsLockedOutAsync(userId);
        }

        public async Task<bool> VerificarPasswordAsync(string userId, string password)
        {
            var usuario = await _userManager.FindByIdAsync(userId);
            return await _userManager.CheckPasswordAsync(usuario, password);
        }

        public async Task RegistrarIntentoFallidoAsync(string userId)
        {
            await _userManager.AccessFailedAsync(userId);
        }

        public async Task<bool> SeBloqueoAsync(string userId)
        {
            return await _userManager.IsLockedOutAsync(userId);
        }

        public async Task<int> ObtenerIntentosRestantesAsync(string userId)
        {
            int intentos = await _userManager.GetAccessFailedCountAsync(userId);
            return _userManager.MaxFailedAccessAttemptsBeforeLockout - intentos;
        }

        public async Task ResetearIntentosAsync(string userId)
        {
            await _userManager.ResetAccessFailedCountAsync(userId);
        }

        public async Task<bool> EsEnRolAsync(string userId, string rol)
        {
            return await _userManager.IsInRoleAsync(userId, rol);
        }

        public void Dispose()
        {
            _userManager?.Dispose();
        }
    }
}
