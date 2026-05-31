namespace HappyTimesBalloons.AccesoADatos.Migraciones
{
    using System.Data.Entity.Migrations;

    public partial class AgregarTriggerDescontarStock : DbMigration
    {
        public override void Up()
        {
            // Red de seguridad absoluta: impide que StockActual quede negativo
            Sql("ALTER TABLE dbo.Inventario ADD CONSTRAINT CK_Inventario_StockNoNegativo CHECK (StockActual >= 0)");

            Sql(@"
CREATE TRIGGER trg_DescontarStock
ON dbo.DetallesPedido
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    -- Verificar stock suficiente antes de descontar
    IF EXISTS (
        SELECT 1
        FROM inserted i
        LEFT JOIN dbo.Inventario inv ON inv.ProductoId = i.ProductoId
        WHERE ISNULL(inv.StockActual, 0) < i.Cantidad
    )
    BEGIN
        RAISERROR('Stock insuficiente para uno o mas productos del pedido.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    -- Descontar stock dentro de la misma transaccion del pedido
    UPDATE inv
    SET inv.StockActual = inv.StockActual - i.Cantidad,
        inv.FechaUltimaActualizacion = GETUTCDATE()
    FROM dbo.Inventario inv
    INNER JOIN inserted i ON inv.ProductoId = i.ProductoId;
END
");
        }

        public override void Down()
        {
            Sql(@"
IF OBJECT_ID('dbo.trg_DescontarStock', 'TR') IS NOT NULL
    DROP TRIGGER dbo.trg_DescontarStock
");
            Sql(@"
IF OBJECT_ID('CK_Inventario_StockNoNegativo') IS NOT NULL
    ALTER TABLE dbo.Inventario DROP CONSTRAINT CK_Inventario_StockNoNegativo
");
        }
    }
}
