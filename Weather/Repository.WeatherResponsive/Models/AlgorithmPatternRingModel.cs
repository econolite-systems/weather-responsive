// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
namespace Econolite.Ode.Repository.WeatherResponsive.Models;

public class AlgorithmPatternRingModel
{
    public int Ring { get; set; }

    public int Phase { get; set; }

    public int PhaseSequence { get; set; }

    public bool IsCoordinated { get; set; }

    public int Split { get; set; }

    public double Yellow { get; set; }

    public double RedClear { get; set; }

    public int Walk { get; set; }

    public int PedClear { get; set; }

    public int MinGreen { get; set; }

    public int MaxGreen { get; set; }

    public int BarrierGroup { get; set; }
}
