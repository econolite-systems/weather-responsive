// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Repository.WeatherResponsive.Models;

namespace Econolite.Ode.Repository.WeatherResponsive;

public interface IWeatherResponsiveEdaptiveRepository
{
    Task<AlgorithmResultsSummaryModel[]> GetResultsSummaryAsync(string authScheme, string authToken);

    Task<AlgorithmResultsModel> GetResultsAsync(string authScheme, string authToken, int algorithmId, DateTime? date, int severity, int pageIndex, int pageSize);

    Task<AlgorithmConfigurationSlimModel[]> GetConfigurationsPerCorridorAsync(string authScheme, string authToken, Guid corridorId);

    Task StartEdaptive(string authScheme, string authToken, int configurationId);

    Task StopEdaptive(string authScheme, string authToken, int configurationId);

    Task<Dictionary<int, string>> GetConfigurationNames(string authScheme, string authToken, int[] configurationIds);

    Task<int> GetAlgorithmIdFromConfigurationIdAsync(string authScheme, string authToken, int configurationId);
}
