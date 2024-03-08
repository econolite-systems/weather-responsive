// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Repository.WeatherResponsive.Models;

namespace Econolite.Ode.Repository.WeatherResponsive;

/// <summary>
/// IWeatherResponsiveGlobalConfigRepository
/// </summary>
public interface IWeatherResponsiveGlobalConfigRepository
{
    Task<WeatherResponsiveGlobalConfig?> GetGlobalConfig();
    Task UpdateGlobalConfig(WeatherResponsiveGlobalConfig globalConfig);
    Task CreateGlobalConfig(WeatherResponsiveGlobalConfig globalConfig);
    Task DeleteGlobalConfig(WeatherResponsiveGlobalConfig globalConfig);
}
