import React, { useState } from 'react';
import { useApp } from '../context/AppContext';
import { ProductCard } from '../components/ProductCard';
import { Input } from '../components/ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../components/ui/select';
import { Alert, AlertDescription } from '../components/ui/alert';
import { Badge } from '../components/ui/badge';
import { Package, Search, Filter, FolderOpen } from 'lucide-react';
import { Product } from '../data/mockData';
import { toast } from 'sonner';

// HU-CAT-001: Ver catálogo con todos los escenarios + integración categorías
export default function Catalog() {
  const { products, categories, addToCart } = useApp();
  const [searchTerm, setSearchTerm] = useState('');
  const [categoryFilter, setCategoryFilter] = useState<string>('all');

  // Usar categorías del contexto (activas + con productos)
  const activeCategories = categories.filter(c => c.isActive);

  // Filtrar solo productos activos
  const activeProducts = products.filter(p => p.isActive !== false);

  // Filtrar productos
  const filteredProducts = activeProducts.filter(product => {
    const matchesSearch = product.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         product.description.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesCategory = categoryFilter === 'all' || product.category === categoryFilter;
    return matchesSearch && matchesCategory;
  });

  const handleAddToCart = (product: Product) => {
    const result = addToCart(product, 1);
    if (result.success) {
      toast.success(result.message);
    } else {
      toast.error(result.message);
    }
  };

  // Obtener nombre de categoría seleccionada para mostrar badge
  const selectedCategoryName = categoryFilter !== 'all'
    ? categories.find(c => c.name === categoryFilter)?.name
    : null;

  return (
    <div className="space-y-6">
      {/* Hero Section */}
      <div className="bg-gradient-to-r from-pink-500 to-purple-500 rounded-2xl p-8 md:p-12 text-white">
        <h1 className="text-3xl md:text-4xl font-bold mb-2">
          🎈 Happy Times Balloons
        </h1>
        <p className="text-lg md:text-xl opacity-90">
          Decoraciones mágicas para tus momentos especiales
        </p>
        <div className="flex flex-wrap gap-2 mt-4">
          {activeCategories.slice(0, 4).map(cat => (
            <button
              key={cat.id}
              onClick={() => setCategoryFilter(cat.name === categoryFilter ? 'all' : cat.name)}
              className={`px-3 py-1 rounded-full text-sm transition ${
                categoryFilter === cat.name
                  ? 'bg-white text-pink-600 font-semibold'
                  : 'bg-white/20 hover:bg-white/30'
              }`}
            >
              {cat.name}
            </button>
          ))}
        </div>
      </div>

      {/* Filtros */}
      <div className="flex flex-col md:flex-row gap-4">
        <div className="flex-1 relative">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            placeholder="Buscar productos..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="pl-10"
          />
        </div>
        <div className="md:w-64">
          <Select value={categoryFilter} onValueChange={setCategoryFilter}>
            <SelectTrigger>
              <Filter className="h-4 w-4 mr-2" />
              <SelectValue placeholder="Categoría" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">
                <div className="flex items-center gap-2">
                  <FolderOpen className="h-4 w-4" />
                  Todas las categorías
                </div>
              </SelectItem>
              {activeCategories.map(category => (
                <SelectItem key={category.id} value={category.name}>
                  {category.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      </div>

      {/* Filtro activo */}
      {selectedCategoryName && (
        <div className="flex items-center gap-2">
          <span className="text-sm text-muted-foreground">Filtrando por:</span>
          <Badge variant="secondary" className="gap-1">
            {selectedCategoryName}
            <button
              className="ml-1 hover:text-destructive"
              onClick={() => setCategoryFilter('all')}
            >
              ×
            </button>
          </Badge>
          <span className="text-sm text-muted-foreground">
            ({filteredProducts.length} producto{filteredProducts.length !== 1 ? 's' : ''})
          </span>
        </div>
      )}

      {/* Escenario 1: Lista de productos */}
      {filteredProducts.length > 0 && (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
          {filteredProducts.map(product => (
            <ProductCard
              key={product.id}
              product={product}
              onAddToCart={handleAddToCart}
            />
          ))}
        </div>
      )}

      {/* Escenario 3: Sin productos */}
      {filteredProducts.length === 0 && (
        <div className="flex flex-col items-center justify-center py-16 text-center">
          <Package className="h-16 w-16 text-muted-foreground mb-4" />
          <h3 className="text-xl font-semibold mb-2">
            No se encontraron productos
          </h3>
          <p className="text-muted-foreground mb-4">
            {searchTerm || categoryFilter !== 'all'
              ? 'Intenta con otros filtros o términos de búsqueda'
              : 'Aún no hay productos disponibles en el catálogo'
            }
          </p>
          {(searchTerm || categoryFilter !== 'all') && (
            <button
              onClick={() => {
                setSearchTerm('');
                setCategoryFilter('all');
              }}
              className="text-pink-600 hover:underline"
            >
              Limpiar filtros
            </button>
          )}
        </div>
      )}

      {/* Información de stock bajo */}
      {activeProducts.some(p => p.stock > 0 && p.stock <= 5) && (
        <Alert className="bg-yellow-50 border-yellow-200">
          <AlertDescription>
            ⚡ Algunos productos tienen stock limitado. ¡Aprovecha antes de que se agoten!
          </AlertDescription>
        </Alert>
      )}
    </div>
  );
}