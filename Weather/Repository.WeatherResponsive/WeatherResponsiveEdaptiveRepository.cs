// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Repository.WeatherResponsive.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Econolite.Ode.Repository.Entities;

namespace Econolite.Ode.Repository.WeatherResponsive;

public class WeatherResponsiveEdaptiveRepository : IWeatherResponsiveEdaptiveRepository
{
    private readonly string _mobilityApiPath;
    private readonly IEntityRepository _entityRepo;
    private readonly HttpClient _httpClient;

    public WeatherResponsiveEdaptiveRepository(IConfiguration config, IEntityRepository entityRepo, HttpClient httpClient)
    {
        _mobilityApiPath = config["MobilityApi"] ?? throw new NullReferenceException("MobilityApi missing from config");
        if (_mobilityApiPath.EndsWith("/"))
            _mobilityApiPath = _mobilityApiPath[..^1];
        _entityRepo = entityRepo;
        _httpClient = httpClient;
    }
    
    public async Task<AlgorithmResultsSummaryModel[]> GetResultsSummaryAsync(string authScheme, string authToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authScheme, authToken);
        var response = await _httpClient.GetAsync($"{_mobilityApiPath}/edaptive-control");
        if (!response.IsSuccessStatusCode)
            await ThrowHttpRequestExceptionOnFailure(response);
        var result = await response.Content.ReadFromJsonAsync<AlgorithmResultsSummaryModel[]>();
        return result!;
    }

    public async Task<AlgorithmResultsModel> GetResultsAsync(string authScheme, string authToken, int algorithmId, DateTime? date, int severity, int pageIndex, int pageSize)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authScheme, authToken);
        var response = await _httpClient.GetAsync($"{_mobilityApiPath}/edaptive-control/{algorithmId}/{date}?severity={severity}&pageIndex={pageIndex}&pageSize={pageSize}");
        if (!response.IsSuccessStatusCode)
            await ThrowHttpRequestExceptionOnFailure(response);
        var result = await response.Content.ReadFromJsonAsync<AlgorithmResultsModel>();
        return result!;
    }

    public async Task<AlgorithmConfigurationSlimModel[]> GetConfigurationsPerCorridorAsync(string authScheme, string authToken, Guid corridorId)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authScheme, authToken);
        var corridor = await _entityRepo.GetByIdAsync(corridorId);
        if (!string.IsNullOrWhiteSpace(corridor?.ExternalId))
        {
            var response = await _httpClient.GetAsync($"{_mobilityApiPath}/edaptive-control/configurations/{corridor.ExternalId}");
            if (!response.IsSuccessStatusCode)
                await ThrowHttpRequestExceptionOnFailure(response);
            var result = await response.Content.ReadFromJsonAsync<AlgorithmConfigurationSlimModel[]>();
            return result!;
        }
        return Array.Empty<AlgorithmConfigurationSlimModel>();
    }

    public async Task StartEdaptive(string authScheme, string authToken, int configurationId)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authScheme, authToken);
        var response = await _httpClient.PostAsync($"{_mobilityApiPath}/edaptive-control/start/{configurationId}", null);
        if (!response.IsSuccessStatusCode)
            await ThrowHttpRequestExceptionOnFailure(response);
    }

    public async Task StopEdaptive(string authScheme, string authToken, int configurationId)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authScheme, authToken);
        var response = await _httpClient.PostAsync($"{_mobilityApiPath}/edaptive-control/stop/{configurationId}", null);
        if (!response.IsSuccessStatusCode)
            await ThrowHttpRequestExceptionOnFailure(response);
    }

    public async Task<Dictionary<int, string>> GetConfigurationNames(string authScheme, string authToken, int[] configurationIds)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authScheme, authToken);
        var response = await _httpClient.GetAsync($"{_mobilityApiPath}/edaptive-control/configurations/names?{string.Join(", ", configurationIds.Select(c => $"configurationId={c}"))}");
        if (!response.IsSuccessStatusCode)
            await ThrowHttpRequestExceptionOnFailure(response);
        return await response.Content.ReadFromJsonAsync<Dictionary<int, string>>() ?? new Dictionary<int, string>();
    }

    public async Task<int> GetAlgorithmIdFromConfigurationIdAsync(string authScheme, string authToken, int configurationId)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authScheme, authToken);
        var response = await _httpClient.GetAsync($"{_mobilityApiPath}/edaptive-control/algorithm-for-configuration/{configurationId}");
        if (!response.IsSuccessStatusCode)
            await ThrowHttpRequestExceptionOnFailure(response);
        return await response.Content.ReadFromJsonAsync<int>();
    }

    public async Task ThrowHttpRequestExceptionOnFailure(HttpResponseMessage httpResponseMessage)
    {
        if (httpResponseMessage != null && httpResponseMessage.RequestMessage != null && !httpResponseMessage.IsSuccessStatusCode)
        {
            var ex = new Exception(httpResponseMessage.RequestMessage.ToString());
            throw new HttpRequestException(httpResponseMessage.ReasonPhrase, ex, httpResponseMessage.StatusCode);
        }

        await Task.CompletedTask;
    }
}
