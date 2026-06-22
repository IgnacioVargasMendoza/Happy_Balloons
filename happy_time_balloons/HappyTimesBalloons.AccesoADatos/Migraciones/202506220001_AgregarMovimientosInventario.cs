namespace HappyTimesBalloons.AccesoADatos.Migraciones
{
    using System.Data.Entity.Migrations;

    public partial class AgregarMovimientosInventario : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MovimientosInventario",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    ProductoId = c.Int(nullable: false),
                    TipoMovimiento = c.Int(nullable: false),
                    Cantidad = c.Int(nullable: false),
                    StockAnterior = c.Int(nullable: false),
                    StockNuevo = c.Int(nullable: false),
                    Motivo = c.String(nullable: false, maxLength: 500),
                    UsuarioId = c.String(nullable: false, maxLength: 128),
                    FechaMovimiento = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Productos", t => t.ProductoId, cascadeDelete: false)
                .Index(t => t.ProductoId)
                .Index(t => t.FechaMovimiento);

            Sql("ALTER TABLE dbo.MovimientosInventario ADD CONSTRAINT CK_MovimientosInventario_StockNuevo CHECK (StockNuevo >= 0)");
        }

        public override void Down()
        {
            Sql("ALTER TABLE dbo.MovimientosInventario DROP CONSTRAINT CK_MovimientosInventario_StockNuevo");
            DropIndex("dbo.MovimientosInventario", new[] { "FechaMovimiento" });
            DropIndex("dbo.MovimientosInventario", new[] { "ProductoId" });
            DropForeignKey("dbo.MovimientosInventario", "ProductoId", "dbo.Productos");
            DropTable("dbo.MovimientosInventario");
        }
    }
}
