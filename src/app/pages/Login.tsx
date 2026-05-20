import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router';
import { useApp } from '../context/AppContext';
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '../components/ui/card';
import { Input } from '../components/ui/input';
import { Button } from '../components/ui/button';
import { Label } from '../components/ui/label';
import { Alert, AlertDescription } from '../components/ui/alert';
import { PartyPopper, AlertCircle, Lock } from 'lucide-react';

// HU-AUT-001: Inicio de sesión con todos los escenarios
export default function Login() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  
  const { login } = useApp();
  const navigate = useNavigate();

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setIsLoading(true);

    // Simular delay de red
    setTimeout(() => {
      const result = login(email, password);
      
      if (result.success) {
        // Escenario 1: Login exitoso
        navigate('/');
      } else {
        // Escenario 2 y 3: Error de credenciales o cuenta bloqueada
        setError(result.message);
      }
      
      setIsLoading(false);
    }, 800);
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-pink-50 via-purple-50 to-blue-50 p-4">
      <div className="w-full max-w-md">
        {/* Logo */}
        <div className="flex justify-center mb-8">
          <div className="flex items-center gap-2">
            <PartyPopper className="h-12 w-12 text-pink-500" />
            <div>
              <h1 className="font-bold text-2xl leading-none">Happy Times</h1>
              <p className="text-sm text-muted-foreground">Balloons</p>
            </div>
          </div>
        </div>

        <Card>
          <CardHeader>
            <CardTitle>Iniciar sesión</CardTitle>
            <CardDescription>
              Ingresa tus credenciales para acceder al sistema
            </CardDescription>
          </CardHeader>

          <form onSubmit={handleSubmit}>
            <CardContent className="space-y-4">
              {/* Mensaje de error - Escenarios 2 y 3 */}
              {error && (
                <Alert variant="destructive">
                  <AlertCircle className="h-4 w-4" />
                  <AlertDescription>{error}</AlertDescription>
                </Alert>
              )}

              <div className="space-y-2">
                <Label htmlFor="email">Correo electrónico</Label>
                <Input
                  id="email"
                  type="email"
                  placeholder="ejemplo@correo.com"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  required
                  autoComplete="email"
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="password">Contraseña</Label>
                <Input
                  id="password"
                  type="password"
                  placeholder="••••••••"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  required
                  autoComplete="current-password"
                />
              </div>

              {/* Cuentas de prueba */}
              <div className="rounded-lg bg-blue-50 p-3 text-sm">
                <p className="font-semibold mb-2">Cuentas de prueba:</p>
                <div className="space-y-1 text-xs">
                  <p><strong>Cliente:</strong> cliente@test.com / 123456</p>
                  <p><strong>Admin:</strong> admin@happytimes.com / admin123</p>
                  <p><strong>Staff:</strong> staff@happytimes.com / staff123</p>
                </div>
              </div>
            </CardContent>

            <CardFooter className="flex flex-col gap-3">
              <Button 
                type="submit" 
                className="w-full"
                disabled={isLoading}
              >
                {isLoading ? 'Iniciando sesión...' : 'Iniciar sesión'}
              </Button>

              <div className="text-sm text-center text-muted-foreground">
                ¿No tienes cuenta?{' '}
                <Link to="/register" className="text-pink-600 hover:underline font-medium">
                  Regístrate aquí
                </Link>
              </div>

              <Button
                type="button"
                variant="ghost"
                className="w-full"
                onClick={() => navigate('/')}
              >
                Continuar sin iniciar sesión
              </Button>
            </CardFooter>
          </form>
        </Card>

        {/* Información sobre bloqueo de cuenta */}
        <div className="mt-4 text-center text-xs text-muted-foreground bg-white rounded-lg p-3">
          <Lock className="h-4 w-4 inline mr-1" />
          Por seguridad, la cuenta se bloqueará después de 3 intentos fallidos
        </div>
      </div>
    </div>
  );
}