// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Persistence.Common.Entities;

namespace Econolite.Ode.Repository.WeatherResponsive.Models;

/// <summary>
/// WeatherResponsiveGlobalConfig
/// </summary>
public class WeatherResponsiveGlobalConfig : IndexedEntityBase<Guid>
{
    public IEnumerable<TimingPlanLogicFlagState>? TimingPlanLogicFlagStates { get; set; }
}

/// <summary>
/// TimingPlanLogicFlagState
/// </summary>
public class TimingPlanLogicFlagState
{
    public int TimingPlan { get; set; }
    public int LogicFlag { get; set; }
    public int LogicFlagState { get; set; }
}
