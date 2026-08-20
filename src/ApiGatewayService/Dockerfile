FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
RUN apt-get update && apt-get install -y --no-install-recommends wget && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ApiGatewayService/ApiGatewayService.csproj", "ApiGatewayService/"]
COPY ["Shared/ObservabilityExtensions.cs", "Shared/"]
RUN dotnet restore "ApiGatewayService/ApiGatewayService.csproj"
COPY ["ApiGatewayService/", "ApiGatewayService/"]
COPY ["Shared/", "Shared/"]
WORKDIR "/src/ApiGatewayService"
RUN dotnet build "ApiGatewayService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ApiGatewayService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ApiGatewayService.dll"]
