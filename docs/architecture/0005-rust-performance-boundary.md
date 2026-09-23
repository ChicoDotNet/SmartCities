# 0005 — Rust performance boundary

## Status

Accepted for project foundation.

## Decision

SmartCities is C#-first. Rust is a specialized implementation option, not a parallel default stack.

## Entry condition

Rust requires evidence of a material constraint such as latency, throughput, memory footprint, concurrency behavior, deterministic numerical workload, or a safety property materially improved by Rust.

"Complex calculation" alone is not evidence.

## .NET interoperability

When a Rust component must be consumed by .NET, prefer **FerrumWeave** so it participates in the managed .NET ecosystem without creating a SmartCities-specific FFI architecture.

Native library / P/Invoke / bespoke FFI requires an accepted ADR explaining why FerrumWeave cannot satisfy the contract.

## Evidence

Before adopting Rust: establish a C# baseline, define the measurable constraint, benchmark representative workload, prove material improvement, compare operational complexity/portability, and preserve the public .NET contract.
