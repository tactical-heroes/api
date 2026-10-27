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

## Observability

The host uses the `PANiXiDA.Core.Observability` MSBuild SDK. Its `.csproj`
references `<Sdk Name="PANiXiDA.Core.Observability" />`; the SDK version is pinned in
`global.json` under `msbuild-sdks`, separately from central NuGet package versions.
The existing `builder.AddObservability()` call is unchanged. Server dependencies
include ASP.NET Core, HTTP, Runtime, and Npgsql instrumentation and OTLP export;
the SDK does not add Blazor WebAssembly dependencies to this backend.

The SDK is resolved from NuGet during project evaluation, before restore,
formatting, or tests. Application telemetry configuration is unchanged.

## Claim Catalogs

Common provides `Permissions.All`, grouped by module and resource. Module names
are display metadata and do not grant access.
Named permissions follow the same hierarchy, for example
`Permissions.Identity.Roles.Read` and `Permissions.Compendium.Factions.Read`.

`GET /api/v1/claims/permissions` returns `{ "type": "permission", "items": [...] }`.
Items include `code`, `displayName`, `moduleCode`, `moduleDisplayName`,
`resourceCode` and `resourceDisplayName`. The catalog requires permission to
create or update users or roles.

User and role requests assign `{ type, value: item.code }` through `claims`.
Only `permission` claims with known catalog values can be assigned. Other claim
types and unknown permission values return HTTP 400.
Administrative use cases require their corresponding permissions. Resource lookups
require the corresponding read permission. Authentication and OAuth flows use their
own session and protocol checks.
`/connect/authorize` checks only user availability without loading roles or claims;
`/connect/token` loads current user and role claims when issuing tokens.

Changes to user permissions, role assignments, role permissions or role names
invalidate affected access tokens while retaining authorization grants and refresh
tokens. Refresh issues new access and ID tokens with current claims. Security stamps
still reject stale access tokens issued concurrently with a permission change.
Blocking, deleting or unconfirming a user continues to revoke their sessions.
Previously revoked refresh tokens and authorization grants are never reactivated.

`RemoveNonPermissionClaims` deletes all user and role claims except `permission`.
Its rollback cannot reconstruct deleted assignments.
The `/api/v1/claims/sections` endpoint and `sectionCode` catalog field are removed;
permission codes remain unchanged. Deploy this API and its migration before the
matching frontend, then sign in again to obtain permissions in the ID token.

## Request Validation

Request DTO constructors and nullable annotations define the JSON contract and
OpenAPI required fields. Optional constructor parameters must have explicit defaults.
FluentValidation and domain factories validate field values; request DTOs do not
use DataAnnotations validation attributes.

User creation and updates require a non-null `status`. The domain factory rejects
blank or unknown statuses. OpenAPI does not automatically export FluentValidation
rules, including status restrictions, as schema constraints.

EF repository concurrency exceptions handled by the mediator pipeline return HTTP 409
with Problem Details. Reload the affected entity before retrying the operation.

## Token Validation

Protected API endpoints validate Bearer tokens through HTTP introspection using
`OpenIddictValidationOptions`. Set `Issuer` to the Identity server's issuer and
`Audiences` and `ClientId` to `tactical-heroes-service`, matching `Identity:Provider:Audience`.
The existing confidential service client handles introspection and retains its
`client_credentials` and token exchange grants. The public `tactical-heroes-web`
client handles user login and refresh; no additional client is seeded.

Deployed environments load the issuer and credentials from OpenBao at
`secret/applications/tactical-heroes-api/<environment>`. Set
`OpenIddictValidationOptions__Issuer` to the same URL as `Identity__Provider__Issuer`
and use the existing service client's secret for both keys:

```text
OpenIddictValidationOptions__ClientSecret
Identity__Provider__Clients__1__ClientSecret
```

For local development, set the corresponding configuration keys with .NET User
Secrets. Each protected API request
contacts Identity, so token revocation takes effect on the next request and Identity
must be reachable.

Introspection uses OpenIddict's standard HTTP handlers. In OpenIddict 7.7.1, cancellation
during an outgoing introspection request can produce 500/Error; the
[upstream fix](https://github.com/openiddict/openiddict-core/commit/77d82cd7e65959ee6486c3288a2beb8a090f6b5e)
is pending a stable release.

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

## Personal Upload Reconciliation

The host registers `ReconcilePersonalFileUploadsCommand` through Core Wolverine's
`AddRecurringCommand`. Its schedule is configured under
`FileManager:Schedules:ReconcilePersonalFileUploadsCommand`: `Enabled`, a stable
`Name`, `CronExpression` (five-field cron, `*/5 * * * *` by default), and
`TimeZoneId` (`UTC`). Wolverine persists recurring schedules in its message store
and coordinates scheduling across replicas.

Operational settings remain under `FileManager:UploadReconciliation`:
`MinimumAge` defaults to fifteen minutes, and `BatchSize` controls database page size
(1–1000). The command runs through the FileManager mediator pipeline without
requesting an enclosing transaction; each file is finalized independently.

The first enabled recurring schedule extends Wolverine's message-store schema.
Apply it using the host's JasperFx `db-apply` command with the target environment's
configuration before starting replicas where automatic schema updates are disabled.
The EF migrator only manages module schemas, not Wolverine's message-store tables.

The recurring command handler receives pending personal-file aggregates directly
from `IFilesRepository`. The repository pages through old records using one fixed
age cutoff and preserves their optimistic concurrency tokens. Each database page
is fully loaded before checking S3, so storage requests do not keep a database
reader open. The handler reads metadata through Core's `IFileStorage.GetMetadataAsync`
using each aggregate's persisted key and the upload adapter's environment prefix,
then calls `File.CompleteUpload` and saves through the unit of work. HTTP uploads
complete and save through their existing repository/unit-of-work flow.

Existing optimistic concurrency tokens protect updates. A conflicting save ends
the current pass so the next invocation reloads current state; earlier successful
saves remain committed. No explicit row locks or enclosing S3 transaction are used.
The age threshold separates recovery from normal uploads but is not a lease:
an exceptionally long HTTP upload can still receive an optimistic concurrency
conflict if recovery completes first.

Missing objects, invalid metadata, and storage errors leave
the record unchanged for subsequent checks. Paging continues past missing objects.
Reconciliation does not delete objects, expire records, restart uploads, or inspect
files already marked `Ready`.
Incomplete multipart cleanup and upload-request idempotency remain separate work.
This recovery path establishes eventual completion for objects that are present;
it does not make S3 and PostgreSQL updates atomic or verify an expected checksum.

## Repository Layout

- `src/` - application source code.
- `tests/` - unit, integration, functional, and architecture tests.
- `tools/` - service tooling, including the EF migrator.
- `deploy/helm/` - Helm deployment values.

## Initialization Notes

This repository was created from the PANiXiDA .NET backend template. Template
packaging files and repository-only examples have been removed. Project,
namespace, Helm, Docker, and CI names were initialized for Tactical Heroes API.
