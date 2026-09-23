# Delivery governance

## Branch topology

```text
features/* ─┐
bugs/*     ─┤
releases/* ─┤
hotfixes/* ─┼─ squash merge ─→ dev ── squash merge ─→ main
tags/*     ─┘                                  |
                                              └─ content-neutral merge sync ─→ dev
```

`main` is stable. `dev` is integration. Normal work occurs on approved working branches.

Working branches retain their detailed history and are squash merged into `dev`. Working branches are retained by default.

## Promotion

A `dev → main` promotion is a separate pull request using squash merge and tied to the exact validated `dev` head.

After promotion, verify `main`, open `main → dev`, prove the merge is content-neutral, and use a regular merge commit so promoted `main` becomes an ancestor of `dev`.

## TDD delivery loop

```text
observable contract
  → failing/characterizing evidence
  → smallest implementation
  → targeted validation
  → broader validation / CI
  → exact-state certification
  → PR review
  → squash merge
  → post-merge verification
```

## Pull-request evidence

A PR should state bounded outcome, source/target branches, skills applied, behavior/tests changed, validation actually executed, coverage when behavior changed, risks/unknowns, exact candidate SHA when certification matters, and the ASBN SCRUMban checkpoint.

Do not merge merely because documentation or code exists. Merge readiness depends on the increment definition of done and its required evidence.
