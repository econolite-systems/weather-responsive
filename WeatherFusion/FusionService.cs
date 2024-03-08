// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
// Ignore Spelling: ess

using Econolite.Ode.Common.Scheduler.Base.Timers;
using Econolite.Ode.Monitoring.Events;
using Econolite.Ode.Monitoring.Events.Extensions;
using Econolite.Ode.Monitoring.Metrics;
using Econolite.Ode.Repository.WeatherResponsive;
using Econolite.Ode.Status.Ess;
using Microsoft.Extensions.Options;
using Status.Ess.Cache;
using Weather.Common.Cache;
using Weather.Fusion;

namespace WeatherFusion
{
    /// <summary>
    /// Note that this is being done as BackgroundService, this will
    /// run until the application stops by signaling the stoppingToken.
    /// </summary>
    public class FusionService : BackgroundService
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly IPeriodicTimerFactory _periodicTimerFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEssStatusCache _essStatusCache;
        private readonly IWeatherFusionCache _weatherFusionCache;
        private readonly IWeatherFusionRepository _weatherFusionRepository;
        private readonly IWeatherGovCache _weatherGovCache;
        private readonly FusionServiceOptions _options;
        private readonly UserEventFactory _userEventFactory;
        private readonly ILogger _logger;
        private readonly IMetricsCounter _loopCounter;
        private readonly IMetricsCounter _corridorCounter;

        public FusionService(IPeriodicTimerFactory periodicTimerFactory, IServiceProvider serviceProvider, IEssStatusCache essStatusCache, IWeatherFusionCache weatherFusionCache, IWeatherFusionRepository weatherFusionRepository, IWeatherGovCache weatherGovCache, IOptions<FusionServiceOptions> options, UserEventFactory userEventFactory, IMetricsFactory metricsFactory, ILoggerFactory loggerFactory)
        {
            _periodicTimerFactory = periodicTimerFactory;
            _serviceProvider = serviceProvider;
            _essStatusCache = essStatusCache;
            _weatherFusionCache = weatherFusionCache;
            _weatherFusionRepository = weatherFusionRepository;
            _weatherGovCache = weatherGovCache;
            _options = options.Value;
            if (_options.WeatherGov.Confidence == null || _options.WeatherGov.Confidence.Distance80 == 0 || _options.WeatherGov.Confidence.Distance50 == 0 || _options.WeatherGov.Confidence.Distance0 == 0)
            {
                throw new ArgumentOutOfRangeException("FusionService", "All FusionService WeatherGov distances must be non-zero");
            }
            else if(_options.WeatherGov.Confidence.Distance80 > _options.WeatherGov.Confidence.Distance50 || _options.WeatherGov.Confidence.Distance50 > _options.WeatherGov.Confidence.Distance0)
            {
                throw new ArgumentOutOfRangeException("FusionService", "Confidence values must have: WeatherGov:Confidence:Distance80 <= WeatherGov.Confidence:Distance50 <= _options.WeatherGov.Confidence:Distance0");
            }
            if (_options.WeatherGov.Confidence.Time80.TotalSeconds <= 0.001 || _options.WeatherGov.Confidence.Time50.TotalSeconds <= 0.001 || _options.WeatherGov.Confidence.Time0.TotalSeconds <= 0.001)
            {
                throw new ArgumentOutOfRangeException("FusionService", "All FusionService WeatherGov times must be non-zero");
            }
            else if (_options.WeatherGov.Confidence.Time80 > _options.WeatherGov.Confidence.Time50 || _options.WeatherGov.Confidence.Time50 > _options.WeatherGov.Confidence.Time0)
            {
                throw new ArgumentOutOfRangeException("FusionService", "Confidence values must have: WeatherGov:Confidence:Time80 <= WeatherGov.Confidence:Time50 <= _options.WeatherGov.Confidence:Time0");
            }

            _userEventFactory = userEventFactory;
            _logger = loggerFactory.CreateLogger(GetType().Name);

            _loopCounter = metricsFactory.GetMetricsCounter("Fusions");
            _corridorCounter = metricsFactory.GetMetricsCounter("Corridors");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var timer = _periodicTimerFactory.CreateTopOfMinuteTimer();

            try
            {
                _logger.LogInformation("Starting Timer");
                timer.Start(() => ExecuteCronStepAsync(stoppingToken));

                // wait for the stoppingToken to trigger stopping
                await Task.Delay(-1, stoppingToken);
            }
            catch(OperationCanceledException)
            {
                _logger.LogDebug("CancellationToken signaled");
            }
            catch(Exception ex)
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
        private async Task ExecuteCronStepAsync(CancellationToken cancellationToken)
        {
            if (await _semaphore.WaitAsync(0, cancellationToken))
            {
                try
                {
                    _logger.LogInformation("Beginning Fusion task");
                    var tasks = new List<Task>();
                    using (var scope = _serviceProvider.CreateAsyncScope())
                    {
                        var weatherResponsiveConfigRepository = scope.ServiceProvider.GetService<IWeatherResponsiveConfigRepository>() ?? throw new NullReferenceException("Unable to obtain scoped IWeatherResponsiveConfigRepository");
                        var corridors = await weatherResponsiveConfigRepository.GetEnabledWeatherResponsiveCorridorIdsAsync(true, cancellationToken);
                        foreach (var corridor in corridors)
                        {
                            var taskFriendlyCorridor = corridor;
                            tasks.Add(CorridorWeatherFusionAsync(taskFriendlyCorridor, weatherResponsiveConfigRepository, cancellationToken));
                            _corridorCounter.Increment();
                        }
                        await Task.WhenAll(tasks);
                        _loopCounter.Increment();
                    }

                    _logger.LogInformation("Completed Fusion task");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Monitoring failed");
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            else
            {
                _logger.LogWarning("Did not complete previous monitoring step");
            }
        }

        private async Task CorridorWeatherFusionAsync(Guid corridorId, IWeatherResponsiveConfigRepository weatherResponsiveConfigRepository, CancellationToken cancellationToken)
        {
            try
            {
                var latestObservation = await _weatherGovCache.GetCorridorWeatherAsync(corridorId, cancellationToken) ?? new CorridorObservation();
                var essDevices = await weatherResponsiveConfigRepository.GetCorridorEssAsync(corridorId, cancellationToken);
                List<EssStatus> essStatuses = new();
                foreach (var ess in essDevices)
                {
                    essStatuses.Add(await _essStatusCache.GetStatusAsync(ess, cancellationToken));
                }

                var corridorStatus = new CorridorStatus
                {
                    CorridorSources = new CorridorSources
                    {
                        EssStatuses = essStatuses.ToArray(),
                        WeatherGovObservation = latestObservation,
                    }
                };
                var allSensors = essStatuses.SelectMany(_ => _.PassiveRoadSensorEntries).ToList();
                var anySensors = allSensors.Any();
                corridorStatus.WaterFilmHeight = anySensors ? (int)Math.Round(allSensors.Average(_ => _.WaterFilmHeight)) : 0;
                corridorStatus.SurfaceTemperature = anySensors ? (int)Math.Round(allSensors.Average(_ => _.SurfaceTemperature)) : (int)(latestObservation?.Observation?.Temperature?.Value ?? 0);
                corridorStatus.FreezingTemperature = anySensors ? (int)Math.Round(allSensors.Average(_ => _.FreezingTemperature)) : (int)(latestObservation?.Observation?.Temperature?.Value ?? 0);
                corridorStatus.Friction = anySensors ? (int)Math.Round(allSensors.Average(_ => _.Friction)) : 0;
                corridorStatus.IcePercentage = anySensors ? (int)Math.Round(allSensors.Average(_ => _.IcePercentage)) : 0;
                corridorStatus.Precipitation = anySensors ? corridorStatus.WaterFilmHeight >= _options.Ess.FilmHeightToPrecipitation : (latestObservation?.Observation?.PrecipitationLastHour?.Value ?? 0) > _options.WeatherGov.PrecipitationLastHourThreshold;
                corridorStatus.RoadCondition = anySensors ? allSensors.Select(_ => _.RoadCondition).Where(_ => (int)_ < (int)enumRoadCondition.Error).Max(_ => _) : corridorStatus.Precipitation ? (corridorStatus.SurfaceTemperature > 0 ? enumRoadCondition.Wet : enumRoadCondition.SnowOrIce) : enumRoadCondition.Dry;
                corridorStatus.Confidence = anySensors ? BuildConfidenceFromEss(allSensors) : BuildConfidenceFromWeatherGov(corridorId, latestObservation);

                //await _weatherFusionCache.PutCorridorWeatherAsync(corridorId, corridorStatus, cancellationToken);
                await _weatherFusionRepository.PutCorridorWeatherAsync(corridorId, corridorStatus, cancellationToken);
                
                _logger.ExposeUserEvent(_userEventFactory.BuildUserEvent(EventLevel.Debug, string.Format("Calculated weather fusion status for corridor: {0}", corridorId)));
            }
            catch (Exception ex)
            {
                _logger.ExposeUserEvent(_userEventFactory.BuildUserEvent(EventLevel.Error, string.Format("Unable to calculate weather fusion status for corridor: {0}", corridorId)));

                _logger.LogError(ex, "Unable to process weather fusion status for corridor: {corridor}", corridorId);
            }
        }

        private int BuildConfidenceFromWeatherGov(Guid corridorId, CorridorObservation? latestObservation)
        {
            _logger.LogDebug("Corridor: {corridor}, distance: {distance}", corridorId, latestObservation?.DistanceFromCorridor);
            var result = 100.0;
            switch ((latestObservation?.DistanceFromCorridor ?? 0) / 1000.0)
            {
                // { } check for null
                case { } i and >= 0 when i < _options.WeatherGov.Confidence!.Distance80:
                    result *= (int)(100 - i / _options.WeatherGov.Confidence.Distance80 * 20) / 100.0;
                    break;
                case { } i when _options.WeatherGov.Confidence!.Distance80 <= i && i < _options.WeatherGov.Confidence.Distance50:
                    result *= (int)(80 - (i - _options.WeatherGov.Confidence.Distance80) / (_options.WeatherGov.Confidence.Distance50 - _options.WeatherGov.Confidence.Distance80) * 30) / 100.0;
                    break;
                case { } i when _options.WeatherGov.Confidence.Distance50 <= i && i < _options.WeatherGov.Confidence.Distance0:
                    result *= (int)(50 - (i - _options.WeatherGov.Confidence.Distance50) / (_options.WeatherGov.Confidence.Distance50 - _options.WeatherGov.Confidence.Distance0) * 50) / 100.0;
                    break;
                default:
                    _logger.LogWarning("Observed distance {distance}, did not fall within configured thresholds: {@options}", (latestObservation?.DistanceFromCorridor ?? 0) / 1000.0, _options.WeatherGov);
                    result = 0;
                    break;
            }
            _logger.LogDebug("Confidence: {result}, from distance: {distance}", result, latestObservation?.DistanceFromCorridor);
            
            switch (DateTimeOffset.UtcNow - latestObservation?.Observation.Timestamp)
            {
                case { } i when i >= TimeSpan.Zero && i < _options.WeatherGov.Confidence.Time80:
                    result *= (int)(100 - i / _options.WeatherGov.Confidence.Time80 * 20) / 100.0;
                    break;
                case { } i when _options.WeatherGov.Confidence.Time80 <= i && i < _options.WeatherGov.Confidence.Time50:
                    result *= (int)(80 - (i - _options.WeatherGov.Confidence.Time80) / (_options.WeatherGov.Confidence.Time50 - _options.WeatherGov.Confidence.Time80) * 30) / 100.0;
                    break;
                case { } i when _options.WeatherGov.Confidence.Time50 <= i && i < _options.WeatherGov.Confidence.Time0:
                    result *= (int)(50 - (i - _options.WeatherGov.Confidence.Time50) / (_options.WeatherGov.Confidence.Time50 - _options.WeatherGov.Confidence.Time0) * 50) / 100.0;
                    break;
                default:
                    _logger.LogWarning("Observed time {time}, did not fall within configured thresholds: {@options}", latestObservation?.Observation.Timestamp, _options.WeatherGov);
                    result = 0;
                    break;
            }
            _logger.LogDebug("Confidence: {result}, after time: {time}", result, DateTimeOffset.UtcNow - latestObservation?.Observation.Timestamp);

            return (int)Math.Round(result);
        }

        private static int BuildConfidenceFromEss(List<passiveRoadSensorEntry> essStatuses)
        {
            var result = 0;
            if (essStatuses.Any())
            {
                double errorCount = essStatuses.Count(_ => _.RoadCondition == enumRoadCondition.Error);
                result = (int)(100 - (errorCount / essStatuses.Count));
            }
            return result;
        }
    }
}
