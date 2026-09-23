# SmartCities agent contract

## Mission

Build a citizen-first open-source Smart City suite whose public interfaces remain simple while urban evidence, modeling, decision support, localization, and human authority remain explicit and auditable.

## Canonical language

- Code, identifiers, schemas, tests, architecture decisions, and canonical documentation are written in English.
- User-facing content is localized.
- Mexico is the first market and ships citizen/operator content in `es-MX`.
- Do not place market copy directly in domain logic when it belongs in localization resources.

## Institutional engineering references

For meaningful engineering work, use the authoritative ASBN practices maintained in `ChicoDotNet/ArquitectoDeSoluciones`:

- `asbn-senior-tdd-developer` for incremental product delivery and test evidence;
- `asbn-scrumban-agent` for owner-visible checkpoints;
- `asbn-ux-cx-architect` for material UI/UX/CX/accessibility work;
- relevant Principal, DevOps, stack, or domain skills only when materially required.

Use the smallest sufficient skill set. Project-local decisions in this repository take precedence over generic defaults.

## Delivery model

- `main` is stable and receives deliberate promotion from `dev`.
- `dev` is the integration branch.
- Working branches use `features/*`, `bugs/*`, `releases/*`, `hotfixes/*`, or `tags/*`.
- Working branches keep their internal history and are **squash merged into `dev`**.
- `dev` stays intentionally clean: one integration commit per accepted working-branch outcome.
- `dev` is **squash merged into `main`** so `main` stays intentionally clean: one stable promotion commit per release/promotion outcome.
- Working branches are retained by default as delivery history.
- No direct product work on `main` or `dev` after bootstrap.
- After each `dev → main` squash promotion, synchronize `main → dev` with a content-neutral merge commit after exact-state validation.

## Implementation language policy

- C# / .NET is the default implementation language.
- The bootstrap executable surface is a class library only.
- React UI code uses TypeScript: `.tsx` components and `.ts` non-visual modules.
- Do not introduce application `.jsx` or plain `.js` when TypeScript can express the same code.
- Rust is introduced only after measurable evidence justifies it.
- .NET-consumed Rust components should prefer FerrumWeave instead of project-specific native FFI; an exception requires an accepted ADR.

## TDD contract

Every behavior change must identify the observable contract and add validation proportional to risk.

Prefer acceptance/contract behavior, regression evidence for defects, deterministic component/unit tests, and integration/E2E evidence for cross-boundary behavior.

For new behavior prefer `RED → GREEN → REFACTOR`.

For compatibility or external-package integration prefer:

`PRESERVE → REPLAY → DIVERGE → EXPLAIN → FIX → VERIFY → REPEAT → PROMOTE`

Never modify expected results merely to make an implementation pass.

## Public/private boundary

Never copy proprietary expert corpora, confidential municipal/customer material, citizen PII, identifiable mobility traces, secrets, or protected training datasets into this repository.

Public code may define contracts that allow external knowledge providers or criterion engines to participate without revealing protected source material.

## Criterion boundary

Public modules depend on a small public abstraction rather than Criterio implementation internals.

Bootstrap: `ICriterionKernel → MockCriterionKernel`

Future: `ICriterionKernel → Criterio E-Kernel Core NuGet adapter`

Citizen and urban-domain modules must not depend directly on provider-specific Criterio types.

## Localization boundary

Server-side `.resx` resources are the canonical source for market-facing strings exposed by the application API. React consumes localized JSON and must not redefine canonical translations independently.

## Human authority

AI-generated recommendations remain advisory unless a lawful, explicit, narrowly scoped workflow grants automated authority.

Consequential decisions preserve the human reviewer/authority and final disposition.

## ASBN SCRUMban checkpoint

At the end of every meaningful delivery interaction report:

1. ¿Cómo estás?
2. ¿En qué avanzaste desde la última interacción?
3. ¿En qué planeas avanzar para la próxima interacción?
4. ¿Qué te bloquea?

Include estimated increment and global-project progress when a defensible denominator exists.
