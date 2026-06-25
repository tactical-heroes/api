# First Template Use

Use this checklist to turn `dotnet-backend-template` into a real service.
Run through it right after creating a repository from the template, before adding production features.

In the final service, this file can be deleted or moved to `docs/` if the team wants to keep the checklist.
Delete it after the first service setup PR if the checklist is no longer useful.

## 0. Prefer `dotnet new`

The preferred way to create a service is the .NET template:

```bash
dotnet new install PANiXiDA.Templates.DotnetBackend

dotnet new panixida-backend \
  -n Company.Product.Service \
  --organization Company \
  --product Product \
  --module Module \
  --service-slug service \
  --repository-name service-repository \
  --registry-owner company \
  -o Company.Product.Service
```

When the service is created through `dotnet new`, template repository files are
already excluded and `README_TEMPLATE.md` is generated as `README.md`.

If the service is created by cloning or copying this repository manually, remove
template-only files from the final service repository:

- `.template.config/`;
- template-only `publish-template` job from `.github/workflows/ci.yml`;
- `template/`;
- `icon.png`;
- `version.json`, unless the service intentionally uses Nerdbank.GitVersioning;
- template repository `README.md`;
- `artifacts/`, if it was created by local build or package commands;
- generated package files, if any;
- `.tmp/`, if it was used for template testing.

For manual usage, keep `README_TEMPLATE.md` only long enough to rename it to
`README.md` for the service, then delete the `README_TEMPLATE.md` file.

This repository also contains repository-only examples for template maintainers.
They live under `RepositoryExamples` folders and are excluded when the service is
generated through `dotnet new`. If the service is created by cloning or copying
this repository manually, remove every `RepositoryExamples` folder.

## 1. Choose Target Names

Define these values before renaming anything:

| Item                  | Example                     | Used in                                       |
| --------------------- | --------------------------- | --------------------------------------------- |
| Service name          | `Billing`                   | README, API title, Helm release, Docker image |
| Root namespace        | `Company.Product`           | all `.cs`, `.csproj`, architecture tests      |
| Module name           | `Billing` or `Payments`     | `*.Module.*` projects, `src/Module` folder    |
| Solution name         | `Company.Product.Billing`   | `.slnx`, `dotnet restore/build/test` commands |
| Service slug          | `billing`                   | Helm, image names, gateway host               |
| Database name         | `billing`                   | appsettings, migrator, deployment config      |
| Repository owner/name | `company/billing-service`   | badges, CI, image repository                  |
| Registry URL          | `ghcr.io`                   | CI/CD, Helm images                            |
| Environments          | `development`, `production` | Helm values, CI/CD, OpenBao keys              |

Do not do a partial rename. Replace all template names first, then start implementing domain features.

If the service was generated through `dotnet new`, the basic replacements from
`.template.config/template.json` have already been applied. Still run the checks
below because service-specific values still must be reviewed manually.

## 2. Rename The Solution, Projects, And Folders

Current template names:

- `PANiXiDA.DotnetTemplate.slnx`;
- `src/Organization.Product.Host`;
- `src/Module/Organization.Product.Module.Domain`;
- `src/Module/Organization.Product.Module.Application`;
- `src/Module/Organization.Product.Module.Infrastructure`;
- `src/Module/Organization.Product.Module.Presentation`;
- `tests/Organization.Product.ArchitectureTests`;
- `tests/Organization.Product.Testing`;
- `tests/Module/Organization.Product.Module.UnitTests`;
- `tests/Module/Organization.Product.Module.IntegrationTests`;
- `tests/Module/Organization.Product.Module.FunctionalTests`;
- `tools/Organization.Product.Ef.Migrator`;
- `deploy/helm/dotnet-template`.

Actions:

1. If generated through `dotnet new`, verify the `.slnx`, project folders and `.csproj` files were renamed as expected.
2. If copied manually, rename the `.slnx` file to the final solution name.
3. Rename project folders and their `.csproj` files.
4. Update project paths inside the `.slnx` file.
5. Update all `ProjectReference` paths in `.csproj` files.
6. Update API and migrator Dockerfiles: `COPY`, `WORKDIR`, `RUN dotnet restore/build/publish`, `ENTRYPOINT`.
7. Update test project names and `InternalsVisibleTo`.
8. Rename Helm directory `deploy/helm/dotnet-template` to `deploy/helm/<service-slug>`.

Check:

```bash
rg "PANiXiDA\.DotnetTemplate|Organization\.Product|dotnet-template|dotnet-backend-template|panixida-templates"
```

After the rename, these values should not remain in code, configuration, CI/CD, Helm, or README unless intentionally kept in historical notes.

## 3. Update Namespaces And Assembly Names

Replace `Organization.Product` with the final root namespace in:

- `namespace ...`;
- `using Organization.Product...`;
- `<Using Include="Organization.Product...">`;
- `<InternalsVisibleTo Include="Organization.Product...">`;
- migration snapshot and migration designer files after real migrations are generated;
- functional, integration, unit, and architecture tests.

Keep the layer suffixes:

- `.Domain`;
- `.Application`;
- `.Infrastructure`;
- `.Presentation`;
- `.Host`.

Architecture tests discover modules by these suffixes. If you intentionally change them, update `tests/.../ArchitectureDefinition.cs`.

## 4. Create The First Real Feature

The generated template starts without sample business features or pre-generated migrations.
Create the first real feature slice only after final names and namespaces are set.

Use this structure for a feature:

```text
src/Module/<Root>.<Module>.Domain/<Feature>/
src/Module/<Root>.<Module>.Application/<Feature>/
src/Module/<Root>.<Module>.Infrastructure/Persistence/Features/<Feature>/
src/Module/<Root>.<Module>.Presentation/Features/<Feature>/
tests/Module/<Root>.<Module>.UnitTests/...
tests/Module/<Root>.<Module>.IntegrationTests/...
tests/Module/<Root>.<Module>.FunctionalTests/...
```

For the first feature, add only the files the service actually needs:

- domain model, value objects, policies and specifications;
- application commands, queries, handlers and validators;
- infrastructure repositories, EF configurations, read models and mappers;
- presentation endpoints, request/response contracts and mappers;
- unit, integration and functional tests for the touched behavior.

Do not add placeholder entities, endpoints, repositories or tests that are not part of the real domain.

## 5. Rename DbContext And Create Migrations

Current template classes:

- `TemplateWriteDbContext`;
- `TemplateReadDbContext`.

Actions:

1. Rename DbContext classes to final service names, for example `<ServiceName>WriteDbContext` and `<ServiceName>ReadDbContext`.
2. Update registrations in:
   - `src/...Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs`;
   - `src/...Infrastructure/DependencyInjection/HostBuilderExtensions.cs`;
   - `tools/...Ef.Migrator/Program.cs`;
   - integration and functional test fixtures.
3. Verify that no template migrations exist in `src/...Infrastructure/Persistence/Core/Migrations`.
4. Generate the first migration only after the real EF model is added.
5. Check `tools/...Ef.Migrator/appsettings.json`:
   - `Ef:ProjectPath`;
   - `Ef:MigrationsDirectory`;
   - `GenerateMigrations`;
   - `ApplyMigrations`;
   - `ConnectionStrings:PostgreSqlConnectionString`.

If the service does not use PostgreSQL or EF Core, remove the migrator project, the migration job in Helm, and related package references.

## 6. Configure Appsettings

Files to check:

- `src/...Host/appsettings.json`;
- `src/...Host/appsettings.Development.json`;
- `tools/...Ef.Migrator/appsettings.json`;
- `src/...Host/Properties/launchSettings.json`;
- `tools/...Ef.Migrator/Properties/launchSettings.json`.

Required actions:

1. Replace `dotnet-template` in connection strings and `OTEL_SERVICE_NAME`.
2. Keep local development passwords in `appsettings.json` if that is the team convention.
3. Update `ScalarConfiguration:Title`.
4. Remove `MongoDbConnectionString` if MongoDB is not used.
5. Remove the `S3` section if file storage is not used.
6. Remove gRPC endpoint `Kestrel:Endpoints:Grpc` if the service does not expose gRPC.
7. Check ports `8080`, `8081`, `launchSettings.json`, and Helm `service.containerPort`.
8. Keep or regenerate the host project local settings identifier only if Visual Studio tooling requires it.

Check:

```bash
rg "dotnet-template|MongoDbConnectionString|S3:" src tools deploy
```

Keep MongoDB or S3 matches only when the final service actually uses them.

## 7. Check Dockerfiles

Files:

- `src/...Host/Dockerfile`;
- `tools/...Ef.Migrator/Dockerfile`.

Check and update:

- `COPY` paths for all `.csproj` files;
- `RUN dotnet restore`;
- `WORKDIR`;
- `RUN dotnet build`;
- `RUN dotnet publish`;
- `ENTRYPOINT` with the final `.dll` name;
- `EXPOSE 8081`, if gRPC is no longer needed;
- `codegen write`, if code generation is no longer used.

The CI/CD pipeline prepares package feed configuration for Docker builds. Keep Dockerfiles aligned with that pipeline behavior.

Check:

```bash
docker build -f src/<HostProject>/Dockerfile -t <service-slug>-api:local .
docker build -f tools/<MigratorProject>/Dockerfile -t <service-slug>-ef-migrator:local .
```

## 8. Update CI/CD

File:

- `.github/workflows/ci.yml`.

Check:

- target branches `main`, `development`;
- reusable workflows `PANiXiDA-Infrastructure/ci-cd`;
- `paths-ignore` for image values;
- `format`, `tests`, and `helm` jobs;
- API build job;
- migrator build job, if the migrator remains;
- Telegram notification job, if notifications are needed.

Configure GitHub repository variables:

- `HELM_VALUES_PATH`;
- `HELM_COMMON_VALUES_FILE`;
- `HELM_DEVELOPMENT_RELEASE_NAME`;
- `HELM_PRODUCTION_RELEASE_NAME`;
- `SERVICE_FOLDER`;
- `SERVICE_NAME`;
- `MIGRATOR_FOLDER`, if the migrator remains;
- `MIGRATOR_NAME`, if the migrator remains;
- `REGISTRY_URL`;
- `TELEGRAM_CHAT_ID`, if notifications are needed.

Configure protected CI/CD values used by the workflow:

- `REGISTRY_USER`;
- `REGISTRY_TOKEN`;
- `TELEGRAM_BOT_TOKEN`, if notifications are needed.

If the service does not use the migrator, remove `build-migrator`, its dependencies, and migrator image updates.

## 9. Update Helm Deployment

Files:

- `deploy/helm/<service-slug>/values.yaml`;
- `deploy/helm/<service-slug>/values-development.yaml`;
- `deploy/helm/<service-slug>/values-production.yaml`;
- `deploy/helm/<service-slug>/images-development.yaml`;
- `deploy/helm/<service-slug>/images-production.yaml`.

Check and replace:

- API image repository;
- migrator image repository;
- registry pull configuration;
- application environment name;
- OpenBao role;
- OpenBao application key;
- OpenBao registry key;
- gateway host;
- gateway HTTPS section name;
- `OTEL_SERVICE_NAME`;
- `OTEL_EXPORTER_OTLP_ENDPOINT`;
- `replicaCount`;
- `resources`;
- `probes`;
- migration job settings.

If S3, MongoDB, a message bus, or external APIs are used in production, add the required environment values and provider mappings.

Reset image tags for the new service until CI/CD updates them:

```yaml
tag: "0"
```

Check:

```bash
rg "dotnet-template|panixida-templates/dotnet-backend-template" deploy .github
```

## 10. Update README

File:

- `README.md`.

The generated `README.md` comes from `README_TEMPLATE.md` in the template
repository. Treat it as a starting point for the real service documentation.

Fill or remove all placeholders:

- `<ServiceName>`;
- `<OWNER>`;
- `<REPOSITORY>`;
- `<REPOSITORY_URL>`;
- `<SolutionName>`;
- `<short purpose>`;
- `<target audience / consumers>`;
- `<main value / main scenario>`;
- `<database-name>`;
- `<ExternalService*>`;
- `<API_DOCS_URL>`;
- `<LicenseName>`;
- maintainer contacts.

Update:

- overview;
- feature list;
- architecture description;
- project structure after renaming;
- quick start commands;
- local configuration;
- local infrastructure;
- configuration table;
- endpoint tables;
- authentication and authorization;
- domain model;
- database section;
- Docker commands;
- observability;
- Helm deployment;
- CI/CD variables and protected values;
- environments;
- troubleshooting;
- template replacement checklist.

Remove README sections that do not apply to the service:

- MongoDB;
- S3;
- gRPC;
- code generation;
- metrics/tracing;
- migrator.

Check:

```bash
rg "<ServiceName>|<OWNER>|<REPOSITORY>|<REPOSITORY_URL>|<SolutionName>|<LicenseName>|Organization\.Product|dotnet-template" README.md
```

## 11. Check Package Management And SDK

Files:

- `global.json`;
- `Directory.Build.props`;
- `Directory.Build.targets`;
- `Directory.Packages.props`;
- `.editorconfig`;
- `.gitattributes`;
- `.gitignore`;
- `.dockerignore`.

Check:

- SDK version in `global.json` is available locally and in CI;
- `TargetFramework` matches the chosen .NET version;
- `TreatWarningsAsErrors` remains enabled;
- package versions are relevant for the service;
- unused package versions are removed;
- package feeds are available in CI and Docker build;
- `.dockerignore` does not exclude files required by Docker build;
- `.gitattributes` keeps the expected line endings.

## 12. Create Missing Service Files

Create only what the final service needs:

- new `.slnx` with correct project paths;
- renamed `.csproj` files;
- first real feature slice;
- first real EF migration;
- `.env.example`, if the team uses local Docker runs through an env file;
- additional Helm values, if there are more environments;
- documentation in `docs/`, if README becomes too large.

Keep these out of the final service changes:

- real `.env` files;
- database dumps;
- local logs;
- `bin/`;
- `obj/`;
- `TestResults/`;
- temporary JSON/TXT/MD files.

## 13. Final Check Before The First PR

Check that template values are gone:

```bash
rg "PANiXiDA\.DotnetTemplate|Organization\.Product|dotnet-template|dotnet-backend-template|panixida-templates|<ServiceName>|<OWNER>|<REPOSITORY>|<SolutionName>|<LicenseName>"
```

Check that no accidental sample feature names remain:

```bash
rg "Users|User" src tests
```

There should be no matches unless the real service domain intentionally uses those words.

Check diff format:

```bash
git diff --check
git status --short
```

Check .NET:

```bash
dotnet restore <SolutionName>.slnx
dotnet format <SolutionName>.slnx --verify-no-changes
dotnet build <SolutionName>.slnx --configuration Release
dotnet test <SolutionName>.slnx --configuration Release
```

Check Docker:

```bash
docker build -f src/<HostProject>/Dockerfile -t <service-slug>-api:local .
docker build -f tools/<MigratorProject>/Dockerfile -t <service-slug>-ef-migrator:local .
```

Check Helm:

```bash
helm lint <chart-source> --values deploy/helm/<service-slug>/values.yaml
```

If the shared chart lives in another repository, use the same chart source that `.github/workflows/ci.yml` uses.

## 14. Recommended Work Order

1. Choose names and environments.
2. Generate the service with `dotnet new` or remove template-only files after manual clone.
3. Verify solution, projects, folders, and namespaces.
4. Update `.slnx`, `.csproj`, and Dockerfiles where needed.
5. Create the first real feature slice.
6. Rename DbContext and generate the first real migration when the EF model is ready.
7. Configure appsettings and deployment values.
8. Update Helm values.
9. Update GitHub Actions variables and protected values.
10. Rewrite README for the service.
11. Run restore, format, build, test, Docker build, and Helm validation.
12. Check with `rg` for leftover template values.
13. Remove local temporary artifacts and open the first PR.
