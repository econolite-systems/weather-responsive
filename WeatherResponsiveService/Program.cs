// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Authorization.Extensions;
using Econolite.Ode.Common.Scheduler.Base.Extensions;
using Econolite.Ode.Extensions.AspNet;
using Econolite.Ode.Monitoring.HealthChecks.Mongo.Extensions;
using Econolite.Ode.Persistence.Mongo;
using Econolite.Ode.Repository.WeatherResponsive;
using WeatherResponsiveService;

await AppBuilder.BuildAndRunWebHostAsync(args, options => { options.Source = "Weather Responsive"; }, (builder, services) =>
{
    services.AddMongo();
    services.AddTimerFactory();
    services.AddTokenHandler(options =>
    {
        options.Authority = builder.Configuration["Authentication:Authority"] ?? throw new NullReferenceException("Authentication:Authority missing in configuration.");
        options.ClientId = builder.Configuration["Authentication:ClientId"] ?? throw new NullReferenceException("Authentication:ClientId missing in configuration.");
        options.ClientSecret = builder.Configuration["Authentication:ClientSecret"] ?? throw new NullReferenceException("Authentication:ClientSecret missing in configuration.");
    });

    services.AddWeatherFusionDependencies();

    services.AddHostedService<FusionActionService>();
}, (_, checksBuilder) => checksBuilder.AddMongoDbHealthCheck());
