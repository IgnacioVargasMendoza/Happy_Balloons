# Happy Times Balloons — Contexto del Proyecto

## ¿Qué es este proyecto?
Aplicación web monolítica para la gestión de pedidos e inventario de una empresa de globos.
Se está migrando desde un prototipo generado en **Figma Make (React + Tailwind)** hacia
una aplicación **ASP.NET MVC 5 (.NET Framework 4.8)** con arquitectura por capas.

## Repositorio
- URL: https://github.com/IgnacioVargasMendoza/Happy_Balloons.git
- Rama del prototipo React (origen): `prototipo`
- Rama de desarrollo .NET (destino): `develop`
- Rama principal: `main`

## Stack tecnológico
- Lenguaje: C# con ASP.NET MVC 5
- Framework: .NET Framework 4.8
- Base de datos: SQL Server Express (Nacho\SQLEXPRESS) — versión 17.0.1000.7
- ORM: Entity Framework 6
- Frontend: Razor Views (.cshtml) + Bootstrap 5 + JavaScript/jQuery
- IDE: Visual Studio 2022
- Autenticación: ASP.NET Identity con roles

## Cadena de conexión (Web.config)
```xml
<connectionStrings>
  <add name="HappyTimesBallooonsContext"
       connectionString="Data Source=Nacho\SQLEXPRESS;Initial Catalog=HappyTimesBalloons;Integrated Security=True;MultipleActiveResultSets=True"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

## Arquitectura — 4 proyectos en una solución

```
HappyTimesBalloons.sln
├── HappyTimesBalloons.Abstraccion      ← Interfaces, DTOs, Enums (Class Library)
├── HappyTimesBalloons.AccesoADatos     ← Repositorios EF6, DbContext (Class Library)
├── HappyTimesBalloons.LogicaNegocio    ← Servicios de negocio (Class Library)
└── HappyTimesBalloons.Web              ← Controladores + Vistas Razor (ASP.NET MVC 5)
```

### Reglas de dependencia entre proyectos
- Web → LogicaNegocio → AccesoADatos
- Todos los proyectos → Abstraccion
- AccesoADatos NUNCA referencia a LogicaNegocio ni a Web
- LogicaNegocio NUNCA referencia a Web

## Roles del sistema
- **Administrador**: acceso total
- **Operador/Staff**: gestión operativa
- **Cliente**: autoservicio de pedidos

## Auditoría
Todas las operaciones CRUD en tablas críticas deben registrar:
- Usuario que realizó la acción
- Timestamp (UTC)
- Tipo de operación (Crear/Leer/Actualizar/Eliminar)
- Tabla afectada
- ID del registro afectado

## Convenciones de código

### C# (backend)
- Interfaces con prefijo I: `IProductoServicio`, `IProductoRepositorio`
- Servicios: `ProductoServicio`, `PedidoServicio`
- Repositorios: `ProductoRepositorio`, `PedidoRepositorio`
- Controladores: `ProductoController`, `PedidoController`
- ViewModels: `ProductoViewModel`, `PedidoViewModel`
- DTOs (capa Abstraccion): `ProductoDTO`, `PedidoDTO`

### Espaciado y formato
- Siempre exactamente **un espacio** entre el nombre de la variable/propiedad y el signo `=`: `var x = 5;`, `Nombre = valor`
- **Prohibido** alinear `=` con espacios extra para que queden en columna: nunca `var corto   = 1;` junto a `var muyLargo = 2;`
- Las llamadas a métodos no llevan espacio antes del paréntesis: `Metodo()`, no `Metodo ()`
- Los operadores de control de flujo sí llevan espacio: `if (`, `foreach (`, `while (`

### Razor Views
- Una carpeta por controlador dentro de `/Views`
- Layouts compartidos en `/Views/Shared`
- Vistas parciales con prefijo `_`: `_NavBar.cshtml`, `_ProductoCard.cshtml`
- Bootstrap 5 para todos los estilos y responsividad

#### Enlaces y botones con iconos
- `Html.ActionLink` requiere texto visible no vacío — pasar `""` lanza `ArgumentException`
- Para enlaces con solo icono (o con icono + contenido personalizado), usar siempre `<a>` con `@Url.Action()`:
  ```html
  <%-- CORRECTO --%>
  <a href="@Url.Action("Index", "Producto")" class="btn btn-outline-secondary" title="Volver">
      <i class="bi bi-chevron-left"></i>
  </a>

  <%-- INCORRECTO — lanza ArgumentException --%>
  @Html.ActionLink("", "Index", "Producto", null, new { @class = "btn" })
  ```
- Reservar `Html.ActionLink` únicamente para enlaces con texto visible plano (sin HTML interno)

#### Formularios dentro de bucles — usar `<form>` HTML, nunca `@using (Html.BeginForm(...))`
Dentro de bloques de código Razor (`@foreach`, `@if`, `@while`, etc.) el parser ya está en contexto C#. Usar `@using` dentro de esos bloques provoca **"Unexpected 'using' keyword after '@' character"**.

```html
<%-- CORRECTO — <form> con @Url.Action() funciona en cualquier contexto --%>
@foreach (var item in Model.Items)
{
    <form action="@Url.Action("Eliminar", "Entidad")" method="post" class="d-inline">
        @Html.AntiForgeryToken()
        <input type="hidden" name="id" value="@item.Id" />
        <button type="submit" class="btn btn-danger">Eliminar</button>
    </form>
}

<%-- INCORRECTO — @using dentro de @foreach está en contexto código; rompe el parser --%>
@foreach (var item in Model.Items)
{
    @using (Html.BeginForm("Eliminar", "Entidad", FormMethod.Post)) { ... }
}
```

`@using (Html.BeginForm(...))` **solo** es válido en contexto HTML (nivel raíz de la vista o dentro de bloques `<div>/<td>` que no estén a su vez dentro de un bloque `@foreach/@if`). Ante la duda, preferir siempre `<form>`.

#### TextAreaFor — siempre usar el overload de 4 argumentos
`Html.TextAreaFor` solo tiene dos overloads válidos. No existe `(expr, rows, htmlAttributes)`:
```html
<%-- CORRECTO — overload completo; columns=0 deja el ancho a Bootstrap --%>
@Html.TextAreaFor(m => m.Campo, 3, 0, new { @class = "form-control" })

<%-- CORRECTO — sin filas explícitas, pasar rows como atributo HTML --%>
@Html.TextAreaFor(m => m.Campo, new { @class = "form-control", rows = 3 })

<%-- INCORRECTO — no existe este overload, falla en runtime --%>
@Html.TextAreaFor(m => m.Campo, 3, new { @class = "form-control" })
```

### Base de datos
- Nombre de la base de datos: `HappyTimesBalloons`
- Servidor: `Nacho\SQLEXPRESS`
- Autenticación: Windows Authentication (Integrated Security)
- Collation: SQL_Latin1_General_CP1_CI_AS
- Nombres de tablas en PascalCase y plural: `Productos`, `Pedidos`, `Usuarios`
- PKs: `Id` (int, identity)
- FKs: `{NombreTabla}Id`
- Tabla de auditoría: `BitacoraAuditoria`

## Migración React → Razor
- JSX/TSX → Razor (.cshtml)
- Tailwind CSS → Bootstrap 5
- useState / props → ViewModels C#
- React Router → Rutas MVC (controller/action)
- fetch / axios → Controller Actions (POST/GET)
- Componentes reutilizables → Vistas parciales (_Partial.cshtml)

## Inyección de dependencias (Autofac)

El proyecto usa **Autofac 6.5.0** + **Autofac.Mvc5 6.1.0** como contenedor DI.

- Archivo de configuración: `HappyTimesBalloons.Web/App_Start/AutofacConfig.cs`
- Se inicializa en `Global.asax.cs → Application_Start()` con `AutofacConfig.Register()` **antes** que `RouteConfig` y `FilterConfig`
- Todos los repositorios y servicios se registran con `InstancePerRequest`
- `ApplicationDbContext` se registra `AsSelf().InstancePerRequest()` — una sola instancia compartida por request
- Los controladores se auto-registran con `RegisterControllers(typeof(MvcApplication).Assembly)`
- **`ApplicationUserManager` NO se registra en Autofac** — es OWIN-bound, se crea con `ApplicationUserManager.Create()`

### Reglas para controladores
- NUNCA instanciar servicios o repositorios con `new` dentro de acciones
- SIEMPRE declarar dependencias en el constructor
- NUNCA inyectar `ApplicationDbContext` directamente en un controlador — si hace falta una query que ningún servicio cubre, agregar el método al repositorio e interfaz correspondientes

## Comportamiento esperado al implementar cada módulo
Cuando trabajes en una pantalla o módulo, SIEMPRE completa el ciclo completo:
1. DTO en Abstraccion/DTOs
2. Interfaz de repositorio en Abstraccion/Interfaces/Repositorios
3. Interfaz de servicio en Abstraccion/Interfaces/Servicios
4. Modelo EF6 en AccesoADatos/Modelos
5. Implementación de repositorio en AccesoADatos/Repositorios
6. Implementación de servicio en LogicaNegocio/Servicios
7. ViewModel en Web/Models/ViewModels
8. Controlador en Web/Controllers con constructor injection
9. Vista Razor con Bootstrap 5 en Web/Views/{Modulo}/
10. **Registrar** el nuevo repositorio e interfaz de servicio en `AutofacConfig.cs`

No dejes capas incompletas. Si una pantalla requiere datos de otra entidad,
crea también esos DTOs e interfaces aunque sea mínimamente.

## Lo que NO debes hacer
- No usar Web Forms (.aspx)
- No mezclar lógica de negocio en los controladores
- No acceder directamente a la base de datos desde Web o LogicaNegocio
- No usar Tailwind CSS (migrar todo a Bootstrap 5)
- No instalar paquetes NuGet sin verificar compatibilidad con .NET Framework 4.8
- No hardcodear la cadena de conexión fuera del Web.config
