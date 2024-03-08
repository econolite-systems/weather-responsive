// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Status.Ess;

namespace Weather.Fusion
{
    public class CorridorStatus
    {
        public int Confidence { get; set; } = 0;
        public bool Precipitation { get; set; } = false;
        public int SurfaceTemperature { get; set; } = 0;
        public int WaterFilmHeight { get; set; } = 0;
        public int FreezingTemperature { get; set; } = 0;
        public int Friction { get; set; } = 0;
        public int IcePercentage { get; set; } = 0;
        public enumRoadCondition RoadCondition { get; set; } = enumRoadCondition.Unknown;
        public CorridorSources CorridorSources { get; set; } = new CorridorSources();
    }
}
