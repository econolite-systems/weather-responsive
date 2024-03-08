// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
namespace Econolite.Ode.Repository.WeatherResponsive.Models;

public class AlgorithmResultHeaderModel
{
    public DateTime AnalysisStart { get; set; }

    public DateTime AnalysisEnd { get; set; }

    public int BasePattern { get; set; }

    public byte Mode { get; set; }

    public string MessageSeverity { get; set; } = string.Empty;
}
