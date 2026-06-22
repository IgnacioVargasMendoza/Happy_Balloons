using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HappyTimesBalloons.Tests.Inventario
{
    [TestClass]
    public class AlertaStockTests
    {
        // Replica la lógica de InventarioItemViewModel.EstadoStock
        private static string ObtenerEstadoStock(int stockActual, int stockMinimo)
        {
            if (stockActual == 0) return "sin_stock";
            if (stockActual <= stockMinimo) return "bajo";
            return "normal";
        }

        [TestMethod]
        public void EstadoStock_StockActualCero_RetornaSinStock()
        {
            Assert.AreEqual("sin_stock", ObtenerEstadoStock(stockActual: 0, stockMinimo: 5));
        }

        [TestMethod]
        public void EstadoStock_StockActualCeroConMinimoEnCero_RetornaSinStock()
        {
            Assert.AreEqual("sin_stock", ObtenerEstadoStock(stockActual: 0, stockMinimo: 0));
        }

        [TestMethod]
        public void EstadoStock_StockActualMenorAlMinimo_RetornaBajo()
        {
            Assert.AreEqual("bajo", ObtenerEstadoStock(stockActual: 3, stockMinimo: 10));
        }

        [TestMethod]
        public void EstadoStock_StockActualIgualAlMinimo_RetornaBajo()
        {
            Assert.AreEqual("bajo", ObtenerEstadoStock(stockActual: 5, stockMinimo: 5));
        }

        [TestMethod]
        public void EstadoStock_StockActualMayorAlMinimo_RetornaNormal()
        {
            Assert.AreEqual("normal", ObtenerEstadoStock(stockActual: 20, stockMinimo: 10));
        }

        [TestMethod]
        public void EstadoStock_StockActualUnoConMinimoUno_RetornaBajo()
        {
            Assert.AreEqual("bajo", ObtenerEstadoStock(stockActual: 1, stockMinimo: 1));
        }

        [TestMethod]
        public void EstadoStock_StockActualUnoConMinimoCero_RetornaNormal()
        {
            Assert.AreEqual("normal", ObtenerEstadoStock(stockActual: 1, stockMinimo: 0));
        }
    }
}
