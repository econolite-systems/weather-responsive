// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Weather.Gov.Models;

namespace Weather.Fusion
{
    public class CorridorObservation
    {
        public Observation Observation { get; set; } = new Observation();
        public double DistanceFromCorridor { get; set; } = 0.0;
    }
}
