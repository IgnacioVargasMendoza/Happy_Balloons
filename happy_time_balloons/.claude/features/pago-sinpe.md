# Contexto: Pago mediante SINPE

## Objetivo de negocio
Permitir que los clientes de Happy Times Balloons (Costa Rica) paguen sus pedidos usando
SINPE Movil, el sistema de pagos del Banco Central de Costa Rica. Al seleccionar SINPE en el
checkout, el stock se reserva de inmediato. El banco notifica el pago via webhook automatico;
el sistema valida el comprobante, descarta duplicados y actualiza el estado del pedido a
Confirmado sin intervencion manual del equipo.

## Rama
`feature/pago-sinpe` — creada desde `develop` el 2026-06-22

## Usuarios y roles
- Roles: Cliente (flujo de pago), Administrador (vistas admin de pagos)
- Requiere autenticacion: Si (checkout requiere sesion; endpoint webhook es publico con
  validacion por firma/token del banco)

## Entidad principal
- Nombre: `PagoSinpe`
- Relaciones:
  - `PedidoId` → `Pedidos.Id` (FK)
- Atributos clave:
  - `Id` (int, identity, PK)
  - `PedidoId` (int, FK)
  - `NumeroComprobante` (string, unico, referencia bancaria)
  - `Monto` (decimal)
  - `NombreTitular` (string)
  - `TelefonoDestino` (string, telefono SINPE del negocio al que llego el pago)
  - `EstadoPago` (enum: Pendiente / Aprobado / Rechazado / Duplicado)
  - `MotivoRechazo` (string, nullable)
  - `FechaRecepcion` (DateTime UTC, cuando llego el webhook)
  - `FechaProcesamiento` (DateTime UTC, cuando el sistema lo proceso)

## Entidades / enums modificados
- `EstadoPedido` enum — agregar `PagoPendiente = 6`
- `TipoOperacion` enum — agregar `ProcesarPagoSinpe`, `RechazarPagoSinpe`, `PagoDuplicadoSinpe`
- Tabla `Pedidos` — campo `MetodoPago` (string, ya existe) y `NumeroReferencia` (string 100, ya existe); no requieren cambio de esquema

## Operaciones confirmadas
| Operacion                       | Incluir |
|---------------------------------|---------|
| Listar pagos (admin)            | Si      |
| Ver detalle pago (admin)        | Si      |
| Recibir webhook del banco       | Si      |
| Validar comprobante y monto     | Si      |
| Rechazar pago invalido          | Si      |
| Detectar y rechazar duplicados  | Si      |
| Registrar auditoria de pagos    | Si      |
| Crear pago manual               | No      |
| Editar pago                     | No      |
| Eliminar pago                   | No      |
| Tests unitarios SinpeServicio   | Si      |

## Reglas de negocio
- El stock se reserva al crear el pedido con MetodoPago = "SINPE"; en ese momento el
  pedido queda en estado `PagoPendiente = 6`.
- El webhook del banco incluye: `NumeroComprobante`, `Monto`, `NombreTitular`,
  `TelefonoDestino`.
- Validacion de comprobante: formato valido segun patron bancario costarricense.
- Validacion de monto: debe coincidir exactamente con `Pedido.Total` (tolerancia cero).
- Deteccion de duplicados: si `NumeroComprobante` ya existe en `PagosSinpe` con
  `EstadoPago = Aprobado`, rechazar el nuevo intento con `EstadoPago = Duplicado`.
- Si la validacion pasa, actualizar `Pedido.EstadoPedido` a `Confirmado` usando
  `IPedidoServicio.ActualizarEstadoAsync`.
- Si la validacion falla (monto incorrecto, comprobante invalido, pedido no encontrado),
  registrar el intento con `EstadoPago = Rechazado` y `MotivoRechazo` descriptivo.
- Toda operacion de pago registra entrada en `BitacoraAuditoria` via `AuditoriaServicio`.
- El endpoint webhook debe responder HTTP 200 al banco incluso cuando el pago es rechazado
  (para evitar reintentos infinitos del banco); la logica de rechazo es interna.

## Flujo de checkout (modificacion)
- `CheckoutViewModel` agrega campo `MetodoPago` con opcion "SINPE".
- La vista de checkout muestra el numero de telefono SINPE del negocio cuando el cliente
  selecciona esa opcion, para que realize la transferencia antes de confirmar.
- Al confirmar el pedido con SINPE, el estado inicial es `PagoPendiente`.

## Dependencias
- `AuditoriaServicio` + `IBitacoraRepositorio` — reutilizados para T6; no requieren cambios.
- `ResultadoOperacionDTO` — reutilizado como tipo de retorno del servicio; no requiere cambios.
- `EstadoPedido` enum — requiere nuevo valor `PagoPendiente = 6`.
- `TipoOperacion` enum — requiere tres nuevos valores.
- `IPedidoServicio.ActualizarEstadoAsync` — invocado al confirmar pago exitoso.
- Trigger DB `trg_DescontarStock` — ya existe; descuenta stock al insertar DetallesPedido
  (no aplica a PagosSinpe directamente).
- Proyecto `HappyTimesBalloons.Tests` — ya existe (MSTest 3.1.1 + Moq); agregar
  `SinpeServicioTests.cs`.

## Estado de implementacion
| Paso | Descripcion                                              | Estado       |
|------|----------------------------------------------------------|--------------|
| 1    | DTOs: SinpeWebhookDTO, PagoSinpeDTO                     | Pendiente    |
| 2    | Interfaz repositorio: ISinpeRepositorio                  | Pendiente    |
| 3    | Interfaz servicio: ISinpeServicio                        | Pendiente    |
| 4    | Ampliar enums: EstadoPedido, TipoOperacion               | Pendiente    |
| 5    | Modelo EF6: PagoSinpe.cs                                 | Pendiente    |
| 6    | Migracion EF6: tabla PagosSinpe                          | Pendiente    |
| 7    | Repositorio: SinpeRepositorio.cs                         | Pendiente    |
| 8    | Servicio: SinpeServicio.cs                               | Pendiente    |
| 9    | ViewModel: SinpeViewModel.cs                             | Pendiente    |
| 10   | Controlador: SinpeController.cs (webhook + admin views)  | Pendiente    |
| 11   | Vistas Razor: admin (lista + detalle de pagos)           | Pendiente    |
| 12   | Modificar checkout: CheckoutViewModel + vista SINPE      | Pendiente    |
| 13   | Registro DI en AutofacConfig.cs                          | Pendiente    |
| 14   | Tests unitarios: SinpeServicioTests.cs                   | Pendiente    |

## Log de cambios
| Fecha      | Tipo           | Descripcion                                          | Agente          |
|------------|----------------|------------------------------------------------------|-----------------|
| 2026-06-22 | Inicializacion | Rama creada, contexto documentado                    | feature-planner |
