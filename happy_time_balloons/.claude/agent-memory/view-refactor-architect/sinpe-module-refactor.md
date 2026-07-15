---
name: sinpe-module-refactor
description: CSS/JS extracted for Sinpe Index + Checkout views; external files created and API established
metadata:
  type: project
---

## Files created / modified

- `Content/Sinpe.css` — badge classes for EstadoPagoSinpe enum states plus Checkout-specific classes (`.cursor-pointer`, `.border-brand`, `.zona-card:hover`, `.pago-card:hover`, `.checkout-sticky-summary`, `.checkout-item-thumb`, `.checkout-item-price`)
- `Scripts/Sinpe.js` — single IIFE exposing two APIs: `Sinpe.init()` (lista de pagos, modal fetch) and `Sinpe.Checkout.init()` (zone selection, payment panel toggle, total calculation)

## JS module API
- `Sinpe.init()` — called from `Views/Sinpe/Index.cshtml` `@section Scripts`
- `Sinpe.Checkout.init()` — called from `Views/Pedido/Checkout.cshtml` `@section scripts`

## .csproj registration
Both `Content\Sinpe.css` and `Scripts\Sinpe.js` are declared as `<Content>` in `HappyTimesBalloons.Web.csproj` (lines ~190-191).

## Documented dynamic inline styles (permitted exceptions)
`Checkout.cshtml` retains two Razor-driven `style="display:none"` inline attributes on `#panelSinpe` and `#panelTarjeta`. These set initial server-side visibility based on `Model.MetodoPago`; `Sinpe.Checkout.bindMetodosPago()` takes over after page load. Both are annotated with `<%-- ... --%>` comments.

## Recurring pattern: @using Html.BeginForm outside @foreach
`Checkout.cshtml` wraps the entire form with `@using (Html.BeginForm(...))` at the view root level (not inside a loop), so this is valid. The `@foreach` inside uses only HTML helpers — no nested `@using Html.BeginForm`.

**Why:** [[layout-no-styles-section]] — no Styles section in layout, so `<link>` for Sinpe.css lives in the view body in both Index.cshtml and Checkout.cshtml.
