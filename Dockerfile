FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

WORKDIR /app
COPY Directory.Build.props .
COPY Directory.Packages.props .
COPY ./W3ChampionsIdentificationService.sln ./

COPY ./W3ChampionsIdentificationService/W3ChampionsIdentificationService.csproj ./W3ChampionsIdentificationService/W3ChampionsIdentificationService.csproj
RUN dotnet restore ./W3ChampionsIdentificationService/W3ChampionsIdentificationService.csproj

COPY ./W3ChampionsIdentificationService ./W3ChampionsIdentificationService
RUN dotnet build ./W3ChampionsIdentificationService/W3ChampionsIdentificationService.csproj -c Release

RUN dotnet publish "./W3ChampionsIdentificationService/W3ChampionsIdentificationService.csproj" -c Release -o "../../app/out"

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .

ENV ASPNETCORE_URLS http://*:80
EXPOSE 80

ENTRYPOINT dotnet W3ChampionsIdentificationService.dll