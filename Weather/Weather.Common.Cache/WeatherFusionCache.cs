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
    public class WeatherFusionCache : IWeatherFusionCache
    {
        private readonly IDistributedCache _distributedCache;
        private readonly DistributedCacheEntryOptions _cacheOptions;

        public WeatherFusionCache(IDistributedCache distributedCache, IOptions<WeatherFusionCacheOptions> options)
        {
            _distributedCache = distributedCache;
            _cacheOptions = new DistributedCacheEntryOptions().SetAbsoluteExpiration(options.Value.StatusTimeout);
        }

        public async Task<CorridorStatus> GetCorridorWeatherAsync(Guid corridorId, CancellationToken cancellationToken = default)
        {
            var cached = (await _distributedCache.GetStringAsync(corridorId.ToCorridorStatusKey(), cancellationToken)) ?? "";
            return ! string.IsNullOrWhiteSpace(cached)
                ? JsonSerializer.Deserialize<CorridorStatus>(cached) ?? new CorridorStatus()
                : new CorridorStatus();
        }

        public async Task PutCorridorWeatherAsync(Guid corridorId, CorridorStatus corridorStatus, CancellationToken cancellationToken = default) =>
            await _distributedCache.SetStringAsync(corridorId.ToCorridorStatusKey(), JsonSerializer.Serialize(corridorStatus), _cacheOptions, cancellationToken);
    }
}
