// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using System;
using System.Threading;
using System.Threading.Tasks;
using Weather.Fusion;

namespace Weather.Common.Cache
{
    public interface IWeatherFusionCache
    {
        Task PutCorridorWeatherAsync(Guid corridorId, CorridorStatus corridorStatus, CancellationToken cancellationToken = default);
        Task<CorridorStatus> GetCorridorWeatherAsync(Guid corridorId, CancellationToken cancellationToken = default);
    }
}
