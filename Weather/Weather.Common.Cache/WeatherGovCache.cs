// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Weather.Common.Cache.Extensions;
using Weather.Fusion;

namespace Weather.Common.Cache
{
    public class WeatherGovCache : IWeatherGovCache
    {
        private readonly IDistributedCache _distributedCache;
        private readonly DistributedCacheEntryOptions _cacheOptions;
        private readonly DistributedCacheEntryOptions _cacheStationOptions;

        public WeatherGovCache(IDistributedCache distributedCache, IOptions<WeatherGovCacheOptions> options)
        {
            _distributedCache = distributedCache;
            _cacheOptions = new DistributedCacheEntryOptions().SetAbsoluteExpiration(options.Value.StatusTimeout);

            _cacheStationOptions = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(24));
        }

        public async Task<CorridorObservation> GetCorridorWeatherAsync(Guid corridorId, CancellationToken cancellationToken = default)
        {
            var cached = (await _distributedCache.GetStringAsync(corridorId.ToNoaaStatusKey(), cancellationToken)) ?? "";
            return ! string.IsNullOrWhiteSpace(cached)
                ? JsonSerializer.Deserialize<CorridorObservation>(cached) ?? new CorridorObservation()
                : new CorridorObservation();
        }
        public async Task PutCorridorWeatherAsync(Guid corridorId, CorridorObservation observation, CancellationToken cancellationToken = default) =>
            await _distributedCache.SetStringAsync(corridorId.ToNoaaStatusKey(), JsonSerializer.Serialize(observation), _cacheOptions, cancellationToken);

        public async Task<(string Identifer, double DistanceToStation)> GetWeatherStationIdentifierAsync(Guid corridorId, CancellationToken cancellationToken = default)
        {
            var cached = (await _distributedCache.GetStringAsync(corridorId.ToNoaaStationIdentifer(), cancellationToken)) ?? "";
            var result =!string.IsNullOrWhiteSpace(cached) ? JsonSerializer.Deserialize<StationCacheData>(cached) ?? new StationCacheData()
                : new StationCacheData();
            return (result.Identifier, result.Distance);
        }

        public async Task PutWeatherStationIdentifierAsync(Guid corridorId, string weatherStationIdentifier, double distanceToStation, CancellationToken cancellationToken = default) =>
            await _distributedCache.SetStringAsync(corridorId.ToNoaaStationIdentifer(), JsonSerializer.Serialize(new StationCacheData
            {
                Distance = distanceToStation,
                Identifier = weatherStationIdentifier,
            }), _cacheStationOptions, cancellationToken);

        private class StationCacheData
        {
            public string Identifier { get; set; } = string.Empty;
            public double Distance { get; set; }
        }
    }
}
