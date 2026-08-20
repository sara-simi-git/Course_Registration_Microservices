FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
RUN apt-get update && apt-get install -y --no-install-recommends wget && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["UserManagementService/UserManagementService.csproj", "UserManagementService/"]
COPY ["Shared/ObservabilityExtensions.cs", "Shared/"]
RUN dotnet restore "UserManagementService/UserManagementService.csproj"
COPY ["UserManagementService/", "UserManagementService/"]
COPY ["Shared/", "Shared/"]
WORKDIR "/src/UserManagementService"
RUN dotnet build "UserManagementService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UserManagementService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserManagementService.dll"]
