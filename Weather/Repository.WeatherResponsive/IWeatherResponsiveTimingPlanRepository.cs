// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
namespace Econolite.Ode.Repository.WeatherResponsive;

public interface IWeatherResponsiveTimingPlanRepository
{
    Task SetTimingPlan(string authScheme, string authToken, long corridorId, int timingPlan, int logicFlag, int logicFlagState);
}
