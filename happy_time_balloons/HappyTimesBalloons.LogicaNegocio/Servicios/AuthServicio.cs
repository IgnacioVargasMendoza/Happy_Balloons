using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using System.Threading.Tasks;

namespace HappyTimesBalloons.LogicaNegocio.Servicios
{
    public class AuthServicio : IAuthServicio
    {
        private readonly IAuthRepositorio _authRepo;

        public AuthServicio(IAuthRepositorio authRepo)
        {
            _authRepo = authRepo;
        }

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

            if (await _authRepo.BuscarPorEmailAsync(registro.Email) != null)
            {
                return ResultadoOperacionDTO.Fallo(
                    "Este correo electrónico ya está registrado.",
                    CodigoResultado.EmailDuplicado);
            }

            var resultado = await _authRepo.CrearCuentaAsync(registro);

            if (resultado.Exito)
                await _authRepo.AsignarRolAsync(resultado.Usuario.Id, "Cliente");

            return resultado;
        }

        public async Task<ResultadoLoginDTO> ValidarCredencialesAsync(string email, string password)
        {
            var usuario = await _authRepo.BuscarPorEmailAsync(email);

            if (usuario == null)
                return new ResultadoLoginDTO
                {
                    Exito = false,
                    Mensaje = "Usuario no encontrado",
                    Email = email,
                    NombreUsuario = email
                };

            if (await _authRepo.EstaBloquedoAsync(usuario.Id))
                return new ResultadoLoginDTO
                {
                    Exito = false,
                    CuentaBloqueada = true,
                    Mensaje = "Cuenta bloqueada temporalmente por múltiples intentos fallidos.",
                    UsuarioId = usuario.Id,
                    NombreUsuario = usuario.Nombre ?? usuario.Email,
                    Email = usuario.Email
                };

            bool passwordValida = await _authRepo.VerificarPasswordAsync(usuario.Id, password);
            if (!passwordValida)
            {
                await _authRepo.RegistrarIntentoFallidoAsync(usuario.Id);

                if (await _authRepo.SeBloqueoAsync(usuario.Id))
                    return new ResultadoLoginDTO
                    {
                        Exito = false,
                        CuentaBloqueada = true,
                        Mensaje = "Cuenta bloqueada por múltiples intentos fallidos.",
                        UsuarioId = usuario.Id,
                        NombreUsuario = usuario.Nombre ?? usuario.Email,
                        Email = usuario.Email
                    };

                int restantes = await _authRepo.ObtenerIntentosRestantesAsync(usuario.Id);
                return new ResultadoLoginDTO
                {
                    Exito = false,
                    IntentosRestantes = restantes,
                    Mensaje = restantes > 0
                        ? $"Contraseña incorrecta. Te quedan {restantes} intento(s)."
                        : "Credenciales incorrectas.",
                    UsuarioId = usuario.Id,
                    NombreUsuario = usuario.Nombre ?? usuario.Email,
                    Email = usuario.Email
                };
            }

            await _authRepo.ResetearIntentosAsync(usuario.Id);

            bool esAdmin = await _authRepo.EsEnRolAsync(usuario.Id, "Administrador");
            return new ResultadoLoginDTO
            {
                Exito = true,
                Mensaje = "Login exitoso",
                UsuarioId = usuario.Id,
                NombreUsuario = usuario.Nombre ?? usuario.Email,
                Email = usuario.Email,
                EsAdmin = esAdmin
            };
        }
    }
}
