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
COPY ["src/Modules/Battle/PANiXiDA.TacticalHeroes.Battle.Infrastructure/PANiXiDA.TacticalHeroes.Battle.Infrastructure.csproj", "src/Modules/Battle/PANiXiDA.TacticalHeroes.Battle.Infrastructure/"]
COPY ["src/Modules/Matchmaking/PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure/PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.csproj", "src/Modules/Matchmaking/PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure/"]
COPY ["src/Modules/Games/PANiXiDA.TacticalHeroes.Games.Infrastructure/PANiXiDA.TacticalHeroes.Games.Infrastructure.csproj", "src/Modules/Games/PANiXiDA.TacticalHeroes.Games.Infrastructure/"]
COPY ["src/Modules/Builds/PANiXiDA.TacticalHeroes.Builds.Infrastructure/PANiXiDA.TacticalHeroes.Builds.Infrastructure.csproj", "src/Modules/Builds/PANiXiDA.TacticalHeroes.Builds.Infrastructure/"]
COPY ["src/Modules/Players/PANiXiDA.TacticalHeroes.Players.Infrastructure/PANiXiDA.TacticalHeroes.Players.Infrastructure.csproj", "src/Modules/Players/PANiXiDA.TacticalHeroes.Players.Infrastructure/"]
COPY ["src/Modules/Battle/PANiXiDA.TacticalHeroes.Battle.Application/PANiXiDA.TacticalHeroes.Battle.Application.csproj", "src/Modules/Battle/PANiXiDA.TacticalHeroes.Battle.Application/"]
COPY ["src/Modules/Matchmaking/PANiXiDA.TacticalHeroes.Matchmaking.Application/PANiXiDA.TacticalHeroes.Matchmaking.Application.csproj", "src/Modules/Matchmaking/PANiXiDA.TacticalHeroes.Matchmaking.Application/"]
COPY ["src/Modules/Games/PANiXiDA.TacticalHeroes.Games.Application/PANiXiDA.TacticalHeroes.Games.Application.csproj", "src/Modules/Games/PANiXiDA.TacticalHeroes.Games.Application/"]
COPY ["src/Modules/Builds/PANiXiDA.TacticalHeroes.Builds.Application/PANiXiDA.TacticalHeroes.Builds.Application.csproj", "src/Modules/Builds/PANiXiDA.TacticalHeroes.Builds.Application/"]
COPY ["src/Modules/Players/PANiXiDA.TacticalHeroes.Players.Application/PANiXiDA.TacticalHeroes.Players.Application.csproj", "src/Modules/Players/PANiXiDA.TacticalHeroes.Players.Application/"]
COPY ["src/Modules/Battle/PANiXiDA.TacticalHeroes.Battle.Contracts/PANiXiDA.TacticalHeroes.Battle.Contracts.csproj", "src/Modules/Battle/PANiXiDA.TacticalHeroes.Battle.Contracts/"]
COPY ["src/Modules/Matchmaking/PANiXiDA.TacticalHeroes.Matchmaking.Contracts/PANiXiDA.TacticalHeroes.Matchmaking.Contracts.csproj", "src/Modules/Matchmaking/PANiXiDA.TacticalHeroes.Matchmaking.Contracts/"]
COPY ["src/Modules/Games/PANiXiDA.TacticalHeroes.Games.Contracts/PANiXiDA.TacticalHeroes.Games.Contracts.csproj", "src/Modules/Games/PANiXiDA.TacticalHeroes.Games.Contracts/"]
COPY ["src/Modules/Builds/PANiXiDA.TacticalHeroes.Builds.Contracts/PANiXiDA.TacticalHeroes.Builds.Contracts.csproj", "src/Modules/Builds/PANiXiDA.TacticalHeroes.Builds.Contracts/"]
COPY ["src/Modules/Players/PANiXiDA.TacticalHeroes.Players.Contracts/PANiXiDA.TacticalHeroes.Players.Contracts.csproj", "src/Modules/Players/PANiXiDA.TacticalHeroes.Players.Contracts/"]
COPY ["src/Modules/Battle/PANiXiDA.TacticalHeroes.Battle.Domain/PANiXiDA.TacticalHeroes.Battle.Domain.csproj", "src/Modules/Battle/PANiXiDA.TacticalHeroes.Battle.Domain/"]
COPY ["src/Modules/Matchmaking/PANiXiDA.TacticalHeroes.Matchmaking.Domain/PANiXiDA.TacticalHeroes.Matchmaking.Domain.csproj", "src/Modules/Matchmaking/PANiXiDA.TacticalHeroes.Matchmaking.Domain/"]
COPY ["src/Modules/Games/PANiXiDA.TacticalHeroes.Games.Domain/PANiXiDA.TacticalHeroes.Games.Domain.csproj", "src/Modules/Games/PANiXiDA.TacticalHeroes.Games.Domain/"]
COPY ["src/Modules/Builds/PANiXiDA.TacticalHeroes.Builds.Domain/PANiXiDA.TacticalHeroes.Builds.Domain.csproj", "src/Modules/Builds/PANiXiDA.TacticalHeroes.Builds.Domain/"]
COPY ["src/Modules/Players/PANiXiDA.TacticalHeroes.Players.Domain/PANiXiDA.TacticalHeroes.Players.Domain.csproj", "src/Modules/Players/PANiXiDA.TacticalHeroes.Players.Domain/"]
COPY ["src/Modules/FileManager/PANiXiDA.TacticalHeroes.FileManager.Infrastructure/PANiXiDA.TacticalHeroes.FileManager.Infrastructure.csproj", "src/Modules/FileManager/PANiXiDA.TacticalHeroes.FileManager.Infrastructure/"]
COPY ["src/Modules/FileManager/PANiXiDA.TacticalHeroes.FileManager.Application/PANiXiDA.TacticalHeroes.FileManager.Application.csproj", "src/Modules/FileManager/PANiXiDA.TacticalHeroes.FileManager.Application/"]
COPY ["src/Modules/FileManager/PANiXiDA.TacticalHeroes.FileManager.Contracts/PANiXiDA.TacticalHeroes.FileManager.Contracts.csproj", "src/Modules/FileManager/PANiXiDA.TacticalHeroes.FileManager.Contracts/"]
COPY ["src/Modules/FileManager/PANiXiDA.TacticalHeroes.FileManager.Domain/PANiXiDA.TacticalHeroes.FileManager.Domain.csproj", "src/Modules/FileManager/PANiXiDA.TacticalHeroes.FileManager.Domain/"]
COPY ["src/Modules/Compendium/PANiXiDA.TacticalHeroes.Compendium.Infrastructure/PANiXiDA.TacticalHeroes.Compendium.Infrastructure.csproj", "src/Modules/Compendium/PANiXiDA.TacticalHeroes.Compendium.Infrastructure/"]
COPY ["src/Modules/Compendium/PANiXiDA.TacticalHeroes.Compendium.Application/PANiXiDA.TacticalHeroes.Compendium.Application.csproj", "src/Modules/Compendium/PANiXiDA.TacticalHeroes.Compendium.Application/"]
COPY ["src/Modules/Compendium/PANiXiDA.TacticalHeroes.Compendium.Contracts/PANiXiDA.TacticalHeroes.Compendium.Contracts.csproj", "src/Modules/Compendium/PANiXiDA.TacticalHeroes.Compendium.Contracts/"]
COPY ["src/Modules/Compendium/PANiXiDA.TacticalHeroes.Compendium.Domain/PANiXiDA.TacticalHeroes.Compendium.Domain.csproj", "src/Modules/Compendium/PANiXiDA.TacticalHeroes.Compendium.Domain/"]
COPY ["src/Modules/Identity/PANiXiDA.TacticalHeroes.Identity.Infrastructure/PANiXiDA.TacticalHeroes.Identity.Infrastructure.csproj", "src/Modules/Identity/PANiXiDA.TacticalHeroes.Identity.Infrastructure/"]
COPY ["src/Modules/Identity/PANiXiDA.TacticalHeroes.Identity.Application/PANiXiDA.TacticalHeroes.Identity.Application.csproj", "src/Modules/Identity/PANiXiDA.TacticalHeroes.Identity.Application/"]
COPY ["src/Modules/Identity/PANiXiDA.TacticalHeroes.Identity.Contracts/PANiXiDA.TacticalHeroes.Identity.Contracts.csproj", "src/Modules/Identity/PANiXiDA.TacticalHeroes.Identity.Contracts/"]
COPY ["src/Modules/Identity/PANiXiDA.TacticalHeroes.Identity.Domain/PANiXiDA.TacticalHeroes.Identity.Domain.csproj", "src/Modules/Identity/PANiXiDA.TacticalHeroes.Identity.Domain/"]
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
