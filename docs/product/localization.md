# Localization and market content

## Canonical language policy

SmartCities separates canonical engineering language from market-facing content.

### Canonical engineering artifacts: English

Authored canonically in English:

- code and identifiers;
- namespaces and package names;
- schemas and API contracts;
- tests and fixture identifiers;
- architecture decisions;
- technical, contributor, and governance documentation;
- telemetry event names.

### Market content: localized

Localized per market:

- citizen navigation and labels;
- forms and validation copy;
- displayed case statuses;
- plain-language decision explanations;
- help and guidance;
- operator-facing market terminology;
- accessibility copy;
- notifications.

## Mexico-first policy

Mexico is the first supported market and uses `es-MX`.

Example citizen copy may include:

- `Reportar un problema de movilidad`
- `Dar seguimiento a mi reporte`
- `¿Por qué se tomó esta decisión?`
- `Participar en una consulta`

These are product resources, not domain identifiers.

## Canonical resource architecture

ASP.NET Core APIs own localized resource material through standard .NET resource files:

```text
Resources/
  SharedResources.resx
  SharedResources.es-MX.resx
  CitizenResources.resx
  CitizenResources.es-MX.resx
  MobilityResources.resx
  MobilityResources.es-MX.resx
```

The neutral `.resx` resource set uses canonical English. Market-specific files override it.

The API resolves a requested locale and returns JSON consumed by React.

See [localization resource API](../architecture/0003-localization-resx-api.md).

## Stable identifiers

Persisted state and public contracts use stable language-neutral identifiers.

Do not persist translated UI strings as business identifiers.

## Translation quality

Machine translation may assist drafting, but consequential government-facing content should support human review appropriate to the market, especially for legal, safety, accessibility, emergency, and rights-related copy.
