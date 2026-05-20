import React, { useState } from 'react';
import { useNavigate } from 'react-router';
import { useApp } from '../context/AppContext';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '../components/ui/tabs';
import { Button } from '../components/ui/button';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import { Alert, AlertDescription } from '../components/ui/alert';
import { Badge } from '../components/ui/badge';
import { Separator } from '../components/ui/separator';
import { 
  Settings, 
  BarChart3, 
  Tag, 
  Shield,
  Calendar,
  Trash2,
  Plus,
  AlertCircle,
  CheckCircle2,
  TrendingUp,
  Package,
  DollarSign,
  Edit,
  Eye,
  Power,
  Search,
  ShoppingBag,
  FolderOpen
} from 'lucide-react';
import { toast } from 'sonner';

// HU-CFG-001, HU-REP-001, HU-PRM-001: Panel de administración
export default function AdminDashboard() {
  const navigate = useNavigate();
  const { 
    currentUser, 
    systemConfig, 
    updateSystemConfig,
    orders,
    products,
    promotions,
    createPromotion,
    deletePromotion
  } = useApp();

  // Configuración
  const [config, setConfig] = useState(systemConfig);
  const [configError, setConfigError] = useState('');

  // Promociones
  const [newPromotion, setNewPromotion] = useState({
    productId: '',
    discountPercent: '',
    startDate: '',
    endDate: '',
    isActive: true
  });
  const [promoErrors, setPromoErrors] = useState<Record<string, string>>({});

  // Reportes - filtro de fechas
  const [dateFilter, setDateFilter] = useState({
    start: '',
    end: ''
  });

  // Verificar que sea admin
  React.useEffect(() => {
    if (!currentUser || currentUser.role !== 'admin') {
      toast.error('No tienes permisos para acceder a esta sección');
      navigate('/');
    }
  }, [currentUser, navigate]);

  if (!currentUser || currentUser.role !== 'admin') {
    return null;
  }

  // HU-CFG-001: Actualizar configuración
  const handleUpdateConfig = () => {
    const result = updateSystemConfig(config);
    
    if (result.success) {
      toast.success('Configuración actualizada correctamente');
      setConfigError('');
    } else {
      // Escenario 4: Datos inválidos
      setConfigError(result.message);
      toast.error(result.message);
    }
  };

  // HU-PRM-001: Crear promoción
  const handleCreatePromotion = () => {
    // Validación
    if (!newPromotion.productId) {
      toast.error('Selecciona un producto');
      return;
    }

    const result = createPromotion({
      productId: newPromotion.productId,
      discountPercent: Number(newPromotion.discountPercent),
      startDate: newPromotion.startDate,
      endDate: newPromotion.endDate,
      isActive: newPromotion.isActive
    });

    if (result.success) {
      toast.success(result.message);
      setNewPromotion({
        productId: '',
        discountPercent: '',
        startDate: '',
        endDate: '',
        isActive: true
      });
    } else {
      toast.error(result.message);
    }
  };

  // HU-REP-001: Filtrar órdenes por fecha
  const filteredOrders = orders.filter(order => {
    if (!dateFilter.start && !dateFilter.end) return true;
    
    const orderDate = new Date(order.createdAt);
    const start = dateFilter.start ? new Date(dateFilter.start) : null;
    const end = dateFilter.end ? new Date(dateFilter.end) : null;

    if (start && orderDate < start) return false;
    if (end && orderDate > end) return false;
    return true;
  });

  // Calcular estadísticas
  const totalSales = filteredOrders.reduce((sum, order) => sum + order.total, 0);
  const totalOrders = filteredOrders.length;
  const avgOrderValue = totalOrders > 0 ? totalSales / totalOrders : 0;

  // Productos más vendidos
  const productSales: Record<string, { name: string; quantity: number; revenue: number }> = {};
  filteredOrders.forEach(order => {
    order.items.forEach(item => {
      if (!productSales[item.product.id]) {
        productSales[item.product.id] = {
          name: item.product.name,
          quantity: 0,
          revenue: 0
        };
      }
      productSales[item.product.id].quantity += item.quantity;
      const price = item.product.discountPrice || item.product.price;
      productSales[item.product.id].revenue += price * item.quantity;
    });
  });

  const topProducts = Object.entries(productSales)
    .sort((a, b) => b[1].revenue - a[1].revenue)
    .slice(0, 5);

  const formatPrice = (price: number) => {
    return `₡${price.toLocaleString('es-CR')}`;
  };

  const formatDate = (date: string) => {
    return new Date(date).toLocaleDateString('es-CR');
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Panel de Administración</h1>
          <p className="text-muted-foreground">Gestiona el sistema Happy Times Balloons</p>
        </div>
        <Badge variant="outline" className="text-base">
          <Shield className="h-4 w-4 mr-1" />
          Administrador
        </Badge>
      </div>

      <Tabs defaultValue="reports" className="space-y-6">
        <TabsList className="grid w-full grid-cols-4">
          <TabsTrigger value="reports">
            <BarChart3 className="h-4 w-4 mr-2" />
            Reportes
          </TabsTrigger>
          <TabsTrigger value="products" onClick={() => navigate('/admin/products')}>
            <Package className="h-4 w-4 mr-2" />
            Productos
          </TabsTrigger>
          <TabsTrigger value="promotions">
            <Tag className="h-4 w-4 mr-2" />
            Promociones
          </TabsTrigger>
          <TabsTrigger value="config">
            <Settings className="h-4 w-4 mr-2" />
            Configuración
          </TabsTrigger>
        </TabsList>

        {/* Accesos rápidos Admin */}
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <Card className="cursor-pointer hover:shadow-md transition-shadow" onClick={() => navigate('/admin/products')}>
            <CardContent className="pt-6">
              <div className="flex items-center gap-3">
                <div className="h-10 w-10 bg-blue-100 rounded-lg flex items-center justify-center">
                  <Package className="h-5 w-5 text-blue-600" />
                </div>
                <div>
                  <p className="font-medium text-sm">Productos</p>
                  <p className="text-xs text-muted-foreground">{products.length} en total</p>
                </div>
              </div>
            </CardContent>
          </Card>
          <Card className="cursor-pointer hover:shadow-md transition-shadow" onClick={() => navigate('/admin/categories')}>
            <CardContent className="pt-6">
              <div className="flex items-center gap-3">
                <div className="h-10 w-10 bg-purple-100 rounded-lg flex items-center justify-center">
                  <FolderOpen className="h-5 w-5 text-purple-600" />
                </div>
                <div>
                  <p className="font-medium text-sm">Categorías</p>
                  <p className="text-xs text-muted-foreground">Organizar catálogo</p>
                </div>
              </div>
            </CardContent>
          </Card>
          <Card className="cursor-pointer hover:shadow-md transition-shadow" onClick={() => navigate('/admin/orders')}>
            <CardContent className="pt-6">
              <div className="flex items-center gap-3">
                <div className="h-10 w-10 bg-green-100 rounded-lg flex items-center justify-center">
                  <ShoppingBag className="h-5 w-5 text-green-600" />
                </div>
                <div>
                  <p className="font-medium text-sm">Pedidos</p>
                  <p className="text-xs text-muted-foreground">{orders.length} en total</p>
                </div>
              </div>
            </CardContent>
          </Card>
          <Card className="cursor-pointer hover:shadow-md transition-shadow">
            <CardContent className="pt-6">
              <div className="flex items-center gap-3">
                <div className="h-10 w-10 bg-pink-100 rounded-lg flex items-center justify-center">
                  <Tag className="h-5 w-5 text-pink-600" />
                </div>
                <div>
                  <p className="font-medium text-sm">Promociones</p>
                  <p className="text-xs text-muted-foreground">{promotions.length} activas</p>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>

        {/* HU-REP-001: REPORTES */}
        <TabsContent value="reports" className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Filtros de reporte</CardTitle>
              <CardDescription>Filtra las ventas por rango de fechas</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="grid md:grid-cols-3 gap-4">
                <div>
                  <Label htmlFor="startDate">Fecha inicio</Label>
                  <Input
                    id="startDate"
                    type="date"
                    value={dateFilter.start}
                    onChange={(e) => setDateFilter({ ...dateFilter, start: e.target.value })}
                  />
                </div>
                <div>
                  <Label htmlFor="endDate">Fecha fin</Label>
                  <Input
                    id="endDate"
                    type="date"
                    value={dateFilter.end}
                    onChange={(e) => setDateFilter({ ...dateFilter, end: e.target.value })}
                  />
                </div>
                <div className="flex items-end">
                  <Button
                    variant="outline"
                    className="w-full"
                    onClick={() => setDateFilter({ start: '', end: '' })}
                  >
                    Limpiar filtros
                  </Button>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Escenario 4: Sin datos */}
          {filteredOrders.length === 0 ? (
            <Card>
              <CardContent className="py-16 text-center">
                <Package className="h-16 w-16 text-muted-foreground mx-auto mb-4" />
                <h3 className="text-xl font-semibold mb-2">No hay datos disponibles</h3>
                <p className="text-muted-foreground">
                  No se encontraron ventas en el rango de fechas seleccionado
                </p>
              </CardContent>
            </Card>
          ) : (
            <>
              {/* Escenario 1: Resumen de ventas */}
              <div className="grid md:grid-cols-3 gap-6">
                <Card>
                  <CardContent className="pt-6">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-sm text-muted-foreground mb-1">Ventas totales</p>
                        <p className="text-2xl font-bold">{formatPrice(totalSales)}</p>
                      </div>
                      <div className="h-12 w-12 bg-green-100 rounded-full flex items-center justify-center">
                        <DollarSign className="h-6 w-6 text-green-600" />
                      </div>
                    </div>
                  </CardContent>
                </Card>

                <Card>
                  <CardContent className="pt-6">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-sm text-muted-foreground mb-1">Pedidos</p>
                        <p className="text-2xl font-bold">{totalOrders}</p>
                      </div>
                      <div className="h-12 w-12 bg-blue-100 rounded-full flex items-center justify-center">
                        <Package className="h-6 w-6 text-blue-600" />
                      </div>
                    </div>
                  </CardContent>
                </Card>

                <Card>
                  <CardContent className="pt-6">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-sm text-muted-foreground mb-1">Promedio por pedido</p>
                        <p className="text-2xl font-bold">{formatPrice(avgOrderValue)}</p>
                      </div>
                      <div className="h-12 w-12 bg-purple-100 rounded-full flex items-center justify-center">
                        <TrendingUp className="h-6 w-6 text-purple-600" />
                      </div>
                    </div>
                  </CardContent>
                </Card>
              </div>

              {/* Escenario 3: Productos más vendidos */}
              <Card>
                <CardHeader>
                  <CardTitle>Top 5 Productos más vendidos</CardTitle>
                </CardHeader>
                <CardContent>
                  {topProducts.length > 0 ? (
                    <div className="space-y-4">
                      {topProducts.map(([productId, data], index) => (
                        <div key={productId} className="flex items-center gap-4">
                          <div className="h-10 w-10 rounded-full bg-pink-100 flex items-center justify-center font-bold text-pink-600">
                            {index + 1}
                          </div>
                          <div className="flex-1">
                            <p className="font-semibold">{data.name}</p>
                            <p className="text-sm text-muted-foreground">
                              {data.quantity} unidades vendidas
                            </p>
                          </div>
                          <div className="text-right">
                            <p className="font-bold">{formatPrice(data.revenue)}</p>
                          </div>
                        </div>
                      ))}
                    </div>
                  ) : (
                    <p className="text-center text-muted-foreground py-8">
                      No hay productos vendidos en este período
                    </p>
                  )}
                </CardContent>
              </Card>

              {/* Escenario 2: Lista de pedidos */}
              <Card>
                <CardHeader>
                  <CardTitle>Últimos pedidos</CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="space-y-4">
                    {filteredOrders.slice(0, 10).map((order) => (
                      <div key={order.id} className="border rounded-lg p-4">
                        <div className="flex justify-between items-start mb-2">
                          <div>
                            <p className="font-semibold">{order.id}</p>
                            <p className="text-sm text-muted-foreground">
                              {formatDate(order.createdAt)}
                            </p>
                          </div>
                          <Badge 
                            variant={
                              order.status === 'entregado' ? 'default' :
                              order.status === 'pagado' ? 'secondary' :
                              'outline'
                            }
                          >
                            {order.status}
                          </Badge>
                        </div>
                        <div className="text-sm space-y-1">
                          <p>Productos: {order.items.length}</p>
                          <p>Zona: {order.deliveryZone}</p>
                          <p className="font-bold">Total: {formatPrice(order.total)}</p>
                        </div>
                      </div>
                    ))}
                  </div>
                </CardContent>
              </Card>
            </>
          )}
        </TabsContent>

        {/* HU-PRM-001: PROMOCIONES */}
        <TabsContent value="promotions" className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Crear nueva promoción</CardTitle>
              <CardDescription>
                Configura descuentos para productos específicos
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid md:grid-cols-2 gap-4">
                <div>
                  <Label htmlFor="product">Producto</Label>
                  <select
                    id="product"
                    className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                    value={newPromotion.productId}
                    onChange={(e) => setNewPromotion({ ...newPromotion, productId: e.target.value })}
                  >
                    <option value="">Selecciona un producto</option>
                    {products.map(product => (
                      <option key={product.id} value={product.id}>
                        {product.name}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <Label htmlFor="discount">Descuento (%)</Label>
                  <Input
                    id="discount"
                    type="number"
                    min="1"
                    max="100"
                    placeholder="15"
                    value={newPromotion.discountPercent}
                    onChange={(e) => setNewPromotion({ ...newPromotion, discountPercent: e.target.value })}
                  />
                </div>

                <div>
                  <Label htmlFor="promoStart">Fecha inicio</Label>
                  <Input
                    id="promoStart"
                    type="date"
                    value={newPromotion.startDate}
                    onChange={(e) => setNewPromotion({ ...newPromotion, startDate: e.target.value })}
                  />
                </div>

                <div>
                  <Label htmlFor="promoEnd">Fecha fin</Label>
                  <Input
                    id="promoEnd"
                    type="date"
                    value={newPromotion.endDate}
                    onChange={(e) => setNewPromotion({ ...newPromotion, endDate: e.target.value })}
                  />
                </div>
              </div>

              <Button onClick={handleCreatePromotion} className="w-full">
                <Plus className="h-4 w-4 mr-2" />
                Crear promoción
              </Button>
            </CardContent>
          </Card>

          {/* Escenario 1 y 3: Lista de promociones */}
          <Card>
            <CardHeader>
              <CardTitle>Promociones activas</CardTitle>
            </CardHeader>
            <CardContent>
              {promotions.length > 0 ? (
                <div className="space-y-3">
                  {promotions.map((promo) => {
                    const product = products.find(p => p.id === promo.productId);
                    const isExpired = new Date(promo.endDate) < new Date();
                    
                    return (
                      <div 
                        key={promo.id} 
                        className="flex items-center justify-between p-4 border rounded-lg"
                      >
                        <div className="flex-1">
                          <p className="font-semibold">{product?.name}</p>
                          <div className="flex gap-2 items-center mt-1">
                            <Badge variant={isExpired ? 'outline' : 'default'}>
                              {promo.discountPercent}% OFF
                            </Badge>
                            <span className="text-sm text-muted-foreground">
                              {formatDate(promo.startDate)} - {formatDate(promo.endDate)}
                            </span>
                            {isExpired && (
                              <Badge variant="destructive">Vencida</Badge>
                            )}
                          </div>
                        </div>
                        <Button
                          variant="ghost"
                          size="icon"
                          className="text-red-500 hover:text-red-700"
                          onClick={() => deletePromotion(promo.id)}
                        >
                          <Trash2 className="h-4 w-4" />
                        </Button>
                      </div>
                    );
                  })}
                </div>
              ) : (
                <p className="text-center text-muted-foreground py-8">
                  No hay promociones activas
                </p>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        {/* HU-CFG-001: CONFIGURACIÓN */}
        <TabsContent value="config" className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Parámetros del sistema</CardTitle>
              <CardDescription>
                Configura los parámetros de seguridad y alertas
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              {configError && (
                <Alert variant="destructive">
                  <AlertCircle className="h-4 w-4" />
                  <AlertDescription>{configError}</AlertDescription>
                </Alert>
              )}

              {/* Escenario 2: Configurar stock mínimo */}
              <div>
                <Label htmlFor="minStock">Stock mínimo (Alerta)</Label>
                <Input
                  id="minStock"
                  type="number"
                  min="0"
                  value={config.minStock}
                  onChange={(e) => setConfig({ ...config, minStock: Number(e.target.value) })}
                  className="mt-2"
                />
                <p className="text-sm text-muted-foreground mt-1">
                  Se mostrará una alerta cuando un producto tenga menos de esta cantidad en stock
                </p>
              </div>

              <Separator />

              {/* Escenario 1 y 3: Configurar intentos de login */}
              <div>
                <Label htmlFor="maxAttempts">Intentos máximos de inicio de sesión</Label>
                <Input
                  id="maxAttempts"
                  type="number"
                  min="1"
                  max="10"
                  value={config.maxLoginAttempts}
                  onChange={(e) => setConfig({ ...config, maxLoginAttempts: Number(e.target.value) })}
                  className="mt-2"
                />
                <p className="text-sm text-muted-foreground mt-1">
                  Número de intentos fallidos antes de bloquear la cuenta
                </p>
              </div>

              <div>
                <Label htmlFor="blockDuration">Duración del bloqueo (minutos)</Label>
                <Input
                  id="blockDuration"
                  type="number"
                  min="1"
                  value={config.blockDurationMinutes}
                  onChange={(e) => setConfig({ ...config, blockDurationMinutes: Number(e.target.value) })}
                  className="mt-2"
                />
                <p className="text-sm text-muted-foreground mt-1">
                  Tiempo que permanecerá bloqueada una cuenta después de superar los intentos máximos
                </p>
              </div>

              <Separator />

              <Button onClick={handleUpdateConfig} className="w-full" size="lg">
                <CheckCircle2 className="h-4 w-4 mr-2" />
                Guardar configuración
              </Button>

              <Alert className="bg-blue-50 border-blue-200">
                <AlertDescription className="text-sm text-blue-700">
                  Los cambios se aplicarán inmediatamente en todo el sistema
                </AlertDescription>
              </Alert>
            </CardContent>
          </Card>

          {/* Vista de productos con bajo stock */}
          <Card>
            <CardHeader>
              <CardTitle>Alertas de stock</CardTitle>
            </CardHeader>
            <CardContent>
              {products.filter(p => p.stock <= systemConfig.minStock).length > 0 ? (
                <div className="space-y-3">
                  {products
                    .filter(p => p.stock <= systemConfig.minStock)
                    .map(product => (
                      <Alert 
                        key={product.id}
                        variant={product.stock === 0 ? 'destructive' : 'default'}
                        className={product.stock === 0 ? '' : 'bg-yellow-50 border-yellow-200'}
                      >
                        <AlertCircle className="h-4 w-4" />
                        <AlertDescription>
                          <strong>{product.name}</strong> - Stock: {product.stock} unidades
                          {product.stock === 0 && ' (SIN STOCK)'}
                        </AlertDescription>
                      </Alert>
                    ))}
                </div>
              ) : (
                <Alert className="bg-green-50 border-green-200">
                  <CheckCircle2 className="h-4 w-4 text-green-700" />
                  <AlertDescription className="text-green-700">
                    Todos los productos tienen stock suficiente
                  </AlertDescription>
                </Alert>
              )}
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}