# RentWise Pro

RentWise Pro is a C# ASP.NET MVC web application that helps prospective rental-property buyers:
- understand **true cash-to-close** via a Purchase Sheet
- forecast **ROI** under multiple assumptions (6 mo, 12 mo, 1 yr, 5 yrs)

## What this repo contains
- MVC web application (Razor UI)
- Read-only SQL access (stored procedures/views permitted)
- Calculation services in C# (mortgage, closing costs, ROI metrics)
- User authentication via ASP.NET Core Identity

## What this repo does NOT contain (Phase 1)
- ETL / ingestion pipelines (Phase 2)
- mobile apps (planned after web is complete)

## Quick Start
1) Configure connection string in `appsettings.Development.json`
2) Apply EF Core migrations (the repo has multiple DbContexts, so the context flag is required):
   - `dotnet ef database update --context RentWiseProDbContext`
   - `dotnet ef database update --context AuthDbContext`
   - `dotnet ef database update --context EtlDbContext`
3) Run in Visual Studio (IIS Express) or `dotnet run`
4) Visit `/` to view the public landing page and create your first user account.

## Docs
- `docs/PROJECT_CONTEXT.md`
- `docs/ARCHITECTURE.md`
- `docs/ROADMAP.md`
- `docs/STATUS.md`
