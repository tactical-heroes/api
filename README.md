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
dotnet test PANiXiDA.TacticalHeroes.slnx --configuration Release
```

Run the API:

```bash
dotnet run --project src/PANiXiDA.TacticalHeroes.Host/PANiXiDA.TacticalHeroes.Host.csproj
```

Run the EF migrator:

```bash
dotnet run --project tools/PANiXiDA.TacticalHeroes.Ef.Migrator/PANiXiDA.TacticalHeroes.Ef.Migrator.csproj --configuration Release
```

## Repository Layout

- `src/` - application source code.
- `tests/` - unit, integration, functional, and architecture tests.
- `tools/` - service tooling, including the EF migrator.
- `deploy/helm/` - Helm deployment values.

## Initialization Notes

This repository was created from the PANiXiDA .NET backend template. Template
packaging files and repository-only examples have been removed. Project,
namespace, Helm, Docker, and CI names were initialized for Tactical Heroes API.
