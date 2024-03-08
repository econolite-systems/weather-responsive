// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Authorization;
using Econolite.Ode.Common.Scheduler.Base.Timers;
using Econolite.Ode.Monitoring.Events;
using Econolite.Ode.Monitoring.Events.Extensions;
using Econolite.Ode.Repository.Entities;
using Econolite.Ode.Repository.WeatherResponsive;
using Econolite.Ode.Repository.WeatherResponsive.Models;
using System.Net;
using Econolite.Ode.Monitoring.Metrics;

namespace WeatherResponsiveService;

public class FusionActionService : BackgroundService
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly IPeriodicTimerFactory _periodicTimerFactory;
    private readonly IServiceProvider _serviceProvider;
    private readonly ITokenHandler _tokenHandler;
    private readonly UserEventFactory _userEventFactory;
    private readonly ILogger _logger;

    private static string? _token;
    private readonly IMetricsCounter _loopCounter;
    private readonly IMetricsCounter _corridorCounter;

    public FusionActionService(IPeriodicTimerFactory periodicTimerFactory, IServiceProvider serviceProvider, ITokenHandler tokenHandler, IMetricsFactory metricsFactory, UserEventFactory userEventFactory, ILogger<FusionActionService> logger)
    {
        _periodicTimerFactory = periodicTimerFactory;
        _serviceProvider = serviceProvider;
        _tokenHandler = tokenHandler;
        _userEventFactory = userEventFactory;
        _logger = logger;

        _loopCounter = metricsFactory.GetMetricsCounter("Responsive");
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
    private async Task ExecuteCronStepAsync(CancellationToken cancellationToken)
    {
        if (await _semaphore.WaitAsync(0, cancellationToken))
        {
            try
            {
                _logger.LogInformation("Beginning responsive task");
                var tasks = new List<Task>();
                await using (var scope = _serviceProvider.CreateAsyncScope())
                {
                    var weatherResponsiveConfigRepository = scope.ServiceProvider.GetService<IWeatherResponsiveConfigRepository>() ?? throw new NullReferenceException("Unable to obtain scoped IWeatherResponsiveConfigRepository");
                    var corridors = (await weatherResponsiveConfigRepository.GetWeatherResponsiveCorridorConfigsAsync(cancellationToken)).ToList();
                    if (corridors.Any())
                    {
                        //var weatherFusionCache = scope.ServiceProvider.GetService<IWeatherFusionCache>() ?? throw new NullReferenceException("Unable to obtain scoped IWeatherFusionRepository");
                        var weatherFusionRepository = scope.ServiceProvider.GetService<IWeatherFusionRepository>() ?? throw new NullReferenceException("Unable to obtain scoped IWeatherFusionRepository");
                        var weatherResponsiveRepository = scope.ServiceProvider.GetService<IWeatherResponsiveRepository>() ?? throw new NullReferenceException("Unable to obtain scoped IWeatherResponsiveRepository");
                        var entityRepository = scope.ServiceProvider.GetService<IEntityRepository>() ?? throw new NullReferenceException("Unable to obtain scoped IEntityRepository");
                        var globalConfigRepository = scope.ServiceProvider.GetService<IWeatherResponsiveGlobalConfigRepository>() ?? throw new NullReferenceException("Unable to obtain scoped IWeatherResponsiveGlobalConfigRepository");

                        WeatherResponsiveGlobalConfig? globalConfig = null;
                        try
                        {
                            globalConfig = await globalConfigRepository.GetGlobalConfig();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Unable to gather global configuration.");
                        }

                        foreach (var corridor in corridors)
                        {
                            // Thread safety
                            var corridorId = corridor.Key;
                            _logger.LogDebug("Running calculations for corridor: {corridor}", corridorId);
                            var configs = corridor.Value;
                            var threadedScope = scope;

                            tasks.Add(Task.Run(async () =>
                            {
                                //var status = await weatherFusionCache.GetCorridorWeatherAsync(corridorId, cancellationToken);
                                var status = await weatherFusionRepository.GetCorridorWeatherAsync(corridorId, cancellationToken);
                                if (status != null)
                                {
                                    WeatherResponsiveConfiguration? match = null;
                                    bool? runEdaptive = null;
                                    bool? changeSpeed = null;
                                    bool? setTimingPlan = null;
                                    int? edaptiveConfigId = null;
                                    double? speedAdjustment = null;
                                    CorridorSpeedOverrideType? speedAdjustmentType = null;
                                    int? oldTimingPlan = null;
                                    int? timingPlan = null;

                                    // Use status to find matching config.
                                    foreach (var config in configs)
                                    {
                                        if (config.HasPrecipitation && !status.Precipitation)
                                            continue;
                                        if (config.MinimumConfidence > 0 && status.Confidence < config.MinimumConfidence)
                                            continue;
                                        // Lower temperature than threshold = match
                                        if ((status.SurfaceTemperature * 9.0 / 5.0 + 32) > config.TemperatureThreshold)
                                            continue;
                                        if (config.RoadConditions.Any() && !config.RoadConditions.Contains((int)status.RoadCondition))
                                            continue;

                                        match = config;

                                        // Use lastResponse to start/stop anything as needed.
                                        var lastResponse = await weatherResponsiveRepository.GetLastResponse(corridorId, cancellationToken);
                                        if (config.EnableEdaptive && config.EdaptiveConfigurationId > 0)
                                        {
                                            runEdaptive = true;
                                            edaptiveConfigId = config.EdaptiveConfigurationId;
                                        }
                                        else if (lastResponse?.RanEdaptive == true)
                                        {
                                            // Undo active Edaptive
                                            runEdaptive = false;
                                            edaptiveConfigId = lastResponse.EdaptiveConfigurationId;
                                        }

                                        if (config.AdjustSpeed && Math.Abs(config.SpeedAdjustment ?? 0) > 0)
                                        {
                                            changeSpeed = true;
                                            speedAdjustment = config.SpeedAdjustment;
                                            speedAdjustmentType = config.SpeedOverrideType;
                                        }
                                        else if (lastResponse?.ChangedSpeed == true)
                                        {
                                            // Undo active speed
                                            changeSpeed = false;
                                        }

                                        if (config.AdjustTimingPlan && config.TimingPlan > 0)
                                        {
                                            setTimingPlan = true;
                                            timingPlan = config.TimingPlan;
                                            if (lastResponse?.ChangedTimingPlan == true)
                                                oldTimingPlan = lastResponse.TimingPlan!.Value;
                                        }
                                        else if (lastResponse?.ChangedTimingPlan == true)
                                        {
                                            // Cancel flag for active timing plan
                                            setTimingPlan = false;
                                            oldTimingPlan = lastResponse.TimingPlan!.Value;
                                        }
                                        break;
                                    }

                                    _logger.LogInformation("Found match for corridor: {corridor}? {@match}, from {count} configs for status: {@status}", corridorId, match, configs.Length, status);
                                    
                                    var errors = new List<string>();
                                    var corridorEntity = await entityRepository.GetByIdAsync(corridorId);
                                    if (long.TryParse(corridorEntity?.ExternalId, out var corridorNumber))
                                    {
                                        try
                                        {
                                            if (changeSpeed != null)
                                            {
                                                var speedRepository = threadedScope.ServiceProvider.GetService<IWeatherResponsiveSpeedRepository>() ?? throw new NullReferenceException("Unable to obtain scoped IWeatherResponsiveSpeedRepository");
                                                if (changeSpeed.Value)
                                                {
                                                    await MakeCallWithAuthAsync((scheme, token) => speedRepository.SetCorridorSpeedOverride(scheme, token, corridorNumber, speedAdjustment!.Value, speedAdjustmentType!.Value), cancellationToken);
                                                    _logger.ExposeUserEvent(_userEventFactory.BuildUserEvent(EventLevel.Information, string.Format("Corridor speed override active, corridor: {0}, speed: {1}, type: {2}", corridorId, speedAdjustment, speedAdjustmentType)));
                                                    //_logger.LogInformation("Corridor speed override active, corridor: {corridor}, speed: {speed}, type: {type}", corridorId, speedAdjustment, speedAdjustmentType);
                                                }
                                                else
                                                {
                                                    await MakeCallWithAuthAsync((scheme, token) => speedRepository.RemoveCorridorSpeedOverride(scheme, token, corridorNumber), cancellationToken);
                                                    _logger.ExposeUserEvent(_userEventFactory.BuildUserEvent(EventLevel.Information, string.Format("Corridor speed override disable, corridor: {0}", corridorId)));
                                                    //_logger.LogInformation("Corridor speed override disable, corridor: {corridor}", corridorId);
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.ExposeUserEvent(_userEventFactory.BuildUserEvent(EventLevel.Error, string.Format("Could not adjust speed for corridor: {0}", corridorId)));
                                            _logger.LogError(ex, "Could not adjust speed for corridor: {corridor}", corridorId);
                                            errors.Add(ex.Message);
                                        }

                                        try
                                        {
                                            var timingPlanRepository = threadedScope.ServiceProvider.GetService<IWeatherResponsiveTimingPlanRepository>() ?? throw new NullReferenceException("Unable to obtain scoped IWeatherResponsiveTimingPlanRepository");

                                            // Send stop of old timing plan if different.
                                            if (oldTimingPlan != null && oldTimingPlan != timingPlan)
                                            {
                                                var state = globalConfig?.TimingPlanLogicFlagStates?.FirstOrDefault(s => s.TimingPlan == oldTimingPlan!.Value);
                                                if (state is { LogicFlagState: > 0 })
                                                {
                                                    // 2 == LogicFlagState.Off
                                                    await MakeCallWithAuthAsync((scheme, token) => timingPlanRepository.SetTimingPlan(scheme, token, corridorNumber, oldTimingPlan.Value, state.TimingPlan, 2), cancellationToken);
                                                    _logger.ExposeUserEvent(_userEventFactory.BuildUserEvent(EventLevel.Information, string.Format("Corridor timing plan stopped, corridor: {0}, timing plan: {1}", corridorId, oldTimingPlan)));
                                                }
                                                else
                                                {
                                                    _logger.ExposeUserEvent(_userEventFactory.BuildUserEvent(EventLevel.Warning, string.Format("Corridor timing plan unable to stop, corridor: {0}, timing plan: {1}, no logic flag configured.", corridorId, oldTimingPlan)));
                                                }
                                            }

                                            // Start/Renew current timing plan
                                            if (setTimingPlan != null)
                                            {
                                                if (setTimingPlan.Value && timingPlan > 0)
                                                {
                                                    var state = globalConfig?.TimingPlanLogicFlagStates?.FirstOrDefault(s => s.TimingPlan == timingPlan!.Value);
                                                    if (state is { LogicFlagState: > 0 })
                                                    {
                                                        await MakeCallWithAuthAsync((scheme, token) => timingPlanRepository.SetTimingPlan(scheme, token, corridorNumber, timingPlan.Value, state.TimingPlan, state.LogicFlagState), cancellationToken);
                                                        _logger.ExposeUserEvent(_userEventFactory.BuildUserEvent(EventLevel.Information, string.Format("Corridor timing plan active, corridor: {0}, timing plan: {1}", corridorId, timingPlan)));
                                                    }
                                                    else
                                                    {
                                                        _logger.ExposeUserEvent(_userEventFactory.BuildUserEvent(EventLevel.Warning, string.Format("Corridor timing plan unable to activate, corridor: {0}, timing plan: {1}, no logic flag configured.", corridorId, timingPlan)));
                                                    }
                                                    //_logger.LogInformation("Corridor timing plan active, corridor: {corridor}, timing plan: {timingPlan}", corridorId, timingPlan);
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.ExposeUserEvent(_userEventFactory.BuildUserEvent(EventLevel.Error, string.Format("Could not set timing plan for corridor: {0}", corridorId)));
                                            _logger.LogError(ex, "Could not set timing plan for corridor: {corridor}", corridorId);
                                            errors.Add(ex.Message);
                                        }

                                        try
                                        {
                                            if (runEdaptive != null)
                                            {
                                                var edaptiveRepository = threadedScope.ServiceProvider.GetService<IWeatherResponsiveEdaptiveRepository>() ?? throw new NullReferenceException("Unable to obtain scoped IWeatherResponsiveEdaptiveRepository");
                                                if (runEdaptive.Value)
                                                {
                                                    await MakeCallWithAuthAsync((scheme, token) => edaptiveRepository.StartEdaptive(scheme, token, edaptiveConfigId!.Value), cancellationToken);
                                                    _logger.ExposeUserEvent(_userEventFactory.BuildUserEvent(EventLevel.Information, string.Format("Corridor edaptive active, corridor: {0}, edaptive config: {1}", corridorId, edaptiveConfigId)));
                                                    //_logger.LogInformation("Corridor timing plan active, corridor: {corridor}, edaptive config: {edaptive}", corridorId, edaptiveConfigId);
                                                }
                                                else
                                                {
                                                    await MakeCallWithAuthAsync((scheme, token) => edaptiveRepository.StopEdaptive(scheme, token, edaptiveConfigId!.Value), cancellationToken);
                                                    _logger.ExposeUserEvent(_userEventFactory.BuildUserEvent(EventLevel.Information, string.Format("Corridor edaptive stopped, corridor: {0}, edaptive config: {1}", corridorId, edaptiveConfigId)));
                                                    //_logger.LogInformation("Corridor timing plan stopped, corridor: {corridor}", corridorId);
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.ExposeUserEvent(_userEventFactory.BuildUserEvent(EventLevel.Error, string.Format("Could not change edaptive for corridor: {0}", corridorId)));
                                            // Still logging so stack trace and actual exception can be used.
                                            _logger.LogError(ex, "Could not change edaptive for corridor: {corridor}", corridorId);
                                            errors.Add(ex.Message);
                                        }

                                        var result = new WeatherResponsiveResult
                                        {
                                            ChangedSpeed = changeSpeed ?? false,
                                            ChangedTimingPlan = setTimingPlan ?? false,
                                            RanEdaptive = runEdaptive ?? false,
                                            EdaptiveConfigurationId = edaptiveConfigId,
                                            SpeedAdjustment = speedAdjustment,
                                            SpeedOverrideType = speedAdjustmentType,
                                            TimingPlan = timingPlan,
                                            Errors = string.Join(", ", errors),
                                            Timestamp = DateTime.UtcNow,
                                            CorridorId = corridorId,
                                            Confidence = status.Confidence,
                                            Precipitation = status.Precipitation,
                                            Temperature = status.SurfaceTemperature,
                                            RoadCondition = status.RoadCondition,
                                        };
                                        await weatherResponsiveRepository.SaveResponse(result, cancellationToken);

                                        _corridorCounter.Increment();
                                    }
                                    else
                                    {
                                        _logger.ExposeUserEvent(_userEventFactory.BuildUserEvent(EventLevel.Warning, string.Format("Corridor does not have external id to run commands: {0}", corridorId)));
                                        //_logger.LogWarning("Corridor does not have external id to run commands: {corridor}", corridorId);
                                    }
                                }
                                else
                                {
                                    _logger.ExposeUserEvent(_userEventFactory.BuildUserEvent(EventLevel.Warning, string.Format("No weather fusion status for corridor: {0}", corridorId)));
                                    //_logger.LogWarning("No weather fusion status for corridor: {corridor}", corridorId);
                                }
                            }, cancellationToken));
                        }

                        await Task.WhenAll(tasks);
                    }
                }

                _loopCounter.Increment();

                _logger.LogInformation("Completed responsive task");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Weather responsive failed");
            }
            finally
            {
                _semaphore.Release();
            }
        }
        else
        {
            _logger.LogWarning("Did not complete previous responsive step");
        }
    }

    /// <summary>
    /// Try to use previously obtained token if there is one, grab new one if expired or don't have one yet, then make a single attempt.
    /// </summary>
    /// <param name="call"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedAccessException"></exception>
    private async Task MakeCallWithAuthAsync(Func<string, string, Task> call, CancellationToken cancellationToken)
    {
        var attempted = false;
        var authorized = !string.IsNullOrWhiteSpace(_token);
        while (!attempted || !authorized)
        {
            try
            {
                if (!authorized)
                {
                    _token = await _tokenHandler.GetTokenAsync(cancellationToken);
                    if (string.IsNullOrWhiteSpace(_token))
                    {
                        throw new UnauthorizedAccessException("Unable to obtain token to talk to other services.");
                    }
                }

                var authScheme = "Bearer";
                await call(authScheme, _token!);
                break;
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == HttpStatusCode.Unauthorized && !attempted)
                {
                    authorized = false;
                    attempted = true;
                    await Task.Delay(100, cancellationToken);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
