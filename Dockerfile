FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PlexLocalscan.csproj", "./"]
RUN dotnet restore "PlexLocalscan.csproj"
COPY . .
RUN dotnet build "PlexLocalscan.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PlexLocalscan.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PlexLocalscan.dll"] 