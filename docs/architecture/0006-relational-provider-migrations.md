# 0006 — Relational providers and migration isolation

## Status

Accepted.

## Decision

SmartCities supports SQL Server and PostgreSQL as first-class relational providers.

Provider-neutral EF Core entities, repositories, and `SmartCitiesDbContext` live in `SmartCities.Infrastructure`.

Each production provider owns a sibling infrastructure assembly with its own EF Core package, provider configuration, model snapshot, and migration history:

- `SmartCities.Infrastructure.SqlServer`;
- `SmartCities.Infrastructure.PostgreSql`.

The stable provider identity is `DbProvider`.

## Why migrations are separate

SQL Server and PostgreSQL differ in types, annotations, generated SQL, identifier behavior, and provider capabilities. A single shared migration chain would either leak provider assumptions or become an unreliable lowest-common-denominator abstraction.

Schema evolution therefore remains conceptually aligned but physically independent per provider.

## Current state

SQL Server is the first implemented production provider. Its migration assembly is `SmartCities.Infrastructure.SqlServer`.

PostgreSQL is declared by the provider contract and will receive its own implementation and migration assembly in the next provider increment.

SQLite remains a relational contract-test provider only and is not a production migration target.
