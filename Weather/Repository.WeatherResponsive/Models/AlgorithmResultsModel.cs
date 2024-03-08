// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
namespace Econolite.Ode.Repository.WeatherResponsive.Models;

public class AlgorithmResultsModel
{
    public string AlgorithmName { get; set; } = string.Empty;

    public long TotalCount { get; set; }

    public AlgorithmConfigurationSlimModel Configuration { get; set; } = null!;

    public AlgorithmCorridorModel[] Corridors { get; set; } = Array.Empty<AlgorithmCorridorModel>();

    public AlgorithmResultModel[] Results { get; set; } = Array.Empty<AlgorithmResultModel>();

    public AlgorithmResultHeaderModel Header { get; set; } = null!;

    public CorridorSpeedOverride[] CorridorSpeedOverrides { get; set; } = Array.Empty<CorridorSpeedOverride>();
}
