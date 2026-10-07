# Tactical Heroes API

Backend API service for Tactical Heroes.

## Stack

- .NET 10
- ASP.NET Core
- PostgreSQL
- EF Core migrator
- Docker
- Helm
- GitHub Actions

## Local Development

Restore packages:

```bash
dotnet restore PANiXiDA.TacticalHeroes.slnx
```

Build the solution:

```bash
dotnet build PANiXiDA.TacticalHeroes.slnx --configuration Release
```

Run tests:

```bash
dotnet test --solution PANiXiDA.TacticalHeroes.slnx --configuration Release
```

Identity and Compendium integration and functional tests run sequentially within
each assembly (`ParallelMode.None`). CI still runs separate test projects in
parallel. Database resets clear only the module schema and preserve migration
history and Wolverine's messaging tables, which remain in use by background workers.

To test the API host with the same pregenerated Wolverine handlers used by the
container build, run from a clean checkout (PowerShell):

```powershell
Push-Location src/PANiXiDA.TacticalHeroes.Host
dotnet run -c Release --no-launch-profile -- codegen write
Pop-Location
$env:WOLVERINE_PREGENERATED = '1'
dotnet test --solution PANiXiDA.TacticalHeroes.slnx -c Release
Remove-Item Env:WOLVERINE_PREGENERATED
```

The test build compiles the generated sources into the Host assembly. The Identity
and Compendium web factories select that assembly explicitly and use strict
`TypeLoadMode.Static` when `WOLVERINE_PREGENERATED=1`; a missing generated handler
fails instead of silently compiling it at runtime. Ordinary functional tests keep
`Auto`. Isolated integration-test hosts keep their own configuration.
The **Pregenerated handler tests** workflow validates this path in addition to
the existing CI tests.

The manually dispatched **Codegen benchmark** workflow runs 20 independent runner
pairs with balanced order and one full warmup of each variant per runner.
The baseline uses `Auto` without generated sources (dynamic fallback); the
candidate uses `codegen write` plus `Static`, including its static handler registry.
Deployed containers currently use `Auto`, so this also validates a stricter mode.
Each measured run starts with cleaned build outputs. Its total includes the Host
bootstrap build, `codegen write`, recompilation and all 13 test projects, including
container startup and shutdown. Both variants exclude package restore, image
downloads, cleanup and warmups. Assemblies run sequentially without coverage;
these timings do not represent the latency of the parallel CI matrix.
The workflow retains raw timings, all test counts, logs and a paired 95% bootstrap
confidence interval without discarding outliers. To reproduce one pair locally
with Python 3 and Docker, use
`python scripts/benchmark-codegen.py --pair 0 --output <outside-repository-directory>`.
It creates and removes its own disposable worktree at the current commit.

Run the API:

```bash
dotnet run --project src/PANiXiDA.TacticalHeroes.Host/PANiXiDA.TacticalHeroes.Host.csproj
```

In the `Development` environment, open `/scalar` to explore the Identity and
Compendium APIs. For protected endpoints, enter an access token in the Bearer
authentication field without the `Bearer` prefix; Scalar adds the authorization
header automatically.

The host's `ScalarConfiguration:BearerAuthenticationSchemes` setting maps
`OpenIddict.Validation.AspNetCore` (protected API endpoints) and
`OpenIddict.Server.AspNetCore` (UserInfo) to Bearer authentication in OpenAPI.
This setting describes authentication in the documentation; endpoint access rules
remain unchanged.

Run the EF migrator:

```bash
dotnet run --project tools/PANiXiDA.TacticalHeroes.Ef.Migrator/PANiXiDA.TacticalHeroes.Ef.Migrator.csproj --configuration Release
```

## OpenIddict Certificates

Store separate signing/encryption certificates as Base64 PFX with passwords in OpenBao at
`secret/applications/tactical-heroes-api/<environment>` (`development` or `production`),
shared by that environment's API replicas:

```text
Identity__Provider__SigningCertificates__0__PfxBase64
Identity__Provider__SigningCertificates__0__Password
Identity__Provider__EncryptionCertificates__0__PfxBase64
Identity__Provider__EncryptionCertificates__0__Password
```

Never commit PFX data or passwords.

Configured certificates take precedence. Development certificates are a fallback only
when both lists are empty in Development, Test, or tooling without a host environment.
Other environments require both lists; partial or invalid configuration fails startup.

For rotation, add certificates at the next index and retain old ones until their tokens
expire. Refresh ExternalSecrets and restart pods after changes. The initial switch
may require users to sign in again.

## Repository Layout

- `src/` - application source code.
- `tests/` - unit, integration, functional, and architecture tests.
- `tools/` - service tooling, including the EF migrator.
- `deploy/helm/` - Helm deployment values.

## Initialization Notes

This repository was created from the PANiXiDA .NET backend template. Template
packaging files and repository-only examples have been removed. Project,
namespace, Helm, Docker, and CI names were initialized for Tactical Heroes API.
