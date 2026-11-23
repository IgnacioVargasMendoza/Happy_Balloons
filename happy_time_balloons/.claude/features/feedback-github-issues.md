# Contexto: Feedback a GitHub Issues

## Objetivo de negocio
Permitir que cualquier usuario autenticado (Administrador, Operador o Cliente) deje
comentarios, reporte bugs o proponga ideas directamente desde la aplicacion. Cada envio
crea un Issue en el repositorio de GitHub con los datos del formulario, el rol del usuario
y, opcionalmente, una captura de pantalla adjunta. Esto centraliza el feedback en el
mismo flujo de trabajo del equipo sin necesitar una herramienta externa.

## Rama
`feedback-github-issues` — creada desde `develop` el 2026-10-28

## Usuarios y roles
- Roles: Administrador, Operador, Cliente
- Requiere autenticacion: Si (solo usuarios autenticados ven el boton)

## Entidad principal
- No hay entidad de base de datos — el feedback se persiste exclusivamente como Issue en GitHub
- Sin modelo EF6, sin repositorio, sin DbSet
- Atributos del formulario:
  - `Titulo` (string, requerido, max 100 chars)
  - `Descripcion` (string, requerido, max 1000 chars)
  - `Tipo` (enum: Bug / Idea / Mejora)
  - `Screenshot` (base64 string, opcional — capturado con html2canvas)

## Operaciones confirmadas
| Operacion           | Incluir |
|---------------------|---------|
| Listar              | No      |
| Ver detalle         | No      |
| Crear (enviar)      | Si      |
| Editar              | No      |
| Eliminar            | No      |
| Estadisticas        | No      |

## Arquitectura de la funcionalidad

### Archivos nuevos
| Archivo | Proyecto | Descripcion |
|---------|----------|-------------|
| `Content/feedback.css` | Web | Estilos del boton flotante y modal |
| `Scripts/feedback.js` | Web | Logica del modal, html2canvas, AJAX POST |
| `Controllers/FeedbackController.cs` | Web | Accion `CrearIssue` (POST) — llama GitHub REST API |
| `Models/ViewModels/FeedbackViewModel.cs` | Web | ViewModel con los 4 campos del formulario |
| `Abstraccion/Interfaces/Servicios/IGitHubServicio.cs` | Abstraccion | Interfaz del servicio GitHub |
| `LogicaNegocio/Servicios/GitHubServicio.cs` | LogicaNegocio | Implementacion — llama GitHub API via HttpClient |

### Archivos modificados
| Archivo | Cambio |
|---------|--------|
| `Views/Shared/_Layout.cshtml` | Agregar boton flotante + incluir feedback.css y feedback.js |
| `App_Start/AutofacConfig.cs` | Registrar `IGitHubServicio` → `GitHubServicio` |
| `HappyTimesBalloons.Web.csproj` | Declarar los archivos nuevos (.cs, .cshtml, .css, .js) |
| `Web.config` | Agregar AppSettings: `GitHub:Token`, `GitHub:Owner`, `GitHub:Repo` |

### Sin vista Razor propia
El formulario se renderiza como modal Bootstrap directamente en `_Layout.cshtml`
(o en una vista parcial `_FeedbackModal.cshtml` incluida desde el Layout).
No hay una ruta navegable para el formulario.

## Integracion con GitHub API
- Endpoint: `POST https://api.github.com/repos/{owner}/{repo}/issues`
- Autenticacion: Bearer token (`GitHub:Token` desde Web.config)
- Headers requeridos: `User-Agent`, `Accept: application/vnd.github+json`
- Body del issue incluye: titulo, cuerpo (descripcion + tipo + usuario + URL de origen)
- Screenshot (si existe): se adjunta como imagen en base64 embebida en el cuerpo del issue
  usando sintaxis Markdown `![screenshot](data:image/png;base64,...)`

## Configuracion Web.config (AppSettings)
```xml
<add key="GitHub:Token" value="ghp_xxxxxxxxxxxxxxxxxxxx" />
<add key="GitHub:Owner" value="IgnacioVargasMendoza" />
<add key="GitHub:Repo" value="Happy_Balloons" />
```
El token debe tener scope `repo` (o `public_repo` si el repo es publico).

## Reglas de negocio
1. Solo usuarios autenticados pueden abrir el modal y enviar feedback
2. El boton flotante no se renderiza para usuarios no autenticados
3. `Titulo` es obligatorio (max 100 caracteres)
4. `Descripcion` es obligatoria (max 1000 caracteres)
5. `Tipo` es obligatorio; valores validos: Bug, Idea, Mejora
6. El screenshot es opcional; si se adjunta, se captura con html2canvas antes del POST
7. El cuerpo del issue siempre incluye: tipo, usuario que reporta, URL de la pagina origen
8. Si la llamada a GitHub API falla (red, token invalido, rate limit), devolver JSON
   `{ success: false, message: "..." }` — no lanzar excepcion no manejada
9. El label del issue en GitHub se asigna segun el tipo:
   - Bug → label `bug`
   - Idea → label `enhancement`
   - Mejora → label `enhancement`
   (Los labels deben existir previamente en el repo; si no existen, omitir el campo labels)

## Dependencias
- html2canvas 1.4.x (CDN) — screenshot opcional en el cliente
- HttpClient en LogicaNegocio — llamada server-side a GitHub REST API
- `GitHub:Token` configurado en Web.config antes de probar
- No depende de ninguna funcionalidad en desarrollo actualmente

## Estado de implementacion
| Paso | Descripcion | Estado |
|------|-------------|--------|
| 1    | FeedbackViewModel | Pendiente |
| 2    | IGitHubServicio (interfaz) | Pendiente |
| 3    | GitHubServicio (implementacion) | Pendiente |
| 4    | FeedbackController | Pendiente |
| 5    | feedback.css (boton flotante + modal) | Pendiente |
| 6    | feedback.js (modal, html2canvas, AJAX) | Pendiente |
| 7    | _FeedbackModal.cshtml (vista parcial) | Pendiente |
| 8    | _Layout.cshtml (incluir modal + assets) | Pendiente |
| 9    | Web.config AppSettings | Pendiente |
| 10   | Registro DI en AutofacConfig.cs | Pendiente |
| 11   | Declarar archivos en .csproj | Pendiente |

## Log de cambios
| Fecha | Tipo | Descripcion | Agente |
|-------|------|-------------|--------|
| 2026-10-28 | Inicializacion | Rama creada, contexto documentado | feature-planner |
