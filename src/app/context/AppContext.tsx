import React, { createContext, useContext, useState, ReactNode } from 'react';
import { 
  User, 
  Product, 
  ProductImage,
  Category,
  CartItem, 
  Order, 
  DeliveryZone,
  SystemConfig,
  Promotion,
  mockUsers, 
  mockProducts,
  mockCategories,
  mockDeliveryZones,
  mockSystemConfig,
  mockOrders,
  mockPromotions
} from '../data/mockData';

interface AppContextType {
  // Auth
  currentUser: User | null;
  login: (email: string, password: string) => { success: boolean; message: string };
  logout: () => void;
  register: (userData: Partial<User>) => { success: boolean; message: string };
  
  // Cart
  cart: CartItem[];
  addToCart: (product: Product, quantity: number) => { success: boolean; message: string };
  removeFromCart: (productId: string) => void;
  updateCartQuantity: (productId: string, quantity: number) => void;
  clearCart: () => void;
  
  // Products
  products: Product[];
  getProductById: (id: string) => Product | undefined;
  createProduct: (product: Omit<Product, 'id'>) => { success: boolean; message: string; productId?: string };
  updateProduct: (id: string, product: Partial<Product>) => { success: boolean; message: string };
  toggleProductStatus: (id: string) => { success: boolean; message: string };
  
  // Categories (HU-CAT-001 a HU-CAT-004)
  categories: Category[];
  createCategory: (category: Omit<Category, 'id' | 'createdAt'>) => { success: boolean; message: string; categoryId?: string };
  updateCategory: (id: string, category: Partial<Category>) => { success: boolean; message: string };
  toggleCategoryStatus: (id: string) => { success: boolean; message: string };
  getCategoryProductCount: (categoryName: string) => number;
  
  // Orders
  orders: Order[];
  createOrder: (orderData: Partial<Order>) => { success: boolean; message: string; orderId?: string };
  updateOrderStatus: (orderId: string, status: Order['status']) => { success: boolean; message: string };
  getOrdersByUser: (userId: string) => Order[];
  
  // Delivery
  deliveryZones: DeliveryZone[];
  selectedZone: DeliveryZone | null;
  setSelectedZone: (zone: DeliveryZone | null) => void;
  
  // Config (Admin)
  systemConfig: SystemConfig;
  updateSystemConfig: (config: Partial<SystemConfig>) => { success: boolean; message: string };
  
  // Promotions (Admin)
  promotions: Promotion[];
  createPromotion: (promo: Omit<Promotion, 'id'>) => { success: boolean; message: string };
  deletePromotion: (id: string) => void;
}

const AppContext = createContext<AppContextType | undefined>(undefined);

export function AppProvider({ children }: { children: ReactNode }) {
  const [currentUser, setCurrentUser] = useState<User | null>(null);
  const [cart, setCart] = useState<CartItem[]>([]);
  const [products, setProducts] = useState<Product[]>(mockProducts);
  const [categories, setCategories] = useState<Category[]>(mockCategories);
  const [orders, setOrders] = useState<Order[]>(mockOrders);
  const [deliveryZones] = useState<DeliveryZone[]>(mockDeliveryZones);
  const [selectedZone, setSelectedZone] = useState<DeliveryZone | null>(null);
  const [systemConfig, setSystemConfig] = useState<SystemConfig>(mockSystemConfig);
  const [promotions, setPromotions] = useState<Promotion[]>(mockPromotions);
  const [users, setUsers] = useState<User[]>(mockUsers);

  // HU-CAT-001: Crear categoría
  const createCategory = (category: Omit<Category, 'id' | 'createdAt'>): { success: boolean; message: string; categoryId?: string } => {
    // HU-CAT-001 Escenario 2: Nombre vacío
    if (!category.name.trim()) {
      return { success: false, message: 'El nombre de la categoría es requerido' };
    }

    // HU-CAT-001 Escenario 3: Nombre duplicado
    const exists = categories.some(c => c.name.toLowerCase() === category.name.trim().toLowerCase());
    if (exists) {
      return { success: false, message: `Ya existe una categoría con el nombre "${category.name}"` };
    }

    // HU-CAT-001 Escenario 1: Creación exitosa
    const newCategory: Category = {
      ...category,
      name: category.name.trim(),
      id: `cat-${Date.now()}`,
      createdAt: new Date().toISOString().split('T')[0]
    };

    setCategories([...categories, newCategory]);
    return { success: true, message: 'Categoría creada exitosamente', categoryId: newCategory.id };
  };

  // HU-CAT-003: Editar categoría
  const updateCategory = (id: string, category: Partial<Category>): { success: boolean; message: string } => {
    // HU-CAT-003 Escenario 2: Nombre vacío
    if (category.name !== undefined && !category.name.trim()) {
      return { success: false, message: 'El nombre de la categoría no puede estar vacío' };
    }

    // HU-CAT-003 Escenario 3: Nombre duplicado (distinto a la categoría actual)
    if (category.name) {
      const exists = categories.some(c => 
        c.id !== id && c.name.toLowerCase() === category.name!.trim().toLowerCase()
      );
      if (exists) {
        return { success: false, message: `Ya existe una categoría con el nombre "${category.name}"` };
      }
    }

    const currentCategory = categories.find(c => c.id === id);
    if (!currentCategory) {
      return { success: false, message: 'Categoría no encontrada' };
    }

    // HU-CAT-003 Escenario 1: Actualización exitosa + actualizar products si cambió el nombre
    const updatedCategories = categories.map(c => 
      c.id === id ? { ...c, ...category, name: category.name?.trim() || c.name } : c
    );
    setCategories(updatedCategories);

    // Si cambió el nombre, actualizar productos que la usen
    if (category.name && category.name.trim() !== currentCategory.name) {
      const updatedProducts = products.map(p => 
        p.category === currentCategory.name ? { ...p, category: category.name!.trim() } : p
      );
      setProducts(updatedProducts);
    }

    return { success: true, message: 'Categoría actualizada exitosamente' };
  };

  // HU-CAT-004: Inactivar categoría
  const toggleCategoryStatus = (id: string): { success: boolean; message: string } => {
    const category = categories.find(c => c.id === id);
    if (!category) {
      return { success: false, message: 'Categoría no encontrada' };
    }

    const updatedCategories = categories.map(c => 
      c.id === id ? { ...c, isActive: !c.isActive } : c
    );
    setCategories(updatedCategories);

    const newStatus = !category.isActive;
    return { 
      success: true, 
      message: newStatus ? 'Categoría activada exitosamente' : 'Categoría inactivada exitosamente'
    };
  };

  // HU-CAT-002: Contar productos por categoría
  const getCategoryProductCount = (categoryName: string): number => {
    return products.filter(p => p.category === categoryName && p.isActive !== false).length;
  };

  // HU-AUT-001 - Login con validación
  const login = (email: string, password: string): { success: boolean; message: string } => {
    const user = users.find(u => u.email === email);
    
    if (!user) {
      return { success: false, message: 'Credenciales incorrectas' };
    }

    // Escenario 3: Cuenta bloqueada
    if (user.isBlocked) {
      return { success: false, message: 'Cuenta bloqueada temporalmente por múltiples intentos fallidos' };
    }

    // Escenario 2: Credenciales incorrectas
    if (user.password !== password) {
      const updatedUsers = users.map(u => {
        if (u.id === user.id) {
          const newAttempts = u.failedAttempts + 1;
          // Escenario 3: Bloquear si supera el límite
          if (newAttempts >= systemConfig.maxLoginAttempts) {
            return { ...u, failedAttempts: newAttempts, isBlocked: true };
          }
          return { ...u, failedAttempts: newAttempts };
        }
        return u;
      });
      setUsers(updatedUsers);
      
      const remainingAttempts = systemConfig.maxLoginAttempts - (user.failedAttempts + 1);
      if (remainingAttempts > 0) {
        return { 
          success: false, 
          message: `Contraseña incorrecta. Te quedan ${remainingAttempts} intentos` 
        };
      } else {
        return { 
          success: false, 
          message: 'Cuenta bloqueada por múltiples intentos fallidos' 
        };
      }
    }

    // Escenario 1: Login exitoso
    const updatedUsers = users.map(u => 
      u.id === user.id ? { ...u, failedAttempts: 0 } : u
    );
    setUsers(updatedUsers);
    setCurrentUser(user);
    return { success: true, message: 'Inicio de sesión exitoso' };
  };

  const logout = () => {
    setCurrentUser(null);
    setCart([]);
    setSelectedZone(null);
  };

  // HU-CLI-001 - Registro de cliente
  const register = (userData: Partial<User>): { success: boolean; message: string } => {
    // Escenario 3: Datos incompletos
    if (!userData.email || !userData.password || !userData.name || !userData.phone) {
      return { success: false, message: 'Por favor completa todos los campos requeridos' };
    }

    // Validación de email
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(userData.email)) {
      return { success: false, message: 'Formato de correo electrónico inválido' };
    }

    // Escenario 2: Correo duplicado
    if (users.some(u => u.email === userData.email)) {
      return { success: false, message: 'Este correo electrónico ya está registrado' };
    }

    // Escenario 1: Registro exitoso
    const newUser: User = {
      id: `user-${Date.now()}`,
      email: userData.email,
      password: userData.password,
      name: userData.name,
      role: 'cliente',
      phone: userData.phone,
      address: userData.address,
      failedAttempts: 0,
      isBlocked: false
    };

    setUsers([...users, newUser]);
    setCurrentUser(newUser);
    return { success: true, message: 'Cuenta creada exitosamente' };
  };

  // HU-PED-001 - Gestión de carrito
  const addToCart = (product: Product, quantity: number): { success: boolean; message: string } => {
    // Escenario 2: Sin stock
    if (product.stock === 0) {
      return { success: false, message: 'Producto sin stock disponible' };
    }

    if (quantity > product.stock) {
      return { success: false, message: `Solo hay ${product.stock} unidades disponibles` };
    }

    // Escenario 3: Producto repetido - aumentar cantidad
    const existingItem = cart.find(item => item.product.id === product.id);
    if (existingItem) {
      const newQuantity = existingItem.quantity + quantity;
      if (newQuantity > product.stock) {
        return { success: false, message: `Stock insuficiente. Máximo ${product.stock} unidades` };
      }
      
      setCart(cart.map(item =>
        item.product.id === product.id
          ? { ...item, quantity: newQuantity }
          : item
      ));
      return { success: true, message: 'Cantidad actualizada en el carrito' };
    }

    // Escenario 1: Agregar nuevo producto
    setCart([...cart, { product, quantity }]);
    return { success: true, message: 'Producto agregado al carrito' };
  };

  const removeFromCart = (productId: string) => {
    setCart(cart.filter(item => item.product.id !== productId));
  };

  const updateCartQuantity = (productId: string, quantity: number) => {
    if (quantity <= 0) {
      removeFromCart(productId);
      return;
    }

    setCart(cart.map(item => {
      if (item.product.id === productId) {
        const maxQty = Math.min(quantity, item.product.stock);
        return { ...item, quantity: maxQty };
      }
      return item;
    }));
  };

  const clearCart = () => {
    setCart([]);
  };

  const getProductById = (id: string): Product | undefined => {
    return products.find(p => p.id === id);
  };

  // HU-PED-001 + HU-PAG-001 - Crear pedido
  const createOrder = (orderData: Partial<Order>): { success: boolean; message: string; orderId?: string } => {
    if (!currentUser) {
      return { success: false, message: 'Debes iniciar sesión para realizar un pedido' };
    }

    // HU-PED-002 Escenario 2: Datos válidos
    if (cart.length === 0) {
      return { success: false, message: 'El carrito está vacío' };
    }

    const orderId = `ORD-${Date.now()}`;
    const newOrder: Order = {
      id: orderId,
      userId: currentUser.id,
      items: [...cart],
      total: orderData.total || 0,
      status: orderData.status || 'pendiente',
      paymentMethod: orderData.paymentMethod,
      deliveryZone: orderData.deliveryZone,
      deliveryCost: orderData.deliveryCost,
      deliveryAddress: orderData.deliveryAddress,
      deliveryNotes: orderData.deliveryNotes,
      createdAt: new Date().toISOString()
    };

    setOrders([...orders, newOrder]);
    clearCart();
    setSelectedZone(null);
    
    return { success: true, message: 'Pedido creado exitosamente', orderId };
  };

  // HU-PED-001 + HU-PAG-001 - Actualizar estado de pedido
  const updateOrderStatus = (orderId: string, status: Order['status']): { success: boolean; message: string } => {
    const updatedOrders = orders.map(order => 
      order.id === orderId ? { ...order, status } : order
    );
    setOrders(updatedOrders);
    return { success: true, message: 'Estado del pedido actualizado exitosamente' };
  };

  // HU-PED-001 + HU-PAG-001 - Obtener pedidos por usuario
  const getOrdersByUser = (userId: string): Order[] => {
    return orders.filter(order => order.userId === userId);
  };

  // HU-CFG-001 - Actualizar configuración (Admin)
  const updateSystemConfig = (config: Partial<SystemConfig>): { success: boolean; message: string } => {
    // Escenario 4: Datos inválidos
    if (config.minStock !== undefined && config.minStock < 0) {
      return { success: false, message: 'El stock mínimo no puede ser negativo' };
    }
    if (config.maxLoginAttempts !== undefined && config.maxLoginAttempts < 1) {
      return { success: false, message: 'Los intentos máximos deben ser al menos 1' };
    }
    if (config.blockDurationMinutes !== undefined && config.blockDurationMinutes < 1) {
      return { success: false, message: 'La duración de bloqueo debe ser al menos 1 minuto' };
    }

    // Escenario 1: Actualización exitosa
    setSystemConfig({ ...systemConfig, ...config });
    return { success: true, message: 'Configuración actualizada correctamente' };
  };

  // HU-PRM-001 - Gestionar promociones
  const createPromotion = (promo: Omit<Promotion, 'id'>): { success: boolean; message: string } => {
    // Escenario 4: Datos inválidos
    if (promo.discountPercent <= 0 || promo.discountPercent > 100) {
      return { success: false, message: 'El descuento debe estar entre 1% y 100%' };
    }

    const startDate = new Date(promo.startDate);
    const endDate = new Date(promo.endDate);
    
    if (endDate <= startDate) {
      return { success: false, message: 'La fecha de fin debe ser posterior a la fecha de inicio' };
    }

    // Escenario 1: Promoción creada
    const newPromo: Promotion = {
      ...promo,
      id: `promo-${Date.now()}`
    };

    setPromotions([...promotions, newPromo]);
    return { success: true, message: 'Promoción creada exitosamente' };
  };

  const deletePromotion = (id: string) => {
    setPromotions(promotions.filter(p => p.id !== id));
  };

  // HU-PRO-001 - Gestionar productos
  const createProduct = (product: Omit<Product, 'id'>): { success: boolean; message: string; productId?: string } => {
    // Escenario 4: Datos inválidos
    if (product.name.trim() === '') {
      return { success: false, message: 'El nombre del producto no puede estar vacío' };
    }
    if (product.price < 0) {
      return { success: false, message: 'El precio no puede ser negativo' };
    }
    if (product.stock < 0) {
      return { success: false, message: 'El stock no puede ser negativo' };
    }

    // Escenario 1: Producto creado
    const newProduct: Product = {
      ...product,
      id: `product-${Date.now()}`
    };

    setProducts([...products, newProduct]);
    return { success: true, message: 'Producto creado exitosamente', productId: newProduct.id };
  };

  const updateProduct = (id: string, product: Partial<Product>): { success: boolean; message: string } => {
    // Escenario 4: Datos inválidos
    if (product.name && product.name.trim() === '') {
      return { success: false, message: 'El nombre del producto no puede estar vacío' };
    }
    if (product.price !== undefined && product.price < 0) {
      return { success: false, message: 'El precio no puede ser negativo' };
    }
    if (product.stock !== undefined && product.stock < 0) {
      return { success: false, message: 'El stock no puede ser negativo' };
    }

    // Escenario 1: Producto actualizado
    const updatedProducts = products.map(p => 
      p.id === id ? { ...p, ...product } : p
    );
    setProducts(updatedProducts);
    return { success: true, message: 'Producto actualizado exitosamente' };
  };

  const toggleProductStatus = (id: string): { success: boolean; message: string } => {
    // Escenario 1: Estado del producto cambiado
    const updatedProducts = products.map(p => 
      p.id === id ? { ...p, isActive: !p.isActive } : p
    );
    setProducts(updatedProducts);
    return { success: true, message: 'Estado del producto cambiado exitosamente' };
  };

  return (
    <AppContext.Provider
      value={{
        currentUser,
        login,
        logout,
        register,
        cart,
        addToCart,
        removeFromCart,
        updateCartQuantity,
        clearCart,
        products,
        getProductById,
        createProduct,
        updateProduct,
        toggleProductStatus,
        categories,
        createCategory,
        updateCategory,
        toggleCategoryStatus,
        getCategoryProductCount,
        orders,
        createOrder,
        updateOrderStatus,
        getOrdersByUser,
        deliveryZones,
        selectedZone,
        setSelectedZone,
        systemConfig,
        updateSystemConfig,
        promotions,
        createPromotion,
        deletePromotion
      }}
    >
      {children}
    </AppContext.Provider>
  );
}

export function useApp() {
  const context = useContext(AppContext);
  if (context === undefined) {
    throw new Error('useApp must be used within an AppProvider');
  }
  return context;
}