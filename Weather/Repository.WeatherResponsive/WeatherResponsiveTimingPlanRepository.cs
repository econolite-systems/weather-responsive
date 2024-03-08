// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;

namespace Econolite.Ode.Repository.WeatherResponsive;

public class WeatherResponsiveTimingPlanRepository : IWeatherResponsiveTimingPlanRepository
{
    private readonly string _mobilityApiPath;
    private readonly HttpClient _httpClient;

    public WeatherResponsiveTimingPlanRepository(IConfiguration config, HttpClient httpClient)
    {
        _mobilityApiPath = config["MobilityApi"] ?? throw new NullReferenceException("MobilityApi missing from config");
        if (_mobilityApiPath.EndsWith("/"))
            _mobilityApiPath = _mobilityApiPath[..^1];
        _httpClient = httpClient;
    }
    
    public async Task SetTimingPlan(string authScheme, string authToken, long corridorId, int timingPlan, int logicFlag, int logicFlagState)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authScheme, authToken);
        var response = await _httpClient.PostAsync($"{_mobilityApiPath}/external-device-control/set-timing-plan/corridor/{corridorId}?timingPlan={timingPlan}&logicFlag={logicFlag}&logicFlagState={logicFlagState}", null);
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
