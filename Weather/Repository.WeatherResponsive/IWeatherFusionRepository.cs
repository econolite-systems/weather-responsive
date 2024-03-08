// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Repository.WeatherResponsive.Models;
using Weather.Fusion;

namespace Econolite.Ode.Repository.WeatherResponsive;

public interface IWeatherFusionRepository
{
    Task PutCorridorWeatherAsync(Guid corridorId, CorridorStatus corridorStatus, CancellationToken cancellationToken = default);
    Task<WeatherFusionDocument?> GetCorridorWeatherAsync(Guid corridorId, CancellationToken cancellationToken = default);
    Task<WeatherFusionResultModel[]> Find(List<Guid> corridorIds, DateTime startDate, DateTime? endDate);
}
