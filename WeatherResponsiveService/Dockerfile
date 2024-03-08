# SPDX-License-Identifier: MIT
# Copyright: 2023 Econolite Systems, Inc.

#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ENV SolutionDir /src
WORKDIR /src
COPY ["WeatherResponsiveService/WeatherResponsiveService.csproj", "WeatherResponsiveService/"]
COPY ["common/Extensions.AspNet/Extensions.AspNet.csproj", "common/Extensions.AspNet/"]
COPY ["common/Authorization/Authorization.csproj", "common/Authorization/"]
COPY ["common/Extensions/Extensions.csproj", "common/Extensions/"]
COPY ["common/Monitoring.HealthChecks.Kafka/Monitoring.HealthChecks.Kafka.csproj", "common/Monitoring.HealthChecks.Kafka/"]
COPY ["common/Messaging/Messaging.csproj", "common/Messaging/"]
COPY ["common/Monitoring.HealthChecks.Mongo/Monitoring.HealthChecks.Mongo.csproj", "common/Monitoring.HealthChecks.Mongo/"]
COPY ["common/Persistence.Mongo/Persistence.Mongo.csproj", "common/Persistence.Mongo/"]
COPY ["common/Persistence.Common/Persistence.Common.csproj", "common/Persistence.Common/"]
COPY ["common/Monitoring/Monitoring.csproj", "common/Monitoring/"]
COPY ["common/Scheduler.Base/Scheduler.Base.csproj", "common/Scheduler.Base/"]
COPY ["Weather/Repository.WeatherResponsive/Repository.WeatherResponsive.csproj", "Weather/Repository.WeatherResponsive/"]
COPY ["entities/Repository.Entities/Repository.Entities.csproj", "entities/Repository.Entities/"]
COPY ["entities/Models.Entities/Models.Entities.csproj", "entities/Models.Entities/"]
COPY ["config/Config/Config.csproj", "config/Config/"]
COPY ["Weather/Models.WeatherResponsive.Mapster/Models.WeatherResponsive.Mapster.csproj", "Weather/Models.WeatherResponsive.Mapster/"]
COPY ["Weather/Weather.Fusion/Weather.Fusion.csproj", "Weather/Weather.Fusion/"]
COPY ["status/Status.Ess/Status.Ess.csproj", "status/Status.Ess/"]
COPY ["status/Status.Common/Status.Common.csproj", "status/Status.Common/"]
COPY ["Weather/Weather.Gov/Weather.Gov.csproj", "Weather/Weather.Gov/"]
RUN dotnet restore "WeatherResponsiveService/WeatherResponsiveService.csproj"
COPY . .
WORKDIR "/src/WeatherResponsiveService"
RUN dotnet build "WeatherResponsiveService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WeatherResponsiveService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WeatherResponsiveService.dll"]