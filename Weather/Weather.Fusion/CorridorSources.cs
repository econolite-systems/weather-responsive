// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
// Ignore Spelling: Ess

using Econolite.Ode.Status.Ess;

namespace Weather.Fusion
{
    public class CorridorSources
    {
        public EssStatus[] EssStatuses { get; set; } = Array.Empty<EssStatus>();
        public CorridorObservation WeatherGovObservation { get; set; } = new CorridorObservation();
    }
}
