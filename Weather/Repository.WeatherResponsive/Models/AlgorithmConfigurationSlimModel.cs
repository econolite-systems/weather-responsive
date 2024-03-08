// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
namespace Econolite.Ode.Repository.WeatherResponsive.Models;

public class AlgorithmConfigurationSlimModel
{
    public int AlgorithmId { get; set; }

    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public byte Mode { get; set; }

    public bool OptimizeOffset { get; set; }

    public bool OptimizeCycleLength { get; set; }

    public bool OptimizeSplits { get; set; }

    public bool OptimizePhaseSequence { get; set; }

    public int DataAffinity { get; set; }

    public int CyclesToOptimize { get; set; }

    public byte CycleAndOffsetPeriod { get; set; }

    public byte SideStreetMaxSplitIncrease { get; set; }

    public byte SideStreetMaxSplitDecrease { get; set; }

    public byte CoordPhaseMaxSplitIncrease { get; set; }

    public byte CoordPhaseMaxSplitDecrease { get; set; }

    public byte MinimumSplitBufferSeconds { get; set; }

    public byte CycleLengthLowerVolumeCapacityThreshold { get; set; }

    public byte CycleLengthUpperVolumeCapacityThreshold { get; set; }

    public int MinCycleLength { get; set; }

    public int MaxCycleLength { get; set; }

    public byte CycleLengthDecreaseDelta { get; set; }

    public byte CycleLengthIncreaseDelta { get; set; }
}
