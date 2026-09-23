# SmartCities.Web

Product React client for the SmartCities citizen experience.

This project is intentionally separate from `docs/site`, which is the explanatory GitHub Pages website.

## Localization

The web client does not carry translated UI copy in TypeScript. It loads the public bundle from:

```text
GET /api/localization/resources/{culture}
```

Stable resource keys remain in TypeScript; human-facing values remain authoritative in the API `.resx` files.

## Local development

Run the API on `http://localhost:5000` or set `SMARTCITIES_API_ORIGIN` before starting Vite.

```bash
npm ci
npm run dev
```

## PWA boundary

The service worker may cache only same-origin GET application-shell resources. Requests under `/api/` and all non-GET requests remain network-only. Citizen report submissions are never queued or replayed by the PWA shell.
