# First vertical slice — citizen mobility report

## Outcome

Prove the complete architecture with one citizen task:

> A citizen reports a mobility problem and can later understand the reviewed outcome.

For Mexico, the citizen-facing interaction is localized in `es-MX`. Canonical code, contracts, tests, and documentation remain in English.

## Flow

```text
1. Citizen opens Digital Town Hall
2. React loads es-MX resources as JSON from the API
3. Citizen chooses "Reportar un problema de movilidad"
4. Citizen supplies location + category + description + optional evidence
5. SmartCities creates an Evidence Case
6. Public domain rules validate and classify the case
7. ICriterionKernel receives a bounded Decision Request
8. MockCriterionKernel returns a deterministic Decision Trace
9. Authorized human reviewer accepts / modifies / rejects / defers
10. Citizen sees status + localized plain-language explanation
11. Audit trail preserves evidence and final disposition
```

## First-slice modules

Required initially:

- Citizen Portal / Digital Town Hall;
- Requests & Cases;
- City Context & GIS;
- Evidence Hub;
- SmartCities.Contracts;
- SmartCities.Criterion;
- SmartCities.HumanOversight;
- SmartCities.Localization;
- Transparency & Decisions.

## TDD acceptance contracts

1. a valid citizen report creates exactly one case;
2. invalid required input produces an actionable localized validation response;
3. evidence references retain provenance metadata;
4. the same deterministic mock request returns the same decision trace;
5. a recommendation cannot become a final disposition without the configured human-review transition;
6. citizen-visible explanation never exposes secrets or private provider internals;
7. the final disposition remains linked to the evidence and recommendation that informed it;
8. switching locale does not change underlying domain or decision identifiers;
9. missing `es-MX` resources fall back deterministically to the neutral English resource.

## Explicit non-goals

The first slice does not require real Criterio E-Kernel NuGet integration, production identity, live municipal GIS, real citizen PII, microsimulation, full transport planning, IoT feeds, or automated governmental decision authority.
