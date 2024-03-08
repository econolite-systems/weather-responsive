// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Models.Entities;
using Econolite.Ode.Persistence.Common.Entities;
using Econolite.Ode.Status.Ess;

namespace Econolite.Ode.Repository.WeatherResponsive.Models;

public class WeatherResponsiveResult : IndexedEntityBase<Guid>
{
    public Guid CorridorId { get; set; }
    public DateTime Timestamp { get; set; }
    public bool RanEdaptive { get; set; }
    public bool ChangedSpeed { get; set; }
    public bool ChangedTimingPlan { get; set; }
    public int? EdaptiveConfigurationId { get; set; }
    public double? SpeedAdjustment { get; set; }
    public CorridorSpeedOverrideType? SpeedOverrideType { get; set; }
    public int? TimingPlan { get; set; }
    public string Errors { get; set; } = string.Empty;
    public int Confidence { get; set; }
    public bool Precipitation { get; set; } = false;
    public int Temperature { get; set; }
    public enumRoadCondition RoadCondition { get; set; } = enumRoadCondition.Unknown;

    public WeatherResponsiveResultModel ToModel(IEnumerable<EntityNode> corridors, Dictionary<int, string> edaptiveConfigs)
    {
        return new WeatherResponsiveResultModel
        {
            Id = Id,
            CorridorId = CorridorId,
            CorridorName = corridors.FirstOrDefault(c => c.Id == CorridorId)?.Name ?? CorridorId.ToString(),
            ChangedTimingPlan = ChangedTimingPlan,
            ChangedSpeed = ChangedSpeed,
            Confidence = Confidence,
            EdaptiveConfigurationName = edaptiveConfigs.FirstOrDefault(c => c.Key == EdaptiveConfigurationId).Value ?? EdaptiveConfigurationId.ToString(),
            TimingPlan = TimingPlan,
            SpeedOverrideType = SpeedOverrideType,
            SpeedAdjustment = SpeedAdjustment,
            Errors = Errors,
            Precipitation = Precipitation,
            RanEdaptive = RanEdaptive,
            RoadCondition = RoadCondition,
            // Converted to Fahrenheit
            Temperature = (int)Math.Round(Temperature * 9.0 / 5.0 + 32),
            // TODO: Convert to customer local time?
            Timestamp = Timestamp,
        };
    }
}
