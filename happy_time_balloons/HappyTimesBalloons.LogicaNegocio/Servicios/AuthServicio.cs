using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using HappyTimesBalloons.AccesoADatos.Contexto;
using HappyTimesBalloons.AccesoADatos.Modelos;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Threading.Tasks;

namespace HappyTimesBalloons.LogicaNegocio.Servicios
{
    public class AuthServicio : IAuthServicio
    {
        private readonly ApplicationDbContext _ctx;

        public AuthServicio(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }

        // HU-CLI-001: Registro de cliente con validación de email duplicado y datos incompletos
        public async Task<ResultadoOperacionDTO> RegistrarAsync(RegistroDTO registro)
        {
            if (string.IsNullOrWhiteSpace(registro.Nombre) ||
                string.IsNullOrWhiteSpace(registro.Email) ||
                string.IsNullOrWhiteSpace(registro.Contrasena) ||
                string.IsNullOrWhiteSpace(registro.Telefono))
            {
                return ResultadoOperacionDTO.Fallo(
                    "Por favor completa todos los campos requeridos.",
                    CodigoResultado.DatosInvalidos);
            }

            var userManager = CrearUserManager();

            if (await userManager.FindByEmailAsync(registro.Email) != null)
            {
                return ResultadoOperacionDTO.Fallo(
                    "Este correo electrónico ya está registrado.",
                    CodigoResultado.EmailDuplicado);
            }

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

            var resultado = await userManager.CreateAsync(nuevoUsuario, registro.Contrasena);

            if (!resultado.Succeeded)
            {
                string errores = string.Join(" ", resultado.Errors);
                return ResultadoOperacionDTO.Fallo(errores, CodigoResultado.DatosInvalidos);
            }

            await userManager.AddToRoleAsync(nuevoUsuario.Id, "Cliente");

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

        private UserManager<ApplicationUser> CrearUserManager()
        {
            var store = new UserStore<ApplicationUser>(_ctx);
            var mgr = new UserManager<ApplicationUser>(store);
            mgr.UserValidator = new UserValidator<ApplicationUser>(mgr)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };
            mgr.PasswordValidator = new PasswordValidator { RequiredLength = 6 };
            mgr.UserLockoutEnabledByDefault = true;
            // TODO: leer MaxFailedAccessAttemptsBeforeLockout desde ConfiguracionSistema (módulo CFG)
            mgr.MaxFailedAccessAttemptsBeforeLockout = 3;
            mgr.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(30);
            return mgr;
        }
    }
}
