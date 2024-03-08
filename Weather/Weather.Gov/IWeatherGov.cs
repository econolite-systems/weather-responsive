// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using System.Threading;
using System.Threading.Tasks;
using Weather.Gov.Models;

namespace Weather.Gov
{
    public interface IWeatherGov
    {
        Task<PointJsonLd> GetPointAsync((double Latitude, double Longitude) location, CancellationToken cancellationToken = default(CancellationToken));
        Task<ObservationGeoJson> GetLatestObservationAsync(string station, CancellationToken cancellationToken = default(CancellationToken));
        Task<GridpointGeoCollectionJson> GetObservationStationsAsync(NWSForecastOfficeId wfo, long gridX, long gridY, CancellationToken cancellationToken = default(CancellationToken));
    }
}
