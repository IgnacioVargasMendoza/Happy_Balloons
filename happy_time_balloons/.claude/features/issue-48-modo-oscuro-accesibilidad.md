# Contexto: Opciones de Accesibilidad (Modo Oscuro y Tamaño de Letra)

## Objetivo de negocio
Mejorar la experiencia de usuario ofreciendo opciones de personalización visual: un toggle de modo oscuro
y controles para aumentar o reducir el tamaño de fuente. Las preferencias se persisten en `localStorage`
para que el usuario no tenga que reconfigurarlas en cada visita. No se requiere backend ni base de datos.

## Rama
`feature/issue-48-modo-oscuro-accesibilidad` — creada desde `develop` el 2026-10-28

## Issue de origen
Issue #48 — "Agregar opciones de accesibilidad (modo oscuro y tamaño de letra)" (karla3460)

## Usuarios y roles
- Roles: todos (Administrador, Operador, Cliente, público)
- Requiere autenticación: No — disponible para cualquier visitante

## Entidad principal
No aplica. Esta funcionalidad es 100 % frontend. No crea ni modifica modelos EF6, repositorios ni servicios.

## Operaciones confirmadas
| Operación          | Incluir | Justificación                                      |
|--------------------|---------|----------------------------------------------------|
| Toggle modo oscuro | SI      | Alterna entre tema claro y oscuro vía Bootstrap 5.3 |
| Aumentar fuente    | SI      | Incrementa el font-size base del documento          |
| Reducir fuente     | SI      | Decrementa el font-size base del documento          |
| Resetear fuente    | SI      | Vuelve al tamaño base por defecto                   |
| Persistir en localStorage | SI | Recupera preferencias al recargar la página    |

## Capas afectadas
- Abstraccion      — NO
- AccesoADatos     — NO
- LogicaNegocio    — NO
- ApplicationDbContext.cs — NO
- AutofacConfig.cs — NO
- Web (Frontend)   — SI: _Layout.cshtml, _NavBar.cshtml, nuevo archivo JS, nuevo archivo CSS

## Reglas de negocio / restricciones UX
- El modo oscuro se implementa con `data-bs-theme="dark"` en el `<html>` (mecanismo nativo Bootstrap 5.3).
  No se agrega ninguna clase CSS propia para el tema en sí, solo overrides puntuales si Bootstrap no cubre algo.
- Tamaños de fuente: mínimo 12px, máximo 22px, incremento/decremento de 2px por paso. Valor por defecto: 16px.
- Las preferencias se guardan en `localStorage` bajo las claves `htb_theme` y `htb_font_size`.
- Al cargar cualquier página (`_Layout.cshtml`), se aplican las preferencias ANTES del primer render
  (inline script en `<head>`) para evitar flash de tema incorrecto (FOUT).
- Los controles de accesibilidad son visibles en todas las páginas sin importar el rol del usuario.

## Plan de implementación

### Archivos a crear
| Paso | Archivo                                                                 | Descripción                                              |
|------|-------------------------------------------------------------------------|----------------------------------------------------------|
| 1    | `HappyTimesBalloons.Web/Content/accessibility.css`                     | Overrides CSS puntuales para modo oscuro y fuente dinámica |
| 2    | `HappyTimesBalloons.Web/Scripts/accessibility.js`                      | Lógica JS: toggle, font-size, persistencia en localStorage |

### Archivos a modificar
| Paso | Archivo                                                                 | Cambio                                                   |
|------|-------------------------------------------------------------------------|----------------------------------------------------------|
| 3    | `HappyTimesBalloons.Web/Views/Shared/_Layout.cshtml`                   | Inline script en `<head>` para aplicar preferencias al cargar; include del CSS y JS |
| 4    | `HappyTimesBalloons.Web/Views/Shared/_NavBar.cshtml`                   | Agregar controles de accesibilidad (toggle oscuro + botones fuente) |
| 5    | `HappyTimesBalloons.Web/HappyTimesBalloons.Web.csproj`                 | Declarar los archivos nuevos (Content y Scripts) como Content |

## Diseño de la UI de controles

Los controles se ubican en el extremo derecho del navbar, fuera de cualquier bloque de rol, visibles siempre.

```html
<!-- Grupo de accesibilidad en _NavBar.cshtml -->
<div class="d-flex align-items-center gap-2 ms-3">
    <!-- Tamaño de fuente -->
    <button id="btn-font-decrease" class="btn btn-sm btn-outline-secondary" title="Reducir texto">A-</button>
    <button id="btn-font-reset"    class="btn btn-sm btn-outline-secondary" title="Tamaño normal">A</button>
    <button id="btn-font-increase" class="btn btn-sm btn-outline-secondary" title="Aumentar texto">A+</button>
    <!-- Modo oscuro -->
    <button id="btn-toggle-theme"  class="btn btn-sm btn-outline-secondary" title="Modo oscuro">
        <i class="bi bi-moon-fill"></i>
    </button>
</div>
```

## Estado de implementación
| Paso | Descripción                                  | Estado       |
|------|----------------------------------------------|--------------|
| 1    | accessibility.css                            | Pendiente    |
| 2    | accessibility.js                             | Pendiente    |
| 3    | _Layout.cshtml — inline script + includes    | Pendiente    |
| 4    | _NavBar.cshtml — controles de accesibilidad  | Pendiente    |
| 5    | .csproj — registro de archivos nuevos        | Pendiente    |

## Dependencias
- Ninguna funcionalidad en desarrollo bloquea esta feature.
- Requiere Bootstrap 5.3+ (ya incluido en el proyecto — `data-bs-theme` es nativo de 5.3).
- Requiere Bootstrap Icons (ya incluido — se usa `bi-moon-fill` / `bi-sun-fill`).

## Log de cambios
| Fecha      | Tipo           | Descripcion                                                                 | Agente          |
|------------|----------------|-----------------------------------------------------------------------------|-----------------|
| 2026-10-28 | Inicializacion | Rama creada, contexto documentado, plan de implementacion definido          | feature-planner |
