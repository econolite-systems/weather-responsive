// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Models.Entities;
using Econolite.Ode.Persistence.Common.Entities;
using Econolite.Ode.Status.Ess;
using Weather.Fusion;

namespace Econolite.Ode.Repository.WeatherResponsive.Models;

public class WeatherFusionDocument : IndexedEntityBase<Guid>
{
    public Guid CorridorId { get; set; }
    public DateTime Timestamp { get; set; }
    public int Confidence { get; set; } = 0;
    public bool Precipitation { get; set; } = false;
    public int SurfaceTemperature { get; set; } = 0;
    public int WaterFilmHeight { get; set; } = 0;
    public int FreezingTemperature { get; set; } = 0;
    public int Friction { get; set; } = 0;
    public int IcePercentage { get; set; } = 0;
    public enumRoadCondition RoadCondition { get; set; } = enumRoadCondition.Unknown;

    public static WeatherFusionDocument ToDb(CorridorStatus other, Guid corridorId)
    {
        return new WeatherFusionDocument
        {
            Id = Guid.NewGuid(),
            Confidence = other.Confidence,
            CorridorId = corridorId,
            FreezingTemperature = other.FreezingTemperature,
            Friction = other.Friction,
            IcePercentage = other.IcePercentage,
            Precipitation = other.Precipitation,
            RoadCondition = other.RoadCondition,
            SurfaceTemperature = other.SurfaceTemperature,
            Timestamp = DateTime.UtcNow,
            WaterFilmHeight = other.WaterFilmHeight,
        };
    }

    public WeatherFusionResultModel ToModel(IEnumerable<EntityNode> corridors)
    {
        return new WeatherFusionResultModel
        {
            Id = Id,
            CorridorId = CorridorId,
            CorridorName = corridors.FirstOrDefault(c => c.Id == CorridorId)?.Name ?? CorridorId.ToString(),
            Confidence = Confidence,
            Precipitation = Precipitation,
            RoadCondition = RoadCondition,
            // Converted to Fahrenheit
            Temperature = (int)Math.Round(SurfaceTemperature * 9.0 / 5.0 + 32),
            // TODO: Convert to customer local time?
            Timestamp = Timestamp,
        };
    }
}
