import React, { useState } from 'react';
import { useNavigate } from 'react-router';
import { useApp } from '../context/AppContext';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card';
import { Button } from '../components/ui/button';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import { RadioGroup, RadioGroupItem } from '../components/ui/radio-group';
import { Alert, AlertDescription } from '../components/ui/alert';
import { Separator } from '../components/ui/separator';
import { Badge } from '../components/ui/badge';
import { 
  CheckCircle2, 
  MapPin, 
  CreditCard, 
  AlertCircle, 
  Truck,
  Smartphone,
  ArrowLeft,
  ArrowRight,
  Package,
  FileText
} from 'lucide-react';
import { toast } from 'sonner';

// HU-ENV-001 + HU-PAG-001: Checkout con 3 pasos
export default function Checkout() {
  const navigate = useNavigate();
  const { 
    cart, 
    deliveryZones, 
    selectedZone, 
    setSelectedZone, 
    createOrder,
    currentUser 
  } = useApp();

  const [step, setStep] = useState(1); // 1: Entrega, 2: Pago, 3: Confirmación
  const [orderId, setOrderId] = useState<string | null>(null);
  const [paymentMethod, setPaymentMethod] = useState<'sinpe' | 'tarjeta'>('sinpe');
  const [sinpeNumber, setSinpeNumber] = useState('');
  const [cardData, setCardData] = useState({
    number: '',
    name: '',
    expiry: '',
    cvv: ''
  });
  const [errors, setErrors] = useState<Record<string, string>>({});

  // Redireccionar si no hay usuario o carrito vacío
  React.useEffect(() => {
    if (!currentUser) {
      toast.error('Debes iniciar sesión para continuar');
      navigate('/login');
      return;
    }
    if (cart.length === 0) {
      navigate('/cart');
    }
  }, [currentUser, cart, navigate]);

  const formatPrice = (price: number) => {
    return `₡${price.toLocaleString('es-CR')}`;
  };

  const subtotal = cart.reduce((sum, item) => {
    const price = item.product.discountPrice || item.product.price;
    return sum + (price * item.quantity);
  }, 0);

  const deliveryCost = selectedZone?.cost || 0;
  const total = subtotal + deliveryCost;

  // HU-ENV-001: Manejar selección de zona
  const handleZoneSelect = (zoneId: string) => {
    const zone = deliveryZones.find(z => z.id === zoneId);
    if (zone) {
      // Escenario 3: Zona inválida
      if (!zone.isAvailable) {
        toast.error('Esta zona no está disponible para entrega');
        setSelectedZone(null);
        return;
      }
      
      // Escenario 1 y 2: Zona válida
      setSelectedZone(zone);
      toast.success(`Zona seleccionada: ${zone.name} - ${formatPrice(zone.cost)}`);
    }
  };

  // Validar paso de entrega
  const validateDeliveryStep = (): boolean => {
    if (!selectedZone) {
      toast.error('Selecciona una zona de entrega');
      return false;
    }
    return true;
  };

  // Validar paso de pago
  const validatePaymentStep = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (paymentMethod === 'sinpe') {
      // HU-PAG-001 Escenario 3: Datos inválidos
      if (!sinpeNumber.trim()) {
        newErrors.sinpeNumber = 'Ingresa el número de teléfono';
      } else if (!/^\d{4}-\d{4}$/.test(sinpeNumber) && !/^\d{8}$/.test(sinpeNumber)) {
        newErrors.sinpeNumber = 'Formato inválido (ej: 8888-8888)';
      }
    } else {
      if (!cardData.number.trim()) {
        newErrors.cardNumber = 'Número de tarjeta requerido';
      } else if (!/^\d{16}$/.test(cardData.number.replace(/\s/g, ''))) {
        newErrors.cardNumber = 'Número de tarjeta inválido';
      }
      if (!cardData.name.trim()) {
        newErrors.cardName = 'Nombre requerido';
      }
      if (!cardData.expiry.trim()) {
        newErrors.cardExpiry = 'Fecha de vencimiento requerida';
      }
      if (!cardData.cvv.trim()) {
        newErrors.cardCvv = 'CVV requerido';
      } else if (!/^\d{3,4}$/.test(cardData.cvv)) {
        newErrors.cardCvv = 'CVV inválido';
      }
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  // Procesar pago y crear orden
  const handlePlaceOrder = () => {
    if (!validatePaymentStep()) {
      return;
    }

    // Mostrar loading toast mientras se procesa
    const loadingToast = toast.loading('Procesando tu pedido...');

    // Simular procesamiento (en producción sería una llamada a API)
    setTimeout(() => {
      // HU-PAG-001 Escenario 2: Pago exitoso
      const result = createOrder({
        total,
        status: 'pagado',
        paymentMethod,
        deliveryZone: selectedZone?.name,
        deliveryCost
      });

      if (result.success) {
        setOrderId(result.orderId);
        
        // Cerrar loading toast y mostrar éxito
        toast.dismiss(loadingToast);
        toast.success('¡Pedido confirmado exitosamente!', {
          description: `Tu pedido #${result.orderId} ha sido procesado`,
          duration: 5000
        });
        
        setStep(3); // Ir a confirmación
      } else {
        toast.dismiss(loadingToast);
        toast.error(result.message);
      }
    }, 1500);
  };

  const goToNextStep = () => {
    if (step === 1 && validateDeliveryStep()) {
      setStep(2);
    } else if (step === 2) {
      handlePlaceOrder();
    }
  };

  if (step === 3) {
    // Paso 3: Confirmación de pedido exitoso
    return (
      <div className="max-w-2xl mx-auto">
        <Card>
          <CardContent className="pt-12 pb-12 text-center">
            <div className="mb-6 flex justify-center">
              <div className="h-20 w-20 rounded-full bg-green-100 flex items-center justify-center">
                <CheckCircle2 className="h-10 w-10 text-green-600" />
              </div>
            </div>
            
            <h1 className="text-3xl font-bold mb-3">
              ¡Pedido confirmado!
            </h1>
            <p className="text-lg text-muted-foreground mb-4">
              Tu pedido ha sido procesado exitosamente
            </p>

            {/* ID del pedido */}
            {orderId && (
              <div className="inline-flex items-center gap-2 bg-blue-50 text-blue-700 px-4 py-2 rounded-lg mb-8">
                <Package className="h-5 w-5" />
                <span className="font-semibold">Pedido #{orderId}</span>
              </div>
            )}

            {/* Estado del pedido */}
            <Alert className="mb-8 text-left bg-green-50 border-green-200">
              <CheckCircle2 className="h-5 w-5 text-green-600" />
              <AlertDescription className="text-green-700">
                <strong className="block mb-1">Estado: Pagado</strong>
                Tu pedido ha sido confirmado y está siendo preparado para envío a <strong>{selectedZone?.name}</strong>
              </AlertDescription>
            </Alert>

            <div className="bg-gray-50 rounded-lg p-6 mb-8 text-left">
              <h3 className="font-semibold mb-4">Resumen del pedido</h3>
              <div className="space-y-2 text-sm">
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Subtotal</span>
                  <span>{formatPrice(subtotal)}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Envío ({selectedZone?.name})</span>
                  <span>{formatPrice(deliveryCost)}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Método de pago</span>
                  <span>{paymentMethod === 'sinpe' ? 'SINPE Móvil' : 'Tarjeta'}</span>
                </div>
                <Separator />
                <div className="flex justify-between font-bold text-lg">
                  <span>Total</span>
                  <span className="text-pink-600">{formatPrice(total)}</span>
                </div>
              </div>
            </div>

            <div className="space-y-3">
              <Button 
                size="lg" 
                className="w-full"
                onClick={() => navigate('/my-orders')}
              >
                <FileText className="mr-2 h-5 w-5" />
                Ver historial de pedidos
              </Button>
              
              <Button 
                size="lg" 
                variant="outline"
                className="w-full" 
                onClick={() => navigate('/')}
              >
                Volver al inicio
              </Button>
            </div>

            <p className="text-sm text-muted-foreground mt-6">
              Recibirás un correo con los detalles de tu pedido y podrás rastrear su estado en la sección "Mis Pedidos"
            </p>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="max-w-6xl mx-auto space-y-6">
      {/* Breadcrumb de pasos */}
      <div className="flex items-center justify-center gap-4 mb-8">
        {[
          { num: 1, label: 'Entrega', icon: MapPin },
          { num: 2, label: 'Pago', icon: CreditCard },
          { num: 3, label: 'Confirmación', icon: CheckCircle2 }
        ].map(({ num, label, icon: Icon }) => (
          <React.Fragment key={num}>
            <div className="flex items-center gap-2">
              <div className={`
                h-10 w-10 rounded-full flex items-center justify-center font-semibold
                ${step >= num 
                  ? 'bg-pink-600 text-white' 
                  : 'bg-gray-200 text-gray-500'
                }
              `}>
                {step > num ? <CheckCircle2 className="h-5 w-5" /> : num}
              </div>
              <span className={`hidden md:inline ${step >= num ? 'font-semibold' : 'text-muted-foreground'}`}>
                {label}
              </span>
            </div>
            {num < 3 && (
              <div className={`h-0.5 w-12 ${step > num ? 'bg-pink-600' : 'bg-gray-200'}`} />
            )}
          </React.Fragment>
        ))}
      </div>

      <div className="grid lg:grid-cols-3 gap-6">
        {/* Formulario */}
        <div className="lg:col-span-2 space-y-6">
          {/* PASO 1: ENTREGA (HU-ENV-001) */}
          {step === 1 && (
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <MapPin className="h-5 w-5" />
                  Información de entrega
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-6">
                <div>
                  <Label className="text-base mb-3 block">Selecciona tu zona de entrega</Label>
                  <RadioGroup 
                    value={selectedZone?.id || ''} 
                    onValueChange={handleZoneSelect}
                  >
                    {deliveryZones.map(zone => (
                      <div 
                        key={zone.id}
                        className={`
                          flex items-center space-x-3 p-4 rounded-lg border-2 cursor-pointer transition
                          ${selectedZone?.id === zone.id 
                            ? 'border-pink-500 bg-pink-50' 
                            : 'border-gray-200 hover:border-gray-300'
                          }
                          ${!zone.isAvailable && 'opacity-50 cursor-not-allowed'}
                        `}
                      >
                        <RadioGroupItem 
                          value={zone.id} 
                          id={zone.id}
                          disabled={!zone.isAvailable}
                        />
                        <label htmlFor={zone.id} className="flex-1 cursor-pointer">
                          <div className="flex justify-between items-center">
                            <div>
                              <p className="font-medium">{zone.name}</p>
                              {!zone.isAvailable && (
                                <Badge variant="destructive" className="mt-1">
                                  No disponible
                                </Badge>
                              )}
                            </div>
                            {zone.isAvailable && (
                              <p className="font-bold text-lg">
                                {formatPrice(zone.cost)}
                              </p>
                            )}
                          </div>
                        </label>
                      </div>
                    ))}
                  </RadioGroup>
                </div>

                {selectedZone && (
                  <Alert className="bg-green-50 border-green-200">
                    <Truck className="h-4 w-4 text-green-700" />
                    <AlertDescription className="text-green-700">
                      Envío a {selectedZone.name}: {formatPrice(selectedZone.cost)}
                    </AlertDescription>
                  </Alert>
                )}
              </CardContent>
            </Card>
          )}

          {/* PASO 2: PAGO (HU-PAG-001) */}
          {step === 2 && (
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <CreditCard className="h-5 w-5" />
                  Método de pago
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-6">
                <RadioGroup value={paymentMethod} onValueChange={(v) => setPaymentMethod(v as any)}>
                  <div 
                    className={`
                      flex items-center space-x-3 p-4 rounded-lg border-2 cursor-pointer
                      ${paymentMethod === 'sinpe' 
                        ? 'border-pink-500 bg-pink-50' 
                        : 'border-gray-200'
                      }
                    `}
                  >
                    <RadioGroupItem value="sinpe" id="sinpe" />
                    <label htmlFor="sinpe" className="flex-1 cursor-pointer">
                      <div className="flex items-center gap-2">
                        <Smartphone className="h-5 w-5" />
                        <span className="font-medium">SINPE Móvil</span>
                      </div>
                    </label>
                  </div>

                  <div 
                    className={`
                      flex items-center space-x-3 p-4 rounded-lg border-2 cursor-pointer
                      ${paymentMethod === 'tarjeta' 
                        ? 'border-pink-500 bg-pink-50' 
                        : 'border-gray-200'
                      }
                    `}
                  >
                    <RadioGroupItem value="tarjeta" id="tarjeta" />
                    <label htmlFor="tarjeta" className="flex-1 cursor-pointer">
                      <div className="flex items-center gap-2">
                        <CreditCard className="h-5 w-5" />
                        <span className="font-medium">Tarjeta de crédito/débito</span>
                      </div>
                    </label>
                  </div>
                </RadioGroup>

                <Separator />

                {/* Formulario SINPE */}
                {paymentMethod === 'sinpe' && (
                  <div className="space-y-4">
                    <div>
                      <Label htmlFor="sinpeNumber">Número de teléfono SINPE</Label>
                      <Input
                        id="sinpeNumber"
                        placeholder="8888-8888"
                        value={sinpeNumber}
                        onChange={(e) => {
                          setSinpeNumber(e.target.value);
                          if (errors.sinpeNumber) {
                            setErrors(prev => ({ ...prev, sinpeNumber: '' }));
                          }
                        }}
                        className={errors.sinpeNumber ? 'border-red-500' : ''}
                      />
                      {errors.sinpeNumber && (
                        <p className="text-sm text-red-500 mt-1">{errors.sinpeNumber}</p>
                      )}
                    </div>
                    <Alert className="bg-blue-50 border-blue-200">
                      <AlertDescription className="text-sm text-blue-700">
                        Recibirás una notificación de pago en tu teléfono
                      </AlertDescription>
                    </Alert>
                  </div>
                )}

                {/* Formulario Tarjeta */}
                {paymentMethod === 'tarjeta' && (
                  <div className="space-y-4">
                    <div>
                      <Label htmlFor="cardNumber">Número de tarjeta</Label>
                      <Input
                        id="cardNumber"
                        placeholder="1234 5678 9012 3456"
                        value={cardData.number}
                        onChange={(e) => {
                          setCardData({ ...cardData, number: e.target.value });
                          if (errors.cardNumber) {
                            setErrors(prev => ({ ...prev, cardNumber: '' }));
                          }
                        }}
                        className={errors.cardNumber ? 'border-red-500' : ''}
                      />
                      {errors.cardNumber && (
                        <p className="text-sm text-red-500 mt-1">{errors.cardNumber}</p>
                      )}
                    </div>

                    <div>
                      <Label htmlFor="cardName">Nombre en la tarjeta</Label>
                      <Input
                        id="cardName"
                        placeholder="JUAN PEREZ"
                        value={cardData.name}
                        onChange={(e) => {
                          setCardData({ ...cardData, name: e.target.value });
                          if (errors.cardName) {
                            setErrors(prev => ({ ...prev, cardName: '' }));
                          }
                        }}
                        className={errors.cardName ? 'border-red-500' : ''}
                      />
                      {errors.cardName && (
                        <p className="text-sm text-red-500 mt-1">{errors.cardName}</p>
                      )}
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                      <div>
                        <Label htmlFor="cardExpiry">Vencimiento</Label>
                        <Input
                          id="cardExpiry"
                          placeholder="MM/AA"
                          value={cardData.expiry}
                          onChange={(e) => {
                            setCardData({ ...cardData, expiry: e.target.value });
                            if (errors.cardExpiry) {
                              setErrors(prev => ({ ...prev, cardExpiry: '' }));
                            }
                          }}
                          className={errors.cardExpiry ? 'border-red-500' : ''}
                        />
                        {errors.cardExpiry && (
                          <p className="text-sm text-red-500 mt-1">{errors.cardExpiry}</p>
                        )}
                      </div>
                      <div>
                        <Label htmlFor="cardCvv">CVV</Label>
                        <Input
                          id="cardCvv"
                          placeholder="123"
                          type="password"
                          maxLength={4}
                          value={cardData.cvv}
                          onChange={(e) => {
                            setCardData({ ...cardData, cvv: e.target.value });
                            if (errors.cardCvv) {
                              setErrors(prev => ({ ...prev, cardCvv: '' }));
                            }
                          }}
                          className={errors.cardCvv ? 'border-red-500' : ''}
                        />
                        {errors.cardCvv && (
                          <p className="text-sm text-red-500 mt-1">{errors.cardCvv}</p>
                        )}
                      </div>
                    </div>
                  </div>
                )}
              </CardContent>
            </Card>
          )}
        </div>

        {/* Resumen del pedido */}
        <div className="lg:col-span-1">
          <Card className="sticky top-24">
            <CardHeader>
              <CardTitle>Resumen</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2 text-sm">
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Subtotal</span>
                  <span>{formatPrice(subtotal)}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Envío</span>
                  <span>{selectedZone ? formatPrice(deliveryCost) : '-'}</span>
                </div>
                <Separator />
                <div className="flex justify-between font-bold text-lg">
                  <span>Total</span>
                  <span className="text-pink-600">{formatPrice(total)}</span>
                </div>
              </div>

              <Separator />

              <div className="space-y-2">
                {step > 1 && (
                  <Button
                    variant="outline"
                    className="w-full"
                    onClick={() => setStep(step - 1)}
                  >
                    <ArrowLeft className="mr-2 h-4 w-4" />
                    Volver
                  </Button>
                )}
                <Button
                  className="w-full"
                  size="lg"
                  onClick={goToNextStep}
                >
                  {step === 1 ? 'Continuar al pago' : 'Confirmar pedido'}
                  <ArrowRight className="ml-2 h-5 w-5" />
                </Button>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}