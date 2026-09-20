FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only server project files for restore
COPY ["server/LicensePlatform.Api/LicensePlatform.Api.csproj", "server/LicensePlatform.Api/"]
COPY ["server/LicensePlatform.Application/LicensePlatform.Application.csproj", "server/LicensePlatform.Application/"]
COPY ["server/LicensePlatform.Domain/LicensePlatform.Domain.csproj", "server/LicensePlatform.Domain/"]
COPY ["server/LicensePlatform.Infrastructure/LicensePlatform.Infrastructure.csproj", "server/LicensePlatform.Infrastructure/"]
COPY ["server/LicensePlatform.Security/LicensePlatform.Security.csproj", "server/LicensePlatform.Security/"]
COPY ["server/LicensePlatform.Shared/LicensePlatform.Shared.csproj", "server/LicensePlatform.Shared/"]

RUN dotnet restore "server/LicensePlatform.Api/LicensePlatform.Api.csproj"

# Copy server source and build
COPY server/ server/

WORKDIR "/src/server/LicensePlatform.Api"
RUN dotnet publish "LicensePlatform.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

ENTRYPOINT ["dotnet", "LicensePlatform.Api.dll"]
