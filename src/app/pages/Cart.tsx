import React from 'react';
import { useNavigate } from 'react-router';
import { useApp } from '../context/AppContext';
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from '../components/ui/card';
import { Button } from '../components/ui/button';
import { Alert, AlertDescription } from '../components/ui/alert';
import { ImageWithFallback } from '../components/figma/ImageWithFallback';
import { ShoppingCart, Trash2, Plus, Minus, ArrowRight, ShoppingBag } from 'lucide-react';
import { Separator } from '../components/ui/separator';
import { Product } from '../data/mockData';

// Helper para obtener imagen del producto (HU-IMG integración)
const getProductImageUrl = (product: Product): string => {
  if (product.images && product.images.length > 0) {
    const primary = product.images.find(img => img.isPrimary) || product.images[0];
    return primary.url;
  }
  return `https://source.unsplash.com/200x200/?${product.image},balloons`;
};

// HU-PED-001: Vista del carrito
export default function Cart() {
  const { cart, removeFromCart, updateCartQuantity } = useApp();
  const navigate = useNavigate();

  const formatPrice = (price: number) => {
    return `₡${price.toLocaleString('es-CR')}`;
  };

  const subtotal = cart.reduce((sum, item) => {
    const price = item.product.discountPrice || item.product.price;
    return sum + (price * item.quantity);
  }, 0);

  const handleQuantityChange = (productId: string, newQuantity: number) => {
    updateCartQuantity(productId, newQuantity);
  };

  if (cart.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-16 text-center">
        <ShoppingCart className="h-24 w-24 text-muted-foreground mb-6" />
        <h2 className="text-2xl font-bold mb-2">Tu carrito está vacío</h2>
        <p className="text-muted-foreground mb-6">
          Agrega productos para comenzar tu pedido
        </p>
        <Button onClick={() => navigate('/')} size="lg">
          <ShoppingBag className="mr-2 h-5 w-5" />
          Ver catálogo
        </Button>
      </div>
    );
  }

  return (
    <div className="max-w-6xl mx-auto space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold">Carrito de compras</h1>
        <Button variant="ghost" onClick={() => navigate('/')}>
          Seguir comprando
        </Button>
      </div>

      <div className="grid lg:grid-cols-3 gap-6">
        {/* Lista de productos */}
        <div className="lg:col-span-2 space-y-4">
          {cart.map((item) => {
            const price = item.product.discountPrice || item.product.price;
            const itemTotal = price * item.quantity;
            const isMaxQuantity = item.quantity >= item.product.stock;

            return (
              <Card key={item.product.id}>
                <CardContent className="p-4">
                  <div className="flex gap-4">
                    {/* Imagen */}
                    <div
                      className="w-24 h-24 rounded-lg overflow-hidden bg-gray-100 flex-shrink-0 cursor-pointer"
                      onClick={() => navigate(`/product/${item.product.id}`)}
                    >
                      {item.product.images && item.product.images.length > 0 ? (
                        <img
                          src={getProductImageUrl(item.product)}
                          alt={item.product.name}
                          className="w-full h-full object-cover"
                        />
                      ) : (
                        <ImageWithFallback
                          src={`https://source.unsplash.com/200x200/?${item.product.image},balloons`}
                          alt={item.product.name}
                          className="w-full h-full object-cover"
                        />
                      )}
                    </div>

                    {/* Información */}
                    <div className="flex-1 min-w-0">
                      <div className="flex justify-between gap-2 mb-2">
                        <h3
                          className="font-semibold line-clamp-1 cursor-pointer hover:text-pink-600"
                          onClick={() => navigate(`/product/${item.product.id}`)}
                        >
                          {item.product.name}
                        </h3>
                        <Button
                          variant="ghost"
                          size="icon"
                          className="h-8 w-8 text-red-500 hover:text-red-700 hover:bg-red-50 flex-shrink-0"
                          onClick={() => removeFromCart(item.product.id)}
                        >
                          <Trash2 className="h-4 w-4" />
                        </Button>
                      </div>

                      <p className="text-sm text-muted-foreground mb-3">
                        {item.product.category}
                      </p>

                      <div className="flex items-center justify-between">
                        {/* Selector de cantidad */}
                        <div className="flex items-center border rounded-lg">
                          <Button
                            variant="ghost"
                            size="icon"
                            className="h-8 w-8"
                            onClick={() => handleQuantityChange(item.product.id, item.quantity - 1)}
                            disabled={item.quantity <= 1}
                          >
                            <Minus className="h-3 w-3" />
                          </Button>
                          <span className="w-12 text-center text-sm font-semibold">
                            {item.quantity}
                          </span>
                          <Button
                            variant="ghost"
                            size="icon"
                            className="h-8 w-8"
                            onClick={() => handleQuantityChange(item.product.id, item.quantity + 1)}
                            disabled={isMaxQuantity}
                          >
                            <Plus className="h-3 w-3" />
                          </Button>
                        </div>

                        {/* Precio */}
                        <div className="text-right">
                          {item.product.discountPrice && (
                            <p className="text-xs line-through text-muted-foreground">
                              {formatPrice(item.product.price * item.quantity)}
                            </p>
                          )}
                          <p className="font-bold">
                            {formatPrice(itemTotal)}
                          </p>
                        </div>
                      </div>

                      {isMaxQuantity && (
                        <p className="text-xs text-yellow-600 mt-2">
                          Stock máximo alcanzado
                        </p>
                      )}
                    </div>
                  </div>
                </CardContent>
              </Card>
            );
          })}
        </div>

        {/* Resumen del pedido */}
        <div className="lg:col-span-1">
          <Card className="sticky top-24">
            <CardHeader>
              <CardTitle>Resumen del pedido</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2">
                <div className="flex justify-between text-sm">
                  <span className="text-muted-foreground">Subtotal</span>
                  <span className="font-medium">{formatPrice(subtotal)}</span>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-muted-foreground">
                    Productos ({cart.reduce((sum, item) => sum + item.quantity, 0)})
                  </span>
                </div>
              </div>

              <Separator />

              <div className="flex justify-between font-bold text-lg">
                <span>Total</span>
                <span className="text-pink-600">{formatPrice(subtotal)}</span>
              </div>

              <Alert className="bg-blue-50 border-blue-200">
                <AlertDescription className="text-sm text-blue-700">
                  El costo de envío se calculará en el siguiente paso
                </AlertDescription>
              </Alert>
            </CardContent>
            <CardFooter>
              <Button
                className="w-full"
                size="lg"
                onClick={() => navigate('/checkout')}
              >
                Continuar con el pedido
                <ArrowRight className="ml-2 h-5 w-5" />
              </Button>
            </CardFooter>
          </Card>
        </div>
      </div>
    </div>
  );
}