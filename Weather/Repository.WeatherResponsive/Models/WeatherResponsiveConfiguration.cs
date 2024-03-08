// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Persistence.Common.Entities;

namespace Econolite.Ode.Repository.WeatherResponsive.Models;

public class WeatherResponsiveConfiguration : IndexedEntityBase<Guid>
{
    public Guid CorridorId { get; set; }

    public int Priority { get; set; }

    public string Name { get; set; } = string.Empty;

    // Conditions to run

    public bool IsEnabled { get; set; }

    public bool HasPrecipitation { get; set; }

    public double TemperatureThreshold { get; set; }

    // Map to enum?
    public int[] RoadConditions { get; set; } = Array.Empty<int>();

    public double MinimumConfidence { get; set; }

    // Plan information

    public bool EnableEdaptive { get; set; }

    public int? EdaptiveConfigurationId { get; set; }

    public bool AdjustSpeed { get; set; }

    public double? SpeedAdjustment { get; set; }

    public CorridorSpeedOverrideType? SpeedOverrideType { get; set; }

    public bool AdjustTimingPlan { get; set; }

    public byte? TimingPlan { get; set; }
}
