// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Weather.Common.Cache.Extensions
{
    public static class Defined
    {
        /// <summary>
        /// This relies on the IDistributedCache being created somewhere else before calling this
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <returns>IServiceCollection</returns>
        public static IServiceCollection AddWeatherCache(this IServiceCollection services) => services
            .AddTransient<IWeatherFusionCache, WeatherFusionCache>()
            .AddTransient<IWeatherGovCache, WeatherGovCache>();

        public static string ToNoaaStatusKey(this Guid corridorId) => $"WeatherDotGovObservation-{corridorId}";
        public static string ToNoaaStationIdentifer(this Guid corridorId) => $"WeatherStationIdentifer-{corridorId}";
        public static string ToCorridorStatusKey(this Guid corridorId) => $"CorridorStatus-{corridorId}";
    }
}
