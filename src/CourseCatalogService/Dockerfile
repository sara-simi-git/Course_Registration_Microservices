FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER root
RUN apt-get update && apt-get install -y --no-install-recommends wget && rm -rf /var/lib/apt/lists/*
USER app
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CourseCatalogService/CourseCatalogService.csproj", "CourseCatalogService/"]
RUN dotnet restore "CourseCatalogService/CourseCatalogService.csproj"
COPY ["CourseCatalogService/", "CourseCatalogService/"]
COPY ["Shared/ObservabilityExtensions.cs", "Shared/"]
WORKDIR "/src/CourseCatalogService"
RUN dotnet build "CourseCatalogService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CourseCatalogService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CourseCatalogService.dll"]