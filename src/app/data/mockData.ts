// Mock data para simular backend
export interface Category {
  id: string;
  name: string;
  description?: string;
  isActive: boolean;
  createdAt: string;
}

export interface Product {
  id: string;
  name: string;
  description: string;
  price: number;
  discountPrice?: number;
  stock: number;
  category: string; // nombre de la categoría (denormalizado para compatibilidad)
  image: string;
  images?: ProductImage[]; // HU-IMG: Galería de imágenes
  hasPromotion?: boolean;
  promotionEndDate?: string;
  isActive?: boolean;
}

// HU-IMG: Estructura de imágenes del producto
export interface ProductImage {
  id: string;
  url: string;
  isPrimary: boolean;
  order: number;
}

export interface User {
  id: string;
  email: string;
  password: string;
  name: string;
  role: 'cliente' | 'admin' | 'staff';
  phone: string;
  address?: string;
  failedAttempts: number;
  isBlocked: boolean;
}

export interface Order {
  id: string;
  userId: string;
  items: CartItem[];
  total: number;
  status: 'pendiente' | 'pagado' | 'enviado' | 'entregado' | 'confirmado';
  paymentMethod?: 'sinpe' | 'tarjeta';
  deliveryZone?: string;
  deliveryCost?: number;
  deliveryAddress?: string;
  deliveryNotes?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CartItem {
  product: Product;
  quantity: number;
}

export interface DeliveryZone {
  id: string;
  name: string;
  cost: number;
  isAvailable: boolean;
}

export interface Promotion {
  id: string;
  productId: string;
  discountPercent: number;
  startDate: string;
  endDate: string;
  isActive: boolean;
}

export interface SystemConfig {
  minStock: number;
  maxLoginAttempts: number;
  blockDurationMinutes: number;
}

// HU-CAT-001, HU-CAT-002: Categorías mock
export const mockCategories: Category[] = [
  { id: 'cat-1', name: 'Arcos', description: 'Arcos decorativos de globos para eventos', isActive: true, createdAt: '2026-01-15' },
  { id: 'cat-2', name: 'Bouquets', description: 'Arreglos y bouquets de globos metálicos', isActive: true, createdAt: '2026-01-15' },
  { id: 'cat-3', name: 'Packs', description: 'Packs de globos variados inflados con helio', isActive: true, createdAt: '2026-01-20' },
  { id: 'cat-4', name: 'Números', description: 'Globos con figuras numéricas gigantes', isActive: true, createdAt: '2026-01-20' },
  { id: 'cat-5', name: 'Sets Temáticos', description: 'Sets completos con temática específica para fiestas', isActive: true, createdAt: '2026-02-01' },
  { id: 'cat-6', name: 'Especiales', description: 'Globos y decoraciones especiales con efectos únicos', isActive: true, createdAt: '2026-02-01' },
];

// Productos mock
export const mockProducts: Product[] = [
  {
    id: '1',
    name: 'Arco de Globos Arcoíris',
    description: 'Hermoso arco de globos con colores del arcoíris, perfecto para fiestas infantiles',
    price: 45000,
    discountPrice: 38000,
    stock: 15,
    category: 'Arcos',
    image: 'rainbow-balloon-arch',
    hasPromotion: true,
    promotionEndDate: '2026-04-15',
    isActive: true
  },
  {
    id: '2',
    name: 'Bouquet de Globos Metálicos',
    description: 'Set de 12 globos metálicos dorados y plateados',
    price: 25000,
    stock: 30,
    category: 'Bouquets',
    image: 'metallic-balloons',
    isActive: true
  },
  {
    id: '3',
    name: 'Globos con Helio - Pack 20',
    description: 'Pack de 20 globos inflados con helio en colores variados',
    price: 18000,
    stock: 0,
    category: 'Packs',
    image: 'helium-balloons',
    isActive: true
  },
  {
    id: '4',
    name: 'Globo Número Gigante',
    description: 'Globo número personalizable de 1 metro de altura',
    price: 12000,
    stock: 25,
    category: 'Números',
    image: 'number-balloon',
    isActive: true
  },
  {
    id: '5',
    name: 'Set Fiesta Unicornio',
    description: 'Set completo con globos, banderines y decoración temática de unicornio',
    price: 55000,
    discountPrice: 48000,
    stock: 8,
    category: 'Sets Temáticos',
    image: 'unicorn-party',
    hasPromotion: true,
    promotionEndDate: '2026-04-10',
    isActive: true
  },
  {
    id: '6',
    name: 'Globos LED Luminosos',
    description: 'Pack de 10 globos con luces LED internas, perfectos para eventos nocturnos',
    price: 32000,
    stock: 12,
    category: 'Especiales',
    image: 'led-balloons',
    isActive: true
  }
];

// Usuarios mock
export const mockUsers: User[] = [
  {
    id: '1',
    email: 'cliente@test.com',
    password: '123456',
    name: 'Juan Pérez',
    role: 'cliente',
    phone: '8888-8888',
    address: 'San José, Costa Rica',
    failedAttempts: 0,
    isBlocked: false
  },
  {
    id: '2',
    email: 'admin@happytimes.com',
    password: 'admin123',
    name: 'María González',
    role: 'admin',
    phone: '8877-7777',
    failedAttempts: 0,
    isBlocked: false
  },
  {
    id: '3',
    email: 'staff@happytimes.com',
    password: 'staff123',
    name: 'Carlos Rodríguez',
    role: 'staff',
    phone: '8866-6666',
    failedAttempts: 0,
    isBlocked: false
  }
];

// Zonas de entrega
export const mockDeliveryZones: DeliveryZone[] = [
  { id: '1', name: 'San José Centro', cost: 2000, isAvailable: true },
  { id: '2', name: 'Escazú', cost: 3500, isAvailable: true },
  { id: '3', name: 'Heredia', cost: 4000, isAvailable: true },
  { id: '4', name: 'Alajuela', cost: 5000, isAvailable: true },
  { id: '5', name: 'Cartago', cost: 4500, isAvailable: true },
  { id: '6', name: 'Zona no disponible', cost: 0, isAvailable: false }
];

// Configuración del sistema
export const mockSystemConfig: SystemConfig = {
  minStock: 5,
  maxLoginAttempts: 3,
  blockDurationMinutes: 30
};

// Pedidos mock (para reportes)
export const mockOrders: Order[] = [
  {
    id: 'ORD-001',
    userId: '1',
    items: [
      { product: mockProducts[0], quantity: 1 },
      { product: mockProducts[1], quantity: 2 }
    ],
    total: 88000,
    status: 'entregado',
    paymentMethod: 'sinpe',
    deliveryZone: 'San José Centro',
    deliveryCost: 2000,
    deliveryAddress: 'Avenida Central, San José',
    createdAt: '2026-03-15T10:30:00'
  },
  {
    id: 'ORD-002',
    userId: '1',
    items: [
      { product: mockProducts[4], quantity: 1 }
    ],
    total: 52000,
    status: 'enviado',
    paymentMethod: 'tarjeta',
    deliveryZone: 'Escazú',
    deliveryCost: 3500,
    deliveryAddress: 'Multiplaza, Escazú',
    createdAt: '2026-03-25T14:20:00'
  },
  {
    id: 'ORD-003',
    userId: '1',
    items: [
      { product: mockProducts[1], quantity: 3 },
      { product: mockProducts[3], quantity: 2 }
    ],
    total: 103000,
    status: 'pagado',
    paymentMethod: 'sinpe',
    deliveryZone: 'Heredia',
    deliveryCost: 4000,
    deliveryAddress: 'Universidad Nacional, Heredia',
    createdAt: '2026-03-28T09:15:00'
  },
  {
    id: 'ORD-004',
    userId: '3',
    items: [
      { product: mockProducts[5], quantity: 1 },
      { product: mockProducts[0], quantity: 1 }
    ],
    total: 82500,
    status: 'confirmado',
    paymentMethod: 'sinpe',
    deliveryZone: 'Alajuela',
    deliveryCost: 5000,
    deliveryAddress: 'Centro de Alajuela',
    createdAt: '2026-03-30T16:45:00'
  },
  {
    id: 'ORD-005',
    userId: '1',
    items: [
      { product: mockProducts[4], quantity: 2 }
    ],
    total: 101500,
    status: 'pendiente',
    paymentMethod: 'tarjeta',
    deliveryZone: 'Cartago',
    deliveryCost: 4500,
    deliveryAddress: 'Basílica de Cartago',
    createdAt: '2026-03-31T11:00:00'
  }
];

export const mockPromotions: Promotion[] = [
  {
    id: '1',
    productId: '1',
    discountPercent: 15,
    startDate: '2026-03-01',
    endDate: '2026-04-15',
    isActive: true
  },
  {
    id: '2',
    productId: '5',
    discountPercent: 12,
    startDate: '2026-03-10',
    endDate: '2026-04-10',
    isActive: true
  }
];