FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /app

# Copy solution and project files for restore
COPY PortfolioApi.sln ./
COPY src/Domain/Domain.csproj ./src/Domain/
COPY src/Application/Application.csproj ./src/Application/
COPY src/Infrastructure/Infrastructure.csproj ./src/Infrastructure/
COPY src/WebAPI/WebAPI.csproj ./src/WebAPI/

# Restore packages for the main projects only
RUN dotnet restore src/WebAPI/WebAPI.csproj

# Copy source code
COPY src/ ./src/

# Build and publish in one step to avoid restore issues
RUN dotnet publish src/WebAPI/WebAPI.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

WORKDIR /app

# Install ca-certificates and update package lists for better SSL/TLS connectivity
RUN apt-get update && apt-get install -y ca-certificates && rm -rf /var/lib/apt/lists/*

RUN addgroup --system --gid 1001 dotnetgroup && \
    adduser --system --uid 1001 --ingroup dotnetgroup dotnetuser

COPY --from=build /app/publish .

RUN chown -R dotnetuser:dotnetgroup /app

USER dotnetuser

EXPOSE 8080

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0

ENTRYPOINT ["dotnet", "WebAPI.dll"]