# SmartCities roadmap

This roadmap is capability-oriented. Dates and percentages must not be inferred from the existence of files.

## F0 — Open-source project foundation

AGPL-3.0-only licensing; governance, DCO, Code of Conduct, support, security; contribution and issue workflows; clean squash-based branch governance; C# class-library foundation; deterministic CI; public GitHub Pages site; canonical architecture and module documentation.

## F1 — Public contracts and first domain primitives

Introduce the smallest useful C# contracts through TDD: evidence references, stable identifiers, criterion request/trace boundaries, human-authority/disposition contracts, and localization vocabulary.

## F2 — Localization API foundation

Introduce ASP.NET Core only when the first API contract is ready:

`.resx → ASP.NET Core localization → JSON → React TypeScript`

Mexico ships first as `es-MX`; neutral resources remain canonical English.

## F3 — Digital Town Hall vertical slice

`Citizen mobility report → Evidence Case → Mock Criterion → Human Review → Explainable Outcome`

## Platform capability — per-Town-Hall feature management

After the F3 MVP, deployments can enable or disable registered vertical slices independently. Non-sensitive feature/configuration state uses a dedicated per-Town-Hall SQLite control plane and remains separate from SQL Server/PostgreSQL domain persistence.

## F4 — Criterio E-Kernel Core adapter

Integrate the first released NuGet package without changing public SmartCities domain contracts. Use contract replay.

## F5+ — Domain expansion

Expand mobility, urban intelligence, and other Smart City modules according to evidence and adopter needs.

## Rust performance lane

Rust is not a milestone by itself. It enters only after measurements establish a material need involving performance, memory, concurrency, determinism, or safety. For .NET consumption, FerrumWeave is the preferred path. Alternative native FFI requires an explicit ADR and evidence.
