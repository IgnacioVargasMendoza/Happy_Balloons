import React from 'react';
import { Link, useNavigate } from 'react-router';
import { useApp } from '../context/AppContext';
import { ShoppingCart, User, LogOut, LayoutDashboard, PartyPopper, Package, ShoppingBag, ClipboardList, FileText, Tag } from 'lucide-react';
import { Button } from '../components/ui/button';
import { Badge } from '../components/ui/badge';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
  DropdownMenuLabel,
} from '../components/ui/dropdown-menu';

export function Header() {
  const { currentUser, cart, logout } = useApp();
  const navigate = useNavigate();

  const cartItemsCount = cart.reduce((sum, item) => sum + item.quantity, 0);

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <header className="sticky top-0 z-50 w-full border-b bg-white shadow-sm">
      <div className="container mx-auto px-4 py-3">
        <div className="flex items-center justify-between">
          {/* Logo y nombre */}
          <Link to="/" className="flex items-center gap-2 hover:opacity-80 transition">
            <PartyPopper className="h-8 w-8 text-pink-500" />
            <div>
              <h1 className="font-bold text-lg leading-none">Happy Times</h1>
              <p className="text-xs text-muted-foreground">Balloons</p>
            </div>
          </Link>

          {/* Navegación */}
          <nav className="hidden md:flex items-center gap-6">
            <Link to="/" className="text-sm hover:text-pink-500 transition">
              Catálogo
            </Link>
            {currentUser?.role === 'admin' && (
              <Link to="/admin" className="text-sm hover:text-pink-500 transition">
                Administración
              </Link>
            )}
          </nav>

          {/* Acciones de usuario */}
          <div className="flex items-center gap-2">
            {/* Mis Pedidos - Solo para clientes */}
            {currentUser?.role === 'cliente' && (
              <Button
                variant="ghost"
                size="sm"
                className="hidden sm:flex items-center gap-2"
                onClick={() => navigate('/my-orders')}
              >
                <FileText className="h-4 w-4" />
                <span className="hidden lg:inline">Mis Pedidos</span>
              </Button>
            )}

            {/* Carrito */}
            <Button
              variant="ghost"
              size="icon"
              className="relative"
              onClick={() => navigate('/cart')}
            >
              <ShoppingCart className="h-5 w-5" />
              {cartItemsCount > 0 && (
                <Badge
                  className="absolute -top-1 -right-1 h-5 w-5 flex items-center justify-center p-0 text-xs"
                  variant="destructive"
                >
                  {cartItemsCount}
                </Badge>
              )}
            </Button>

            {/* Usuario */}
            {currentUser ? (
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <Button variant="ghost" size="icon">
                    <User className="h-5 w-5" />
                  </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end" className="w-56">
                  <div className="flex flex-col space-y-1 p-2">
                    <p className="text-sm font-medium">{currentUser.name}</p>
                    <p className="text-xs text-muted-foreground">{currentUser.email}</p>
                    <Badge variant="outline" className="w-fit mt-1">
                      {currentUser.role.charAt(0).toUpperCase() + currentUser.role.slice(1)}
                    </Badge>
                  </div>
                  <DropdownMenuSeparator />
                  {currentUser.role === 'cliente' && (
                    <>
                      <DropdownMenuItem onClick={() => navigate('/my-orders')}>
                        <ClipboardList className="mr-2 h-4 w-4" />
                        Mis pedidos
                      </DropdownMenuItem>
                      <DropdownMenuSeparator />
                    </>
                  )}
                  {(currentUser.role === 'admin' || currentUser.role === 'staff') && (
                    <>
                      <DropdownMenuLabel className="text-xs text-muted-foreground font-normal">
                        Panel Administrativo
                      </DropdownMenuLabel>
                      <DropdownMenuItem onClick={() => navigate('/admin')}>
                        <LayoutDashboard className="mr-2 h-4 w-4" />
                        Dashboard
                      </DropdownMenuItem>
                      <DropdownMenuItem onClick={() => navigate('/admin/products')}>
                        <Package className="mr-2 h-4 w-4" />
                        Productos
                      </DropdownMenuItem>
                      {currentUser.role === 'admin' && (
                        <DropdownMenuItem onClick={() => navigate('/admin/categories')}>
                          <Tag className="mr-2 h-4 w-4" />
                          Categorías
                        </DropdownMenuItem>
                      )}
                      <DropdownMenuItem onClick={() => navigate('/admin/orders')}>
                        <ShoppingBag className="mr-2 h-4 w-4" />
                        Pedidos
                      </DropdownMenuItem>
                      <DropdownMenuSeparator />
                    </>
                  )}
                  <DropdownMenuItem onClick={handleLogout}>
                    <LogOut className="mr-2 h-4 w-4" />
                    Cerrar sesión
                  </DropdownMenuItem>
                </DropdownMenuContent>
              </DropdownMenu>
            ) : (
              <Button
                variant="default"
                size="sm"
                onClick={() => navigate('/login')}
              >
                Iniciar sesión
              </Button>
            )}
          </div>
        </div>
      </div>
    </header>
  );
}