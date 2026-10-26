# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["Directory.Build.targets", "."]
COPY ["src/PANiXiDA.TacticalHeroes.Common/PANiXiDA.TacticalHeroes.Common.csproj", "src/PANiXiDA.TacticalHeroes.Common/"]
COPY ["tools/PANiXiDA.TacticalHeroes.Ef.Migrator/PANiXiDA.TacticalHeroes.Ef.Migrator.csproj", "tools/PANiXiDA.TacticalHeroes.Ef.Migrator/"]
COPY ["src/Battle/PANiXiDA.TacticalHeroes.Battle.Infrastructure/PANiXiDA.TacticalHeroes.Battle.Infrastructure.csproj", "src/Battle/PANiXiDA.TacticalHeroes.Battle.Infrastructure/"]
COPY ["src/Matchmaking/PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure/PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.csproj", "src/Matchmaking/PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure/"]
COPY ["src/Games/PANiXiDA.TacticalHeroes.Games.Infrastructure/PANiXiDA.TacticalHeroes.Games.Infrastructure.csproj", "src/Games/PANiXiDA.TacticalHeroes.Games.Infrastructure/"]
COPY ["src/Builds/PANiXiDA.TacticalHeroes.Builds.Infrastructure/PANiXiDA.TacticalHeroes.Builds.Infrastructure.csproj", "src/Builds/PANiXiDA.TacticalHeroes.Builds.Infrastructure/"]
COPY ["src/Battle/PANiXiDA.TacticalHeroes.Battle.Application/PANiXiDA.TacticalHeroes.Battle.Application.csproj", "src/Battle/PANiXiDA.TacticalHeroes.Battle.Application/"]
COPY ["src/Matchmaking/PANiXiDA.TacticalHeroes.Matchmaking.Application/PANiXiDA.TacticalHeroes.Matchmaking.Application.csproj", "src/Matchmaking/PANiXiDA.TacticalHeroes.Matchmaking.Application/"]
COPY ["src/Games/PANiXiDA.TacticalHeroes.Games.Application/PANiXiDA.TacticalHeroes.Games.Application.csproj", "src/Games/PANiXiDA.TacticalHeroes.Games.Application/"]
COPY ["src/Builds/PANiXiDA.TacticalHeroes.Builds.Application/PANiXiDA.TacticalHeroes.Builds.Application.csproj", "src/Builds/PANiXiDA.TacticalHeroes.Builds.Application/"]
COPY ["src/Battle/PANiXiDA.TacticalHeroes.Battle.Contracts/PANiXiDA.TacticalHeroes.Battle.Contracts.csproj", "src/Battle/PANiXiDA.TacticalHeroes.Battle.Contracts/"]
COPY ["src/Matchmaking/PANiXiDA.TacticalHeroes.Matchmaking.Contracts/PANiXiDA.TacticalHeroes.Matchmaking.Contracts.csproj", "src/Matchmaking/PANiXiDA.TacticalHeroes.Matchmaking.Contracts/"]
COPY ["src/Games/PANiXiDA.TacticalHeroes.Games.Contracts/PANiXiDA.TacticalHeroes.Games.Contracts.csproj", "src/Games/PANiXiDA.TacticalHeroes.Games.Contracts/"]
COPY ["src/Builds/PANiXiDA.TacticalHeroes.Builds.Contracts/PANiXiDA.TacticalHeroes.Builds.Contracts.csproj", "src/Builds/PANiXiDA.TacticalHeroes.Builds.Contracts/"]
COPY ["src/Battle/PANiXiDA.TacticalHeroes.Battle.Domain/PANiXiDA.TacticalHeroes.Battle.Domain.csproj", "src/Battle/PANiXiDA.TacticalHeroes.Battle.Domain/"]
COPY ["src/Matchmaking/PANiXiDA.TacticalHeroes.Matchmaking.Domain/PANiXiDA.TacticalHeroes.Matchmaking.Domain.csproj", "src/Matchmaking/PANiXiDA.TacticalHeroes.Matchmaking.Domain/"]
COPY ["src/Games/PANiXiDA.TacticalHeroes.Games.Domain/PANiXiDA.TacticalHeroes.Games.Domain.csproj", "src/Games/PANiXiDA.TacticalHeroes.Games.Domain/"]
COPY ["src/Builds/PANiXiDA.TacticalHeroes.Builds.Domain/PANiXiDA.TacticalHeroes.Builds.Domain.csproj", "src/Builds/PANiXiDA.TacticalHeroes.Builds.Domain/"]
COPY ["src/FileManager/PANiXiDA.TacticalHeroes.FileManager.Infrastructure/PANiXiDA.TacticalHeroes.FileManager.Infrastructure.csproj", "src/FileManager/PANiXiDA.TacticalHeroes.FileManager.Infrastructure/"]
COPY ["src/FileManager/PANiXiDA.TacticalHeroes.FileManager.Application/PANiXiDA.TacticalHeroes.FileManager.Application.csproj", "src/FileManager/PANiXiDA.TacticalHeroes.FileManager.Application/"]
COPY ["src/FileManager/PANiXiDA.TacticalHeroes.FileManager.Contracts/PANiXiDA.TacticalHeroes.FileManager.Contracts.csproj", "src/FileManager/PANiXiDA.TacticalHeroes.FileManager.Contracts/"]
COPY ["src/FileManager/PANiXiDA.TacticalHeroes.FileManager.Domain/PANiXiDA.TacticalHeroes.FileManager.Domain.csproj", "src/FileManager/PANiXiDA.TacticalHeroes.FileManager.Domain/"]
COPY ["src/Compendium/PANiXiDA.TacticalHeroes.Compendium.Infrastructure/PANiXiDA.TacticalHeroes.Compendium.Infrastructure.csproj", "src/Compendium/PANiXiDA.TacticalHeroes.Compendium.Infrastructure/"]
COPY ["src/Compendium/PANiXiDA.TacticalHeroes.Compendium.Application/PANiXiDA.TacticalHeroes.Compendium.Application.csproj", "src/Compendium/PANiXiDA.TacticalHeroes.Compendium.Application/"]
COPY ["src/Compendium/PANiXiDA.TacticalHeroes.Compendium.Contracts/PANiXiDA.TacticalHeroes.Compendium.Contracts.csproj", "src/Compendium/PANiXiDA.TacticalHeroes.Compendium.Contracts/"]
COPY ["src/Compendium/PANiXiDA.TacticalHeroes.Compendium.Domain/PANiXiDA.TacticalHeroes.Compendium.Domain.csproj", "src/Compendium/PANiXiDA.TacticalHeroes.Compendium.Domain/"]
COPY ["src/Identity/PANiXiDA.TacticalHeroes.Identity.Infrastructure/PANiXiDA.TacticalHeroes.Identity.Infrastructure.csproj", "src/Identity/PANiXiDA.TacticalHeroes.Identity.Infrastructure/"]
COPY ["src/Identity/PANiXiDA.TacticalHeroes.Identity.Application/PANiXiDA.TacticalHeroes.Identity.Application.csproj", "src/Identity/PANiXiDA.TacticalHeroes.Identity.Application/"]
COPY ["src/Identity/PANiXiDA.TacticalHeroes.Identity.Contracts/PANiXiDA.TacticalHeroes.Identity.Contracts.csproj", "src/Identity/PANiXiDA.TacticalHeroes.Identity.Contracts/"]
COPY ["src/Identity/PANiXiDA.TacticalHeroes.Identity.Domain/PANiXiDA.TacticalHeroes.Identity.Domain.csproj", "src/Identity/PANiXiDA.TacticalHeroes.Identity.Domain/"]
RUN dotnet restore "./tools/PANiXiDA.TacticalHeroes.Ef.Migrator/PANiXiDA.TacticalHeroes.Ef.Migrator.csproj"
COPY . .
WORKDIR "/src/tools/PANiXiDA.TacticalHeroes.Ef.Migrator"
RUN dotnet build "./PANiXiDA.TacticalHeroes.Ef.Migrator.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PANiXiDA.TacticalHeroes.Ef.Migrator.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PANiXiDA.TacticalHeroes.Ef.Migrator.dll"]
