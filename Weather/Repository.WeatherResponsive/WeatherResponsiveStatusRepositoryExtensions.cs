// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Repository.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Econolite.Ode.Repository.WeatherResponsive;

public static class WeatherResponsiveStatusRepositoryExtensions
{
    public static IServiceCollection AddWeatherFusionDependencies(this IServiceCollection services) => services
        .AddScoped<IEntityRepository, EntityRepository>()
        .AddScoped<IWeatherFusionRepository, WeatherFusionRepository>()
        .AddHttpClient<WeatherResponsiveEdaptiveRepository>().Services
        .AddScoped<IWeatherResponsiveEdaptiveRepository, WeatherResponsiveEdaptiveRepository>()
        .AddHttpClient<WeatherResponsiveSpeedRepository>().Services
        .AddScoped<IWeatherResponsiveSpeedRepository, WeatherResponsiveSpeedRepository>()
        .AddHttpClient<WeatherResponsiveTimingPlanRepository>().Services
        .AddScoped<IWeatherResponsiveTimingPlanRepository, WeatherResponsiveTimingPlanRepository>()
        .AddScoped<IWeatherResponsiveRepository, WeatherResponsiveRepository>()
        .AddScoped<IWeatherResponsiveConfigRepository, WeatherResponsiveConfigRepository>()
        .AddScoped<IWeatherResponsiveGlobalConfigRepository, WeatherResponsiveGlobalConfigRepository>();

    public static IServiceCollection AddWeatherResponsiveRepositories(this IServiceCollection services) => services
        .AddSingleton<IWeatherFusionRepository, WeatherFusionRepository>()
        .AddSingleton<IEntityRepository, EntityRepository>()
        .AddSingleton<IWeatherResponsiveConfigRepository, WeatherResponsiveConfigRepository>()
        .AddSingleton<IWeatherResponsiveGlobalConfigRepository, WeatherResponsiveGlobalConfigRepository>();
}
