// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
namespace Econolite.Ode.Repository.WeatherResponsive.Models;

public class CorridorSpeedOverride
{
    public long CorridorId { get; set; }
    public DateTime StartTime { get; set; }
    public double SpeedAdjustment { get; set; }
    public CorridorSpeedOverrideType SpeedOverrideType { get; set; }
}
