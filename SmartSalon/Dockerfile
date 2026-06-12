# SmartSalon Monolith - Multi-stage Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY SmartSalon/SmartSalon.csproj SmartSalon/
RUN dotnet restore SmartSalon/SmartSalon.csproj

# Copy everything and build
COPY . .
RUN dotnet publish SmartSalon/SmartSalon.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install ICU for globalization (Persian dates)
RUN apt-get update && apt-get install -y --no-install-recommends libicu-dev curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5015
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

EXPOSE 5015

ENTRYPOINT ["dotnet", "SmartSalon.dll"]
