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
COPY ["src/Organization.Product.Host/Organization.Product.Host.csproj", "src/Organization.Product.Host/"]
COPY ["src/Module/Organization.Product.Module.Application/Organization.Product.Module.Application.csproj", "src/Module/Organization.Product.Module.Application/"]
COPY ["src/Module/Organization.Product.Module.Domain/Organization.Product.Module.Domain.csproj", "src/Module/Organization.Product.Module.Domain/"]
COPY ["src/Module/Organization.Product.Module.Infrastructure/Organization.Product.Module.Infrastructure.csproj", "src/Module/Organization.Product.Module.Infrastructure/"]
COPY ["src/Module/Organization.Product.Module.Presentation/Organization.Product.Module.Presentation.csproj", "src/Module/Organization.Product.Module.Presentation/"]
RUN dotnet restore "./src/Organization.Product.Host/Organization.Product.Host.csproj"
COPY . .
WORKDIR "/src/src/Organization.Product.Host"
RUN dotnet build "./Organization.Product.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet run --project "./Organization.Product.Host.csproj" -c $BUILD_CONFIGURATION --no-restore --no-launch-profile -- codegen write
RUN dotnet publish "./Organization.Product.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Organization.Product.Host.dll"]
