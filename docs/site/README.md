# SmartCities project website

This directory contains the React + TypeScript + Vite site published through GitHub Pages.

Its job is to explain what SmartCities is, why it is open source from day zero, how IAIA(oh) preserves human authority, the module vision, the future Criterio E-Kernel integration boundary, and how to contribute.

The project website is not the future municipal application.

## UI stack

- React
- TypeScript / TSX
- Fluent UI controls and theme tokens
- Bootstrap grid and spacing utilities
- project-owned CSS for visual identity

No application JSX/JavaScript source is used.

## Localization distinction

The static project website carries a small typed bilingual copy module because it has no backend.

The future SmartCities product UI follows the accepted product contract:

`.resx → ASP.NET Core API → localized JSON → React TypeScript`

## Local development

```bash
cd docs/site
npm ci
npm run dev
```

Production validation:

```bash
npm run build
```

GitHub Pages deploys from `main`; integration branches only build the site in CI.
