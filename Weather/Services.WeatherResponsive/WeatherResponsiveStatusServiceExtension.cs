// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Repository.Entities;
using Econolite.Ode.Repository.WeatherResponsive;
using Microsoft.Extensions.DependencyInjection;

namespace Econolite.Ode.Services.WeatherResponsive;

public static class WeatherResponsiveStatusServiceExtension
{
    public static IServiceCollection AddWeatherResponsiveReportSupport(this IServiceCollection services)
    {
        services.AddScoped<IEntityRepository, EntityRepository>();
        services.AddScoped<IWeatherResponsiveConfigRepository, WeatherResponsiveConfigRepository>();
        services.AddScoped<IWeatherResponsiveGlobalConfigRepository, WeatherResponsiveGlobalConfigRepository>();
        services.AddHttpClient<WeatherResponsiveEdaptiveRepository>();
        services.AddScoped<IWeatherResponsiveEdaptiveRepository, WeatherResponsiveEdaptiveRepository>();
        services.AddScoped<IWeatherResponsiveRepository, WeatherResponsiveRepository>();
        services.AddScoped<IWeatherFusionRepository, WeatherFusionRepository>();

        return services;
    }

    /// <summary>
    /// Requires IEntityRepository to be injected already.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddWeatherResponsiveConfigurationSupport(this IServiceCollection services)
    {
        //services.AddScoped<IEntityRepository, EntityRepository>();
        services.AddScoped<IWeatherResponsiveConfigRepository, WeatherResponsiveConfigRepository>();
        services.AddScoped<IWeatherResponsiveGlobalConfigRepository, WeatherResponsiveGlobalConfigRepository>();
        services.AddHttpClient<WeatherResponsiveEdaptiveRepository>();
        services.AddScoped<IWeatherResponsiveEdaptiveRepository, WeatherResponsiveEdaptiveRepository>();
        services.AddHttpClient<WeatherResponsiveSpeedRepository>();
        services.AddScoped<IWeatherResponsiveSpeedRepository, WeatherResponsiveSpeedRepository>();
        services.AddHttpClient<WeatherResponsiveTimingPlanRepository>();
        services.AddScoped<IWeatherResponsiveTimingPlanRepository, WeatherResponsiveTimingPlanRepository>();

        return services;
    }
}
