#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Daibitx.Nuxus/Daibitx.Nuxus.csproj", "Daibitx.Nuxus/"]
COPY ["Daibitx.Nuxus.Hmac/Daibitx.Nuxus.Hmac.csproj", "Daibitx.Nuxus.Hmac/"]
RUN dotnet restore "./Daibitx.Nuxus/Daibitx.Nuxus.csproj"
COPY . .
WORKDIR "/src/Daibitx.Nuxus"
RUN dotnet build "./Daibitx.Nuxus.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Daibitx.Nuxus.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Daibitx.Nuxus.dll"]