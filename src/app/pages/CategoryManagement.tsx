import React, { useState } from 'react';
import { useNavigate } from 'react-router';
import { useApp } from '../context/AppContext';
import { Category } from '../data/mockData';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { Button } from '../components/ui/button';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import { Textarea } from '../components/ui/textarea';
import { Alert, AlertDescription } from '../components/ui/alert';
import { Badge } from '../components/ui/badge';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '../components/ui/dialog';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../components/ui/table';
import {
  Plus,
  Search,
  Edit,
  Power,
  AlertCircle,
  CheckCircle2,
  Tag,
  Package,
  FolderOpen,
  Info
} from 'lucide-react';
import { toast } from 'sonner';

// HU-CAT-001, HU-CAT-002, HU-CAT-003, HU-CAT-004: Gestión completa de categorías
export default function CategoryManagement() {
  const navigate = useNavigate();
  const {
    currentUser,
    categories,
    createCategory,
    updateCategory,
    toggleCategoryStatus,
    getCategoryProductCount,
    products
  } = useApp();

  const [searchQuery, setSearchQuery] = useState('');
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false);
  const [isEditDialogOpen, setIsEditDialogOpen] = useState(false);
  const [isToggleDialogOpen, setIsToggleDialogOpen] = useState(false);
  const [selectedCategory, setSelectedCategory] = useState<Category | null>(null);
  const [formData, setFormData] = useState({ name: '', description: '' });
  const [formErrors, setFormErrors] = useState<Record<string, string>>({});

  // Verificar permisos (solo admin)
  React.useEffect(() => {
    if (!currentUser || currentUser.role !== 'admin') {
      toast.error('Solo los administradores pueden gestionar categorías');
      navigate('/');
    }
  }, [currentUser, navigate]);

  if (!currentUser || currentUser.role !== 'admin') {
    return null;
  }

  // HU-CAT-002 Escenario 2: Filtrar categorías
  const filteredCategories = categories.filter(cat =>
    cat.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
    (cat.description || '').toLowerCase().includes(searchQuery.toLowerCase())
  );

  // Validar formulario
  const validateForm = (): boolean => {
    const errors: Record<string, string> = {};
    if (!formData.name.trim()) {
      errors.name = 'El nombre de la categoría es requerido';
    } else if (formData.name.trim().length < 2) {
      errors.name = 'El nombre debe tener al menos 2 caracteres';
    } else if (formData.name.trim().length > 50) {
      errors.name = 'El nombre no puede exceder 50 caracteres';
    }
    setFormErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const resetForm = () => {
    setFormData({ name: '', description: '' });
    setFormErrors({});
  };

  // HU-CAT-001: Crear categoría
  const handleCreateCategory = () => {
    if (!validateForm()) {
      toast.error('Por favor corrige los errores en el formulario');
      return;
    }

    const result = createCategory({
      name: formData.name.trim(),
      description: formData.description.trim() || undefined,
      isActive: true
    });

    if (result.success) {
      // Escenario 1: Éxito
      toast.success(result.message);
      setIsCreateDialogOpen(false);
      resetForm();
    } else {
      // Escenario 2/3: Error - nombre vacío o duplicado
      toast.error(result.message);
      if (result.message.includes('nombre')) {
        setFormErrors({ name: result.message });
      }
    }
  };

  // HU-CAT-003: Editar categoría
  const handleEditCategory = () => {
    if (!selectedCategory || !validateForm()) {
      toast.error('Por favor corrige los errores en el formulario');
      return;
    }

    const result = updateCategory(selectedCategory.id, {
      name: formData.name.trim(),
      description: formData.description.trim() || undefined
    });

    if (result.success) {
      // Escenario 1: Éxito + actualización en productos
      toast.success(result.message);
      setIsEditDialogOpen(false);
      setSelectedCategory(null);
      resetForm();
    } else {
      // Escenario 2/3: Error
      toast.error(result.message);
      if (result.message.includes('nombre') || result.message.includes('existe')) {
        setFormErrors({ name: result.message });
      }
    }
  };

  // HU-CAT-004: Toggle estado
  const handleToggleStatus = () => {
    if (!selectedCategory) return;

    const productCount = getCategoryProductCount(selectedCategory.name);

    // HU-CAT-004: La regla es no ELIMINAR si tiene productos, pero sí se puede inactivar
    // Aun si tiene productos, se puede inactivar mostrando advertencia
    const result = toggleCategoryStatus(selectedCategory.id);

    if (result.success) {
      const isNowActive = !selectedCategory.isActive;
      toast.success(result.message);
      if (!isNowActive && productCount > 0) {
        toast.warning(`${productCount} producto(s) de esta categoría permanecen activos`, {
          description: 'Los productos no fueron afectados'
        });
      }
      setIsToggleDialogOpen(false);
      setSelectedCategory(null);
    } else {
      toast.error(result.message);
    }
  };

  const handleEditClick = (category: Category) => {
    setSelectedCategory(category);
    setFormData({
      name: category.name,
      description: category.description || ''
    });
    setFormErrors({});
    setIsEditDialogOpen(true);
  };

  const handleToggleClick = (category: Category) => {
    setSelectedCategory(category);
    setIsToggleDialogOpen(true);
  };

  const handleCreateClick = () => {
    resetForm();
    setIsCreateDialogOpen(true);
  };

  // Estadísticas
  const totalCategories = categories.length;
  const activeCategories = categories.filter(c => c.isActive).length;
  const inactiveCategories = categories.filter(c => !c.isActive).length;
  const totalProductsWithCategory = products.filter(p => p.isActive !== false).length;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Gestión de Categorías</h1>
          <p className="text-muted-foreground">Administra las categorías del catálogo de productos</p>
        </div>
        <Button onClick={handleCreateClick}>
          <Plus className="h-4 w-4 mr-2" />
          Crear categoría
        </Button>
      </div>

      {/* Estadísticas rápidas */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Total</p>
                <p className="text-2xl font-bold">{totalCategories}</p>
              </div>
              <div className="h-10 w-10 bg-blue-100 rounded-full flex items-center justify-center">
                <FolderOpen className="h-5 w-5 text-blue-600" />
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Activas</p>
                <p className="text-2xl font-bold text-green-600">{activeCategories}</p>
              </div>
              <div className="h-10 w-10 bg-green-100 rounded-full flex items-center justify-center">
                <CheckCircle2 className="h-5 w-5 text-green-600" />
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Inactivas</p>
                <p className="text-2xl font-bold text-gray-400">{inactiveCategories}</p>
              </div>
              <div className="h-10 w-10 bg-gray-100 rounded-full flex items-center justify-center">
                <Power className="h-5 w-5 text-gray-400" />
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Productos</p>
                <p className="text-2xl font-bold">{totalProductsWithCategory}</p>
              </div>
              <div className="h-10 w-10 bg-pink-100 rounded-full flex items-center justify-center">
                <Package className="h-5 w-5 text-pink-600" />
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Búsqueda */}
      <Card>
        <CardContent className="pt-6">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <Input
              placeholder="Buscar categorías por nombre o descripción..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="pl-10"
            />
          </div>
        </CardContent>
      </Card>

      {/* HU-CAT-002 Escenario 3: Lista vacía */}
      {filteredCategories.length === 0 ? (
        <Card>
          <CardContent className="py-16 text-center">
            <Tag className="h-16 w-16 text-muted-foreground mx-auto mb-4" />
            <h3 className="text-xl font-semibold mb-2">
              {searchQuery ? 'No se encontraron categorías' : 'No hay categorías'}
            </h3>
            <p className="text-muted-foreground mb-4">
              {searchQuery
                ? 'Intenta con otros términos de búsqueda'
                : 'Crea la primera categoría para organizar tu catálogo'}
            </p>
            {!searchQuery && (
              <Button onClick={handleCreateClick}>
                <Plus className="h-4 w-4 mr-2" />
                Crear primera categoría
              </Button>
            )}
          </CardContent>
        </Card>
      ) : (
        /* HU-CAT-002 Escenario 1: Lista de categorías */
        <Card>
          <CardHeader>
            <CardTitle>Categorías ({filteredCategories.length})</CardTitle>
            <CardDescription>
              Lista completa de categorías. El contador de productos muestra productos activos.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="rounded-md border">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Nombre</TableHead>
                    <TableHead className="hidden md:table-cell">Descripción</TableHead>
                    <TableHead>Productos activos</TableHead>
                    <TableHead>Estado</TableHead>
                    <TableHead className="hidden md:table-cell">Creada</TableHead>
                    <TableHead className="text-right">Acciones</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {filteredCategories.map((category) => {
                    const productCount = getCategoryProductCount(category.name);
                    const hasProducts = productCount > 0;

                    return (
                      <TableRow key={category.id}>
                        <TableCell>
                          <div className="flex items-center gap-2">
                            <div className={`h-2 w-2 rounded-full ${category.isActive ? 'bg-green-500' : 'bg-gray-300'}`} />
                            <span className="font-medium">{category.name}</span>
                          </div>
                        </TableCell>
                        <TableCell className="hidden md:table-cell text-sm text-muted-foreground max-w-xs truncate">
                          {category.description || '—'}
                        </TableCell>
                        <TableCell>
                          <div className="flex items-center gap-1">
                            <Package className="h-3 w-3 text-muted-foreground" />
                            <Badge
                              variant={hasProducts ? 'secondary' : 'outline'}
                              className={hasProducts ? 'bg-blue-50 text-blue-700' : ''}
                            >
                              {productCount} producto{productCount !== 1 ? 's' : ''}
                            </Badge>
                          </div>
                        </TableCell>
                        <TableCell>
                          <Badge variant={category.isActive ? 'default' : 'outline'}>
                            {category.isActive ? 'Activa' : 'Inactiva'}
                          </Badge>
                        </TableCell>
                        <TableCell className="hidden md:table-cell text-sm text-muted-foreground">
                          {new Date(category.createdAt).toLocaleDateString('es-CR')}
                        </TableCell>
                        <TableCell className="text-right">
                          <div className="flex justify-end gap-1">
                            <Button
                              variant="ghost"
                              size="icon"
                              onClick={() => handleEditClick(category)}
                              title="Editar categoría"
                            >
                              <Edit className="h-4 w-4" />
                            </Button>
                            <Button
                              variant="ghost"
                              size="icon"
                              onClick={() => handleToggleClick(category)}
                              title={category.isActive ? 'Inactivar' : 'Activar'}
                            >
                              <Power
                                className={`h-4 w-4 ${category.isActive ? 'text-green-600' : 'text-gray-400'}`}
                              />
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

      {/* Alerta: Categorías inactivas con productos */}
      {categories.some(c => !c.isActive && getCategoryProductCount(c.name) > 0) && (
        <Alert className="bg-yellow-50 border-yellow-200">
          <AlertCircle className="h-4 w-4 text-yellow-700" />
          <AlertDescription className="text-yellow-700">
            Hay categorías inactivas que aún tienen productos activos asignados. Los productos seguirán visibles en el catálogo.
          </AlertDescription>
        </Alert>
      )}

      {/* HU-CAT-001: Diálogo Crear Categoría */}
      <Dialog open={isCreateDialogOpen} onOpenChange={setIsCreateDialogOpen}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2">
              <Tag className="h-5 w-5 text-pink-600" />
              Crear nueva categoría
            </DialogTitle>
            <DialogDescription>
              Las categorías organizan los productos del catálogo
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-4">
            <div>
              <Label htmlFor="cat-name">Nombre de la categoría *</Label>
              <Input
                id="cat-name"
                value={formData.name}
                onChange={(e) => {
                  setFormData({ ...formData, name: e.target.value });
                  if (formErrors.name) setFormErrors({ ...formErrors, name: '' });
                }}
                placeholder="Ej: Arcos, Bouquets, Packs..."
                className={formErrors.name ? 'border-red-500' : ''}
                maxLength={50}
              />
              {formErrors.name && (
                <p className="text-sm text-destructive mt-1">{formErrors.name}</p>
              )}
              <p className="text-xs text-muted-foreground mt-1">
                {formData.name.length}/50 caracteres
              </p>
            </div>

            <div>
              <Label htmlFor="cat-desc">Descripción (opcional)</Label>
              <Textarea
                id="cat-desc"
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                placeholder="Describe brevemente los productos de esta categoría..."
                rows={3}
                maxLength={200}
              />
              <p className="text-xs text-muted-foreground mt-1">
                {formData.description.length}/200 caracteres
              </p>
            </div>

            {/* Info: Escenario Duplicado */}
            <Alert className="bg-blue-50 border-blue-200">
              <Info className="h-4 w-4 text-blue-600" />
              <AlertDescription className="text-sm text-blue-700">
                No se permiten categorías con nombres duplicados
              </AlertDescription>
            </Alert>
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={() => { setIsCreateDialogOpen(false); resetForm(); }}>
              Cancelar
            </Button>
            <Button onClick={handleCreateCategory}>
              <CheckCircle2 className="h-4 w-4 mr-2" />
              Crear categoría
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* HU-CAT-003: Diálogo Editar Categoría */}
      <Dialog open={isEditDialogOpen} onOpenChange={setIsEditDialogOpen}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2">
              <Edit className="h-5 w-5 text-blue-600" />
              Editar categoría
            </DialogTitle>
            <DialogDescription>
              Los cambios de nombre se reflejarán automáticamente en todos los productos asociados
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-4">
            <div>
              <Label htmlFor="edit-cat-name">Nombre de la categoría *</Label>
              <Input
                id="edit-cat-name"
                value={formData.name}
                onChange={(e) => {
                  setFormData({ ...formData, name: e.target.value });
                  if (formErrors.name) setFormErrors({ ...formErrors, name: '' });
                }}
                placeholder="Nombre de la categoría"
                className={formErrors.name ? 'border-red-500' : ''}
                maxLength={50}
              />
              {formErrors.name && (
                <p className="text-sm text-destructive mt-1">{formErrors.name}</p>
              )}
            </div>

            <div>
              <Label htmlFor="edit-cat-desc">Descripción (opcional)</Label>
              <Textarea
                id="edit-cat-desc"
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                placeholder="Descripción de la categoría..."
                rows={3}
                maxLength={200}
              />
            </div>

            {/* HU-CAT-003: Impacto en productos */}
            {selectedCategory && getCategoryProductCount(selectedCategory.name) > 0 && (
              <Alert className="bg-amber-50 border-amber-200">
                <Package className="h-4 w-4 text-amber-700" />
                <AlertDescription className="text-amber-700">
                  <strong>{getCategoryProductCount(selectedCategory.name)} producto(s)</strong> serán actualizados automáticamente si cambias el nombre
                </AlertDescription>
              </Alert>
            )}
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={() => { setIsEditDialogOpen(false); resetForm(); }}>
              Cancelar
            </Button>
            <Button onClick={handleEditCategory}>
              <CheckCircle2 className="h-4 w-4 mr-2" />
              Guardar cambios
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* HU-CAT-004: Diálogo Toggle Estado */}
      <Dialog open={isToggleDialogOpen} onOpenChange={setIsToggleDialogOpen}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>
              {selectedCategory?.isActive ? '⚠️ Inactivar categoría' : '✅ Activar categoría'}
            </DialogTitle>
            <DialogDescription>
              {selectedCategory?.isActive
                ? 'La categoría dejará de estar disponible para asignar nuevos productos'
                : 'La categoría volverá a estar disponible'}
            </DialogDescription>
          </DialogHeader>

          {selectedCategory && (
            <div className="space-y-4">
              <Alert>
                <AlertCircle className="h-4 w-4" />
                <AlertDescription>
                  <strong className="block">{selectedCategory.name}</strong>
                  {selectedCategory.description && (
                    <span className="text-sm text-muted-foreground">{selectedCategory.description}</span>
                  )}
                </AlertDescription>
              </Alert>

              {/* HU-CAT-004: Advertencia si tiene productos activos */}
              {selectedCategory.isActive && getCategoryProductCount(selectedCategory.name) > 0 && (
                <Alert className="bg-yellow-50 border-yellow-200">
                  <AlertCircle className="h-4 w-4 text-yellow-700" />
                  <AlertDescription className="text-yellow-700">
                    <strong>Atención:</strong> Esta categoría tiene{' '}
                    <strong>{getCategoryProductCount(selectedCategory.name)} producto(s) activo(s)</strong>.
                    Al inactivar la categoría, los productos existentes no se eliminarán pero la categoría no estará disponible para nuevos productos.
                  </AlertDescription>
                </Alert>
              )}

              {/* HU-CAT-004: Categoría sin productos → inactivar sin problema */}
              {selectedCategory.isActive && getCategoryProductCount(selectedCategory.name) === 0 && (
                <Alert className="bg-green-50 border-green-200">
                  <CheckCircle2 className="h-4 w-4 text-green-700" />
                  <AlertDescription className="text-green-700">
                    Esta categoría no tiene productos activos. Puede inactivarse de forma segura.
                  </AlertDescription>
                </Alert>
              )}
            </div>
          )}

          <DialogFooter>
            <Button variant="outline" onClick={() => { setIsToggleDialogOpen(false); setSelectedCategory(null); }}>
              Cancelar
            </Button>
            <Button
              variant={selectedCategory?.isActive ? 'destructive' : 'default'}
              onClick={handleToggleStatus}
            >
              {selectedCategory?.isActive ? 'Inactivar' : 'Activar'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
