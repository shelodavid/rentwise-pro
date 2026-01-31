# AGENTS.md — RentWise Pro (Codex + Copilot Operating Instructions)

## Mission
Build **RentWise Pro**, a C# ASP.NET MVC web app that helps a user:
1) **Purchase Sheet / Data Generator** — estimate total cash-to-close and end-to-end purchasing costs for a rental property using an Investment Profile.
2) **ROI Forecast** — forecast monthly and long-term ROI (6 months, 12 months, 1 year, 5 years) using:
   - property data (price, taxes, estimated rent, etc.)
   - investment profile assumptions (interest, down payment, vacancy, PM, maintenance, etc.)
   - purchase sheet assumptions (fees, closing costs, one-time renovation, etc.)

## 🧩 ARCHITECTURAL CONSTRAINTS
- Framework: **ASP.NET MVC (C#)**
- Database: **SQL Server**
- UI: **Razor Views + Partial Views**
- All financial calculations occur in **C# services**
- SQL is **read-oriented only** (views / stored procedures allowed, but no write-side business logic in SQL)
- ETL is **out-of-scope (Phase 1)** — assume data is present.
  - Phase 2 will implement continuous ingestion from sources like Zillow, HUD, Rentometer (or alternatives).

## Repo Workflow Rules (Non-negotiable)
- Work must happen on a **new branch** (never commit directly to `master`).
- Every change must be delivered via a **Pull Request**.
- PR must pass CI and include:
  - a clear summary of changes
  - test/verification notes (even if “manual smoke test”)
- Prefer small PRs: 200–600 lines changed when possible.

## Execution Rule (Important)
- **Do not run commands that assume dotnet is installed** unless a workflow explicitly installs it.
- In GitHub Actions, always begin with:
  - `actions/setup-dotnet` specifying a version present in the runner or the repo `global.json`.
- In Codex execution environments where dotnet may not exist:
  - you may update code, but **do not attempt to execute builds/tests**.
  - use static reasoning + compilation constraints.

## Context Refresh Strategy (to avoid long conversation degradation)
After each merged PR:
1) Update `docs/STATUS.md` with:
   - what was done (bullets)
   - current behavior
   - what’s next (top 3 items)
2) Update `docs/ROADMAP.md` by checking off completed tasks.
3) If a new session starts, read **only**:
   - `README.md`
   - `docs/PROJECT_CONTEXT.md`
   - `docs/ARCHITECTURE.md`
   - `docs/ROADMAP.md`
   - `docs/STATUS.md`

## Definition of Done (phase-gated)
See `docs/DEFINITION_OF_DONE.md`.
