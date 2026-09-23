# 0002 — Criterion Kernel port

## Status

Accepted for project foundation.

## Goal

Allow SmartCities to use governed decision support without blocking on the first release of Criterio E-Kernel Core.

## Bootstrap contract

The first implementation will expose a small public port conceptually equivalent to:

```csharp
public interface ICriterionKernel
{
    Task<CriterionDecisionTrace> EvaluateAsync(
        CriterionDecisionRequest request,
        CancellationToken cancellationToken = default);
}
```

The exact DTO design is deferred to the first TDD implementation slice.

Dependency direction:

```text
Citizen / urban modules
        |
        v
ICriterionKernel
   |           |
   v           v
Mock       NuGet adapter
              |
              v
      Criterio E-Kernel Core
```

## Mock behavior

`MockCriterionKernel` must be deterministic and should return recommendation state, evidence references consumed, assumptions, uncertainty, alternatives considered, required human authority, warnings/objections, and a public explanation payload.

It must not pretend to reproduce Criterio E-Kernel's protected knowledge or future inference quality.

## Future NuGet integration

When the first Criterio E-Kernel Core NuGet package is released:

1. preserve the SmartCities public port;
2. implement an adapter against the released package;
3. use contract replay tests against public invariants;
4. add provider-specific tests only for behaviors guaranteed by that package;
5. keep provider-specific types out of citizen and urban-domain modules;
6. retain the mock for offline development, deterministic tests, and demonstrations.

## Human oversight

A criterion trace is not the final civic disposition.

The public workflow distinguishes `Recommendation`, `HumanReview`, and `FinalDisposition`.

Traceability means observable evidence, assumptions, alternatives, public criteria, uncertainty, authority, and disposition. It does not require private model chain-of-thought.
