# SmartSalon Monolith - Pre-built publish output
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install ICU for globalization (Persian dates)
RUN apt-get update && apt-get install -y --no-install-recommends libicu-dev curl && rm -rf /var/lib/apt/lists/*

COPY SmartSalon/publish_output/ .

ENV ASPNETCORE_URLS=http://+:5015
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

EXPOSE 5015

ENTRYPOINT ["dotnet", "SmartSalon.dll"]
