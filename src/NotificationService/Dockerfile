FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
RUN apt-get update && apt-get install -y --no-install-recommends wget && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["NotificationService/NotificationService.csproj", "NotificationService/"]
COPY ["Shared/ObservabilityExtensions.cs", "Shared/"]
RUN dotnet restore "NotificationService/NotificationService.csproj"
COPY ["NotificationService/", "NotificationService/"]
COPY ["Shared/", "Shared/"]
WORKDIR "/src/NotificationService"
RUN dotnet publish "NotificationService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NotificationService.dll"]
