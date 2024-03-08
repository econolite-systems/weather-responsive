// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
namespace Econolite.Ode.Repository.WeatherResponsive.Models;

public class AlgorithmResultIntersectionModel
{
    public long SignalId { get; set; }

    public string Name { get; set; } = string.Empty;

    //public int Adjustment { get; set; }

    public int Pattern { get; set; }

    //public int? UpstreamPhase { get; set; }

    //public int? DownstreamPhase { get; set; }

    public int CycleLengthBefore { get; set; }

    public int CycleLengthPredicted { get; set; }

    public int OffsetBefore { get; set; }

    public int OffsetPredicted { get; set; }

    public byte OffsetRefMode { get; set; }

    //public int UpstreamMoeBeforeProgrammed { get; set; }

    //public int UpstreamMoePredictedProgrammed { get; set; }

    //public int DownstreamMoeBeforeProgrammed { get; set; }

    //public int DownstreamMoePredictedProgrammed { get; set; }

    public double UpstreamVolumeCapacityRatio { get; set; }

    public double DownstreamVolumeCapacityRatio { get; set; }

    public virtual AlgorithmPatternRingModel[] RingsBefore { get; set; } = Array.Empty<AlgorithmPatternRingModel>();

    public virtual AlgorithmPatternRingModel[] RingsAfter { get; set; } = Array.Empty<AlgorithmPatternRingModel>();

    public string CommandedPattern { get; set; } = string.Empty;

    public long? CorridorOverride { get; set; }
}
