namespace HappyTimesBalloons.AccesoADatos.Migraciones
{
    using System.Data.Entity.Migrations;

    public partial class AgregarTablaInventario : DbMigration
    {
        public override void Up()
        {
            // 1. Crear tabla Inventario
            CreateTable(
                "dbo.Inventario",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    ProductoId = c.Int(nullable: false),
                    StockActual = c.Int(nullable: false),
                    StockMinimo = c.Int(nullable: false),
                    FechaUltimaActualizacion = c.DateTime(nullable: false),
                    UsuarioUltimaActualizacionId = c.String(maxLength: 128),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Productos", t => t.ProductoId, cascadeDelete: true)
                .Index(t => t.ProductoId, unique: true);

            // 2. Poblar Inventario desde el Stock existente en Productos
            //    Debe ejecutarse ANTES de eliminar la columna Stock.
            Sql(@"
                INSERT INTO dbo.Inventario
                    (ProductoId, StockActual, StockMinimo, FechaUltimaActualizacion)
                SELECT
                    Id,
                    ISNULL(Stock, 0),
                    5,
                    GETUTCDATE()
                FROM dbo.Productos
            ");

            // 3. Eliminar columna Stock de Productos (ahora ya migrada)
            DropColumn("dbo.Productos", "Stock");
        }

        public override void Down()
        {
            // 1. Restaurar columna Stock en Productos
            AddColumn("dbo.Productos", "Stock", c => c.Int(nullable: false));

            // 2. Recuperar valores desde Inventario antes de eliminar la tabla
            Sql(@"
                UPDATE p
                SET p.Stock = ISNULL(i.StockActual, 0)
                FROM dbo.Productos p
                LEFT JOIN dbo.Inventario i ON i.ProductoId = p.Id
            ");

            // 3. Eliminar tabla Inventario
            DropIndex("dbo.Inventario", new[] { "ProductoId" });
            DropForeignKey("dbo.Inventario", "ProductoId", "dbo.Productos");
            DropTable("dbo.Inventario");
        }
    }
}
