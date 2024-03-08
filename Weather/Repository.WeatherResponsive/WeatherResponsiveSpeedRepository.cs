// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Repository.WeatherResponsive.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Econolite.Ode.Repository.WeatherResponsive;

public class WeatherResponsiveSpeedRepository : IWeatherResponsiveSpeedRepository
{
    private readonly string _mobilityApiPath;
    private readonly HttpClient _httpClient;

    public WeatherResponsiveSpeedRepository(IConfiguration config, HttpClient httpClient)
    {
        _mobilityApiPath = config["MobilityApi"] ?? throw new NullReferenceException("MobilityApi missing from config");
        if (_mobilityApiPath.EndsWith("/"))
            _mobilityApiPath = _mobilityApiPath[..^1];
        _httpClient = httpClient;
    }

    public async Task<ICollection<CorridorSpeedOverride>> GetAllCorridorSpeedOverrides(string authScheme, string authToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authScheme, authToken);
        var response = await _httpClient.GetAsync($"{_mobilityApiPath}/edaptive-speed");
        if (!response.IsSuccessStatusCode)
            await ThrowHttpRequestExceptionOnFailure(response);
        var result = await response.Content.ReadFromJsonAsync<CorridorSpeedOverride[]>();
        return result!;
    }

    public async Task<ICollection<CorridorSpeedOverride>> GetCorridorSpeedOverridesForDate(string authScheme, string authToken, Guid corridorId, DateTime? date)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authScheme, authToken);
        var response = await _httpClient.GetAsync($"{_mobilityApiPath}/edaptive-speed/{corridorId}{(date.HasValue ? "?date=" + date.Value.ToString("d") : "")}");
        if (!response.IsSuccessStatusCode)
            await ThrowHttpRequestExceptionOnFailure(response);
        var result = await response.Content.ReadFromJsonAsync<CorridorSpeedOverride[]>();
        return result!;
    }

    public async Task SetCorridorSpeedOverride(string authScheme, string authToken, long corridorId, double speedAdjustment, CorridorSpeedOverrideType speedType)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authScheme, authToken);
        var response = await _httpClient.PostAsync($"{_mobilityApiPath}/edaptive-speed/{corridorId}?speedAdjustment={speedAdjustment}&speedType={speedType}", null);
        if (!response.IsSuccessStatusCode)
            await ThrowHttpRequestExceptionOnFailure(response);
    }

    public async Task RemoveCorridorSpeedOverride(string authScheme, string authToken, long corridorId)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authScheme, authToken);
        var response = await _httpClient.DeleteAsync($"{_mobilityApiPath}/edaptive-speed/{corridorId}");
        if (!response.IsSuccessStatusCode)
            await ThrowHttpRequestExceptionOnFailure(response);
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
