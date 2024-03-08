// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
namespace Econolite.Ode.Models.WeatherResponsive.Db
{
    public sealed record WeatherResponsiveDocument(
        DateTime TimeStamp,
        Guid CorridorId,
        WeatherResponsiveAlgorithmStatusEnum AlgorithmStatus,
        WeatherResponsiveSourceEnum Source,
        WeatherResponsiveWeatherStatusEnum WeatherStatus,
        int TimingPlan,
        bool Edaptive,
        bool SpeedHarmonizationEnabled,
        double? SpeedHarmonizationValue
        );    
}
