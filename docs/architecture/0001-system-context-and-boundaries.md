# 0001 — System context and public/private boundaries

## Status

Accepted for project foundation.

## Context

SmartCities is public AGPL software intended to serve multiple jurisdictions while allowing private or organization-specific expertise to improve decision support through explicit boundaries.

The public repository must remain useful on its own and must never require publication of protected knowledge.

## Public system responsibility

```text
Citizen / Operator / Expert / Authority
                 |
                 v
        React + Fluent UI client
                 |
                 v
          ASP.NET Core APIs
        /        |        \
Localization  Urban Domain  Evidence/GIS
        \        |        /
                 v
          Decision Request
                 |
                 v
        ICriterionKernel
          /            \
         v              v
MockCriterionKernel  future NuGet adapter
                         |
                         v
                Criterio E-Kernel Core
                 |
                 v
          Decision Trace
                 |
                 v
          Human Oversight
                 |
                 v
      Citizen-visible disposition
```

## SmartCities owns publicly

- citizen and operator interaction contracts;
- urban domain models;
- geospatial and evidence references;
- scenario and appraisal contracts;
- lawful-to-publish generic algorithms;
- deterministic mock criterion behavior;
- human-review workflow contracts;
- localization infrastructure and public resource content;
- explainability and transparency surfaces;
- open interoperability.

## SmartCities does not own publicly

- proprietary expert corpora;
- confidential municipal/customer datasets;
- private knowledge packs or decision rubrics;
- provider-private reasoning assets;
- organization-specific confidential governance rules;
- legal, technical, administrative, or political authority.

## Data classes

### Safe to version publicly

Synthetic fixtures, small public-domain/open-license examples with provenance, schemas, deterministic expected outputs, and non-reidentifiable anonymized examples.

### Runtime only

Authenticated citizen records, precise identifiable mobility traces, governed photographs containing personal data, live operational telemetry, and confidential municipal/customer information.

### Never in Git

Credentials, secrets, production tokens, private keys, or protected datasets whose redistribution is not authorized.

## Multi-city design

City-specific configuration must be separable from public application code. A municipality should be able to configure modules, geography, branding, authority roles, evidence sources, locales, and criterion provider without forking the product.
