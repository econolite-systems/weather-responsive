// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Common.Scheduler.Base.Timers;
using Econolite.Ode.Monitoring.Events;
using Econolite.Ode.Monitoring.Events.Extensions;
using Econolite.Ode.Monitoring.Metrics;
using Econolite.Ode.Repository.WeatherResponsive;
using GeoCoordinatePortable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Weather.Common.Cache;
using Weather.Fusion;
using Weather.Gov;
using Weather.Gov.Models;

namespace WeatherMonitoring
{
    public class MonitoringService : BackgroundService
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly IPeriodicTimerFactory _periodicTimerFactory;
        private readonly IWeatherGov _weatherGov;
        private readonly IWeatherGovCache _weatherGovCache;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMetricsCounter _loopCounter;
        private readonly IMetricsCounter _totalCorridorCounter;
        private readonly UserEventFactory _userEventFactory;
        private readonly ILogger _logger;

        public MonitoringService(IPeriodicTimerFactory periodicTimerFactory, IWeatherGov weatherGov, IWeatherGovCache weatherGovCache, IServiceProvider serviceProvider, IMetricsFactory metricsFactory, UserEventFactory userEventFactory, ILoggerFactory loggerFactory)
        {
            _periodicTimerFactory = periodicTimerFactory;
            _weatherGov = weatherGov;
            _weatherGovCache = weatherGovCache;
            _serviceProvider = serviceProvider;
            _loopCounter = metricsFactory.GetMetricsCounter("Monitoring");
            _totalCorridorCounter = metricsFactory.GetMetricsCounter("Corridors");
            _userEventFactory = userEventFactory;
            _logger = loggerFactory.CreateLogger(GetType().Name);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var timer = _periodicTimerFactory.CreateTopOfMinuteTimer();
            _logger.LogInformation("Starting Timer");
            timer.Start(() => MonitoringStepAsync(stoppingToken));

            // wait for the stoppingToken to trigger stopping
            try
            {
                await Task.Delay(-1, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("CancellationToken signaled");
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "CancellationToken signaled");
            }

            _logger.LogInformation("Stopping timer");
            await timer.StopAsync();
            _logger.LogInformation("Timer Stopped");
        }

        /// <summary>
        /// Main processing that takes place once a time period. 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task MonitoringStepAsync(CancellationToken cancellationToken)
        {
            if (await _semaphore.WaitAsync(0, cancellationToken))
            {
                try
                {
                    _logger.LogInformation("Beginning Monitoring task");
                    var tasks = new List<Task>();
                    await using (var scope = _serviceProvider.CreateAsyncScope())
                    {
                        var weatherResponsiveConfigRepository = scope.ServiceProvider.GetService<IWeatherResponsiveConfigRepository>() ?? throw new NullReferenceException("Unable to obtain scoped IWeatherResponsiveConfigRepository");
                        var corridors = await weatherResponsiveConfigRepository.GetEnabledWeatherResponsiveCorridorIdsAsync(true, cancellationToken);
                        foreach (var corridor in corridors)
                        {
                            var taskSafeCorridor = corridor;
                            tasks.Add(CorridorStatusAsync(taskSafeCorridor, cancellationToken));
                        }
                    }
                    // Not including the cancellationToken here as each task should cancel
                    await Task.WhenAll(tasks);
                    _logger.LogInformation("Completed Monitoring task");
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Monitoring failed");
                }
                finally
                {
                    _semaphore.Release();
                }

                _loopCounter.Increment();
            }
            else
            {
                _logger.LogWarning("Did not complete previous monitoring step");
            }
        }

        private async Task CorridorStatusAsync(Guid corridorId, CancellationToken cancellationToken)
        {
            try
            {
                var (stationIdentifier, distance) = await _weatherGovCache.GetWeatherStationIdentifierAsync(corridorId, cancellationToken);
                if (string.IsNullOrEmpty(stationIdentifier))
                {
                    _logger.LogInformation("Determining Station Identifier for corridor {@}", corridorId);
                    await using var scope = _serviceProvider.CreateAsyncScope();
                    var weatherResponsiveConfigRepository = scope.ServiceProvider.GetService<IWeatherResponsiveConfigRepository>() ?? throw new NullReferenceException("Unable to obtain scoped IWeatherResponsiveConfigRepository");

                    var location = await weatherResponsiveConfigRepository.GetCorridorLocationAsync(corridorId, cancellationToken);
                    if (Math.Abs(location.Latitude) < 0.001 && Math.Abs(location.Longitude) < 0.001)
                    {
                        _logger.LogWarning("Skipping retrieval of corridor: {corridor}, location unconfigured.", corridorId);
                    }
                    else
                    {
                        var point = await _weatherGov.GetPointAsync(location, cancellationToken);

                        var stations = await _weatherGov.GetObservationStationsAsync(point.Properties.GridId, point.Properties.GridX, point.Properties.GridY, cancellationToken);

                        var nearestStations = OrderFeatureByProximity(location, stations.Features);
                        var nearestStation = nearestStations[0];
                        var stationId = nearestStation.Feature.Properties as JObject ?? throw new NullReferenceException("Nearest station does not have properties");
                        if (stationId.TryGetValue("stationIdentifier", out var stationIdentifierObject))
                        {
                            stationIdentifier = (string?)stationIdentifierObject ?? throw new NullReferenceException("Unable to determine station identifier");
                            _logger.LogInformation("Caching stationIdentifier {@}", new { corridorId, stationidentifer = stationIdentifier });
                            await _weatherGovCache.PutWeatherStationIdentifierAsync(corridorId, stationIdentifier, nearestStation.Distance, cancellationToken);

                            var currentObservation = await _weatherGov.GetLatestObservationAsync(stationIdentifier, cancellationToken);
                            await _weatherGovCache.PutCorridorWeatherAsync(corridorId, new CorridorObservation
                            {
                                Observation = currentObservation.Properties,
                                DistanceFromCorridor = nearestStation.Distance,
                            }, cancellationToken);
                        }
                    }
                }
                else
                {
                    var currentObservation = await _weatherGov.GetLatestObservationAsync(stationIdentifier, cancellationToken);
                    await _weatherGovCache.PutCorridorWeatherAsync(corridorId, new CorridorObservation
                    {
                        Observation = currentObservation.Properties,
                        DistanceFromCorridor = distance,
                    }, cancellationToken);
                }

                _logger.ExposeUserEvent(_userEventFactory.BuildUserEvent(EventLevel.Debug, string.Format("Gathered weather status for corridor: {0}", corridorId)));
            }
            catch (Exception ex)
            {
                _logger.ExposeUserEvent(_userEventFactory.BuildUserEvent(EventLevel.Error, string.Format("Unable to gather weather status for corridor: {0}", corridorId)));

                _logger.LogError(ex, "Error retrieving status for corridor: {corridor}", corridorId);
            }

            _totalCorridorCounter.Increment();
        }

        // Distance is in meters
        private (double Distance, GeoJsonFeature Feature)[] OrderFeatureByProximity((double Latitude, double Longitude) latLonLocation, IEnumerable<GeoJsonFeature> features)
        {
            var result = new List<(double Distance, GeoJsonFeature Feature)>();
            var location = new GeoCoordinate(latLonLocation.Latitude, latLonLocation.Longitude);
            foreach (var feature in features)
            {
                feature.Geometry.AdditionalProperties.TryGetValue("coordinates", out var coordinateObject);
                if (coordinateObject is JArray { Count: > 1 } coordinates)
                {
                    var otherLatitude = (double)coordinates[1];
                    var otherLongitude = (double)coordinates[0];
                    var otherLocation = new GeoCoordinate(otherLatitude, otherLongitude);
                    result.Add((location.GetDistanceTo(otherLocation), feature));
                }
            }
            return result.OrderBy(_ => _.Distance).ToArray();
        }
    }
}
