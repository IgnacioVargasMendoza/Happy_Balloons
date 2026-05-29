using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.AccesoADatos.Contexto;
using HappyTimesBalloons.AccesoADatos.Modelos;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace HappyTimesBalloons.AccesoADatos.Repositorios
{
    public class RolRepositorio : IRolRepositorio, IDisposable
    {
        private readonly ApplicationDbContext _ctx;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RolRepositorio(ApplicationDbContext ctx)
        {
            _ctx = ctx;
            _roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(ctx));
            _userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(ctx));
        }

        public async Task<List<RolDTO>> ObtenerTodosAsync()
        {
            return await _ctx.Roles
                .OrderBy(r => r.Name)
                .Select(r => new RolDTO
                {
                    Id = r.Id,
                    Nombre = r.Name,
                    CantidadUsuarios = r.Users.Count
                })
                .ToListAsync();
        }

        public async Task<RolDTO> ObtenerPorIdAsync(string id)
        {
            return await _ctx.Roles
                .Where(r => r.Id == id)
                .Select(r => new RolDTO
                {
                    Id = r.Id,
                    Nombre = r.Name,
                    CantidadUsuarios = r.Users.Count
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> ExisteNombreAsync(string nombre)
        {
            return await _ctx.Roles.AnyAsync(r => r.Name == nombre);
        }

        public async Task<string> CrearAsync(string nombre)
        {
            var rol = new IdentityRole(nombre);
            var resultado = await _roleManager.CreateAsync(rol);
            return resultado.Succeeded ? rol.Id : null;
        }

        public async Task<bool> ActualizarAsync(string id, string nuevoNombre)
        {
            var rol = await _roleManager.FindByIdAsync(id);
            if (rol == null) return false;
            rol.Name = nuevoNombre;
            var resultado = await _roleManager.UpdateAsync(rol);
            return resultado.Succeeded;
        }

        public async Task<bool> EliminarAsync(string id)
        {
            var rol = await _roleManager.FindByIdAsync(id);
            if (rol == null) return false;
            var resultado = await _roleManager.DeleteAsync(rol);
            return resultado.Succeeded;
        }

        public async Task<List<UsuarioConRolDTO>> ObtenerTodosLosUsuariosAsync()
        {
            var rolesDict = await _ctx.Roles.ToDictionaryAsync(r => r.Id, r => r.Name);

            var usuarios = await _ctx.Users
                .Include(u => u.Roles)
                .OrderBy(u => u.Nombre)
                .ToListAsync();

            return usuarios.Select(u => new UsuarioConRolDTO
            {
                Id = u.Id,
                Nombre = u.Nombre,
                Email = u.Email,
                Roles = u.Roles
                    .Select(ur => rolesDict.ContainsKey(ur.RoleId) ? rolesDict[ur.RoleId] : ur.RoleId)
                    .ToList()
            }).ToList();
        }

        public async Task<bool> AsignarRolAsync(string usuarioId, string rolNombre)
        {
            var resultado = await _userManager.AddToRoleAsync(usuarioId, rolNombre);
            return resultado.Succeeded;
        }

        public async Task<bool> RevocarRolAsync(string usuarioId, string rolNombre)
        {
            var resultado = await _userManager.RemoveFromRoleAsync(usuarioId, rolNombre);
            return resultado.Succeeded;
        }

        public void Dispose()
        {
            _roleManager?.Dispose();
            _userManager?.Dispose();
        }
    }
}
