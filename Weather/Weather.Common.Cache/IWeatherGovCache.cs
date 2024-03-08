// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using System;
using System.Threading;
using System.Threading.Tasks;
using Weather.Fusion;

namespace Weather.Common.Cache
{
    public interface IWeatherGovCache
    {
        Task PutCorridorWeatherAsync(Guid corridorId, CorridorObservation observation, CancellationToken cancellationToken = default);
        Task<CorridorObservation> GetCorridorWeatherAsync(Guid corridorId, CancellationToken cancellationToken = default);

        Task PutWeatherStationIdentifierAsync(Guid corridorId, string weatherStationIdentifier, double distanceToStation, CancellationToken cancellationToken = default);
        Task<(string Identifer, double DistanceToStation)> GetWeatherStationIdentifierAsync(Guid corridorId, CancellationToken cancellationToken = default);
    }
}
