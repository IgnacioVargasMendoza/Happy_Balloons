using HappyTimesBalloons.AccesoADatos.Modelos;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace HappyTimesBalloons.AccesoADatos.Contexto
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("HappyTimesBallooonsContext", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public DbSet<BitacoraAuditoria> BitacoraAuditoria { get; set; }

        public DbSet<Categoria> Categorias { get; set; }

        public DbSet<Producto> Productos { get; set; }

        public DbSet<ImagenProducto> ImagenesProducto { get; set; }

        public DbSet<Pedido> Pedidos { get; set; }

        public DbSet<DetallePedido> DetallesPedido { get; set; }

        public DbSet<ZonaEntrega> ZonasEntrega { get; set; }

        public DbSet<Promocion> Promociones { get; set; }
    }
}