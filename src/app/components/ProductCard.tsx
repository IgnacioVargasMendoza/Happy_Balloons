import React from 'react';
import { Product } from '../data/mockData';
import { Card, CardContent, CardFooter } from './ui/card';
import { Button } from './ui/button';
import { Badge } from './ui/badge';
import { ShoppingCart, Tag, Image as ImageIcon } from 'lucide-react';
import { useNavigate } from 'react-router';
import { ImageWithFallback } from './figma/ImageWithFallback';

interface ProductCardProps {
  product: Product;
  onAddToCart?: (product: Product) => void;
}

// HU-IMG integración: Helper para obtener URL de imagen principal
const getProductImageUrl = (product: Product, size = '400x400'): string | null => {
  if (product.images && product.images.length > 0) {
    const primary = product.images.find(img => img.isPrimary) || product.images[0];
    return primary.url;
  }
  return null;
};

export function ProductCard({ product, onAddToCart }: ProductCardProps) {
  const navigate = useNavigate();

  const formatPrice = (price: number) => {
    return `₡${price.toLocaleString('es-CR')}`;
  };

  const isLowStock = product.stock > 0 && product.stock <= 5;
  const isOutOfStock = product.stock === 0;
  const primaryImageUrl = getProductImageUrl(product);
  const hasCustomImage = primaryImageUrl !== null;

  return (
    <Card className="overflow-hidden hover:shadow-lg transition-shadow cursor-pointer group">
      <div
        onClick={() => navigate(`/product/${product.id}`)}
        className="relative aspect-square overflow-hidden bg-gray-100"
      >
        {hasCustomImage ? (
          <img
            src={primaryImageUrl!}
            alt={product.name}
            className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
          />
        ) : (
          <ImageWithFallback
            src={`https://source.unsplash.com/400x400/?${product.image},balloons,party`}
            alt={product.name}
            className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
          />
        )}

        {/* Badges */}
        <div className="absolute top-2 left-2 flex flex-col gap-1">
          {product.hasPromotion && (
            <Badge className="bg-red-500 text-white">
              <Tag className="h-3 w-3 mr-1" />
              Promoción
            </Badge>
          )}
          {isOutOfStock && (
            <Badge variant="destructive">Sin stock</Badge>
          )}
          {isLowStock && (
            <Badge variant="outline" className="bg-yellow-50 text-yellow-700 border-yellow-300">
              Últimas {product.stock} unidades
            </Badge>
          )}
        </div>

        {/* Indicador de galería múltiple */}
        {product.images && product.images.length > 1 && (
          <div className="absolute bottom-2 right-2 bg-black/60 text-white text-xs px-2 py-0.5 rounded-full flex items-center gap-1">
            <ImageIcon className="h-3 w-3" />
            {product.images.length}
          </div>
        )}
      </div>

      <CardContent className="pt-4" onClick={() => navigate(`/product/${product.id}`)}>
        <h3 className="font-semibold text-base mb-1 line-clamp-1">
          {product.name}
        </h3>
        <p className="text-sm text-muted-foreground mb-2 line-clamp-2">
          {product.description}
        </p>

        <div className="flex items-center gap-2">
          {product.discountPrice ? (
            <>
              <span className="text-lg font-bold text-pink-600">
                {formatPrice(product.discountPrice)}
              </span>
              <span className="text-sm line-through text-muted-foreground">
                {formatPrice(product.price)}
              </span>
            </>
          ) : (
            <span className="text-lg font-bold">
              {formatPrice(product.price)}
            </span>
          )}
        </div>

        <p className="text-xs text-muted-foreground mt-1">
          Stock: {product.stock} unidades
        </p>
      </CardContent>

      <CardFooter>
        <Button
          className="w-full"
          disabled={isOutOfStock}
          onClick={(e) => {
            e.stopPropagation();
            if (onAddToCart) {
              onAddToCart(product);
            }
          }}
        >
          <ShoppingCart className="h-4 w-4 mr-2" />
          {isOutOfStock ? 'Sin stock' : 'Agregar al carrito'}
        </Button>
      </CardFooter>
    </Card>
  );
}