// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
namespace Econolite.Ode.Repository.WeatherResponsive.Models;

public class AlgorithmCorridorModel
{
    public string CorridorName { get; set; } = string.Empty;

    public long CorridorId { get; set; }

    public int Index { get; set; }

    public bool IsSelected { get; set; }

    public int SplitLockGroup { get; set; }

    public AlgorithmSignalModel[] Signals { get; set; } = Array.Empty<AlgorithmSignalModel>();
}

public class AlgorithmSignalModel
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public int Index { get; set; }

    public long CorridorOverride { get; set; }

    public bool SignalLocatedAtEnd { get; set; }

    public CorridorOverrideType? OverrideType { get; set; }
}

public enum CorridorOverrideType
{
    Schedule,
    VolumeCapacity,
    MachineLearning
}
