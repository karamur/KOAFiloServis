# CRM Filo Servis - Production Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

# Install curl for healthcheck
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["CRMFiloServis.sln", "."]
COPY ["CRMFiloServis.Web/CRMFiloServis.Web.csproj", "CRMFiloServis.Web/"]
COPY ["CRMFiloServis.Shared/CRMFiloServis.Shared.csproj", "CRMFiloServis.Shared/"]

# Restore dependencies
RUN dotnet restore

# Copy everything else
COPY . .

# Build
WORKDIR "/src/CRMFiloServis.Web"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create necessary directories
RUN mkdir -p /app/data /app/logs

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "CRMFiloServis.Web.dll"]
