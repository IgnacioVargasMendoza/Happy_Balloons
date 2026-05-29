using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class RolIndexViewModel
    {
        public List<RolItemViewModel> Roles { get; set; } = new List<RolItemViewModel>();
    }

    public class RolItemViewModel
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public int CantidadUsuarios { get; set; }
    }

    public class RolFormViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "El nombre del rol es requerido.")]
        [StringLength(256, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 256 caracteres.")]
        [Display(Name = "Nombre del rol")]
        public string Nombre { get; set; }
    }

    public class UsuariosRolViewModel
    {
        public List<UsuarioRolItemViewModel> Usuarios { get; set; } = new List<UsuarioRolItemViewModel>();
        public List<RolItemViewModel> RolesDisponibles { get; set; } = new List<RolItemViewModel>();
    }

    public class UsuarioRolItemViewModel
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public string RolActual => Roles.Count > 0 ? string.Join(", ", Roles) : "Sin rol";
    }
}
