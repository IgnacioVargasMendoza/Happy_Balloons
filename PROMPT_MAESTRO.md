# PROMPT MAESTRO — Migración Happy Times Balloons
# Figma Make (React) → ASP.NET MVC 5 (.NET Framework 4.8)
# Repo: https://github.com/IgnacioVargasMendoza/Happy_Balloons.git

---

## ROL
Eres el agente orquestador de la migración del sistema Happy Times Balloons.
Tu trabajo es coordinar subagentes especializados para completar la migración
completa del prototipo React (rama `prototipo`) a una solución ASP.NET MVC 5
con arquitectura por capas. Sigue estrictamente el contexto definido en CLAUDE.md.

---

## FASE 0 — Reconocimiento (ejecutar PRIMERO)

> Explora la rama `prototipo` del repositorio. Lista:
> 1. Todas las pantallas/páginas (archivos de página o rutas en el router)
> 2. Todos los componentes reutilizables
> 3. Los modelos de datos que se infieren de los props y estados
> 4. Las llamadas a API o fetch que existan
> 5. Los flujos de navegación entre pantallas
>
> Genera un archivo `MIGRACION_MAPA.md` en la raíz con ese inventario completo.
> No hagas ningún cambio de código todavía, solo reconocimiento.

**NO avances a Fase 1 hasta tener MIGRACION_MAPA.md generado y revisado.**

---

## FASE 1 — Esqueleto de la solución

> Crea la estructura completa de la solución Visual Studio 2022:
>
> 1. Solución: `HappyTimesBalloons.sln`
> 2. Proyecto `HappyTimesBalloons.Abstraccion` (Class Library, .NET Framework 4.8)
>    - Carpetas: /Interfaces/Repositorios, /Interfaces/Servicios, /DTOs, /Enums
> 3. Proyecto `HappyTimesBalloons.AccesoADatos` (Class Library, .NET Framework 4.8)
>    - NuGet: EntityFramework 6.4.4, Microsoft.AspNet.Identity.EntityFramework 2.2.3
>    - Carpetas: /Contexto, /Modelos, /Repositorios, /Migraciones
> 4. Proyecto `HappyTimesBalloons.LogicaNegocio` (Class Library, .NET Framework 4.8)
>    - Carpetas: /Servicios
> 5. Proyecto `HappyTimesBalloons.Web` (ASP.NET MVC 5, .NET Framework 4.8)
>    - NuGet: EntityFramework 6.4.4, Microsoft.AspNet.Identity.Owin 2.2.2,
>      Microsoft.Owin.Host.SystemWeb 4.2.2
>    - Bootstrap 5.3.2 via libman o descarga manual en /Content y /Scripts
>    - Carpetas: /Controllers, /Models/ViewModels, /Views (subcarpeta por módulo),
>      /Helpers, /App_Start
>
> Configura las referencias entre proyectos según las reglas en CLAUDE.md.
>
> Agrega en Web.config la cadena de conexión:
> ```xml
> <add name="HappyTimesBallooonsContext"
>      connectionString="Data Source=Nacho\SQLEXPRESS;Initial Catalog=HappyTimesBalloons;Integrated Security=True;MultipleActiveResultSets=True"
>      providerName="System.Data.SqlClient" />
> ```

---

## FASE 2 — Infraestructura transversal

Ejecuta estos 3 subagentes EN PARALELO:

### Subagente 2A — Autenticación y roles
> Implementa el sistema de autenticación con ASP.NET Identity:
> - ApplicationUser extendiendo IdentityUser: agregar Nombre, Apellido, FechaCreacion
> - ApplicationDbContext en AccesoADatos/Contexto heredando IdentityDbContext<ApplicationUser>
> - Roles del sistema definidos como constantes: Administrador, Operador, Cliente
> - RoleInitializer con seed de los 3 roles y un usuario Administrador por defecto
> - AccountController con acciones: Login (GET/POST), Logout, Register (GET/POST)
> - Vistas Razor Bootstrap 5: Login.cshtml, Register.cshtml (responsivas)
> - Filtro global de autenticación en FilterConfig.cs
> - Configurar OWIN en Startup.cs con UseCookieAuthentication

### Subagente 2B — Bitácora de auditoría
> Implementa el sistema de auditoría transversal:
> - Modelo EF6: BitacoraAuditoria con campos:
>   Id (int PK), UsuarioId (string), NombreUsuario (string),
>   Accion (string: Crear/Leer/Actualizar/Eliminar),
>   TablaAfectada (string), RegistroId (string),
>   FechaHoraUTC (DateTime), DireccionIP (string)
> - Atributo personalizado: [AuditarCambios] para decorar modelos críticos
> - Interface IAuditoriaServicio en Abstraccion/Interfaces/Servicios
> - Implementación AuditoriaServicio en LogicaNegocio/Servicios
> - AuditoriaRepositorio en AccesoADatos/Repositorios
> - DbInterceptor de EF6 que capture INSERT/UPDATE/DELETE automáticamente
>   en entidades decoradas con [AuditarCambios]

### Subagente 2C — Layout base y componentes compartidos
> Lee los componentes de la rama `prototipo` que sean reutilizables
> (navbar, sidebar, footer, cards, modales) y conviértelos a Razor con Bootstrap 5:
> - _Layout.cshtml: layout principal responsive con navbar y footer
> - _NavBar.cshtml: barra de navegación con menú adaptado por rol
>   (usa User.IsInRole("Administrador") para mostrar/ocultar opciones)
> - _Footer.cshtml
> - _Mensajes.cshtml: partial para mostrar TempData["Exito"] y TempData["Error"]
> - _Paginacion.cshtml: partial reutilizable con paginación Bootstrap 5
> Preserva la paleta de colores y estilo visual del prototipo.
> NO uses clases de Tailwind, tradúcelas a Bootstrap 5 o CSS personalizado.

---

## FASE 3 — Módulos de negocio

Para CADA módulo del MIGRACION_MAPA.md, lanza UN subagente con este template.
Módulos independientes pueden ejecutarse en paralelo.

### Template por módulo:
> Implementa el módulo completo de **{NOMBRE_MODULO}** siguiendo el ciclo de 9 pasos
> definido en CLAUDE.md:
>
> BACKEND:
> 1. {Entidad}DTO en Abstraccion/DTOs
> 2. I{Entidad}Repositorio en Abstraccion/Interfaces/Repositorios
> 3. I{Entidad}Servicio en Abstraccion/Interfaces/Servicios
> 4. Modelo EF6 {Entidad} en AccesoADatos/Modelos
>    - Decora con [AuditarCambios] si es tabla crítica
>    - Agrega DataAnnotations para validaciones
> 5. {Entidad}Repositorio en AccesoADatos/Repositorios
>    - Implementa CRUD completo + métodos de búsqueda/filtro
> 6. {Entidad}Servicio en LogicaNegocio/Servicios
>    - Incluye validaciones de negocio
>    - Inyecta IAuditoriaServicio para registrar operaciones críticas
>
> FRONTEND:
> 7. ViewModels: {Entidad}ViewModel, Crear{Entidad}ViewModel, Editar{Entidad}ViewModel
> 8. {Entidad}Controller con acciones:
>    - Index (listado con paginación)
>    - Detalle/{id}
>    - Crear (GET + POST)
>    - Editar/{id} (GET + POST)
>    - Eliminar/{id} (GET + POST)
>    - Proteger con [Authorize(Roles = "...")] según rol correspondiente
> 9. Vistas Razor en Web/Views/{Entidad}/:
>    - Index.cshtml: tabla Bootstrap responsive con buscador y paginación
>    - Detalle.cshtml: vista de solo lectura
>    - Crear.cshtml + Editar.cshtml: formularios con validación client/server side
>    - Eliminar.cshtml: confirmación antes de borrar
>
> Registra las interfaces e implementaciones en el contenedor de DI (UnityConfig.cs
> o el mecanismo configurado en Web/App_Start/).
>
> Convierte el diseño del componente React correspondiente de la rama `prototipo`
> a Bootstrap 5. Preserva estructura visual, colores y espaciado. Sin Tailwind.

### Orden de ejecución sugerido (ajustar con MIGRACION_MAPA.md):
1. Productos / Catálogo         ← base del negocio
2. Clientes                     ← independiente, puede ir en paralelo con Productos
3. Inventario                   ← depende de Productos
4. Pedidos                      ← depende de Productos y Clientes
5. Administración de Usuarios   ← depende de Identity (Fase 2A)
6. Reportes                     ← depende de todos los anteriores
7. Dashboard / Home             ← último, consume datos de todos los módulos

---

## FASE 4 — Migraciones EF6 y seed de base de datos

> Con todos los modelos implementados, ejecuta las migraciones:
>
> 1. Enable-Migrations -ProjectName HappyTimesBalloons.AccesoADatos
> 2. Add-Migration InitialCreate -ProjectName HappyTimesBalloons.AccesoADatos
> 3. Update-Database -ProjectName HappyTimesBalloons.AccesoADatos
>    (apunta a Data Source=Nacho\SQLEXPRESS;Initial Catalog=HappyTimesBalloons)
> 4. Guarda el script SQL equivalente en /BaseDeDatos/Scripts/001_InitialCreate.sql
> 5. Crea SeedData.cs en AccesoADatos con:
>    - Roles: Administrador, Operador, Cliente
>    - Usuario admin por defecto (credenciales en appsettings, NO hardcodeadas)
>    - Datos de prueba: mínimo 5 productos, 2 pedidos, 2 clientes
> 6. Configura el inicializador en ApplicationDbContext

---

## FASE 5 — Revisión y cierre

> Realiza una auditoría del código generado y verifica:
>
> ARQUITECTURA:
> □ Ningún controlador tiene lógica de negocio directa
> □ AccesoADatos no referencia Web ni LogicaNegocio
> □ Todos los controladores usan inyección de dependencias
> □ Todas las interfaces están en Abstraccion
>
> FRONTEND:
> □ Todas las vistas tienen meta viewport y son responsive en móvil
> □ No hay clases de Tailwind en ningún .cshtml
> □ Todos los formularios tienen validación client-side (jQuery Validate) y server-side
> □ El _Layout.cshtml funciona correctamente en todas las vistas
>
> SEGURIDAD:
> □ Todos los controladores tienen [Authorize] donde corresponde
> □ Los endpoints de Administrador no son accesibles por Cliente
> □ HTTPS redirect configurado en Web.config
> □ No hay credenciales hardcodeadas en el código
>
> AUDITORÍA:
> □ Las tablas críticas tienen [AuditarCambios]
> □ El interceptor de EF6 está registrado en DbConfiguration
> □ La tabla BitacoraAuditoria se crea correctamente en la migración
>
> Genera REVISION_FINAL.md en la raíz con los resultados y cualquier
> problema encontrado con su solución propuesta.

---

## INSTRUCCIONES DE USO

```
1. CLAUDE.md y PROMPT_MAESTRO.md deben estar en la raíz del repo
2. Abre Claude Code en el terminal de Visual Studio (Developer PowerShell)
3. Ejecuta FASE 0 → revisa MIGRACION_MAPA.md → compártelo con tu asesor
4. Ejecuta FASE 1 → verifica que la solución compila antes de continuar
5. Ejecuta FASE 2 (los 3 subagentes pueden ir en paralelo o secuencial)
6. Ejecuta FASE 3 módulo por módulo según el mapa generado
7. Ejecuta FASE 4 → verifica que la BD se crea correctamente en SSMS
8. Ejecuta FASE 5 → revisa REVISION_FINAL.md
```

## NOTAS IMPORTANTES
- Guarda un commit de Git después de cada fase completada
- Si Claude Code pide confirmación para modificar archivos, responde "sí, procede"
- Si un subagente falla, puedes relanzarlo — CLAUDE.md permite retomar sin repetir trabajo
- Cualquier duda sobre decisiones de arquitectura: consultar antes de implementar
- La BD HappyTimesBalloons se crea automáticamente con EF6 si no existe
