using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using System;
using System.Threading.Tasks;

namespace HappyTimesBalloons.LogicaNegocio.Servicios
{
    public class RecuperacionPasswordServicio : IRecuperacionPasswordServicio
    {
        private readonly IRecuperacionPasswordRepositorio _repo;
        private const int ExpiracionMinutos = 60;

        public RecuperacionPasswordServicio(IRecuperacionPasswordRepositorio repo)
        {
            _repo = repo;
        }

        public async Task<ResultadoOperacionDTO<string>> SolicitarRecuperacionAsync(string email)
        {
            // Respuesta ambigua para no revelar si el email existe
            if (!await _repo.ExisteEmailAsync(email))
                return ResultadoOperacionDTO<string>.Ok("Si el correo existe, recibirás un enlace de recuperación.");

            var usuarioId = await _repo.ObtenerUsuarioIdPorEmailAsync(email);
            var token = Guid.NewGuid().ToString("N");
            var expiracion = DateTime.UtcNow.AddMinutes(ExpiracionMinutos);

            await _repo.GuardarTokenAsync(usuarioId, token, expiracion);

            return ResultadoOperacionDTO<string>.Ok("Token generado.", token);
        }

        public async Task<TokenRecuperacionDTO> ValidarTokenAsync(string token)
        {
            return await _repo.ObtenerTokenValidoAsync(token);
        }

        public async Task<ResultadoOperacionDTO> RestablecerContrasenaAsync(string token, string nuevaContrasena)
        {
            var tokenDto = await _repo.ObtenerTokenValidoAsync(token);
            if (tokenDto == null)
                return ResultadoOperacionDTO.Fallo(
                    "El enlace de recuperación no es válido o ha expirado.",
                    CodigoResultado.DatosInvalidos);

            var resultado = await _repo.RestablecerPasswordAsync(tokenDto.UsuarioId, nuevaContrasena);
            if (!resultado.Exito)
                return resultado;

            await _repo.MarcarComoUsadoAsync(tokenDto.Id);
            return ResultadoOperacionDTO.Ok("Contraseña actualizada exitosamente.");
        }
    }
}
