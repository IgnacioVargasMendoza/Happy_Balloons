import React, { useState, useCallback } from 'react';
import { ProductImage } from '../data/mockData';
import { Button } from './ui/button';
import { Label } from './ui/label';
import { Alert, AlertDescription } from './ui/alert';
import { Card, CardContent } from './ui/card';
import { 
  Upload, 
  X, 
  Star, 
  Image as ImageIcon,
  AlertCircle,
  GripVertical 
} from 'lucide-react';
import { toast } from 'sonner';

interface ImageManagerProps {
  images: ProductImage[];
  onChange: (images: ProductImage[]) => void;
  maxImages?: number;
  maxSizeMB?: number;
}

// HU-IMG-001, HU-IMG-002, HU-IMG-003, HU-IMG-004: Gestión completa de imágenes
export function ImageManager({ 
  images, 
  onChange, 
  maxImages = 5,
  maxSizeMB = 5 
}: ImageManagerProps) {
  const [dragActive, setDragActive] = useState(false);
  const [uploadError, setUploadError] = useState<string>('');

  // HU-IMG-001 Escenario 2 y 3: Validar archivo
  const validateFile = (file: File): { valid: boolean; error?: string } => {
    // Escenario 2: Validar formato
    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp'];
    if (!allowedTypes.includes(file.type)) {
      return { 
        valid: false, 
        error: 'Formato no válido. Solo se permiten archivos JPG, PNG y WEBP' 
      };
    }

    // Escenario 3: Validar tamaño
    const maxSizeBytes = maxSizeMB * 1024 * 1024;
    if (file.size > maxSizeBytes) {
      return { 
        valid: false, 
        error: `El archivo excede el tamaño máximo de ${maxSizeMB}MB` 
      };
    }

    return { valid: true };
  };

  // HU-IMG-001 Escenario 1: Cargar imagen válida
  const handleFileUpload = useCallback((files: FileList | null) => {
    if (!files || files.length === 0) return;

    setUploadError('');

    // Verificar límite de imágenes
    if (images.length >= maxImages) {
      const error = `Máximo ${maxImages} imágenes permitidas`;
      setUploadError(error);
      toast.error(error);
      return;
    }

    const file = files[0];
    const validation = validateFile(file);

    if (!validation.valid) {
      // Escenario 2 y 3: Mostrar error de validación
      setUploadError(validation.error || 'Error al validar archivo');
      toast.error(validation.error);
      return;
    }

    // Simular carga de imagen (en producción, subiría al servidor)
    const reader = new FileReader();
    reader.onload = (e) => {
      const newImage: ProductImage = {
        id: `img-${Date.now()}`,
        url: e.target?.result as string,
        isPrimary: images.length === 0, // Primera imagen es principal por defecto
        order: images.length
      };

      onChange([...images, newImage]);
      toast.success('Imagen agregada exitosamente');
      setUploadError('');
    };
    reader.readAsDataURL(file);
  }, [images, onChange, maxImages]);

  // Drag & Drop handlers
  const handleDrag = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (e.type === 'dragenter' || e.type === 'dragover') {
      setDragActive(true);
    } else if (e.type === 'dragleave') {
      setDragActive(false);
    }
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setDragActive(false);
    
    if (e.dataTransfer.files && e.dataTransfer.files[0]) {
      handleFileUpload(e.dataTransfer.files);
    }
  };

  // HU-IMG-003 Escenario 1: Seleccionar imagen principal
  const handleSetPrimary = (imageId: string) => {
    const updatedImages = images.map(img => ({
      ...img,
      isPrimary: img.id === imageId
    }));
    onChange(updatedImages);
    toast.success('Imagen principal actualizada');
  };

  // HU-IMG-004: Eliminar imagen
  const handleRemoveImage = (imageId: string) => {
    const imageToRemove = images.find(img => img.id === imageId);
    const updatedImages = images.filter(img => img.id !== imageId);
    
    // Si eliminamos la imagen principal, hacer principal la primera restante
    if (imageToRemove?.isPrimary && updatedImages.length > 0) {
      updatedImages[0].isPrimary = true;
    }

    // Reordenar
    const reorderedImages = updatedImages.map((img, index) => ({
      ...img,
      order: index
    }));

    onChange(reorderedImages);
    toast.success('Imagen eliminada');
  };

  // HU-IMG-003 Escenario 2: Reordenar imágenes (simplificado - mover arriba/abajo)
  const handleMoveImage = (imageId: string, direction: 'up' | 'down') => {
    const currentIndex = images.findIndex(img => img.id === imageId);
    if (currentIndex === -1) return;

    const newIndex = direction === 'up' ? currentIndex - 1 : currentIndex + 1;
    if (newIndex < 0 || newIndex >= images.length) return;

    const updatedImages = [...images];
    const [movedImage] = updatedImages.splice(currentIndex, 1);
    updatedImages.splice(newIndex, 0, movedImage);

    // Actualizar orden
    const reorderedImages = updatedImages.map((img, index) => ({
      ...img,
      order: index
    }));

    onChange(reorderedImages);
    toast.success('Orden actualizado');
  };

  return (
    <div className="space-y-4">
      <div>
        <Label>Imágenes del producto</Label>
        <p className="text-sm text-muted-foreground mb-3">
          Sube hasta {maxImages} imágenes (JPG, PNG, WEBP - máx. {maxSizeMB}MB cada una)
        </p>

        {/* HU-IMG-001: Área de subida - Drag & Drop */}
        <div
          className={`border-2 border-dashed rounded-lg p-8 text-center transition-colors ${
            dragActive 
              ? 'border-primary bg-primary/5' 
              : 'border-gray-300 hover:border-gray-400'
          } ${images.length >= maxImages ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer'}`}
          onDragEnter={handleDrag}
          onDragLeave={handleDrag}
          onDragOver={handleDrag}
          onDrop={handleDrop}
          onClick={() => {
            if (images.length < maxImages) {
              document.getElementById('image-upload')?.click();
            }
          }}
        >
          <Upload className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
          <p className="text-sm font-medium mb-1">
            {images.length >= maxImages 
              ? `Máximo de ${maxImages} imágenes alcanzado`
              : 'Arrastra una imagen aquí o haz clic para seleccionar'
            }
          </p>
          <p className="text-xs text-muted-foreground">
            JPG, PNG o WEBP - Máx. {maxSizeMB}MB
          </p>
          <input
            id="image-upload"
            type="file"
            className="hidden"
            accept="image/jpeg,image/jpg,image/png,image/webp"
            onChange={(e) => handleFileUpload(e.target.files)}
            disabled={images.length >= maxImages}
          />
        </div>

        {/* HU-IMG-001 Escenario 2 y 3: Mostrar error de validación */}
        {uploadError && (
          <Alert variant="destructive" className="mt-3">
            <AlertCircle className="h-4 w-4" />
            <AlertDescription>{uploadError}</AlertDescription>
          </Alert>
        )}
      </div>

      {/* HU-IMG-002 Escenario 2: Estado vacío */}
      {images.length === 0 ? (
        <Card>
          <CardContent className="py-8 text-center">
            <ImageIcon className="h-12 w-12 text-muted-foreground mx-auto mb-3" />
            <p className="text-sm text-muted-foreground">
              No hay imágenes agregadas. Sube al menos una imagen del producto.
            </p>
          </CardContent>
        </Card>
      ) : (
        /* HU-IMG-002 Escenario 1: Galería visible */
        <div className="space-y-3">
          <Label>Galería de imágenes ({images.length}/{maxImages})</Label>
          <div className="grid gap-4">
            {images.map((image, index) => (
              <Card key={image.id} className="overflow-hidden">
                <CardContent className="p-4">
                  <div className="flex gap-4 items-start">
                    {/* Vista previa */}
                    <div className="relative w-24 h-24 rounded-md overflow-hidden bg-gray-100 flex-shrink-0">
                      <img 
                        src={image.url} 
                        alt={`Imagen ${index + 1}`}
                        className="w-full h-full object-cover"
                      />
                      {/* HU-IMG-003 Escenario 1: Indicador de imagen principal */}
                      {image.isPrimary && (
                        <div className="absolute top-1 right-1 bg-yellow-400 rounded-full p-1">
                          <Star className="h-3 w-3 text-white fill-current" />
                        </div>
                      )}
                    </div>

                    {/* Información y controles */}
                    <div className="flex-1 space-y-2">
                      <div className="flex items-center justify-between">
                        <div>
                          <p className="text-sm font-medium">
                            Imagen {index + 1}
                            {image.isPrimary && (
                              <span className="ml-2 text-xs bg-yellow-100 text-yellow-800 px-2 py-0.5 rounded">
                                Principal
                              </span>
                            )}
                          </p>
                          <p className="text-xs text-muted-foreground">
                            Orden: {image.order + 1}
                          </p>
                        </div>

                        {/* HU-IMG-004: Botón eliminar */}
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => handleRemoveImage(image.id)}
                          className="text-destructive hover:text-destructive hover:bg-destructive/10"
                        >
                          <X className="h-4 w-4" />
                        </Button>
                      </div>

                      <div className="flex gap-2 flex-wrap">
                        {/* HU-IMG-003 Escenario 1: Marcar como principal */}
                        {!image.isPrimary && (
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => handleSetPrimary(image.id)}
                          >
                            <Star className="h-3 w-3 mr-1" />
                            Marcar como principal
                          </Button>
                        )}

                        {/* HU-IMG-003 Escenario 2: Reordenar */}
                        <div className="flex gap-1">
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => handleMoveImage(image.id, 'up')}
                            disabled={index === 0}
                            title="Mover arriba"
                          >
                            <GripVertical className="h-3 w-3 mr-1" />
                            ↑
                          </Button>
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => handleMoveImage(image.id, 'down')}
                            disabled={index === images.length - 1}
                            title="Mover abajo"
                          >
                            <GripVertical className="h-3 w-3 mr-1" />
                            ↓
                          </Button>
                        </div>
                      </div>
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
