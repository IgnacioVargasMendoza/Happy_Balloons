namespace HappyTimesBalloons.AccesoADatos.Migraciones
{
    using System.Data.Entity.Migrations;

    public partial class AgregarTablaPagosSinpe : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PagosSinpe",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    PedidoId = c.Int(nullable: false),
                    NumeroComprobante = c.String(nullable: false, maxLength: 100),
                    Monto = c.Decimal(nullable: false, precision: 18, scale: 2),
                    NombreTitular = c.String(maxLength: 200),
                    TelefonoDestino = c.String(maxLength: 20),
                    EstadoPago = c.Int(nullable: false),
                    MotivoRechazo = c.String(maxLength: 500),
                    FechaRecepcion = c.DateTime(nullable: false),
                    FechaProcesamiento = c.DateTime(nullable: true),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Pedidos", t => t.PedidoId, cascadeDelete: false)
                .Index(t => t.PedidoId)
                .Index(t => t.NumeroComprobante, unique: true, name: "IX_PagosSinpe_NumeroComprobante");
        }

        public override void Down()
        {
            DropIndex("dbo.PagosSinpe", "IX_PagosSinpe_NumeroComprobante");
            DropIndex("dbo.PagosSinpe", new[] { "PedidoId" });
            DropForeignKey("dbo.PagosSinpe", "PedidoId", "dbo.Pedidos");
            DropTable("dbo.PagosSinpe");
        }
    }
}
