
FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
WORKDIR /src
COPY weather.csproj weather/
RUN dotnet restore weather/weather.csproj
WORKDIR /src/weather
COPY . .
RUN dotnet build weather.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish weather.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "weather.dll"]
