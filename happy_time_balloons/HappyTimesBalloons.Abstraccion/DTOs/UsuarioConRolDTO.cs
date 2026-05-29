using System.Collections.Generic;

namespace HappyTimesBalloons.Abstraccion.DTOs
{
    public class UsuarioConRolDTO
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
    }
}
