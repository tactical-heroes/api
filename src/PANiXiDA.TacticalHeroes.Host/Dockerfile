# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["Directory.Build.targets", "."]
COPY ["src/PANiXiDA.TacticalHeroes.Host/PANiXiDA.TacticalHeroes.Host.csproj", "src/PANiXiDA.TacticalHeroes.Host/"]
COPY ["src/Identity/PANiXiDA.TacticalHeroes.Identity.Application/PANiXiDA.TacticalHeroes.Identity.Application.csproj", "src/Identity/PANiXiDA.TacticalHeroes.Identity.Application/"]
COPY ["src/Identity/PANiXiDA.TacticalHeroes.Identity.Domain/PANiXiDA.TacticalHeroes.Identity.Domain.csproj", "src/Identity/PANiXiDA.TacticalHeroes.Identity.Domain/"]
COPY ["src/Identity/PANiXiDA.TacticalHeroes.Identity.Infrastructure/PANiXiDA.TacticalHeroes.Identity.Infrastructure.csproj", "src/Identity/PANiXiDA.TacticalHeroes.Identity.Infrastructure/"]
COPY ["src/Identity/PANiXiDA.TacticalHeroes.Identity.Presentation/PANiXiDA.TacticalHeroes.Identity.Presentation.csproj", "src/Identity/PANiXiDA.TacticalHeroes.Identity.Presentation/"]
RUN dotnet restore "./src/PANiXiDA.TacticalHeroes.Host/PANiXiDA.TacticalHeroes.Host.csproj"
COPY . .
WORKDIR "/src/src/PANiXiDA.TacticalHeroes.Host"
RUN dotnet build "./PANiXiDA.TacticalHeroes.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet run --project "./PANiXiDA.TacticalHeroes.Host.csproj" -c $BUILD_CONFIGURATION --no-restore --no-launch-profile -- codegen write
RUN dotnet publish "./PANiXiDA.TacticalHeroes.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PANiXiDA.TacticalHeroes.Host.dll"]
