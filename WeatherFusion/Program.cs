// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Common.Scheduler.Base.Extensions;
using Econolite.Ode.Extensions.AspNet;
using Econolite.Ode.Monitoring.HealthChecks.Mongo.Extensions;
using Econolite.Ode.Monitoring.HealthChecks.Redis.Extensions;
using Econolite.Ode.Persistence.Mongo;
using Econolite.Ode.Repository.WeatherResponsive;
using Status.Ess.Cache.Extensions;
using Weather.Common.Cache.Extensions;
using Weather.Fusion;
using WeatherFusion;

await AppBuilder.BuildAndRunWebHostAsync(args, options => { options.Source = "Weather Fusion"; }, (builder, services) =>
{
    services.AddMongo();
    services.AddTimerFactory()
        .AddWeatherResponsiveRepositories()
        .AddStackExchangeRedisCache(options =>
        {
            options.Configuration = builder.Configuration[Consts.RedisConnection];
        })
        .AddEssCache()
        .AddWeatherCache()
        .Configure<FusionServiceOptions>(builder.Configuration.GetSection("FusionService"))
        .AddHostedService<FusionService>();
}, (builder, checksBuilder) => checksBuilder.AddMongoDbHealthCheck().AddRedisHealthCheck(builder.Configuration[Consts.RedisConnection] ?? throw new NullReferenceException($"{Consts.RedisConnection} missing from config.")));
