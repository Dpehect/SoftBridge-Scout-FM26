FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore src/FmScout.Api/FmScout.Api.csproj
RUN dotnet publish src/FmScout.Api/FmScout.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 10000

CMD ["sh", "-c", "dotnet FmScout.Api.dll --urls http://0.0.0.0:${PORT:-10000}"]
