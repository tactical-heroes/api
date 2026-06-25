# <ServiceName>

`<ServiceName>` is a .NET backend service for <short purpose>.

It is designed for <target audience / consumers> who need <main value / main scenario>.

## Status

[![CI](https://github.com/<OWNER>/<REPOSITORY>/actions/workflows/ci.yml/badge.svg)](https://github.com/<OWNER>/<REPOSITORY>/actions/workflows/ci.yml)
[![Target Framework](https://img.shields.io/badge/target-net10.0-512BD4)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/docker-ready-2496ED)](https://www.docker.com/)
[![Helm](https://img.shields.io/badge/helm-ready-0F1689)](https://helm.sh/)
[![License](https://img.shields.io/github/license/<OWNER>/<REPOSITORY>.svg)](LICENSE)

## Overview

Describe:

- what business problem this service solves;
- which product area or bounded context it belongs to;
- which clients, services or systems consume it;
- what external systems it depends on;
- what this service intentionally does not own.

Keep this section short and practical.

Example:

`<ServiceName>` provides <domain capability> for <product/system>.
It exposes HTTP API endpoints for <main operations>, stores data in PostgreSQL, and integrates with <external services>.

## Features

- Feature 1
- Feature 2
- Feature 3
- Feature 4
- Feature 5

## Architecture

This service uses a modular backend structure.

```text
Client / External System
        │
        ▼
Organization.Product.Host
        │
        ▼
Module.Presentation
        │
        ▼
Module.Application
        │
        ▼
Module.Domain
        │
        ▼
Module.Infrastructure
        │
        ▼
PostgreSQL / S3 / Messaging / External Services
```

### Main projects

- `src/Organization.Product.Host` — ASP.NET Core host, composition root, runtime configuration, Docker image entry point.
- `src/Module/Organization.Product.Module.Domain` — domain model, aggregate roots, entities, value objects, domain events, policies and specifications.
- `src/Module/Organization.Product.Module.Application` — use cases, commands, queries, handlers, validators, application abstractions.
- `src/Module/Organization.Product.Module.Infrastructure` — persistence, EF Core integration, messaging, external infrastructure adapters.
- `src/Module/Organization.Product.Module.Presentation` — HTTP endpoints, request/response models, mappers and presentation dependency injection.
- `tests/Module/Organization.Product.Module.UnitTests` — isolated unit tests.
- `tests/Module/Organization.Product.Module.IntegrationTests` — integration tests for module behavior and infrastructure.
- `tests/Module/Organization.Product.Module.FunctionalTests` — API/host-level functional tests.
- `tests/Organization.Product.ArchitectureTests` — architecture and dependency rules.
- `tests/Organization.Product.Testing` — shared testing helpers, factories, assertions and infrastructure.
- `tools/Organization.Product.Ef.Migrator` — EF migration generation/application tool.
- `deploy/helm/<service-name>` — Helm deployment values.

### Dependency direction

Keep dependencies intentional:

```text
Host
 ├── Presentation
 ├── Infrastructure
 ├── Application
 └── Domain

Presentation ─────► Application
Application ──────► Domain
Infrastructure ───► Application + Domain
Domain ───────────► no application/infrastructure/presentation dependencies
```

## Quick Start

### Requirements

- .NET 10 SDK
- Docker
- PostgreSQL
- S3-compatible storage, if file storage is used
- MongoDB, if the final service actually uses it
- Kubernetes and Helm, if deploying through Helm
- Access to required NuGet feeds, if private packages are used
- Access to required external services:
  - `<ExternalService1>`
  - `<ExternalService2>`
  - `<ExternalService3>`

### Clone

```bash
git clone <REPOSITORY_URL>
cd <REPOSITORY>
```

### Restore

```bash
dotnet restore PANiXiDA.DotnetTemplate.slnx
```

Rename the solution file for the final service if needed.

Example:

```bash
dotnet restore <SolutionName>.slnx
```

### Configure local settings

For local development, fill `src/Organization.Product.Host/appsettings.json`
and `tools/Organization.Product.Ef.Migrator/appsettings.json`.

CI/CD and deployed environments can override these values through environment files,
environment variables or platform configuration.

Remove unused settings from the final service.

### Run local infrastructure

PostgreSQL:

```bash
docker run --name <service-name>-postgres \
  -e POSTGRES_DB=<database> \
  -e POSTGRES_USER=<username> \
  -e POSTGRES_PASSWORD=<password> \
  -p 5432:5432 \
  -d postgres:latest
```

MinIO / S3-compatible storage, only if the service uses S3:

```bash
docker run --name <service-name>-minio \
  -e MINIO_ROOT_USER=<access-key> \
  -e MINIO_ROOT_PASSWORD=<password> \
  -p 9000:9000 \
  -p 9001:9001 \
  -d minio/minio server /data --console-address ":9001"
```

MongoDB, only if the service uses MongoDB:

```bash
docker run --name <service-name>-mongo \
  -p 27017:27017 \
  -d mongo:latest
```

### Run database migrator

```bash
dotnet run \
  --project tools/Organization.Product.Ef.Migrator/Organization.Product.Ef.Migrator.csproj \
  --configuration Release
```

### Run API

```bash
dotnet run \
  --project src/Organization.Product.Host/Organization.Product.Host.csproj
```

Default local endpoints:

```text
HTTP API: http://localhost:8080
gRPC:     http://localhost:8081
API docs: <LOCAL_API_DOCS_URL>
```

Fill the actual API docs URL after configuring OpenAPI/Scalar routes in the final service.

## Configuration

Configuration is read from:

- `appsettings.json`;
- `appsettings.Development.json`;
- environment variables;
- Helm values;
- deployed environment configuration.

### Main settings

| Key                                            | Required                    | Description                                             |
| ---------------------------------------------- | --------------------------- | ------------------------------------------------------- |
| `ASPNETCORE_ENVIRONMENT`                       | yes                         | ASP.NET Core environment name.                          |
| `DOTNET_ENVIRONMENT`                           | yes                         | Generic .NET environment name.                          |
| `AllowedHosts`                                 | yes                         | ASP.NET Core allowed hosts setting.                     |
| `Kestrel:Endpoints:Http:Url`                   | yes                         | HTTP endpoint binding.                                  |
| `Kestrel:Endpoints:Http:Protocols`             | yes                         | HTTP protocol configuration.                            |
| `Kestrel:Endpoints:Grpc:Url`                   | if gRPC is used             | gRPC endpoint binding.                                  |
| `Kestrel:Endpoints:Grpc:Protocols`             | if gRPC is used             | gRPC protocol configuration.                            |
| `ConnectionStrings:PostgreSqlConnectionString` | yes                         | PostgreSQL connection string.                           |
| `ConnectionStrings:MongoDbConnectionString`    | if MongoDB is used          | MongoDB connection string.                              |
| `S3:ServiceUrl`                                | if S3 is used               | S3-compatible service URL.                              |
| `S3:Region`                                    | if S3 is used               | S3 region.                                              |
| `S3:ForcePathStyle`                            | if S3 is used               | Path-style addressing flag for S3-compatible providers. |
| `S3:AccessKey`                                 | if S3 is used               | S3 access key.                                          |
| `S3:SecretKey`                                 | if S3 is used               | S3 access password.                                     |
| `S3:BucketName`                                | if S3 is used               | Bucket name.                                            |
| `S3:BasePrefix`                                | if S3 is used               | Prefix for objects created by this service.             |
| `S3:DefaultPresignTtl`                         | if S3 is used               | Default presigned URL lifetime.                         |
| `ScalarConfiguration:Title`                    | if API docs are enabled     | API reference title.                                    |
| `OTEL_SERVICE_NAME`                            | if observability is enabled | OpenTelemetry service name.                             |
| `OTEL_EXPORTER_OTLP_ENDPOINT`                  | if observability is enabled | OTLP collector endpoint.                                |
| `OTEL_EXPORTER_OTLP_PROTOCOL`                  | if observability is enabled | OTLP protocol, for example `grpc`.                      |

### Environment variables

Use double underscores for nested configuration keys:

```bash
ASPNETCORE_ENVIRONMENT=Development
DOTNET_ENVIRONMENT=Development

ConnectionStrings__PostgreSqlConnectionString=Host=localhost;Port=5432;Database=<database>;Username=<username>;Password=<password>
ConnectionStrings__MongoDbConnectionString=mongodb://localhost:27017/<database>

S3__ServiceUrl=http://localhost:9000
S3__Region=us-east-1
S3__ForcePathStyle=true
S3__AccessKey=<access-key>
S3__SecretKey=<password>
S3__BucketName=<bucket-name>
S3__BasePrefix=development
S3__DefaultPresignTtl=00:15:00

ScalarConfiguration__Title=<ServiceName> API Reference

OTEL_SERVICE_NAME=<service-name>-api-development
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317
OTEL_EXPORTER_OTLP_PROTOCOL=grpc
```

### Example `appsettings.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:8080",
        "Protocols": "Http1"
      },
      "Grpc": {
        "Url": "http://0.0.0.0:8081",
        "Protocols": "Http2"
      }
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "PostgreSqlConnectionString": "Host=localhost;Port=5432;Database=<database>;Username=<username>;Password=<password>",
    "MongoDbConnectionString": "mongodb://localhost:27017/<database>"
  },
  "S3": {
    "ServiceUrl": "http://localhost:9000",
    "Region": "us-east-1",
    "ForcePathStyle": true,
    "AccessKey": "<access-key>",
    "SecretKey": "<password>",
    "BucketName": "<bucket-name>",
    "BasePrefix": "development",
    "DefaultPresignTtl": "00:15:00"
  },
  "ScalarConfiguration": {
    "Title": "<ServiceName> API Reference"
  },
  "OTEL_SERVICE_NAME": "<service-name>-api",
  "OTEL_EXPORTER_OTLP_ENDPOINT": "http://localhost:4317"
}
```

## API

### HTTP API

Describe endpoint groups exposed by the service.

| Area      | Route             | Version | Description     |
| --------- | ----------------- | ------- | --------------- |
| `<Area1>` | `/api/v1/<area1>` | `v1`    | `<description>` |
| `<Area2>` | `/api/v1/<area2>` | `v1`    | `<description>` |
| `<Area3>` | `/api/v1/<area3>` | `v1`    | `<description>` |

Replace these rows with the real endpoint groups exposed by the service.

### API documentation

API documentation is available at:

```text
<API_DOCS_URL>
```

Use it to inspect:

- available endpoints;
- request and response models;
- status codes;
- validation errors;
- authentication requirements.

### Authentication and authorization

Describe the actual authentication model.

Possible options:

- public API;
- JWT Bearer authentication;
- service-to-service authentication;
- role-based authorization;
- policy-based authorization.

Example:

This service uses JWT Bearer authentication.
Tokens are validated against `<IdentityProvider>`.

Required claims:

| Claim            | Required            | Description           |
| ---------------- | ------------------- | --------------------- |
| `sub`            | yes                 | Subject identifier.   |
| `role`           | depends on endpoint | User or service role. |
| `<custom-claim>` | depends on endpoint | `<description>`.      |

Remove this section if the service does not use authentication.

### gRPC

Describe gRPC only if this service exposes gRPC endpoints.

| Service         | Endpoint                | Description     |
| --------------- | ----------------------- | --------------- |
| `<GrpcService>` | `http://localhost:8081` | `<description>` |

Remove this section if gRPC is not used.

## Domain Model

Describe the main domain concepts.

| Concept           | Description     |
| ----------------- | --------------- |
| `<AggregateRoot>` | `<description>` |
| `<Entity>`        | `<description>` |
| `<ValueObject>`   | `<description>` |
| `<DomainEvent>`   | `<description>` |
| `<Policy>`        | `<description>` |
| `<Specification>` | `<description>` |

### Domain rules

Document important invariants:

- rule 1;
- rule 2;
- rule 3.

### Domain events

| Event           | Raised when   | Consumers    |
| --------------- | ------------- | ------------ |
| `<DomainEvent>` | `<condition>` | `<consumer>` |

## Application Layer

Application layer contains use cases and orchestration logic.

Suggested feature structure:

```text
src/Module/Organization.Product.Module.Application/
└── <Feature>/
    ├── Abstractions/
    ├── Create/
    ├── Delete/
    ├── Events/
    ├── GetDetails/
    ├── GetList/
    ├── Update/
    └── <Feature>FilterParameters.cs
```

Describe:

- commands;
- queries;
- handlers;
- validators;
- application services;
- persistence abstractions;
- integration events;
- authorization checks, if implemented here.

## Infrastructure Layer

Infrastructure layer contains technical implementations.

Possible responsibilities:

- EF Core persistence;
- read/write `DbContext`;
- repositories;
- database mappings;
- external service clients;
- message bus integration;
- file storage;
- infrastructure dependency injection.

Suggested structure:

```text
src/Module/Organization.Product.Module.Infrastructure/
├── DependencyInjection/
└── Persistence/
    ├── Core/
    └── Features/
        └── <Feature>/
```

## Database

This service uses PostgreSQL.

### Database name

```text
<database-name>
```

### Connection string

```text
Host=<host>;Port=5432;Database=<database>;Username=<username>;Password=<password>
```

### Local PostgreSQL with Docker

```bash
docker run --name <service-name>-postgres \
  -e POSTGRES_DB=<database> \
  -e POSTGRES_USER=<username> \
  -e POSTGRES_PASSWORD=<password> \
  -p 5432:5432 \
  -d postgres:latest
```

### EF migrator

Migrator project:

```text
tools/Organization.Product.Ef.Migrator/Organization.Product.Ef.Migrator.csproj
```

Run migrator:

```bash
dotnet run \
  --project tools/Organization.Product.Ef.Migrator/Organization.Product.Ef.Migrator.csproj \
  --configuration Release
```

### Migrator configuration

Example:

```json
{
  "Ef": {
    "ProjectPath": "../../../../../src/Module/Organization.Product.Module.Infrastructure",
    "MigrationsDirectory": "Persistence/Core/Migrations"
  },
  "GenerateMigrations": true,
  "ApplyMigrations": true,
  "ConnectionStrings": {
    "PostgreSqlConnectionString": "Host=localhost;Port=5432;Database=<database>;Username=<username>;Password=<password>"
  }
}
```

Configuration keys:

| Key                                            | Description                                                              |
| ---------------------------------------------- | ------------------------------------------------------------------------ |
| `Ef:ProjectPath`                               | Relative path to the infrastructure project containing EF configuration. |
| `Ef:MigrationsDirectory`                       | Directory where migrations are generated.                                |
| `GenerateMigrations`                           | Enables migration generation.                                            |
| `ApplyMigrations`                              | Enables applying migrations to the database.                             |
| `ConnectionStrings:PostgreSqlConnectionString` | PostgreSQL connection string used by the migrator.                       |

## File Storage

Keep this section only if the service uses S3-compatible storage.

| Setting                | Description                                 |
| ---------------------- | ------------------------------------------- |
| `S3:ServiceUrl`        | S3-compatible endpoint.                     |
| `S3:Region`            | Region.                                     |
| `S3:ForcePathStyle`    | Enables path-style addressing.              |
| `S3:AccessKey`         | Access key.                                 |
| `S3:SecretKey`         | S3 access password.                         |
| `S3:BucketName`        | Bucket name.                                |
| `S3:BasePrefix`        | Object prefix for this service/environment. |
| `S3:DefaultPresignTtl` | Default presigned URL TTL.                  |

## Code Generation

The API Dockerfile runs host code generation before publishing:

```bash
dotnet run \
  --project src/Organization.Product.Host/Organization.Product.Host.csproj \
  --configuration Release \
  --no-restore \
  --no-launch-profile \
  -- codegen write
```

Run this command locally when generated code must be refreshed before build/publish.

Remove this section if the final service does not use runtime/tooling code generation.

## Project Structure

```text
.
├── .github/
│   └── workflows/
│       └── ci.yml
├── deploy/
│   └── helm/
│       └── <service-name>/
│           ├── images-development.yaml
│           ├── images-production.yaml
│           ├── values-development.yaml
│           ├── values-production.yaml
│           └── values.yaml
├── src/
│   ├── Organization.Product.Host/
│   │   ├── Common/
│   │   ├── Properties/
│   │   ├── Dockerfile
│   │   ├── Organization.Product.Host.csproj
│   │   ├── Program.cs
│   │   ├── Program.ExposeForTests.cs
│   │   ├── appsettings.Development.json
│   │   └── appsettings.json
│   └── Module/
│       ├── Organization.Product.Module.Application/
│       ├── Organization.Product.Module.Domain/
│       ├── Organization.Product.Module.Infrastructure/
│       └── Organization.Product.Module.Presentation/
├── tests/
│   ├── Module/
│   │   ├── Organization.Product.Module.FunctionalTests/
│   │   ├── Organization.Product.Module.IntegrationTests/
│   │   └── Organization.Product.Module.UnitTests/
│   ├── Organization.Product.ArchitectureTests/
│   └── Organization.Product.Testing/
├── tools/
│   └── Organization.Product.Ef.Migrator/
│       ├── Properties/
│       ├── Dockerfile
│       ├── Organization.Product.Ef.Migrator.csproj
│       ├── Program.cs
│       └── appsettings.json
├── .dockerignore
├── .editorconfig
├── .gitattributes
├── .gitignore
├── Directory.Build.props
├── Directory.Build.targets
├── Directory.Packages.props
├── global.json
├── LICENSE
├── PANiXiDA.DotnetTemplate.slnx
└── README.md
```

### Main repository files

- `.github/workflows/ci.yml` — CI/CD workflow.
- `PANiXiDA.DotnetTemplate.slnx` — solution file. Rename it for the final service.
- `Directory.Build.props` — shared MSBuild settings.
- `Directory.Build.targets` — shared build targets.
- `Directory.Packages.props` — centralized package versions.
- `global.json` — .NET SDK and test runner configuration.
- `src/Organization.Product.Host/Dockerfile` — API Docker image.
- `tools/Organization.Product.Ef.Migrator/Dockerfile` — migrator Docker image.
- `deploy/helm/<service-name>` — Helm deployment configuration.
- `README.md` — service overview and development documentation.
- `LICENSE` — project license.

## Development

### Restore

```bash
dotnet restore PANiXiDA.DotnetTemplate.slnx
```

### Build

```bash
dotnet build PANiXiDA.DotnetTemplate.slnx --configuration Release
```

### Format

```bash
dotnet format PANiXiDA.DotnetTemplate.slnx
```

### Test

```bash
dotnet test PANiXiDA.DotnetTemplate.slnx --configuration Release
```

### Run unit tests

```bash
dotnet test \
  tests/Module/Organization.Product.Module.UnitTests/Organization.Product.Module.UnitTests.csproj \
  --configuration Release
```

### Run integration tests

```bash
dotnet test \
  tests/Module/Organization.Product.Module.IntegrationTests/Organization.Product.Module.IntegrationTests.csproj \
  --configuration Release
```

### Run functional tests

```bash
dotnet test \
  tests/Module/Organization.Product.Module.FunctionalTests/Organization.Product.Module.FunctionalTests.csproj \
  --configuration Release
```

### Run architecture tests

```bash
dotnet test \
  tests/Organization.Product.ArchitectureTests/Organization.Product.ArchitectureTests.csproj \
  --configuration Release
```

### Run API locally

```bash
dotnet run \
  --project src/Organization.Product.Host/Organization.Product.Host.csproj
```

### Run migrator locally

```bash
dotnet run \
  --project tools/Organization.Product.Ef.Migrator/Organization.Product.Ef.Migrator.csproj \
  --configuration Release
```

### Full local validation

```bash
dotnet restore PANiXiDA.DotnetTemplate.slnx
dotnet format PANiXiDA.DotnetTemplate.slnx
dotnet build PANiXiDA.DotnetTemplate.slnx --configuration Release
dotnet test PANiXiDA.DotnetTemplate.slnx --configuration Release
```

## Testing

This repository contains several test levels.

| Test project                                   | Purpose                                                 |
| ---------------------------------------------- | ------------------------------------------------------- |
| `Organization.Product.Module.UnitTests`        | Tests domain/application logic in isolation.            |
| `Organization.Product.Module.IntegrationTests` | Tests module behavior with infrastructure dependencies. |
| `Organization.Product.Module.FunctionalTests`  | Tests the API through the host.                         |
| `Organization.Product.ArchitectureTests`       | Tests architecture rules and dependency boundaries.     |
| `Organization.Product.Testing`                 | Shared testing infrastructure.                          |

### Testing stack

This repository may use:

- xUnit v3;
- Shouldly;
- NSubstitute;
- ASP.NET Core MVC Testing;
- Testcontainers;
- Respawn;
- ArchUnitNET;
- Microsoft Testing Platform;
- code coverage and TRX reports.

Remove items that are not used in the final service.

### Test data

Describe:

- where test data factories are located;
- how database state is reset between tests;
- how external services are mocked;
- which tests require Docker;
- which tests can run without infrastructure.

## Docker

### Build API image

```bash
docker build \
  -f src/Organization.Product.Host/Dockerfile \
  -t <registry>/<repository>/<service-name>-api:local \
  .
```

### Run API container

```bash
docker run --rm \
  --name <service-name>-api \
  -p 8080:8080 \
  -p 8081:8081 \
  --env-file .env \
  <registry>/<repository>/<service-name>-api:local
```

Expose `8081` only if gRPC is used.

### Build migrator image

```bash
docker build \
  -f tools/Organization.Product.Ef.Migrator/Dockerfile \
  -t <registry>/<repository>/<service-name>-ef-migrator:local \
  .
```

### Run migrator container

```bash
docker run --rm \
  --name <service-name>-ef-migrator \
  --env-file .env \
  <registry>/<repository>/<service-name>-ef-migrator:local
```

### Example `.env`

```env
ASPNETCORE_ENVIRONMENT=Development
DOTNET_ENVIRONMENT=Development

ConnectionStrings__PostgreSqlConnectionString=Host=<host>;Port=5432;Database=<database>;Username=<username>;Password=<password>
ConnectionStrings__MongoDbConnectionString=mongodb://<host>:27017/<database>

S3__ServiceUrl=http://<host>:9000
S3__Region=us-east-1
S3__ForcePathStyle=true
S3__AccessKey=<access-key>
S3__SecretKey=<password>
S3__BucketName=<bucket-name>
S3__BasePrefix=development
S3__DefaultPresignTtl=00:15:00

ScalarConfiguration__Title=<ServiceName> API Reference

OTEL_SERVICE_NAME=<service-name>-api-development
OTEL_EXPORTER_OTLP_ENDPOINT=http://<otel-collector>:4317
OTEL_EXPORTER_OTLP_PROTOCOL=grpc
```

## Observability

The service supports observability through OpenTelemetry configuration.

### Logging

Describe:

- where logs are written locally;
- where logs are sent in deployed environments;
- how to filter logs by `OTEL_SERVICE_NAME`;
- important fields and correlation identifiers.

### Metrics

Describe metrics only if enabled.

Possible topics:

- Prometheus endpoint;
- OpenTelemetry metrics exporter;
- dashboards;
- alerting rules;
- SLO/SLA metrics.

Remove this section if metrics are not enabled.

### Tracing

Describe tracing only if enabled.

Possible topics:

- OTLP exporter;
- trace id propagation;
- correlation id;
- external service spans;
- database spans.

Remove this section if tracing is not enabled.

## Helm Deployment

Helm values are located here:

```text
deploy/helm/<service-name>
```

Expected files:

```text
values.yaml
values-development.yaml
values-production.yaml
images-development.yaml
images-production.yaml
```

### Common values

`values.yaml` contains shared deployment settings:

- replica count;
- API image repository and tag;
- image pull configuration;
- service port and container port;
- environment variables;
- external configuration provider settings;
- migrations job configuration;
- gateway configuration;
- security context;
- resource requests and limits;
- startup/readiness/liveness probes;
- node selector, tolerations and affinity.

### Environment values

`values-development.yaml` and `values-production.yaml` contain environment-specific overrides:

- `ASPNETCORE_ENVIRONMENT`;
- `DOTNET_ENVIRONMENT`;
- `OTEL_SERVICE_NAME`;
- external configuration role;
- external configuration remote key;
- gateway host;
- HTTPS section name;
- replica count.

### Image values

`images-development.yaml` and `images-production.yaml` contain image tags for:

- API image;
- EF migrator image.

These files can be updated by CI/CD after Docker images are built.

### Manual Helm upgrade

Example:

```bash
helm upgrade --install <release-name> <chart-name> \
  --namespace <namespace> \
  --values deploy/helm/<service-name>/values.yaml \
  --values deploy/helm/<service-name>/values-development.yaml \
  --values deploy/helm/<service-name>/images-development.yaml
```

Adapt the command to the actual chart source used by your platform.

## CI/CD

CI/CD is configured in:

```text
.github/workflows/ci.yml
```

The pipeline may include:

- formatting validation;
- automated tests;
- Helm validation;
- environment resolution;
- API Docker image build;
- EF migrator Docker image build;
- deployment image values update;
- Telegram notification.

### Required repository variables

Fill only the variables actually used by the final service.

| Variable                        | Description                                     |
| ------------------------------- | ----------------------------------------------- |
| `HELM_VALUES_PATH`              | Path to Helm values directory.                  |
| `HELM_COMMON_VALUES_FILE`       | Common Helm values file name.                   |
| `HELM_DEVELOPMENT_RELEASE_NAME` | Development Helm release name.                  |
| `HELM_PRODUCTION_RELEASE_NAME`  | Production Helm release name.                   |
| `SERVICE_FOLDER`                | API service folder.                             |
| `SERVICE_NAME`                  | API service image name.                         |
| `MIGRATOR_FOLDER`               | EF migrator folder.                             |
| `MIGRATOR_NAME`                 | EF migrator image name.                         |
| `REGISTRY_URL`                  | Container registry URL.                         |
| `TELEGRAM_CHAT_ID`              | Telegram chat id for notifications, if enabled. |

## Deployment

### Environments

| Environment | Branch        | URL                 | Notes     |
| ----------- | ------------- | ------------------- | --------- |
| Development | `development` | `<development-url>` | `<notes>` |
| Production  | `main`        | `<production-url>`  | `<notes>` |

### Deployment flow

Describe the actual deployment flow.

Example:

1. Pull request runs formatting, tests and Helm validation.
2. Push to `development` builds API and migrator images for Development.
3. Push to `main` builds API and migrator images for Production.
4. CI updates environment image values.
5. Helm deploys API and migration job.
6. Notification is sent after completion.

### Rollback

Describe rollback strategy:

- redeploy previous image tag;
- revert Helm image values;
- disable feature flag if available;
- restore database backup if schema/data migration requires it;
- notify affected teams.

## Operational Notes

### Common commands

```bash
# Check SDK
dotnet --info

# Restore
dotnet restore PANiXiDA.DotnetTemplate.slnx

# Build
dotnet build PANiXiDA.DotnetTemplate.slnx --configuration Release

# Format
dotnet format PANiXiDA.DotnetTemplate.slnx

# Test
dotnet test PANiXiDA.DotnetTemplate.slnx --configuration Release

# Run API
dotnet run --project src/Organization.Product.Host/Organization.Product.Host.csproj

# Run migrator
dotnet run --project tools/Organization.Product.Ef.Migrator/Organization.Product.Ef.Migrator.csproj --configuration Release

# Build API Docker image
docker build -f src/Organization.Product.Host/Dockerfile -t <service-name>-api:local .

# Build migrator Docker image
docker build -f tools/Organization.Product.Ef.Migrator/Dockerfile -t <service-name>-ef-migrator:local .
```

### Troubleshooting

#### API does not start

Check:

- `ASPNETCORE_ENVIRONMENT`;
- `DOTNET_ENVIRONMENT`;
- PostgreSQL connection string;
- S3 configuration, if S3 is used;
- MongoDB configuration, if MongoDB is used;
- external service URLs;
- port conflicts on `8080` and `8081`;
- missing NuGet credentials;
- missing environment variables.

#### Database connection fails

Check:

- PostgreSQL container or server is running;
- host, port, database, username and password are correct;
- database exists;
- network access is allowed;
- connection string is supplied to both API and migrator.

#### Migrator fails

Check:

- `Ef:ProjectPath`;
- `Ef:MigrationsDirectory`;
- `GenerateMigrations`;
- `ApplyMigrations`;
- PostgreSQL connection string;
- migration permissions;
- generated migration files;
- Docker image environment variables.

#### API documentation is unavailable

Check:

- API is running;
- correct HTTP port is used;
- OpenAPI/Scalar configuration is registered;
- reverse proxy routes are configured correctly;
- environment-specific settings do not disable API docs.

#### Integration or functional tests fail locally

Check:

- Docker daemon is running;
- required containers can be pulled;
- PostgreSQL port conflicts;
- stale containers from previous test runs;
- test configuration;
- external services are mocked or available.

## Template Replacement Checklist

Before using this README in a final service, replace:

- `<ServiceName>`;
- `<OWNER>`;
- `<REPOSITORY>`;
- `<REPOSITORY_URL>`;
- `<SolutionName>`;
- `<short purpose>`;
- `<target audience / consumers>`;
- `<main value / main scenario>`;
- `<database-name>`;
- `<ExternalService1>`;
- `<ExternalService2>`;
- `<ExternalService3>`;
- endpoint tables;
- feature list;
- deployment environments;
- CI/CD variables and protected values;
- maintainer contacts;
- license name.

Also review and rename template-specific values:

- `PANiXiDA.DotnetTemplate.slnx`;
- `Organization.Product`;
- `Organization.Product.Host`;
- `Organization.Product.Module`;
- `Organization.Product.Ef.Migrator`;
- `Organization.Product.Testing`;
- `Organization.Product.ArchitectureTests`;
- `Module`;
- `dotnet-template`;
- `dotnet-template-api`;
- `dotnet-template-api-development`;
- `dotnet-template-api-production`;
- `dotnet-template-registry`;
- `dotnet-template-api-env`;
- `dotnet-template-development`;
- `dotnet-template-production`;
- `applications/dotnet-template/development`;
- `applications/dotnet-template/production`;
- `ghcr.io/panixida-templates/dotnet-backend-template/api`;
- `ghcr.io/panixida-templates/dotnet-backend-template/ef-migrator`;
- default database name;
- default PostgreSQL credentials;
- default S3 bucket;
- default gateway hosts;
- unused MongoDB sections;
- unused S3 sections;
- unused gRPC sections;
- unused observability sections.

## Tooling and Conventions

This repository uses:

- .NET 10;
- nullable reference types;
- implicit usings;
- central package management;
- warnings as errors;
- ASP.NET Core;
- EF Core;
- PostgreSQL;
- Docker;
- Helm;
- GitHub Actions;
- OpenTelemetry-ready configuration;
- xUnit v3;
- Shouldly;
- NSubstitute;
- functional, integration, unit and architecture tests;
- optional S3-compatible storage;
- optional MongoDB;
- optional gRPC.

Add more items only if they are actually relevant for the final service.

## Contributing

Contributions are welcome.

### General rules

- keep public contracts intentional;
- avoid unnecessary dependencies;
- preserve repository structure and naming conventions;
- keep module boundaries explicit;
- do not introduce breaking changes without review;
- keep configuration documented;
- keep migrations consistent with model changes;
- keep Helm values and CI variables updated;
- keep documentation updated.

### Code style

- follow the repository `.editorconfig`;
- prefer readable and explicit code;
- keep naming consistent with the existing codebase;
- keep business logic out of presentation endpoints;
- keep infrastructure details out of domain and application contracts;
- avoid leaking persistence models into public API contracts.

### Tests

- add or update tests for meaningful behavior changes;
- cover both success and failure scenarios where applicable;
- add regression tests for bug fixes;
- keep integration tests deterministic;
- avoid relying on shared mutable external state;
- update architecture tests when intentional boundary rules change.

### Validation before completion

Run:

```bash
dotnet restore PANiXiDA.DotnetTemplate.slnx
dotnet format PANiXiDA.DotnetTemplate.slnx
dotnet build PANiXiDA.DotnetTemplate.slnx --configuration Release
dotnet test PANiXiDA.DotnetTemplate.slnx --configuration Release
```

## License

This project is licensed under the <LicenseName> license.

See the [LICENSE](LICENSE) file for details.

## Maintainers / Contacts

Maintained by <Author / Team / Organization>.

For questions or improvements, use:

- GitHub Issues;
- Pull Requests;
- GitHub Discussions, if enabled;
- `<team-channel>`;
- `<team-email>`.
