// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Common.Scheduler.Base.Extensions;
using Econolite.Ode.Extensions.AspNet;
using Econolite.Ode.Monitoring.HealthChecks.Mongo.Extensions;
using Econolite.Ode.Monitoring.HealthChecks.Redis.Extensions;
using Econolite.Ode.Persistence.Mongo;
using Econolite.Ode.Repository.WeatherResponsive;
using Weather.Common.Cache.Extensions;
using Weather.Gov;
using WeatherMonitoring;

await AppBuilder.BuildAndRunWebHostAsync(args, options => { options.Source = "Weather Monitoring"; }, (builder, services) =>
{
    services.AddMongo();
    services.AddHttpClient<IWeatherGov, WeatherGov>(_ =>
    {
        _.BaseAddress = new Uri("https://api.weather.gov");
        _.DefaultRequestHeaders.Add("User-Agent", builder.Configuration[Consts.WeatherGovUserAgent]);
    });
    services.AddTimerFactory()
        .AddWeatherResponsiveRepositories()
        .AddStackExchangeRedisCache(options => { options.Configuration = builder.Configuration[Weather.Fusion.Consts.RedisConnection]; })
        .AddWeatherCache()
        .AddHostedService<MonitoringService>();
}, (builder, checksBuilder) => checksBuilder.AddMongoDbHealthCheck().AddRedisHealthCheck(builder.Configuration[Weather.Fusion.Consts.RedisConnection] ?? throw new NullReferenceException($"{Weather.Fusion.Consts.RedisConnection} missing from config.")));
