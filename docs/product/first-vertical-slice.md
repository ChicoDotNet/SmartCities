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

## Citizen reviewed outcome

The citizen-facing recovery path is:

```text
GET /api/citizen/mobility-reports/{reportId}/outcome
```

The endpoint resolves the authoritative report, follows its Evidence Case to the persisted human review, and returns only:

- report and case identifiers;
- stable machine review status;
- stable final human disposition when present;
- localized citizen-facing status and explanation.

It deliberately does not expose recommendation identifiers, Criterion request identifiers, evidence identifiers, reviewer subject identifiers, authentication/provider claims, tokens, secrets, or private reasoning.

Pending reports return `pending-human-review` with no disposition. Finalized reports return `finalized` plus `accepted`, `modified`, `rejected`, or `deferred`.

Outcome responses are `no-store`. The React client keeps the opaque `reportId` in the current URL so refresh and direct navigation can recover the authoritative backend state. It does not store an independent outcome truth in localStorage. Going offline does not make stale browser state authoritative.

Changing locale re-queries the backend and changes citizen-facing copy only. Report ID, case ID, machine status, and human disposition remain unchanged. Unsupported locales deterministically fall back to neutral English.

## F3 MVP completion

The reference implementation now has executable behavior for the complete documented path from Digital Town Hall submission through Evidence Case, deterministic mock criterion evaluation, protected human review, persisted final disposition, and citizen-visible localized outcome recovery.

The real PostgreSQL vertical-slice gate proves both the pending and finalized citizen views, canonical human authorization, first-writer-wins review finalization, post-finalization report replay, locale invariance of machine identity, and neutral-English localization fallback.

This closes the first vertical slice MVP. It does not claim completion of production identity, real municipal GIS, real citizen PII handling, live Criterio E-Kernel integration, or broader SmartCities modules.

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
