# SmartSalon Monolith - Build from source
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY SmartSalon/SmartSalon/*.csproj ./SmartSalon/
RUN dotnet restore "./SmartSalon/SmartSalon.csproj"
# Copy source code for build
COPY SmartSalon/. ./SmartSalon/
WORKDIR /src/SmartSalon
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
ENV LD_LIBRARY_PATH=/app/runtimes/linux-x64/native
RUN apt-get update && apt-get install -y --no-install-recommends libicu-dev libunwind8 curl && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish ./
ENV ASPNETCORE_URLS=http://+:5015
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
EXPOSE 5015
ENTRYPOINT ["dotnet", "SmartSalon.dll"]
