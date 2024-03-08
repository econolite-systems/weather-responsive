// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Repository.WeatherResponsive.Models;

namespace Econolite.Ode.Repository.WeatherResponsive;

public interface IWeatherResponsiveRepository
{
    Task<WeatherResponsiveResult?> GetLastResponse(Guid corridorId, CancellationToken cancellationToken);
    Task SaveResponse(WeatherResponsiveResult result, CancellationToken cancellationToken);
    Task<ICollection<WeatherResponsiveResultModel>> Find(string authScheme, string authToken, List<Guid> corridorIds, DateTime startDate, DateTime? endDate);
    Task<ICollection<WeatherResponsiveResultModel>> FindAllLatest(string authScheme, string authToken);
}
