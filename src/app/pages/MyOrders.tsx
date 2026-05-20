import React from 'react';
import { useNavigate } from 'react-router';
import { useApp } from '../context/AppContext';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { Button } from '../components/ui/button';
import { Badge } from '../components/ui/badge';
import { Alert, AlertDescription } from '../components/ui/alert';
import { 
  Package, 
  ShoppingBag,
  CheckCircle2,
  Clock,
  Truck,
  PackageCheck,
  MapPin,
  CreditCard
} from 'lucide-react';

// HU-PED-003: Ver estados del pedido (Cliente)
export default function MyOrders() {
  const navigate = useNavigate();
  const { currentUser, orders, getOrdersByUser } = useApp();

  // Verificar autenticación
  React.useEffect(() => {
    if (!currentUser) {
      navigate('/login');
    }
  }, [currentUser, navigate]);

  if (!currentUser) {
    return null;
  }

  const userOrders = getOrdersByUser(currentUser.id);
  const sortedOrders = [...userOrders].sort((a, b) => 
    new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
  );

  const formatPrice = (price: number) => {
    return `₡${price.toLocaleString('es-CR')}`;
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString('es-CR', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const getStatusInfo = (status: string) => {
    const statusMap = {
      pendiente: {
        icon: Clock,
        label: 'Pendiente',
        description: 'Tu pedido está siendo procesado',
        color: 'text-yellow-600',
        bgColor: 'bg-yellow-50',
        borderColor: 'border-yellow-200'
      },
      pagado: {
        icon: CheckCircle2,
        label: 'Pagado',
        description: 'El pago ha sido confirmado',
        color: 'text-blue-600',
        bgColor: 'bg-blue-50',
        borderColor: 'border-blue-200'
      },
      confirmado: {
        icon: CheckCircle2,
        label: 'Confirmado',
        description: 'Tu pedido ha sido confirmado',
        color: 'text-green-600',
        bgColor: 'bg-green-50',
        borderColor: 'border-green-200'
      },
      enviado: {
        icon: Truck,
        label: 'En camino',
        description: 'Tu pedido está en camino',
        color: 'text-purple-600',
        bgColor: 'bg-purple-50',
        borderColor: 'border-purple-200'
      },
      entregado: {
        icon: PackageCheck,
        label: 'Entregado',
        description: 'Tu pedido ha sido entregado',
        color: 'text-green-600',
        bgColor: 'bg-green-50',
        borderColor: 'border-green-200'
      }
    };

    return statusMap[status as keyof typeof statusMap] || statusMap.pendiente;
  };

  return (
    <div className="space-y-6 max-w-5xl mx-auto">
      {/* Header */}
      <div>
        <h1 className="text-3xl font-bold">Mis Pedidos</h1>
        <p className="text-muted-foreground">Revisa el estado de tus pedidos</p>
      </div>

      {/* Lista de pedidos */}
      {sortedOrders.length === 0 ? (
        <Card>
          <CardContent className="py-16 text-center">
            <ShoppingBag className="h-16 w-16 text-muted-foreground mx-auto mb-4" />
            <h3 className="text-xl font-semibold mb-2">No tienes pedidos</h3>
            <p className="text-muted-foreground mb-6">
              Comienza a comprar en nuestro catálogo
            </p>
            <Button onClick={() => navigate('/')}>
              Ver catálogo
            </Button>
          </CardContent>
        </Card>
      ) : (
        <div className="space-y-4">
          {sortedOrders.map((order) => {
            const statusInfo = getStatusInfo(order.status);
            const StatusIcon = statusInfo.icon;

            return (
              <Card key={order.id} className="overflow-hidden">
                <CardContent className="p-0">
                  {/* Header del pedido */}
                  <div className={`p-4 ${statusInfo.bgColor} border-b ${statusInfo.borderColor}`}>
                    <div className="flex justify-between items-start">
                      <div>
                        <p className="text-sm text-muted-foreground">Pedido</p>
                        <p className="font-mono font-semibold">{order.id}</p>
                        <p className="text-sm text-muted-foreground mt-1">
                          {formatDate(order.createdAt)}
                        </p>
                      </div>
                      <div className="text-right">
                        <div className="flex items-center gap-2 justify-end mb-1">
                          <StatusIcon className={`h-5 w-5 ${statusInfo.color}`} />
                          <Badge variant="outline" className={statusInfo.color}>
                            {statusInfo.label}
                          </Badge>
                        </div>
                        <p className={`text-sm ${statusInfo.color}`}>
                          {statusInfo.description}
                        </p>
                      </div>
                    </div>
                  </div>

                  {/* Detalles del pedido */}
                  <div className="p-6 space-y-4">
                    {/* Productos */}
                    <div>
                      <h4 className="font-semibold mb-3 flex items-center gap-2">
                        <Package className="h-4 w-4" />
                        Productos ({order.items.length})
                      </h4>
                      <div className="space-y-2">
                        {order.items.map((item, index) => (
                          <div key={index} className="flex justify-between items-start p-3 bg-gray-50 rounded-lg">
                            <div className="flex-1">
                              <p className="font-medium">{item.product.name}</p>
                              <p className="text-sm text-muted-foreground">
                                Cantidad: {item.quantity}
                              </p>
                            </div>
                            <p className="font-semibold">
                              {formatPrice(item.product.price * item.quantity)}
                            </p>
                          </div>
                        ))}
                      </div>
                    </div>

                    {/* Información de entrega y pago */}
                    <div className="grid md:grid-cols-2 gap-4 pt-4 border-t">
                      <div>
                        <h4 className="font-semibold mb-2 flex items-center gap-2">
                          <MapPin className="h-4 w-4" />
                          Entrega
                        </h4>
                        <p className="text-sm text-muted-foreground">
                          Zona: {order.deliveryZone}
                        </p>
                        {order.deliveryAddress && (
                          <p className="text-sm text-muted-foreground">
                            {order.deliveryAddress}
                          </p>
                        )}
                        <p className="text-sm font-medium mt-1">
                          Costo: {formatPrice(order.deliveryCost || 0)}
                        </p>
                      </div>

                      <div>
                        <h4 className="font-semibold mb-2 flex items-center gap-2">
                          <CreditCard className="h-4 w-4" />
                          Pago
                        </h4>
                        <p className="text-sm text-muted-foreground">
                          Método: {order.paymentMethod === 'sinpe' ? 'SINPE Móvil' : 'Tarjeta'}
                        </p>
                      </div>
                    </div>

                    {/* Total */}
                    <div className="pt-4 border-t">
                      <div className="flex justify-between items-center">
                        <span className="text-lg font-semibold">Total pagado:</span>
                        <span className="text-2xl font-bold text-pink-600">
                          {formatPrice(order.total)}
                        </span>
                      </div>
                    </div>

                    {/* Estado de seguimiento (HU-PED-003) */}
                    <div className="pt-4 border-t">
                      <h4 className="font-semibold mb-3">Seguimiento del pedido</h4>
                      <div className="flex items-center justify-between">
                        {['pendiente', 'pagado', 'confirmado', 'enviado', 'entregado'].map((status, index) => {
                          const info = getStatusInfo(status);
                          const Icon = info.icon;
                          const isCompleted = ['pendiente', 'pagado', 'confirmado', 'enviado', 'entregado']
                            .indexOf(order.status) >= index;
                          
                          return (
                            <React.Fragment key={status}>
                              <div className="flex flex-col items-center">
                                <div className={`
                                  h-10 w-10 rounded-full flex items-center justify-center
                                  ${isCompleted 
                                    ? 'bg-green-100 text-green-600' 
                                    : 'bg-gray-100 text-gray-400'
                                  }
                                `}>
                                  <Icon className="h-5 w-5" />
                                </div>
                                <span className={`text-xs mt-1 text-center ${
                                  isCompleted ? 'font-medium' : 'text-muted-foreground'
                                }`}>
                                  {info.label}
                                </span>
                              </div>
                              {index < 4 && (
                                <div className={`flex-1 h-0.5 ${
                                  isCompleted ? 'bg-green-600' : 'bg-gray-200'
                                }`} />
                              )}
                            </React.Fragment>
                          );
                        })}
                      </div>
                    </div>
                  </div>
                </CardContent>
              </Card>
            );
          })}
        </div>
      )}
    </div>
  );
}
