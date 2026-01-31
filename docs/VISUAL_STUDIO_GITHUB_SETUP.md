# Step-by-step: Create the new RentWise Pro project (Visual Studio + GitHub)

## 1) Create GitHub repo
1. On GitHub, create a new repository named `RentWisePro`.
2. Initialize with a README **unchecked** (we’ll add our own).
3. Copy the repo URL.

## 2) Create the solution in Visual Studio
1. Open Visual Studio 2022.
2. **Create a new project**.
3. Choose **ASP.NET Core Web App (Model-View-Controller)**.
4. Project name: `RentWisePro.Web`
5. Solution name: `RentWisePro`
6. Framework: latest LTS available to you (e.g., .NET 8 LTS).
7. Authentication: **None** (Phase 1).
8. Enable HTTPS: Yes.

## 3) Initialize git and connect remote
From the solution root in PowerShell:
1. `git init`
2. `git add .`
3. `git commit -m "chore: initial MVC scaffold"`
4. `git branch -M master`
5. `git remote add origin <PASTE_YOUR_GITHUB_URL>`
6. `git push -u origin master`

## 4) Add baseline docs + agent instructions
1. Add `AGENTS.md`, `README.md`, and `docs/*` from this bootstrap pack.
2. Commit on a branch:
   - `git checkout -b chore/bootstrap-docs`
   - `git add .`
   - `git commit -m "chore: add steering docs and roadmap"`
   - `git push -u origin chore/bootstrap-docs`
3. Open PR and merge.

## 5) Add SQL migration + local DB
1. Add `migrations/001_init.sql`
2. Create a local SQL Server database (LocalDB or SQL Express)
3. Run the migration script.
4. Add connection string to `appsettings.Development.json` (do not commit secrets).

## 6) Add CI (for Codex)
1. Add a workflow under `.github/workflows/ci.yml`:
   - Use `actions/setup-dotnet`
   - `dotnet restore`
   - `dotnet build -c Release`
2. Commit and ensure PR goes green.

## 7) Codex integration tips
- Always give Codex:
  - the branch name to create
  - exact files to touch
  - verification steps
- After merge, update `docs/STATUS.md` and `docs/ROADMAP.md`
