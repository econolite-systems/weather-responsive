// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
namespace Econolite.Ode.Repository.WeatherResponsive.Models;

public class AlgorithmResultModel
{
    public long Id { get; set; }

    public int AlgorithmConfigurationId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public bool Success { get; set; }

    public bool Rollback { get; set; }

    public byte Mode { get; set; }

    public string Message { get; set; } = string.Empty;

    public string MessageSeverity { get; set; } = string.Empty;

    public int BasePattern { get; set; }

    public double AverageVolumeCapacityRatio { get; set; }

    public AlgorithmResultIntersectionModel[] Intersections { get; set; } = Array.Empty<AlgorithmResultIntersectionModel>();
}
