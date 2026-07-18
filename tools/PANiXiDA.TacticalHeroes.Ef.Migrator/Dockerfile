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
COPY ["tools/PANiXiDA.TacticalHeroes.Ef.Migrator/PANiXiDA.TacticalHeroes.Ef.Migrator.csproj", "tools/PANiXiDA.TacticalHeroes.Ef.Migrator/"]
COPY ["src/Identity/PANiXiDA.TacticalHeroes.Identity.Infrastructure/PANiXiDA.TacticalHeroes.Identity.Infrastructure.csproj", "src/Identity/PANiXiDA.TacticalHeroes.Identity.Infrastructure/"]
COPY ["src/Identity/PANiXiDA.TacticalHeroes.Identity.Application/PANiXiDA.TacticalHeroes.Identity.Application.csproj", "src/Identity/PANiXiDA.TacticalHeroes.Identity.Application/"]
COPY ["src/Identity/PANiXiDA.TacticalHeroes.Identity.Domain/PANiXiDA.TacticalHeroes.Identity.Domain.csproj", "src/Identity/PANiXiDA.TacticalHeroes.Identity.Domain/"]
RUN dotnet restore "./tools/PANiXiDA.TacticalHeroes.Ef.Migrator/PANiXiDA.TacticalHeroes.Ef.Migrator.csproj"
COPY . .
WORKDIR "/src/tools/PANiXiDA.TacticalHeroes.Ef.Migrator"
RUN dotnet build "./PANiXiDA.TacticalHeroes.Ef.Migrator.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PANiXiDA.TacticalHeroes.Ef.Migrator.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PANiXiDA.TacticalHeroes.Ef.Migrator.dll"]
