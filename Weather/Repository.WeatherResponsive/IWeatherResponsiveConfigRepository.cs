// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Repository.WeatherResponsive.Models;

namespace Econolite.Ode.Repository.WeatherResponsive;

public interface IWeatherResponsiveConfigRepository
{
    Task<Guid[]> GetEnabledWeatherResponsiveCorridorIdsAsync(bool enabledOnly = true, CancellationToken cancellationToken = default);
    Task<(double Latitude, double Longitude)> GetCorridorLocationAsync(Guid corridorId, CancellationToken cancellationToken = default);
    Task<Guid[]> GetCorridorEssAsync(Guid corridorId, CancellationToken cancellationToken = default);
    
    Task<ICollection<WeatherResponsiveConfiguration>> GetConfigurations(Guid corridorId, params Guid[] ids);

    Task UpdateConfiguration(WeatherResponsiveConfiguration model);

    Task DeleteConfiguration(Guid configurationId);

    Task UpdateConfigurationOrder(WeatherResponsiveConfigurationOrder model);

    Task<Dictionary<Guid, WeatherResponsiveConfiguration[]>> GetWeatherResponsiveCorridorConfigsAsync(CancellationToken cancellationToken = default);
}
