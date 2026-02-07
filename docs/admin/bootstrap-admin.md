# Bootstrap an Admin User (Development Only)

This guide explains how to enable the Development-only admin bootstrapper for RentWise Pro.

## What it does
- Ensures the `Admin` role exists.
- Ensures a bootstrap admin user exists (using a configured email).
- Ensures that user is assigned to the `Admin` role.

The bootstrapper runs **only** when:
- `ASPNETCORE_ENVIRONMENT=Development`
- `AdminBootstrap:Enabled=true`

## Configure user-secrets
From the repo root, run:

```bash
dotnet user-secrets set "AdminBootstrap:Enabled" "true"
dotnet user-secrets set "AdminBootstrap:Email" "admin@example.com"
dotnet user-secrets set "AdminBootstrap:Password" "<use-a-strong-password>"
```

## Configure environment variables

```bash
export AdminBootstrap__Enabled=true
export AdminBootstrap__Email=admin@example.com
export AdminBootstrap__Password="<use-a-strong-password>"
```

## Run once to bootstrap
1. Start the web app in Development.
2. The bootstrapper will create the role and user if needed, and assign the role.
3. Sign out and sign back in to refresh the auth cookie.

## Disable after bootstrap
Remove or set `AdminBootstrap:Enabled=false` to prevent the bootstrapper from running.

## Notes
- If the user already exists, the bootstrapper will **not** reset the password.
- If the user exists but is not in the `Admin` role, it will add the role membership.
- Passwords must never be committed to source control.
