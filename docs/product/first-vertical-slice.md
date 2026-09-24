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

## Executable MVP orchestration

The reference F3 runtime connects the persisted citizen report path to the human-review path using the public deterministic `MockCriterionKernel`:

```text
accepted report
  -> authoritative persisted Evidence Case
  -> deterministic criterion request
  -> MockCriterionKernel
  -> trace validation
  -> persisted pending DecisionReview
  -> protected human finalization
```

Criterion request identifiers are deterministically derived from the authoritative Evidence Case identifier. The mock therefore produces the same recommendation identifier for an idempotent report replay.

The report record and the decision-review record are not claimed to be one distributed/relational atomic write. Report acceptance is persisted first; every acceptance/replay then ensures the review from the authoritative persisted Evidence Case. If review creation is interrupted after the report commit, a retry repairs the missing review instead of creating a new case. If the review has already been finalized, a report replay preserves the original human authority and disposition.

A conflicting trace for an already-known recommendation fails closed rather than replacing the authoritative review.

This is intentionally the open-source MVP seam. A future Criterio E-Kernel adapter must preserve the same traceability and replay contracts without changing citizen-domain contracts.

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
