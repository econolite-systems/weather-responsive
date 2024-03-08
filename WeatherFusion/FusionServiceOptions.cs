// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
// Ignore Spelling: Ess

using System;

namespace WeatherFusion
{
    public class WeatherGovOptions
    {
        public WeatherGovConfidenceOptions? Confidence { get; set; }
        public double PrecipitationLastHourThreshold { get; set; }
    }

    public class WeatherGovConfidenceOptions
    {
        public double Distance80 { get; set; }
        public double Distance50 { get; set; }
        public double Distance0 { get; set; }
        public TimeSpan Time80 { get; set; }
        public TimeSpan Time50 { get; set; }
        public TimeSpan Time0 { get; set; }
    }

    public class Ess
    {
        public double FilmHeightToPrecipitation { get; set; }
    }

    public class FusionServiceOptions
    {
        public WeatherGovOptions WeatherGov { get; set; } = new();
        public Ess Ess { get; set; } = new();
    }
}
