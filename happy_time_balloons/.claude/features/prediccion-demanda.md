# Contexto: Prediccion de Demanda

## Objetivo de negocio
Happy Times Balloons acumula historico de ventas en las tablas `Pedidos` y `DetallesPedido`.
Hoy ese historico no se usa para planificar compras ni produccion: el negocio repone stock
de forma reactiva. Este modulo calcula una prediccion de cantidad demandada por producto
para el proximo periodo (semana, mes o trimestre) usando promedios moviles y tendencia lineal
simple implementados en C# dentro de `LogicaNegocio`, sin dependencias de ML externas.
El Administrador y el Operador pueden consultar la pantalla, filtrar por periodo, y tomar
decisiones de aprovisionamiento basadas en datos historicos reales.

## Rama
`prediccion-demanda` — creada desde `develop` el 2026-07-16

## Usuarios y roles
- Rol(es): Administrador, Operador
- Requiere autenticacion: Si (acceso denegado a Clientes y publico)

## Entidades involucradas (solo lectura — sin modelo nuevo)
Este modulo NO crea tablas nuevas. Consulta las entidades existentes:

| Entidad        | Tabla BD          | Uso en prediccion                                    |
|----------------|-------------------|------------------------------------------------------|
| `Pedido`       | `Pedidos`         | Fuente de `FechaPedido` y `EstadoPedido` (filtro)    |
| `DetallePedido`| `DetallesPedido`  | Fuente de `ProductoId` y `Cantidad` vendida          |
| `Producto`     | `Productos`       | Nombre y categoria del producto                      |
| `Categoria`    | `Categorias`      | Agrupacion opcional en la vista                      |

Relaciones relevantes:
- `DetallePedido.PedidoId` -> `Pedido.Id`
- `DetallePedido.ProductoId` -> `Producto.Id`
- `Producto.CategoriaId` -> `Categoria.Id`

Solo se incluyen pedidos con `EstadoPedido` en estado confirmado/entregado
(excluir Cancelados y Pendientes de pago) para que los datos reflejen demanda real.

## Operaciones confirmadas
| Operacion         | Incluir | Justificacion                                              |
|-------------------|---------|------------------------------------------------------------|
| Listar            | Si      | Tabla de prediccion por producto para el proximo periodo   |
| Ver detalle       | Si      | Desglose de periodos historicos usados en el calculo       |
| Crear             | No      | No hay entidad a persistir; la prediccion es calculada     |
| Editar            | No      | Resultado de calculo; no editable por el usuario           |
| Eliminar          | No      | No aplica                                                  |
| Filtrar periodo   | Si      | Semanal / Mensual / Trimestral via parametro GET           |
| Validar datos     | Si      | Alerta en UI cuando un producto tiene menos de 3 periodos  |

## Algoritmo de prediccion (implementado en LogicaNegocio)

### Terminologia
- **Periodo base**: la unidad de tiempo elegida (semana = 7 dias, mes = 30 dias, trimestre = 90 dias).
- **Ventana historica**: 6 periodos hacia atras desde hoy.
- **Serie**: lista ordenada de cantidades vendidas por producto en cada uno de los 6 periodos.

### Paso 1 — Agregacion historica
Para cada producto se suman las cantidades de `DetallePedido.Cantidad` agrupadas por
el periodo al que pertenece `Pedido.FechaPedido`. El resultado es una lista de hasta 6
valores `[v1, v2, v3, v4, v5, v6]` donde `v6` es el periodo mas reciente.

### Paso 2 — Validacion de datos suficientes
Si un producto tiene menos de 3 periodos con ventas > 0, se marca con
`DatosSuficientes = false` y se muestra una advertencia en la vista.
La prediccion se calcula igual (con los datos disponibles) pero se indica al usuario
que el resultado es poco confiable.

### Paso 3 — Promedio movil simple (PMS)
```
PMS = Sum(v_i) / N    donde N = cantidad de periodos con datos
```

### Paso 4 — Ajuste de tendencia lineal
Usando regresion lineal minima (formula de minimos cuadrados sobre los N valores):
```
pendiente = (N * Sum(i * v_i) - Sum(i) * Sum(v_i)) / (N * Sum(i^2) - Sum(i)^2)
```
donde `i` es el indice del periodo (1..N).

### Paso 5 — Prediccion del proximo periodo
```
Prediccion = PMS + pendiente * 1
```
Redondeado al entero superior (Math.Ceiling). Minimo = 0 (nunca negativo).

### Precision y limites
- Si `pendiente` es positiva: el producto esta en tendencia creciente.
- Si `pendiente` es negativa: el producto esta en tendencia decreciente.
- La prediccion se acota en `[0, PMS * 3]` para evitar proyecciones absurdas
  ante un unico pico outlier.

## Tareas de la historia de usuario

| # | Tarea                                 | Estado      | Nota                                                              |
|---|---------------------------------------|-------------|-------------------------------------------------------------------|
| 1 | Disenar modulo de prediccion          | Completada  | DTOs, interfaces, algoritmo documentados                          |
| 2 | Consultar datos historicos de ventas  | Completada  | PrediccionDemandaRepositorio.cs con GROUP BY ProductoId + periodo |
| 3 | Validar datos suficientes             | Completada  | DatosSuficientes = N >= 3 periodos en PrediccionDemandaServicio   |
| 4 | Generar prediccion de demanda         | Completada  | Algoritmo PMS + tendencia lineal implementado                     |
| 5 | Mostrar detalle de prediccion         | Completada  | Detalle.cshtml con grafico Chart.js + tabla historico             |
| 6 | Filtrar prediccion por periodo        | Completada  | Parametro GET `tipoPeriodo` (Semanal/Mensual/Trimestral)          |
| 7 | Pruebas de prediccion de demanda      | Completada  | 13 tests en PrediccionDemandaServicioTests.cs, todos pasan        |

## Reglas de negocio
1. Solo se incluyen pedidos con `EstadoPedido` = Confirmado o Entregado (no Cancelado, no PendientePago).
2. Si un producto no tiene ventas en ningun periodo de la ventana historica, se excluye del resultado.
3. Si un producto tiene entre 1 y 2 periodos con datos, se incluye con advertencia `DatosSuficientes = false`.
4. La prediccion nunca puede ser negativa; el minimo devuelto es 0.
5. La prediccion se acota en `[0, PMS * 3]` para neutralizar outliers.
6. El filtro de periodo por defecto es Mensual al cargar la pantalla.
7. La ventana historica siempre son los 6 periodos inmediatamente anteriores al periodo actual.
8. Acceso restringido a roles Administrador y Operador; los Clientes reciben 403.

## Archivos a crear por capa

### Abstraccion — DTOs
| Archivo                              | Contenido                                                      |
|--------------------------------------|----------------------------------------------------------------|
| `DTOs/VentaHistoricaPeriodoDTO.cs`   | ProductoId, NombreProducto, Categoria, lista de cantidades por periodo |
| `DTOs/PrediccionProductoDTO.cs`      | ProductoId, NombreProducto, Categoria, CantidadPrediccion, Tendencia (enum), DatosSuficientes, HistoricoUnidades |
| `DTOs/PrediccionDemandaDTO.cs`       | TipoPeriodo (enum), PeriodoDescripcion, Lista<PrediccionProductoDTO>, FechaCalculo |

### Abstraccion — Enums
| Archivo                              | Contenido                                                      |
|--------------------------------------|----------------------------------------------------------------|
| `Enums/TipoPeriodo.cs`               | Semanal = 1, Mensual = 2, Trimestral = 3                       |
| `Enums/TendenciaPrediccion.cs`       | Creciente = 1, Estable = 2, Decreciente = 3                    |

### Abstraccion — Interfaces
| Archivo                                                        | Contenido                                      |
|----------------------------------------------------------------|------------------------------------------------|
| `Interfaces/Repositorios/IPrediccionDemandaRepositorio.cs`     | ObtenerVentasHistoricasAsync(tipoPeriodo, nPeriodos) |
| `Interfaces/Servicios/IPrediccionDemandaServicio.cs`           | GenerarPrediccionAsync(tipoPeriodo)            |

### AccesoADatos — Repositorio
| Archivo                                              | Contenido                                                           |
|------------------------------------------------------|---------------------------------------------------------------------|
| `Repositorios/PrediccionDemandaRepositorio.cs`       | Query EF6 sobre DetallesPedido JOIN Pedidos, GROUP BY ProductoId y periodo; devuelve VentaHistoricaPeriodoDTO |

### LogicaNegocio — Servicio
| Archivo                                          | Contenido                                                                  |
|--------------------------------------------------|----------------------------------------------------------------------------|
| `Servicios/PrediccionDemandaServicio.cs`         | Llama al repositorio, aplica el algoritmo PMS + tendencia, devuelve PrediccionDemandaDTO |

### Web — ViewModel, Controlador, Vistas, Assets
| Archivo                                                      | Contenido                                                           |
|--------------------------------------------------------------|---------------------------------------------------------------------|
| `Models/ViewModels/PrediccionDemandaViewModel.cs`            | Encapsula PrediccionDemandaDTO + TipoPeriodoSeleccionado            |
| `Controllers/PrediccionDemandaController.cs`                 | Index(tipoPeriodo = Mensual), Detalle(productoId, tipoPeriodo)      |
| `Views/PrediccionDemanda/Index.cshtml`                       | Tabla de prediccion con badge de tendencia y alerta de datos insuficientes |
| `Views/PrediccionDemanda/Detalle.cshtml`                     | Tabla de periodos historicos + calculo mostrado al usuario          |
| `Content/prediccion-demanda.css`                             | Estilos especificos del modulo (badges de tendencia, tabla)         |
| `Scripts/prediccion-demanda.js`                              | Chart.js: grafico de barras con historico + punto de prediccion     |

## Dependencias
- `Pedido` / `DetallePedido` / `Producto` / `Categoria` — entidades ya existentes, solo lectura.
- `AutofacConfig.cs` — debe registrar `IPrediccionDemandaRepositorio` e `IPrediccionDemandaServicio`.
- `ApplicationDbContext` — NO requiere DbSet nuevo (no hay modelo nuevo).
- No depende de ningun modulo en desarrollo activo.

## Estado de implementacion
| Paso | Descripcion                                              | Estado      |
|------|----------------------------------------------------------|-------------|
| 1    | DTOs: PeriodoVentaDTO, PrediccionDemandaItemDTO, PrediccionDemandaDetalleDTO | Completado |
| 2    | Enums: TipoPeriodo                                       | Completado  |
| 3    | Interfaz repositorio: IPrediccionDemandaRepositorio      | Completado  |
| 4    | Interfaz servicio: IPrediccionDemandaServicio            | Completado  |
| 5    | Repositorio: PrediccionDemandaRepositorio.cs             | Completado  |
| 6    | Servicio: PrediccionDemandaServicio.cs (algoritmo PMS + tendencia) | Completado |
| 7    | ViewModel: PrediccionDemandaViewModel.cs + DetalleViewModel | Completado |
| 8    | Controlador: PrediccionDemandaController.cs              | Completado  |
| 9    | Vistas Razor: Index.cshtml + Detalle.cshtml              | Completado  |
| 10   | Assets: prediccion-demanda.css + prediccion-demanda.js   | Completado  |
| 11   | Registro DI en AutofacConfig.cs                         | Completado  |
| 12   | Pruebas unitarias: PrediccionDemandaServicioTests.cs (13 tests) | Completado |
| 13   | Fix build: ProgramacionEntregaServicioTests actualizado con pedidoRepo | Completado |

## Log de cambios
| Fecha      | Tipo           | Descripcion                                                                         | Agente          |
|------------|----------------|-------------------------------------------------------------------------------------|-----------------|
| 2026-07-16 | Inicializacion | Rama creada desde develop, contexto documentado, algoritmo definido, archivos mapeados por capa | feature-planner |
| 2026-07-18 | Implementacion | Modulo completo: todas las capas implementadas + 13 tests unitarios, build verde     | Claude Code     |
