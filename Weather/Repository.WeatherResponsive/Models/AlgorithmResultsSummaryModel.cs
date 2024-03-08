// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
namespace Econolite.Ode.Repository.WeatherResponsive.Models;

public class AlgorithmResultsSummaryModel
{
    public int AlgorithmId { get; set; }

    public string AlgorithmName { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public DateTime? LastRunTime { get; set; }

    public string LastResult { get; set; } = string.Empty;

    public bool IsScheduled { get; set; }

    public bool IsEnabled { get; set; }
}
