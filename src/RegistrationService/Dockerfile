FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["RegistrationService/CourseRegistrationService.csproj", "RegistrationService/"]
RUN dotnet restore "RegistrationService/CourseRegistrationService.csproj"
COPY ["RegistrationService/", "RegistrationService/"]
COPY ["Shared/ObservabilityExtensions.cs", "Shared/"]
WORKDIR "/src/RegistrationService"
RUN dotnet build "CourseRegistrationService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CourseRegistrationService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CourseRegistrationService.dll"]