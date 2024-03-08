// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Repository.WeatherResponsive.Models;

namespace Econolite.Ode.Repository.WeatherResponsive;

public interface IWeatherResponsiveSpeedRepository
{
    Task<ICollection<CorridorSpeedOverride>> GetAllCorridorSpeedOverrides(string authScheme, string authToken);
    Task<ICollection<CorridorSpeedOverride>> GetCorridorSpeedOverridesForDate(string authScheme, string authToken, Guid corridorId, DateTime? date);
    Task SetCorridorSpeedOverride(string authScheme, string authToken, long corridorId, double speedAdjustment, CorridorSpeedOverrideType speedType);
    Task RemoveCorridorSpeedOverride(string authScheme, string authToken, long corridorId);
}
