---
name: close-issue
description: Cierra el trabajo en un issue de Happy Times Balloons creando el PR en GitHub con Closes #N y actualizando la tabla de issues en memoria. Acepta el número de issue como argumento opcional (/close-issue 48). Si no se pasa argumento, lo infiere desde el nombre de la rama actual.
---

Eres el asistente de cierre de issues del proyecto Happy Times Balloons. Tu trabajo es completar el ciclo de un issue: verificar que todo está listo, generar el PR bien formado, y actualizar el registro en memoria.

## Paso 1 — Determinar el número de issue

Si el usuario pasó un número como argumento (`/close-issue 48`), usarlo directamente.

Si no, infiere el número desde el nombre de la rama actual:

```bash
git branch --show-current
```

El nombre de rama sigue el patrón `fix/issue-N-descripcion` o `feature/issue-N-descripcion`. Extrae `N`.

Si no puedes determinar el número de issue, pregunta al usuario: "¿Cuál es el número del issue que quieres cerrar?"

---

## Paso 2 — Verificaciones previas

Ejecuta en paralelo:

```bash
# Cambios sin commitear
git status --short

# Commits en esta rama que no están en develop
git log develop..HEAD --oneline
```

**Si hay archivos sin commitear:** informa al usuario y detente.
```
⚠️ Hay cambios sin commitear. Commítalos antes de crear el PR:
{lista de archivos}
```

**Si `git log` está vacío (0 commits sobre develop):** informa al usuario y detente.
```
⚠️ Esta rama no tiene commits nuevos sobre develop. No hay nada que mergear.
```

---

## Paso 3 — Verificación de prueba en browser (solo si aplica)

Lee qué archivos cambiaron en los commits de esta rama:

```bash
git diff develop..HEAD --name-only
```

Si la lista incluye archivos `.cshtml`, `.js`, o `.css`, pregunta explícitamente antes de continuar:

```
Esta rama modifica vistas o scripts:
{lista de archivos .cshtml/.js/.css}

¿Verificaste el comportamiento en el browser? (El proyecto requiere prueba manual
en el navegador para cambios de UI — build verde no es suficiente.)

Responde "sí" para continuar o "no" para detener y probar primero.
```

No crees el PR hasta recibir confirmación afirmativa.

---

## Paso 4 — Recopilar información del issue y la rama

Ejecuta en paralelo:

```bash
# Título e información del issue desde GitHub
"C:/Program Files/GitHub CLI/gh.exe" issue view N --json number,title,body,labels
```

```bash
# Resumen de commits de la rama
git log develop..HEAD --oneline
```

Busca el feature context file (puede o no existir):
- `.claude/features/issue-N-*.md`
- `.claude/features/<nombre-de-rama>.md`

Si existe, léelo para extraer el objetivo de negocio y los cambios realizados.

---

## Paso 5 — Clasificar el tipo de PR

Determina el prefijo del título según la categoría del issue:

| Categoría | Señales | Prefijo del título |
|---|---|---|
| Bug | Labels `bug`; palabras "error", "no funciona", "incorrecto" | `fix(#N):` |
| Mejora UX / refactor | "modificar", "cambiar", "renombrar", "filtrar" | `fix(#N):` |
| Feature nueva | "crear", "implementar", "agregar módulo" | `feat(#N):` |

---

## Paso 6 — Generar y crear el PR

Construye el PR con:
- **Base:** `develop`
- **Título:** `fix(#N): {descripción corta en español}` o `feat(#N): {descripción}`
- **Cuerpo:** estructura a continuación

**SIEMPRE usa el Bash tool** para `gh pr create` — el heredoc `$(cat <<'EOF')` no funciona en PowerShell:

```bash
"C:/Program Files/GitHub CLI/gh.exe" pr create \
  --title "fix(#N): descripción" \
  --base develop \
  --body "$(cat <<'EOF'
Closes #N

## Resumen
{1-3 bullets con qué cambió y por qué}

## Archivos modificados
{lista de los archivos principales que cambiaron}

## Pruebas realizadas
{qué se verificó — ej: "Flujo de checkout probado en browser", "Compilación limpia sin errores"}

🤖 Generated with [Claude Code](https://claude.com/claude-code)
EOF
)"
```

Muestra la URL del PR al terminar.

---

## Paso 7 — Actualizar la memoria del proyecto

Después de que el PR se crea exitosamente, lee y actualiza `.claude/memory/project_issues_karla.md`:

1. Localiza la fila del issue `#N` en la tabla correspondiente (Bloque 1, 2, 3 o 4).
2. Actualiza el estado de la celda a `cerrado ✓ (PR #<número_pr>)`.
3. Si el issue no está en la tabla (es nuevo), agrega una fila al bloque que corresponda.

---

## Paso 8 — Resumen final

Presenta al usuario:

```
## Issue #N cerrado ✓

**PR creado:** {URL del PR}
**Título:** {título del PR}
**Base:** develop ← {rama del issue}
**Commits incluidos:**
{lista del git log}

**Memoria actualizada:** project_issues_karla.md → estado "cerrado ✓ (PR #X)"

Próximo paso: cuando el PR sea mergeado en develop, ejecuta:
  git checkout develop && git pull origin develop
```

---

## Reglas críticas

- **`gh` CLI:** siempre ruta completa `"C:/Program Files/GitHub CLI/gh.exe"` — no está en el PATH
- **Heredoc:** solo funciona en Bash tool, nunca en PowerShell
- **PR base:** siempre `--base develop`, nunca `main`
- **`Closes #N`** debe estar en el cuerpo del PR (primera línea), no en el título
- **No hacer `gh issue close`** — GitHub lo cierra automáticamente al mergear el PR con `Closes #N`
- **No crear el PR si hay uncommitted changes**
- **No crear el PR sin confirmación de browser test si hay cambios en .cshtml/.js/.css**
