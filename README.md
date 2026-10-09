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

## Password changes and sessions

`POST /api/v1/auth/change-password` requires a user access token and the current
password. The request and `204` response are unchanged. The current session is
identified by the validated token's OpenIddict authorization ID, never by a
client-supplied session ID. Access tokens without an authorization are rejected.

A successful change revokes the user's other authorizations and tokens, including
unredeemed authorization codes. Code exchange also rejects a stale security stamp,
including codes created by authorization requests that started before the change.
The current authorization remains usable: clients
can immediately rotate their access/refresh pair through `/connect/token` with
`grant_type=refresh_token`, without asking the user to log in. The change-password
response itself does not contain OAuth tokens. Clients must store the refresh
response as usual; a server-side client keeps these tokens in its own session.

When the request includes this user's Identity login cookie, the response renews
it with the new security stamp and preserves its authentication properties.
Other Identity cookies fail validation on their next request. Cookie and
OAuth authorization validation now check their persisted state on every request.
A bearer-only call preserves its OAuth session but cannot renew a browser cookie
that was not included in the request. A separate browser/client authorization,
even on the same physical device, is treated as another session.

Failed password checks leave passwords and sessions unchanged. Password recovery
(`reset-password`) continues to revoke all of the user's OAuth tokens.

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

## File Storage Keys

FileManager generates and persists each file's storage key at creation:
`avatar/{fileId}` or `personal/{userId}/{fileId}`. Renaming or moving a file
does not change its key. Folder hierarchy and display names remain database metadata;
the storage adapter adds the environment prefix separately.

FileManager registers `PANiXiDA.Core.Infrastructure.Storage.S3` as `IFileStorage`.
The host's `appsettings.json` contains the complete mock configuration under
`FileManager:AWS` and `FileManager:S3Storage`. Replace the endpoint, bucket, region,
and credentials before using storage. Credentials can be overridden with
`FileManager__S3Storage__AccessKey` and `FileManager__S3Storage__SecretKey`.
The base prefix is `production`; `appsettings.Development.json` sets `development`.

## Repository Layout

- `src/` - application source code.
- `tests/` - unit, integration, functional, and architecture tests.
- `tools/` - service tooling, including the EF migrator.
- `deploy/helm/` - Helm deployment values.

## Initialization Notes

This repository was created from the PANiXiDA .NET backend template. Template
packaging files and repository-only examples have been removed. Project,
namespace, Helm, Docker, and CI names were initialized for Tactical Heroes API.
