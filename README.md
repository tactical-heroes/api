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

Deployed environments use separate signing and encryption certificates, shared by
all API replicas in that environment. Store password-protected PFX files as Base64
and their passwords in OpenBao, never in repository configuration:

```text
Identity__Provider__SigningCertificates__0__PfxBase64
Identity__Provider__SigningCertificates__0__Password
Identity__Provider__EncryptionCertificates__0__PfxBase64
Identity__Provider__EncryptionCertificates__0__Password
```

Use different certificate pairs in `secret/applications/tactical-heroes-api/development`
and `secret/applications/tactical-heroes-api/production`. The Helm chart extracts
these fields into `tactical-heroes-api-env` and supplies them through `envFrom`.
Private keys are loaded into memory without importing them into the host certificate store.

Configured certificates take precedence in every environment, including Development.
When neither list is configured, local Development, Test, and infrastructure tooling
without a host environment retain development certificates. Other environments require
both lists. Partial configuration or invalid PFX data fails startup rather than
silently generating replacement keys.
Certificate presence is checked when OpenIddict server options are created, with
`ValidateOnStart` enforcing this before the API starts. Wolverine's `codegen write`
command can build the service graph without starting the API, so image creation
does not require production certificates or a Development environment override.

For rotation, add the new certificates at the next list index and retain the old ones
until tokens protected by them expire. OpenIddict selects a currently valid certificate
with the latest expiration date for new tokens. Refresh the ExternalSecret before
rolling out the API; changes to environment variables require new pods.
When first replacing per-pod development certificates, existing tokens may become
invalid and users may need to sign in again.

## Repository Layout

- `src/` - application source code.
- `tests/` - unit, integration, functional, and architecture tests.
- `tools/` - service tooling, including the EF migrator.
- `deploy/helm/` - Helm deployment values.

## Initialization Notes

This repository was created from the PANiXiDA .NET backend template. Template
packaging files and repository-only examples have been removed. Project,
namespace, Helm, Docker, and CI names were initialized for Tactical Heroes API.
