import { createBrowserRouter, Outlet } from 'react-router';
import { Layout } from './components/Layout';
import { AppProvider } from './context/AppContext';
import Login from './pages/Login';
import Register from './pages/Register';
import Catalog from './pages/Catalog';
import ProductDetail from './pages/ProductDetail';
import Cart from './pages/Cart';
import Checkout from './pages/Checkout';
import AdminDashboard from './pages/AdminDashboard';
import ProductManagement from './pages/ProductManagement';
import OrderManagement from './pages/OrderManagement';
import CategoryManagement from './pages/CategoryManagement';
import MyOrders from './pages/MyOrders';

// Root component that provides context to all routes
function RootLayout() {
  return (
    <AppProvider>
      <Outlet />
    </AppProvider>
  );
}

export const router = createBrowserRouter([
  {
    Component: RootLayout,
    children: [
      {
        path: '/login',
        Component: Login,
      },
      {
        path: '/register',
        Component: Register,
      },
      {
        path: '/',
        Component: Layout,
        children: [
          { index: true, Component: Catalog },
          { path: 'product/:id', Component: ProductDetail },
          { path: 'cart', Component: Cart },
          { path: 'checkout', Component: Checkout },
          { path: 'my-orders', Component: MyOrders },
          { path: 'admin', Component: AdminDashboard },
          { path: 'admin/products', Component: ProductManagement },
          { path: 'admin/orders', Component: OrderManagement },
          { path: 'admin/categories', Component: CategoryManagement },
        ],
      },
    ],
  },
]);