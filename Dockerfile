FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env

WORKDIR /app
COPY ./W3ChampionsChatService.sln ./

COPY ./W3ChampionsChatService/W3ChampionsChatService.csproj ./W3ChampionsChatService/W3ChampionsChatService.csproj
RUN dotnet restore ./W3ChampionsChatService/W3ChampionsChatService.csproj

COPY ./W3ChampionsChatService ./W3ChampionsChatService
RUN dotnet build ./W3ChampionsChatService/W3ChampionsChatService.csproj -c Release

RUN dotnet publish "./W3ChampionsChatService/W3ChampionsChatService.csproj" -c Release -o "../../app/out"

FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build-env /app/out .

ENV ASPNETCORE_URLS http://*:80
EXPOSE 80

ENTRYPOINT dotnet W3ChampionsChatService.dll