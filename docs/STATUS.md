# Status — RentWise Pro

Last updated: 2026-02-11

## What was done recently
- Stabilized .NET build outputs by routing project `bin/` and `obj/` artifacts into project-scoped folders under `artifacts/`.
- Added safeguards so source compilation excludes generated `artifacts/**/*.cs` files that can cause recursive compile issues.
- Landed Identity-backed auth and user-scoped data surfaces for investment profiles, saved properties, and saved analyses.
- Expanded app modules for purchase sheet, forecast calculations, diagnostics, ETL admin controls, and reusable UI partial components.

## Current behavior
- Users can register/login, manage investment profiles, browse listings, save properties, and store/review saved analyses.
- Forecast and purchase-sheet flows are available through MVC controllers and Razor views backed by C# calculation services.
- Admin ETL operations and diagnostics endpoints/views are present for operations visibility in Phase 1 environments.
- Build artifact isolation is configured through `Directory.Build.props` to reduce cross-project output collisions.

## Next up (top 3)
1) Add/expand automated test coverage for calculation services and controller flows (especially forecast and purchase-sheet paths).
2) Complete UX modernization pass (layout consistency, input validation quality, and error messaging polish).
3) Harden CI quality gates (build + tests + linting/static checks) and document expected local verification workflow.
