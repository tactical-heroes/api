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

The host replaces OpenIddict's introspection HTTP send handler so transport exceptions
reach the shared HTTP middleware. Client cancellation stays 499/Warning; independent
network failures stay 500/Error instead of losing their exception details.

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
