# Contributing to SmartCities

Thank you for helping build SmartCities.

SmartCities is **100% open-source software from the beginning**. Reusable product code, public contracts, reference UI, interoperability, tests, CI, and engineering documentation belong in this public repository under AGPL-3.0-only.

Private expert knowledge may participate only through explicit public boundaries. The public product must remain understandable, buildable, testable, and useful without private repository access.

## Before writing code

Read the README, GOVERNANCE.md, AGENTS.md, module catalog, delivery governance, and relevant ADRs.

Open a design proposal before broad implementation affecting public contracts, privacy/identity, GIS conventions, human authority, Criterio integration, localization contracts, runtime dependencies, Rust adoption, or another cross-cutting boundary.

## Canonical language

Code, identifiers, schemas, tests, ADRs, and canonical technical documentation are English. User-facing content is localized by market. Mexico is first and uses `es-MX`.

## Technology policy

### C# first

C#/.NET is the default product language. The bootstrap creates only a class library. Additional projects appear when a real executable contract needs them.

### TypeScript UI

React UI uses `.tsx` components and `.ts` non-visual modules. Do not add application `.jsx` or plain `.js` when TypeScript can express the same code. Fluent UI is the primary control system; Bootstrap is layout/utilities.

### Rust only after evidence

Rust requires measurable justification such as profiling, latency, throughput, memory, concurrency, numerical determinism, or safety evidence.

For a .NET-consumed Rust component, prefer **FerrumWeave**. A project-specific native FFI path requires an accepted ADR explaining why FerrumWeave cannot satisfy the contract.

## TDD

New behavior follows `RED → GREEN → REFACTOR → CERTIFY`.

Compatibility work follows `PRESERVE → REPLAY → DIVERGE → EXPLAIN → FIX → VERIFY → REPEAT → PROMOTE`.

Do not add meaningless tests only to inflate coverage. The bootstrap class library intentionally contains no invented behavior and therefore starts without fake unit tests.

## Branch and merge flow

Normal work targets `dev` through retained working branches: `features/*`, `bugs/*`, `releases/*`, `hotfixes/*`, or `tags/*`.

**Working branch → `dev`: squash merge.** The working branch preserves detailed history. `dev` receives one concise integration commit per accepted outcome.

**`dev` → `main`: squash merge.** `main` receives one concise stable commit per promoted outcome.

After promotion, synchronize `main → dev` with a content-neutral regular merge commit so ancestry remains healthy without duplicating content.

Result: working branches preserve detail; `dev` stays clean; `main` stays clean.

## Pull requests

State bounded outcome, non-goals, observable contracts, validation actually executed, exact candidate SHA when certification matters, public/private impact, privacy impact, localization impact, and material AI assistance.

## AI-assisted contributions

AI tools are welcome. The contributor remains accountable for correctness, licensing, provenance, security, and maintainability.

## Contribution licensing and DCO

SmartCities is **AGPL-3.0-only**. Contributions use DCO 1.1 sign-off. Add `Signed-off-by: Your Name <you@example.com>`; `git commit -s` can do this automatically.

See [DCO.md](DCO.md).

## Community

Review the change, not the author. Strong technical disagreement is welcome; hostility is not. See [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md).
