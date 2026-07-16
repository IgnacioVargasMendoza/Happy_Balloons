---
name: ranking-productos-refactor
description: CSS/JS extracted for RankingProductos module; Chart.js data passed via data-* attributes
metadata:
  type: project
---

`Views/RankingProductos/Index.cshtml` was refactored. Files created:

- `Content/ranking-productos.css` — four rules: `.ranking-col-posicion` (60px width), `.ranking-col-progreso` (180px width), `.ranking-progress` (height 10px), `.ranking-badge-bronce` (#cd7f32 background for position-3 medal badge)
- `Scripts/ranking-productos.js` — `RankingProductos` IIFE module with `init`, `initChartBarras`, `initChartDona`; reads chart data from `data-*` attributes on `#ranking-charts-data` div

**Why:** The original view had four inline `style=` attributes and a full Chart.js initialization `<script>` block with Razor variables inline. Extracted per project separation-of-concerns rules.

**How to apply:** The `#ranking-charts-data` div pattern (hidden div with `data-*` attributes holding JSON-serialized Razor data) is now the established pattern for passing server-side collection data to external JS files. Use it for any future Chart.js or similar list-data-driven modules.

Key detail: `progress-bar` `style="width:X%"` was intentionally left as a dynamic inline style because the width is per-row Razor data that cannot be expressed as a static CSS class. This is the sole accepted exception in this view, documented with a comment.

See [[layout-no-styles-section]] for CSS link placement convention.
