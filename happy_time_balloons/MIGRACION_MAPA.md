# Mapa de Migración — Happy Times Balloons (rama `prototipo`)

> Reconocimiento estático de la rama `prototipo`. Sin modificaciones de código.
> Fecha de análisis: 2026-05-20

---

## 1. Pantallas / Páginas

Todas las rutas están definidas en `src/app/routes.tsx` usando `createBrowserRouter` de React Router v7.

| Ruta | Archivo | Historia de Usuario | Acceso |
|---|---|---|---|
| `/login` | `src/app/pages/Login.tsx` | HU-AUT-001 | Público |
| `/register` | `src/app/pages/Register.tsx` | HU-CLI-001 | Público |
| `/` (index) | `src/app/pages/Catalog.tsx` | HU-CAT-001 (catálogo) | Público |
| `/product/:id` | `src/app/pages/ProductDetail.tsx` | HU-CAT-001 Esc.2 + HU-IMG | Público |
| `/cart` | `src/app/pages/Cart.tsx` | HU-PED-001 | Público (carrito vacío si no hay sesión) |
| `/checkout` | `src/app/pages/Checkout.tsx` | HU-PED-002 + HU-PAG-001 | Requiere login |
| `/my-orders` | `src/app/pages/MyOrders.tsx` | HU-PED-003 | Requiere login (rol: cliente) |
| `/admin` | `src/app/pages/AdminDashboard.tsx` | HU-CFG-001 + HU-REP-001 + HU-PRM-001 | Requiere login (rol: admin) |
| `/admin/products` | `src/app/pages/ProductManagement.tsx` | HU-PRO-001..004 + HU-IMG + HU-CAT | Requiere login (rol: admin o staff) |
| `/admin/orders` | `src/app/pages/OrderManagement.tsx` | HU-PED-004 | Requiere login (rol: admin o staff) |
| `/admin/categories` | `src/app/pages/CategoryManagement.tsx` | HU-CAT-001..004 | Requiere login (rol: admin) |

### Estructura del router

```
RootLayout (AppProvider)
└── /login            → Login
└── /register         → Register
└── /                 → Layout (Header + Toaster)
    ├── (index)       → Catalog
    ├── product/:id   → ProductDetail
    ├── cart          → Cart
    ├── checkout      → Checkout
    ├── my-orders     → MyOrders
    ├── admin         → AdminDashboard
    ├── admin/products → ProductManagement
    ├── admin/orders  → OrderManagement
    └── admin/categories → CategoryManagement
```

---

## 2. Componentes Reutilizables

### 2.1 Componentes de la aplicación (`src/app/components/`)

| Componente | Archivo | Propósito |
|---|---|---|
| `Header` | `components/Header.tsx` | Navbar global: logo, búsqueda, carrito con badge, menú de usuario con dropdown. Detecta rol para mostrar enlaces admin/staff/cliente. |
| `Layout` | `components/Layout.tsx` | Wrapper raíz de rutas autenticadas: monta `<Header>`, `<Outlet>` y `<Toaster>`. |
| `ProductCard` | `components/ProductCard.tsx` | Tarjeta de producto para el catálogo: imagen (galería o fallback Unsplash), badges de promoción/stock, precio con descuento, botón "Agregar al carrito". |
| `ImageManager` | `components/ImageManager.tsx` | Gestión de galería de imágenes de producto: drag-and-drop, validación de formato/tamaño (JPG/PNG/WEBP, máx 5 MB), marcado de imagen principal, reordenamiento (↑/↓), eliminación. HU-IMG-001..004. |
| `ImageWithFallback` | `components/figma/ImageWithFallback.tsx` | `<img>` con estado de error: si la imagen falla, muestra un SVG placeholder inline en base64. |

### 2.2 Componentes UI (shadcn/ui — `src/app/components/ui/`)

Librería de primitivos basada en Radix UI + Tailwind. No contienen lógica de negocio.

`accordion` · `alert-dialog` · `alert` · `aspect-ratio` · `avatar` · `badge` · `breadcrumb` · `button` · `calendar` · `card` · `carousel` · `chart` · `checkbox` · `collapsible` · `command` · `context-menu` · `dialog` · `drawer` · `dropdown-menu` · `form` · `hover-card` · `input-otp` · `input` · `label` · `menubar` · `navigation-menu` · `pagination` · `popover` · `progress` · `radio-group` · `resizable` · `scroll-area` · `select` · `separator` · `sheet` · `sidebar` · `skeleton` · `slider` · `sonner` · `switch` · `table` · `tabs` · `textarea` · `toggle-group` · `toggle` · `tooltip`

Utilidades: `use-mobile.ts` (hook breakpoint móvil) · `utils.ts` (función `cn` de clsx + tailwind-merge)

---

## 3. Modelos de Datos

Definidos como interfaces TypeScript en `src/app/data/mockData.ts`.

### `Category`
```ts
{
  id: string;
  name: string;
  description?: string;
  isActive: boolean;
  createdAt: string;          // "YYYY-MM-DD"
}
```

### `Product`
```ts
{
  id: string;
  name: string;
  description: string;
  price: number;              // precio base en colones (₡)
  discountPrice?: number;     // precio con descuento
  stock: number;
  category: string;           // nombre de categoría (denormalizado)
  image: string;              // slug para Unsplash fallback
  images?: ProductImage[];    // galería (HU-IMG)
  hasPromotion?: boolean;
  promotionEndDate?: string;  // "YYYY-MM-DD"
  isActive?: boolean;
}
```

### `ProductImage`
```ts
{
  id: string;
  url: string;                // base64 (Data URL) o URL externa
  isPrimary: boolean;
  order: number;
}
```

### `User`
```ts
{
  id: string;
  email: string;
  password: string;           // ⚠ en claro (solo prototipo)
  name: string;
  role: 'cliente' | 'admin' | 'staff';  // ⚠ ver nota de migración abajo
  phone: string;
  address?: string;
  failedAttempts: number;
  isBlocked: boolean;
}
```

> ⚠️ **Inconsistencia de roles — decisión de migración**: El prototipo usa `'staff'` pero el CLAUDE.md define el rol como `'Operador'`. **Nombre canónico para la migración: `Operador`**. Al crear los roles de ASP.NET Identity usar `"Administrador"`, `"Operador"` y `"Cliente"`. Todos los `[Authorize(Roles = "...")]` en los controladores deben usar estos nombres exactos. Los permisos de `staff` en el prototipo (gestión de productos y pedidos, sin acceso a categorías ni configuración) se mapean íntegramente al rol `Operador`.

### `CartItem`
```ts
{
  product: Product;           // objeto completo embebido
  quantity: number;
}
```

### `Order`
```ts
{
  id: string;                 // "ORD-{timestamp}"
  userId: string;
  items: CartItem[];
  total: number;
  status: 'pendiente' | 'pagado' | 'confirmado' | 'enviado' | 'entregado';
  paymentMethod?: 'sinpe' | 'tarjeta';
  deliveryZone?: string;
  deliveryCost?: number;
  deliveryAddress?: string;
  deliveryNotes?: string;
  createdAt: string;          // ISO 8601
  updatedAt?: string;
}
```

### `DeliveryZone`
```ts
{
  id: string;
  name: string;
  cost: number;               // costo en colones
  isAvailable: boolean;
}
```

### `Promotion`
```ts
{
  id: string;
  productId: string;
  discountPercent: number;    // 1..100
  startDate: string;          // "YYYY-MM-DD"
  endDate: string;
  isActive: boolean;
}
```

### `SystemConfig`
```ts
{
  minStock: number;           // alerta de stock bajo
  maxLoginAttempts: number;   // intentos antes de bloqueo
  blockDurationMinutes: number;
}
```

> ⚠️ **Decisión de migración — SystemConfig**: En el prototipo este objeto vive en `useState` (volátil). En .NET se recomienda persistirlo en una tabla SQL `ConfiguracionSistema` (una fila por clave-valor o una sola fila con todas las columnas), gestionada desde el `AdminDashboard`. Esto permite modificarla en tiempo de ejecución sin redespliegue, a diferencia de `AppSettings` en `web.config`.

---

### `BitacoraAuditoria` *(no existe en el prototipo — entidad nueva requerida por arquitectura)*

> Esta entidad **no forma parte del prototipo React** porque el prototipo no tiene backend. Sin embargo, el CLAUDE.md exige registrar auditoría para todas las operaciones CRUD en tablas críticas. Debe crearse como tabla transversal desde el inicio de la migración.

```csharp
// Equivalente C# — modelo EF6 en AccesoADatos/Modelos
{
  Id:              int (PK, identity)
  UsuarioId:       string              // FK → AspNetUsers.Id
  NombreUsuario:   string              // desnormalizado para historial
  FechaHoraUtc:    DateTime            // UTC, no local
  TipoOperacion:   string             // "Crear" | "Leer" | "Actualizar" | "Eliminar"
  TablaAfectada:   string             // e.g. "Productos", "Pedidos"
  RegistroId:      string             // ID del registro afectado (como string para versatilidad)
  Detalle:         string (nullable)  // JSON o texto libre con el before/after
}
```

**Tablas críticas que deben auditarse:** `Productos`, `Pedidos`, `Categorias`, `Usuarios`, `Promociones`, `ConfiguracionSistema`.

---

## 4. Llamadas a API / Fetch

### Resultado: **No existen llamadas a API reales.**

El prototipo es 100 % frontend con estado en memoria (React `useState`). Toda la persistencia vive en `src/app/context/AppContext.tsx` mediante arrays inicializados desde `mockData.ts`.

| Operación | Mecanismo real |
|---|---|
| Login / Logout / Register | Mutación de estado local en `AppContext` |
| CRUD Productos | `useState<Product[]>` en `AppContext` |
| CRUD Categorías | `useState<Category[]>` en `AppContext` |
| Crear / actualizar Pedidos | `useState<Order[]>` en `AppContext` |
| Carrito | `useState<CartItem[]>` en `AppContext` |
| Configuración del sistema | `useState<SystemConfig>` en `AppContext` |
| Promociones | `useState<Promotion[]>` en `AppContext` |
| Imágenes de producto | `FileReader.readAsDataURL()` → base64 en memoria (no se persiste) |
| Imágenes de fallback | URL construida a `https://source.unsplash.com/{size}/?{slug},balloons` |

> **Implicación para migración**: al conectar un backend real se deben reemplazar todas las funciones del contexto (`login`, `createProduct`, `createOrder`, etc.) por llamadas `fetch`/`axios` a los endpoints correspondientes.

---

## 5. Flujos de Navegación

### 5.1 Flujo de Autenticación

```
[Cualquier pantalla]
    ├── Clic "Iniciar sesión" → /login
    │       ├── Credenciales válidas → / (Catalog)
    │       ├── Error / cuenta bloqueada → mensaje en pantalla
    │       └── "Continuar sin sesión" → / (Catalog)
    └── Clic "Regístrate" → /register
            ├── Registro exitoso → / (Catalog) + setCurrentUser
            └── Email duplicado → mensaje en pantalla
```

### 5.2 Flujo de Compra (Cliente)

```
/ (Catalog)
    ├── Buscar / filtrar por categoría (estado local, sin navegación)
    ├── Clic tarjeta producto → /product/:id (ProductDetail)
    │       ├── Galería de imágenes (navegación interna con índice)
    │       ├── Selector de cantidad
    │       └── "Agregar al carrito" → actualiza AppContext.cart
    │
    ├── "Agregar al carrito" directo desde ProductCard → actualiza AppContext.cart
    │
    └── Icono carrito (Header) → /cart
            ├── Modificar cantidades / eliminar ítems
            ├── Carrito vacío → botón "Ver catálogo" → /
            └── "Proceder al pago" → /checkout
                    ├── Seleccionar zona de entrega
                    ├── Ingresar dirección y notas
                    ├── Elegir método de pago (SINPE / Tarjeta)
                    ├── Confirmar pedido → crea Order en AppContext, vacía carrito
                    │       └── Redirige a /my-orders
                    └── "Volver al carrito" → /cart
```

### 5.3 Flujo de Mis Pedidos (Cliente)

```
Header → "Mis Pedidos" (solo visible si rol = cliente) → /my-orders
    ├── Sin pedidos → botón "Ver catálogo" → /
    └── Lista de pedidos con seguimiento visual de estados:
        pendiente → pagado → confirmado → enviado → entregado
```

### 5.4 Flujo Administrativo

```
Header (rol admin/staff) → menú dropdown
    ├── Dashboard → /admin
    │       Tabs: Reportes | Productos* | Promociones | Configuración
    │       (* Tab Productos redirige programáticamente a /admin/products)
    │
    ├── Productos → /admin/products
    │       ├── Buscar / filtrar por categoría
    │       ├── Ver detalle (Dialog)
    │       ├── Crear producto (Dialog + ImageManager)
    │       ├── Editar producto (Dialog + ImageManager)
    │       └── Activar / desactivar (Dialog de confirmación)
    │
    ├── Pedidos → /admin/orders  (admin y staff)
    │       ├── Buscar por ID / filtrar por estado
    │       ├── Ver detalle completo (Dialog)
    │       └── Cambiar estado del pedido (Dialog de confirmación)
    │
    └── Categorías → /admin/categories  (solo admin)
            ├── Buscar categorías
            ├── Crear categoría (Dialog)
            ├── Editar nombre/descripción (Dialog)
            └── Activar / desactivar (Dialog de confirmación)
```

### 5.5 Redirecciones de Protección de Rutas

| Condición | Pantalla intentada | Redirección |
|---|---|---|
| No autenticado | `/my-orders` | `/login` (useEffect) |
| No autenticado o rol incorrecto | `/admin` | `/` (useEffect) |
| No autenticado o rol incorrecto | `/admin/products` | `/` (useEffect) |
| No autenticado o rol incorrecto | `/admin/orders` | `/` (useEffect) |
| Rol distinto de admin | `/admin/categories` | `/` (useEffect) |

> **Nota prototipo**: la protección se implementa con `useEffect` + `navigate` dentro de cada página, no con un componente `<PrivateRoute>` centralizado.
>
> **Migración → .NET**: consolidar toda la protección de rutas con atributos `[Authorize(Roles = "...")]` en los controladores. Usar los nombres canónicos: `Administrador`, `Operador`, `Cliente`. Referencia rápida:
>
> | Ruta React | Controlador/Acción MVC | Roles permitidos |
> |---|---|---|
> | `/login` `/register` | `AuthController` | Público |
> | `/` `/product/:id` | `CatalogoController` | Público |
> | `/cart` `/checkout` | `PedidoController` | `Cliente` (checkout) |
> | `/my-orders` | `PedidoController.MisPedidos` | `Cliente` |
> | `/admin` | `AdminController.Dashboard` | `Administrador` |
> | `/admin/products` | `ProductoController` (admin) | `Administrador`, `Operador` |
> | `/admin/orders` | `PedidoController` (admin) | `Administrador`, `Operador` |
> | `/admin/categories` | `CategoriaController` | `Administrador` |

---

## Resumen Técnico del Stack (rama `prototipo`)

| Categoría | Tecnología |
|---|---|
| Framework UI | React 18.3 + TypeScript |
| Bundler | Vite 6.3 |
| Router | React Router v7 (`createBrowserRouter`) |
| Estado global | React Context API + `useState` (sin Redux / Zustand) |
| Estilos | Tailwind CSS v4 |
| Componentes UI | shadcn/ui (Radix UI + CVA) |
| Notificaciones | Sonner (toasts) |
| Iconos | Lucide React |
| Backend / API | **Ninguno** — todo es mock en memoria |
| Persistencia | **Ninguna** — se pierde al recargar la página |
