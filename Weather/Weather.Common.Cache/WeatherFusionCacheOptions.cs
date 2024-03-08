// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using System;

namespace Weather.Common.Cache
{
    public class WeatherFusionCacheOptions
    {
        public TimeSpan StatusTimeout { get; set; } = TimeSpan.FromMinutes(15);
    }
}
