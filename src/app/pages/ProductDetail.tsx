import React, { useState } from 'react';
import { useParams, useNavigate } from 'react-router';
import { useApp } from '../context/AppContext';
import { Button } from '../components/ui/button';
import { Card, CardContent } from '../components/ui/card';
import { Badge } from '../components/ui/badge';
import { Alert, AlertDescription } from '../components/ui/alert';
import { ImageWithFallback } from '../components/figma/ImageWithFallback';
import { ShoppingCart, Minus, Plus, ArrowLeft, Tag, Package, CheckCircle2, AlertCircle, Star, ChevronLeft, ChevronRight } from 'lucide-react';
import { toast } from 'sonner';

// HU-CAT-001 Escenario 2: Detalle del producto + HU-IMG: Galería de imágenes
export default function ProductDetail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { getProductById, addToCart } = useApp();
  const [quantity, setQuantity] = useState(1);
  const [selectedImageIndex, setSelectedImageIndex] = useState(0);

  const product = getProductById(id || '');

  if (!product) {
    return (
      <div className="flex flex-col items-center justify-center py-16">
        <Package className="h-16 w-16 text-muted-foreground mb-4" />
        <h2 className="text-2xl font-bold mb-2">Producto no encontrado</h2>
        <p className="text-muted-foreground mb-6">
          El producto que buscas no existe o fue eliminado
        </p>
        <Button onClick={() => navigate('/')}>
          Volver al catálogo
        </Button>
      </div>
    );
  }

  const formatPrice = (price: number) => `₡${price.toLocaleString('es-CR')}`;

  const isOutOfStock = product.stock === 0;
  const isLowStock = product.stock > 0 && product.stock <= 5;

  // HU-IMG: Manejo de galería de imágenes
  const hasGallery = product.images && product.images.length > 0;
  const sortedImages = hasGallery
    ? [...product.images!].sort((a, b) => a.order - b.order)
    : [];
  const primaryImageIndex = hasGallery
    ? Math.max(0, sortedImages.findIndex(img => img.isPrimary))
    : 0;

  // Inicializar con imagen principal
  const [activeIndex, setActiveIndex] = useState(primaryImageIndex);

  const handlePrevImage = () => {
    setActiveIndex(prev => (prev === 0 ? sortedImages.length - 1 : prev - 1));
  };

  const handleNextImage = () => {
    setActiveIndex(prev => (prev === sortedImages.length - 1 ? 0 : prev + 1));
  };

  const handleQuantityChange = (delta: number) => {
    const newQuantity = quantity + delta;
    if (newQuantity >= 1 && newQuantity <= product.stock) {
      setQuantity(newQuantity);
    }
  };

  const handleAddToCart = () => {
    const result = addToCart(product, quantity);
    if (result.success) {
      toast.success(result.message);
      setQuantity(1);
    } else {
      toast.error(result.message);
    }
  };

  const totalPrice = (product.discountPrice || product.price) * quantity;

  return (
    <div className="max-w-6xl mx-auto space-y-6">
      {/* Breadcrumb */}
      <Button variant="ghost" onClick={() => navigate('/')} className="gap-2">
        <ArrowLeft className="h-4 w-4" />
        Volver al catálogo
      </Button>

      <div className="grid md:grid-cols-2 gap-8">
        {/* ── HU-IMG-002: Sección de imágenes ── */}
        <div className="space-y-3">
          {/* Imagen principal */}
          <div className="relative aspect-square rounded-2xl overflow-hidden bg-gray-100">
            {hasGallery ? (
              <>
                <img
                  src={sortedImages[activeIndex].url}
                  alt={`${product.name} - imagen ${activeIndex + 1}`}
                  className="w-full h-full object-cover"
                />
                {/* Navegación galería */}
                {sortedImages.length > 1 && (
                  <>
                    <button
                      onClick={handlePrevImage}
                      className="absolute left-2 top-1/2 -translate-y-1/2 bg-black/40 hover:bg-black/60 text-white rounded-full p-2 transition"
                    >
                      <ChevronLeft className="h-5 w-5" />
                    </button>
                    <button
                      onClick={handleNextImage}
                      className="absolute right-2 top-1/2 -translate-y-1/2 bg-black/40 hover:bg-black/60 text-white rounded-full p-2 transition"
                    >
                      <ChevronRight className="h-5 w-5" />
                    </button>
                    <div className="absolute bottom-3 left-1/2 -translate-x-1/2 flex gap-1.5">
                      {sortedImages.map((_, idx) => (
                        <button
                          key={idx}
                          onClick={() => setActiveIndex(idx)}
                          className={`w-2 h-2 rounded-full transition ${
                            idx === activeIndex ? 'bg-white' : 'bg-white/50'
                          }`}
                        />
                      ))}
                    </div>
                  </>
                )}
              </>
            ) : (
              <ImageWithFallback
                src={`https://source.unsplash.com/800x800/?${product.image},balloons,party`}
                alt={product.name}
                className="w-full h-full object-cover"
              />
            )}

            {/* Badges en la imagen */}
            <div className="absolute top-4 left-4 flex flex-col gap-2">
              {product.hasPromotion && (
                <Badge className="bg-red-500 text-white text-base">
                  <Tag className="h-4 w-4 mr-1" />
                  ¡Promoción!
                </Badge>
              )}
            </div>
          </div>

          {/* HU-IMG-002: Miniaturas de galería */}
          {hasGallery && sortedImages.length > 1 && (
            <div className="flex gap-2 overflow-x-auto pb-1">
              {sortedImages.map((img, idx) => (
                <button
                  key={img.id}
                  onClick={() => setActiveIndex(idx)}
                  className={`relative w-16 h-16 rounded-lg overflow-hidden flex-shrink-0 border-2 transition ${
                    idx === activeIndex ? 'border-pink-500' : 'border-transparent hover:border-gray-300'
                  }`}
                >
                  <img src={img.url} alt={`Miniatura ${idx + 1}`} className="w-full h-full object-cover" />
                  {/* HU-IMG-003: Indicador imagen principal */}
                  {img.isPrimary && (
                    <div className="absolute top-0.5 right-0.5 bg-yellow-400 rounded-full p-0.5">
                      <Star className="h-2.5 w-2.5 text-white fill-current" />
                    </div>
                  )}
                </button>
              ))}
            </div>
          )}
        </div>

        {/* Información del producto */}
        <div className="space-y-6">
          <div>
            <Badge variant="outline" className="mb-3">
              {product.category}
            </Badge>
            <h1 className="text-3xl md:text-4xl font-bold mb-3">
              {product.name}
            </h1>
            <p className="text-lg text-muted-foreground">
              {product.description}
            </p>
          </div>

          {/* Precio */}
          <Card>
            <CardContent className="pt-6">
              <div className="flex items-baseline gap-3 mb-4">
                {product.discountPrice ? (
                  <>
                    <span className="text-3xl font-bold text-pink-600">
                      {formatPrice(product.discountPrice)}
                    </span>
                    <span className="text-xl line-through text-muted-foreground">
                      {formatPrice(product.price)}
                    </span>
                    <Badge variant="destructive">
                      {Math.round((1 - product.discountPrice / product.price) * 100)}% OFF
                    </Badge>
                  </>
                ) : (
                  <span className="text-3xl font-bold">
                    {formatPrice(product.price)}
                  </span>
                )}
              </div>

              {product.hasPromotion && product.promotionEndDate && (
                <p className="text-sm text-muted-foreground">
                  🎉 Promoción válida hasta: {new Date(product.promotionEndDate).toLocaleDateString('es-CR')}
                </p>
              )}
            </CardContent>
          </Card>

          {/* Stock */}
          {isOutOfStock ? (
            <Alert variant="destructive">
              <AlertCircle className="h-4 w-4" />
              <AlertDescription>Producto sin stock disponible</AlertDescription>
            </Alert>
          ) : isLowStock ? (
            <Alert className="bg-yellow-50 border-yellow-200">
              <Package className="h-4 w-4 text-yellow-700" />
              <AlertDescription className="text-yellow-700">
                ¡Últimas {product.stock} unidades disponibles!
              </AlertDescription>
            </Alert>
          ) : (
            <Alert className="bg-green-50 border-green-200">
              <CheckCircle2 className="h-4 w-4 text-green-700" />
              <AlertDescription className="text-green-700">
                Disponible en stock ({product.stock} unidades)
              </AlertDescription>
            </Alert>
          )}

          {/* Selector de cantidad */}
          {!isOutOfStock && (
            <div className="space-y-3">
              <label className="text-sm font-medium">Cantidad</label>
              <div className="flex items-center gap-4">
                <div className="flex items-center border rounded-lg">
                  <Button variant="ghost" size="icon" onClick={() => handleQuantityChange(-1)} disabled={quantity <= 1}>
                    <Minus className="h-4 w-4" />
                  </Button>
                  <span className="w-12 text-center font-semibold">{quantity}</span>
                  <Button variant="ghost" size="icon" onClick={() => handleQuantityChange(1)} disabled={quantity >= product.stock}>
                    <Plus className="h-4 w-4" />
                  </Button>
                </div>
                <div className="text-sm text-muted-foreground">
                  Subtotal: <span className="font-bold text-foreground">{formatPrice(totalPrice)}</span>
                </div>
              </div>
            </div>
          )}

          {/* Botón agregar al carrito */}
          <div className="flex gap-3">
            <Button className="flex-1" size="lg" disabled={isOutOfStock} onClick={handleAddToCart}>
              <ShoppingCart className="h-5 w-5 mr-2" />
              {isOutOfStock ? 'Sin stock' : 'Agregar al carrito'}
            </Button>
          </div>

          {/* Info adicional */}
          <Card className="bg-blue-50">
            <CardContent className="pt-6 space-y-2 text-sm">
              <p>✓ Entrega a domicilio disponible</p>
              <p>✓ Instalación incluida (según zona)</p>
              <p>✓ Globos de alta calidad</p>
              <p>✓ Colores personalizables</p>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}