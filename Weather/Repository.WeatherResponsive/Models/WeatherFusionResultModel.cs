// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Status.Ess;

namespace Econolite.Ode.Repository.WeatherResponsive.Models;

public class WeatherFusionResultModel
{
    public Guid Id { get; set; }
    public Guid CorridorId { get; set; }
    public string? CorridorName { get; set; }
    public DateTime Timestamp { get; set; }
    public int Confidence { get; set; } = 0;
    public bool Precipitation { get; set; } = false;
    public int Temperature { get; set; } = 0;
    public enumRoadCondition RoadCondition { get; set; } = enumRoadCondition.Unknown;
}
