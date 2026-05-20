import React, { useState } from 'react';
import { useNavigate } from 'react-router';
import { useApp } from '../context/AppContext';
import { Product, ProductImage } from '../data/mockData';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { Button } from '../components/ui/button';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import { Textarea } from '../components/ui/textarea';
import { Alert, AlertDescription } from '../components/ui/alert';
import { Badge } from '../components/ui/badge';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '../components/ui/dialog';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../components/ui/table';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../components/ui/select';
import { Separator } from '../components/ui/separator';
import { ImageManager } from '../components/ImageManager';
import {
  Plus,
  Search,
  Edit,
  Eye,
  Power,
  AlertCircle,
  CheckCircle2,
  Package,
  Tag,
  Image as ImageIcon,
  AlertTriangle
} from 'lucide-react';
import { toast } from 'sonner';

// Helpers para imágenes de productos
const getProductDisplayUrl = (product: Product): string => {
  if (product.images && product.images.length > 0) {
    const primary = product.images.find(img => img.isPrimary) || product.images[0];
    return primary.url;
  }
  return `https://source.unsplash.com/80x80/?${product.image},balloons`;
};

// HU-PRO-001, HU-PRO-002, HU-PRO-003, HU-PRO-004 + HU-IMG + HU-CAT integración
export default function ProductManagement() {
  const navigate = useNavigate();
  const {
    currentUser,
    products,
    categories,
    createProduct,
    updateProduct,
    toggleProductStatus
  } = useApp();

  // Estado para crear/editar producto
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false);
  const [isEditDialogOpen, setIsEditDialogOpen] = useState(false);
  const [isDetailDialogOpen, setIsDetailDialogOpen] = useState(false);
  const [isToggleDialogOpen, setIsToggleDialogOpen] = useState(false);

  const [selectedProduct, setSelectedProduct] = useState<Product | null>(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [categoryFilter, setCategoryFilter] = useState('all');

  const emptyForm = {
    name: '',
    description: '',
    price: '',
    stock: '',
    category: '',
    images: [] as ProductImage[]
  };

  const [formData, setFormData] = useState(emptyForm);
  const [formErrors, setFormErrors] = useState<Record<string, string>>({});

  // Verificar permisos
  React.useEffect(() => {
    if (!currentUser || (currentUser.role !== 'admin' && currentUser.role !== 'staff')) {
      toast.error('No tienes permisos para acceder a esta sección');
      navigate('/');
    }
  }, [currentUser, navigate]);

  if (!currentUser || (currentUser.role !== 'admin' && currentUser.role !== 'staff')) {
    return null;
  }

  // Categorías activas para el Select
  const activeCategories = categories.filter(c => c.isActive);

  // HU-PRO-002: Filtrar productos
  const filteredProducts = products.filter(product => {
    const matchesSearch =
      product.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      product.category.toLowerCase().includes(searchQuery.toLowerCase());
    const matchesCategory = categoryFilter === 'all' || product.category === categoryFilter;
    return matchesSearch && matchesCategory;
  });

  // Validar formulario
  const validateForm = (): boolean => {
    const errors: Record<string, string> = {};

    if (!formData.name.trim()) errors.name = 'El nombre es requerido';
    if (!formData.description.trim()) errors.description = 'La descripción es requerida';
    if (!formData.price || Number(formData.price) <= 0) errors.price = 'El precio debe ser mayor a 0';
    if (!formData.stock || Number(formData.stock) < 0) errors.stock = 'El stock no puede ser negativo';
    if (!formData.category) errors.category = 'Selecciona una categoría';

    setFormErrors(errors);
    return Object.keys(errors).length === 0;
  };

  // HU-PRO-001: Crear producto (con imágenes)
  const handleCreateProduct = () => {
    if (!validateForm()) {
      toast.error('Por favor corrige los errores en el formulario');
      return;
    }

    try {
      const result = createProduct({
        name: formData.name,
        description: formData.description,
        price: Number(formData.price),
        stock: Number(formData.stock),
        category: formData.category,
        image: formData.images.length > 0 ? 'custom' : 'product-placeholder',
        images: formData.images.length > 0 ? formData.images : undefined,
        isActive: true
      });

      if (result.success) {
        toast.success(result.message);
        setIsCreateDialogOpen(false);
        resetForm();
      } else {
        toast.error(result.message);
      }
    } catch (error) {
      toast.error('Error al crear el producto');
    }
  };

  // HU-PRO-003: Actualizar producto (con imágenes)
  const handleUpdateProduct = () => {
    if (!selectedProduct || !validateForm()) {
      toast.error('Por favor corrige los errores en el formulario');
      return;
    }

    try {
      const result = updateProduct(selectedProduct.id, {
        name: formData.name,
        description: formData.description,
        price: Number(formData.price),
        stock: Number(formData.stock),
        category: formData.category,
        images: formData.images
      });

      if (result.success) {
        toast.success(result.message);
        setIsEditDialogOpen(false);
        setSelectedProduct(null);
        resetForm();
      } else {
        toast.error(result.message);
      }
    } catch (error) {
      toast.error('Error al actualizar el producto');
    }
  };

  // HU-PRO-004: Desactivar/Reactivar producto
  const handleToggleProductStatus = () => {
    if (!selectedProduct) return;

    try {
      const result = toggleProductStatus(selectedProduct.id);
      if (result.success) {
        const newStatus = !selectedProduct.isActive;
        toast.success(newStatus ? 'Producto activado exitosamente' : 'Producto desactivado exitosamente');
        setIsToggleDialogOpen(false);
        setSelectedProduct(null);
      } else {
        toast.error(result.message);
      }
    } catch (error) {
      toast.error('Error al cambiar el estado del producto');
    }
  };

  const handleViewDetail = (product: Product) => {
    setSelectedProduct(product);
    setIsDetailDialogOpen(true);
  };

  const handleEditClick = (product: Product) => {
    setSelectedProduct(product);
    setFormData({
      name: product.name,
      description: product.description,
      price: product.price.toString(),
      stock: product.stock.toString(),
      category: product.category,
      images: product.images || []
    });
    setFormErrors({});
    setIsEditDialogOpen(true);
  };

  const handleToggleClick = (product: Product) => {
    setSelectedProduct(product);
    setIsToggleDialogOpen(true);
  };

  const resetForm = () => {
    setFormData(emptyForm);
    setFormErrors({});
  };

  const handleCreateClick = () => {
    resetForm();
    setIsCreateDialogOpen(true);
  };

  const formatPrice = (price: number) => `₡${price.toLocaleString('es-CR')}`;

  // Detección de inconsistencias (FASE 4)
  const productsWithoutCategory = products.filter(p => !p.category || !categories.find(c => c.name === p.category));
  const productsWithoutImages = products.filter(p => !p.images || p.images.length === 0);

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Gestión de Productos</h1>
          <p className="text-muted-foreground">Administra el catálogo de productos con imágenes y categorías</p>
        </div>
        <Button onClick={handleCreateClick}>
          <Plus className="h-4 w-4 mr-2" />
          Crear producto
        </Button>
      </div>

      {/* Alertas de inconsistencia (FASE 4 – Errores a detectar) */}
      {productsWithoutImages.length > 0 && (
        <Alert className="bg-amber-50 border-amber-200">
          <ImageIcon className="h-4 w-4 text-amber-700" />
          <AlertDescription className="text-amber-700">
            <strong>{productsWithoutImages.length} producto(s) sin imagen.</strong>{' '}
            Edítalos para mejorar la experiencia visual del catálogo.
          </AlertDescription>
        </Alert>
      )}

      {/* Búsqueda y filtros */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-col sm:flex-row gap-4">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Buscar productos por nombre o categoría..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="pl-10"
              />
            </div>
            <Select value={categoryFilter} onValueChange={setCategoryFilter}>
              <SelectTrigger className="w-full sm:w-[200px]">
                <SelectValue placeholder="Filtrar categoría" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Todas las categorías</SelectItem>
                {categories.map(cat => (
                  <SelectItem key={cat.id} value={cat.name}>
                    {cat.name} {!cat.isActive && '(inactiva)'}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </CardContent>
      </Card>

      {/* Lista vacía */}
      {filteredProducts.length === 0 ? (
        <Card>
          <CardContent className="py-16 text-center">
            <Package className="h-16 w-16 text-muted-foreground mx-auto mb-4" />
            <h3 className="text-xl font-semibold mb-2">No hay productos</h3>
            <p className="text-muted-foreground mb-4">
              {searchQuery || categoryFilter !== 'all'
                ? 'No se encontraron productos con ese criterio de búsqueda'
                : 'Comienza creando tu primer producto'}
            </p>
            {!searchQuery && categoryFilter === 'all' && (
              <Button onClick={handleCreateClick}>
                <Plus className="h-4 w-4 mr-2" />
                Crear primer producto
              </Button>
            )}
          </CardContent>
        </Card>
      ) : (
        <Card>
          <CardHeader>
            <CardTitle>Productos ({filteredProducts.length})</CardTitle>
            <CardDescription>Lista completa de productos en el sistema</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="rounded-md border">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Imagen</TableHead>
                    <TableHead>Nombre</TableHead>
                    <TableHead>Categoría</TableHead>
                    <TableHead>Precio</TableHead>
                    <TableHead>Stock</TableHead>
                    <TableHead>Estado</TableHead>
                    <TableHead className="text-right">Acciones</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {filteredProducts.map((product) => {
                    const categoryObj = categories.find(c => c.name === product.category);
                    const categoryInactive = categoryObj && !categoryObj.isActive;
                    const hasImages = product.images && product.images.length > 0;

                    return (
                      <TableRow key={product.id}>
                        {/* HU-IMG: Miniatura de imagen principal */}
                        <TableCell>
                          <div className="w-12 h-12 rounded-md overflow-hidden bg-gray-100 flex items-center justify-center flex-shrink-0">
                            {hasImages ? (
                              <img
                                src={getProductDisplayUrl(product)}
                                alt={product.name}
                                className="w-full h-full object-cover"
                              />
                            ) : (
                              <ImageIcon className="h-5 w-5 text-gray-400" />
                            )}
                          </div>
                        </TableCell>
                        <TableCell className="font-medium">
                          <div>
                            <p>{product.name}</p>
                            {!hasImages && (
                              <p className="text-xs text-amber-600 flex items-center gap-1 mt-0.5">
                                <ImageIcon className="h-3 w-3" />
                                Sin imagen
                              </p>
                            )}
                          </div>
                        </TableCell>
                        <TableCell>
                          <div className="flex flex-col gap-1">
                            <Badge variant="outline">{product.category}</Badge>
                            {categoryInactive && (
                              <Badge variant="secondary" className="text-xs bg-yellow-50 text-yellow-700 border-yellow-200">
                                Cat. inactiva
                              </Badge>
                            )}
                          </div>
                        </TableCell>
                        <TableCell>{formatPrice(product.price)}</TableCell>
                        <TableCell>
                          <Badge
                            variant={
                              product.stock === 0 ? 'destructive' :
                              product.stock <= 5 ? 'secondary' : 'default'
                            }
                          >
                            {product.stock} uds.
                          </Badge>
                        </TableCell>
                        <TableCell>
                          <Badge variant={product.isActive === false ? 'outline' : 'default'}>
                            {product.isActive === false ? 'Inactivo' : 'Activo'}
                          </Badge>
                        </TableCell>
                        <TableCell className="text-right">
                          <div className="flex justify-end gap-1">
                            <Button variant="ghost" size="icon" onClick={() => handleViewDetail(product)} title="Ver detalle">
                              <Eye className="h-4 w-4" />
                            </Button>
                            <Button variant="ghost" size="icon" onClick={() => handleEditClick(product)} title="Editar">
                              <Edit className="h-4 w-4" />
                            </Button>
                            <Button
                              variant="ghost"
                              size="icon"
                              onClick={() => handleToggleClick(product)}
                              title={product.isActive === false ? 'Activar' : 'Desactivar'}
                            >
                              <Power className={`h-4 w-4 ${product.isActive === false ? 'text-gray-400' : 'text-green-600'}`} />
                            </Button>
                          </div>
                        </TableCell>
                      </TableRow>
                    );
                  })}
                </TableBody>
              </Table>
            </div>
          </CardContent>
        </Card>
      )}

      {/* ── HU-PRO-001: Diálogo Crear Producto ── */}
      <Dialog open={isCreateDialogOpen} onOpenChange={setIsCreateDialogOpen}>
        <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Crear nuevo producto</DialogTitle>
            <DialogDescription>
              Completa los campos para agregar un nuevo producto al catálogo
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-4">
            {/* Campos básicos */}
            <div>
              <Label htmlFor="name">Nombre del producto *</Label>
              <Input
                id="name"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                placeholder="Ej: Arco de Globos Premium"
              />
              {formErrors.name && <p className="text-sm text-destructive mt-1">{formErrors.name}</p>}
            </div>

            <div>
              <Label htmlFor="description">Descripción *</Label>
              <Textarea
                id="description"
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                placeholder="Describe el producto en detalle..."
                rows={3}
              />
              {formErrors.description && <p className="text-sm text-destructive mt-1">{formErrors.description}</p>}
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <Label htmlFor="price">Precio (₡) *</Label>
                <Input
                  id="price"
                  type="number"
                  min="0"
                  step="100"
                  value={formData.price}
                  onChange={(e) => setFormData({ ...formData, price: e.target.value })}
                  placeholder="0"
                />
                {formErrors.price && <p className="text-sm text-destructive mt-1">{formErrors.price}</p>}
              </div>
              <div>
                <Label htmlFor="stock">Stock inicial *</Label>
                <Input
                  id="stock"
                  type="number"
                  min="0"
                  value={formData.stock}
                  onChange={(e) => setFormData({ ...formData, stock: e.target.value })}
                  placeholder="0"
                />
                {formErrors.stock && <p className="text-sm text-destructive mt-1">{formErrors.stock}</p>}
              </div>
            </div>

            {/* HU-CAT integración: Select de categoría */}
            <div>
              <Label htmlFor="category">Categoría *</Label>
              {activeCategories.length > 0 ? (
                <Select value={formData.category} onValueChange={(v) => setFormData({ ...formData, category: v })}>
                  <SelectTrigger id="category" className={formErrors.category ? 'border-red-500' : ''}>
                    <SelectValue placeholder="Selecciona una categoría" />
                  </SelectTrigger>
                  <SelectContent>
                    {activeCategories.map(cat => (
                      <SelectItem key={cat.id} value={cat.name}>
                        <div className="flex items-center gap-2">
                          <Tag className="h-3 w-3" />
                          {cat.name}
                        </div>
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              ) : (
                <Alert className="bg-yellow-50 border-yellow-200">
                  <AlertTriangle className="h-4 w-4 text-yellow-700" />
                  <AlertDescription className="text-yellow-700">
                    No hay categorías activas.{' '}
                    <button
                      className="underline font-medium"
                      onClick={() => { setIsCreateDialogOpen(false); navigate('/admin/categories'); }}
                    >
                      Crea una categoría primero
                    </button>
                  </AlertDescription>
                </Alert>
              )}
              {formErrors.category && <p className="text-sm text-destructive mt-1">{formErrors.category}</p>}
            </div>

            <Separator />

            {/* HU-IMG integración: ImageManager */}
            <div>
              <h4 className="font-medium mb-1 flex items-center gap-2">
                <ImageIcon className="h-4 w-4" />
                Imágenes del producto
              </h4>
              <ImageManager
                images={formData.images}
                onChange={(images) => setFormData({ ...formData, images })}
                maxImages={5}
                maxSizeMB={5}
              />
            </div>

            {Object.keys(formErrors).length > 0 && (
              <Alert variant="destructive">
                <AlertCircle className="h-4 w-4" />
                <AlertDescription>Por favor corrige los errores antes de continuar</AlertDescription>
              </Alert>
            )}
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={() => { setIsCreateDialogOpen(false); resetForm(); }}>
              Cancelar
            </Button>
            <Button onClick={handleCreateProduct} disabled={activeCategories.length === 0}>
              <CheckCircle2 className="h-4 w-4 mr-2" />
              Crear producto
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* ── HU-PRO-003: Diálogo Editar Producto ── */}
      <Dialog open={isEditDialogOpen} onOpenChange={setIsEditDialogOpen}>
        <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Editar producto</DialogTitle>
            <DialogDescription>Modifica los campos del producto</DialogDescription>
          </DialogHeader>

          <div className="space-y-4">
            <div>
              <Label htmlFor="edit-name">Nombre del producto *</Label>
              <Input
                id="edit-name"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                placeholder="Ej: Arco de Globos Premium"
              />
              {formErrors.name && <p className="text-sm text-destructive mt-1">{formErrors.name}</p>}
            </div>

            <div>
              <Label htmlFor="edit-description">Descripción *</Label>
              <Textarea
                id="edit-description"
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                placeholder="Describe el producto en detalle..."
                rows={3}
              />
              {formErrors.description && <p className="text-sm text-destructive mt-1">{formErrors.description}</p>}
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <Label htmlFor="edit-price">Precio (₡) *</Label>
                <Input
                  id="edit-price"
                  type="number"
                  min="0"
                  step="100"
                  value={formData.price}
                  onChange={(e) => setFormData({ ...formData, price: e.target.value })}
                  placeholder="0"
                />
                {formErrors.price && <p className="text-sm text-destructive mt-1">{formErrors.price}</p>}
              </div>
              <div>
                <Label htmlFor="edit-stock">Stock *</Label>
                <Input
                  id="edit-stock"
                  type="number"
                  min="0"
                  value={formData.stock}
                  onChange={(e) => setFormData({ ...formData, stock: e.target.value })}
                  placeholder="0"
                />
                {formErrors.stock && <p className="text-sm text-destructive mt-1">{formErrors.stock}</p>}
              </div>
            </div>

            {/* HU-CAT integración: Select de categoría en edición */}
            <div>
              <Label htmlFor="edit-category">Categoría *</Label>
              <Select
                value={formData.category}
                onValueChange={(v) => setFormData({ ...formData, category: v })}
              >
                <SelectTrigger id="edit-category" className={formErrors.category ? 'border-red-500' : ''}>
                  <SelectValue placeholder="Selecciona una categoría" />
                </SelectTrigger>
                <SelectContent>
                  {/* Mostrar todas las categorías en edición, incluyendo inactivas si el producto ya la tenía */}
                  {categories.map(cat => (
                    <SelectItem key={cat.id} value={cat.name}>
                      <div className="flex items-center gap-2">
                        <Tag className="h-3 w-3" />
                        {cat.name}
                        {!cat.isActive && <Badge variant="outline" className="text-xs ml-1">inactiva</Badge>}
                      </div>
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              {formErrors.category && <p className="text-sm text-destructive mt-1">{formErrors.category}</p>}
            </div>

            <Separator />

            {/* HU-IMG integración: ImageManager en edición */}
            <div>
              <h4 className="font-medium mb-1 flex items-center gap-2">
                <ImageIcon className="h-4 w-4" />
                Imágenes del producto
              </h4>
              <ImageManager
                images={formData.images}
                onChange={(images) => setFormData({ ...formData, images })}
                maxImages={5}
                maxSizeMB={5}
              />
            </div>

            {Object.keys(formErrors).length > 0 && (
              <Alert variant="destructive">
                <AlertCircle className="h-4 w-4" />
                <AlertDescription>Por favor corrige los errores antes de continuar</AlertDescription>
              </Alert>
            )}
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={() => { setIsEditDialogOpen(false); resetForm(); }}>
              Cancelar
            </Button>
            <Button onClick={handleUpdateProduct}>
              <CheckCircle2 className="h-4 w-4 mr-2" />
              Guardar cambios
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* ── HU-PRO-002 Escenario 2: Diálogo Ver Detalle ── */}
      <Dialog open={isDetailDialogOpen} onOpenChange={setIsDetailDialogOpen}>
        <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Detalle del producto</DialogTitle>
            <DialogDescription>Información completa del producto</DialogDescription>
          </DialogHeader>

          {selectedProduct && (
            <div className="space-y-4">
              {/* Galería de imágenes del producto */}
              {selectedProduct.images && selectedProduct.images.length > 0 ? (
                <div>
                  <Label className="text-muted-foreground mb-2 block">Galería de imágenes</Label>
                  <div className="flex gap-3 flex-wrap">
                    {[...selectedProduct.images].sort((a, b) => a.order - b.order).map((img) => (
                      <div key={img.id} className="relative w-20 h-20 rounded-md overflow-hidden border-2 border-gray-200">
                        <img src={img.url} alt="Imagen del producto" className="w-full h-full object-cover" />
                        {img.isPrimary && (
                          <div className="absolute top-1 left-1 bg-yellow-400 text-white text-xs rounded px-1">
                            ★
                          </div>
                        )}
                      </div>
                    ))}
                  </div>
                </div>
              ) : (
                <Alert className="bg-amber-50 border-amber-200">
                  <ImageIcon className="h-4 w-4 text-amber-700" />
                  <AlertDescription className="text-amber-700">
                    Este producto no tiene imágenes. Edítalo para agregar imágenes.
                  </AlertDescription>
                </Alert>
              )}

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <Label className="text-muted-foreground">ID</Label>
                  <p className="font-mono text-sm">{selectedProduct.id}</p>
                </div>
                <div>
                  <Label className="text-muted-foreground">Estado</Label>
                  <div className="mt-1">
                    <Badge variant={selectedProduct.isActive === false ? 'outline' : 'default'}>
                      {selectedProduct.isActive === false ? 'Inactivo' : 'Activo'}
                    </Badge>
                  </div>
                </div>
              </div>

              <div>
                <Label className="text-muted-foreground">Nombre</Label>
                <p className="font-semibold text-lg">{selectedProduct.name}</p>
              </div>

              <div>
                <Label className="text-muted-foreground">Descripción</Label>
                <p>{selectedProduct.description}</p>
              </div>

              <div className="grid grid-cols-3 gap-4">
                <div>
                  <Label className="text-muted-foreground">Precio</Label>
                  <p className="font-bold text-lg">{formatPrice(selectedProduct.price)}</p>
                </div>
                <div>
                  <Label className="text-muted-foreground">Stock</Label>
                  <p className="font-bold text-lg">{selectedProduct.stock}</p>
                </div>
                <div>
                  <Label className="text-muted-foreground">Categoría</Label>
                  <div className="mt-1">
                    <Badge variant="outline">{selectedProduct.category}</Badge>
                  </div>
                </div>
              </div>

              {selectedProduct.hasPromotion && (
                <Alert className="bg-pink-50 border-pink-200">
                  <Tag className="h-4 w-4 text-pink-600" />
                  <AlertDescription className="text-pink-700">
                    Este producto tiene una promoción activa
                    {selectedProduct.promotionEndDate &&
                      ` hasta el ${new Date(selectedProduct.promotionEndDate).toLocaleDateString('es-CR')}`}
                  </AlertDescription>
                </Alert>
              )}
            </div>
          )}

          <DialogFooter>
            <Button variant="outline" onClick={() => setIsDetailDialogOpen(false)}>Cerrar</Button>
            {selectedProduct && (
              <Button onClick={() => { setIsDetailDialogOpen(false); handleEditClick(selectedProduct); }}>
                <Edit className="h-4 w-4 mr-2" />
                Editar producto
              </Button>
            )}
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* ── HU-PRO-004: Diálogo Confirmación Toggle ── */}
      <Dialog open={isToggleDialogOpen} onOpenChange={setIsToggleDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              {selectedProduct?.isActive === false ? 'Activar' : 'Desactivar'} producto
            </DialogTitle>
            <DialogDescription>
              ¿Estás seguro de que deseas {selectedProduct?.isActive === false ? 'activar' : 'desactivar'} este producto?
            </DialogDescription>
          </DialogHeader>

          {selectedProduct && (
            <Alert>
              <AlertCircle className="h-4 w-4" />
              <AlertDescription>
                <strong>{selectedProduct.name}</strong>
                <br />
                {selectedProduct.isActive === false
                  ? 'Al activar este producto, estará visible en el catálogo para los clientes.'
                  : 'Al desactivar este producto, no estará visible en el catálogo para los clientes.'}
              </AlertDescription>
            </Alert>
          )}

          <DialogFooter>
            <Button variant="outline" onClick={() => setIsToggleDialogOpen(false)}>Cancelar</Button>
            <Button
              variant={selectedProduct?.isActive === false ? 'default' : 'destructive'}
              onClick={handleToggleProductStatus}
            >
              {selectedProduct?.isActive === false ? 'Activar' : 'Desactivar'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
