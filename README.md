# RentWise Pro

RentWise Pro is a C# ASP.NET MVC web application that helps prospective rental-property buyers:
- understand **true cash-to-close** via a Purchase Sheet
- forecast **ROI** under multiple assumptions (6 mo, 12 mo, 1 yr, 5 yrs)

## What this repo contains
- MVC web application (Razor UI)
- Read-only SQL access (stored procedures/views permitted)
- Calculation services in C# (mortgage, closing costs, ROI metrics)

## What this repo does NOT contain (Phase 1)
- ETL / ingestion pipelines (Phase 2)
- user authentication (planned)
- mobile apps (planned after web is complete)

## Quick Start
1) Create SQL schema using `migrations/001_init.sql`
2) Configure connection string in `appsettings.Development.json`
3) Run in Visual Studio (IIS Express) or `dotnet run`

## Docs
- `docs/PROJECT_CONTEXT.md`
- `docs/ARCHITECTURE.md`
- `docs/ROADMAP.md`
- `docs/STATUS.md`
