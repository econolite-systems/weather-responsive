// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
namespace Econolite.Ode.Repository.WeatherResponsive.Models;

public class WeatherResponsiveConfigurationOrder
{
    public Guid CorridorId { get; set; }

    public Guid[] ConfigurationOrder { get; set; } = Array.Empty<Guid>();
}
