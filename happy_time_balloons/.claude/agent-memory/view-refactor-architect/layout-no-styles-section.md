---
name: layout-no-styles-section
description: _Layout.cshtml does not define @RenderSection("Styles") — module CSS links go in the view body, not in a section
metadata:
  type: project
---

`Views/Shared/_Layout.cshtml` has no `@RenderSection("Styles", required: false)` in the `<head>`. It only defines `@RenderSection("scripts", required: false)` before `</body>`.

**Why:** The project started as a minimal layout and the Styles section was never added. Adding it would require modifying _Layout.cshtml and all existing views.

**How to apply:** When a view needs a module-specific CSS file, place the `<link rel="stylesheet" href="@Url.Content(...)">` at the top of the view body (before the first `<div>`), not in a `@section Styles` block. This is the established project convention until a Styles section is added to the layout.

See [[sinpe-module-refactor]] for the first instance of this pattern.
