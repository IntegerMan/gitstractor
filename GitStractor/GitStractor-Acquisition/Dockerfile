#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:7.0-alpine3.16 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine3.16 AS build
WORKDIR /src
COPY ["NuGet.Config", "."]
COPY ["GitStractor-Acquisition/GitStractor-Acquire.csproj", "GitStractor-Acquisition/"]
COPY ["GitStractor.Workers/GitStractor.Workers.csproj", "GitStractor.Workers/"]
COPY ["GitStractor/GitStractor.csproj", "GitStractor/"]
# Ignore failed sources is needed since Telerik uses its own NuGet Feed
RUN dotnet restore "GitStractor-Acquisition/GitStractor-Acquire.csproj" --ignore-failed-sources
COPY . .
WORKDIR "/src/GitStractor-Acquisition"
RUN dotnet build "GitStractor-Acquire.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GitStractor-Acquire.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GitStractor-Acquire.dll"]