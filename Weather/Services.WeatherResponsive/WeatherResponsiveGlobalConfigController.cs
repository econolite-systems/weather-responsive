// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Auditing;
using Econolite.Ode.Auditing.Extensions;
using Econolite.Ode.Authorization;
using Econolite.Ode.Repository.WeatherResponsive;
using Econolite.Ode.Repository.WeatherResponsive.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Econolite.Ode.Services.WeatherResponsive;

/// <summary>
/// WeatherResponsiveGlobalConfigController
/// </summary>
[ApiController]
[Route("weather-responsive-global-config")]
[AuthorizeOde(MoundRoadRole.ReadOnly)]
public class WeatherResponsiveGlobalConfigController : ControllerBase
{
    private readonly ILogger _logger;
    private readonly IAuditCrudScopeFactory _auditCrudScopeFactory;
    private readonly string _auditEventType;

    private readonly IWeatherResponsiveGlobalConfigRepository _globalConfigRepository;

    /// <summary>
    /// WeatherResponsiveGlobalConfigController
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="auditCrudScopeFactory"></param>
    /// <param name="globalConfigRepository"></param>
    public WeatherResponsiveGlobalConfigController(ILogger<WeatherResponsiveGlobalConfigController> logger, IAuditCrudScopeFactory auditCrudScopeFactory, IWeatherResponsiveGlobalConfigRepository globalConfigRepository)
    {
        _logger = logger;
        _auditCrudScopeFactory = auditCrudScopeFactory;
        _auditEventType = SupportedAuditEventTypes.AuditEventTypes[AuditEventType.WeatherResponsive].Event;

        _globalConfigRepository = globalConfigRepository;
    }

    /// <summary>
    /// Get the Weather Responsive Global Configuration
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(WeatherResponsiveGlobalConfig), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get()
    {
        try
        {
            var globalConfig = await _globalConfigRepository.GetGlobalConfig();
            if (globalConfig == null)
            {
                globalConfig = new WeatherResponsiveGlobalConfig
                {
                    Id = Guid.NewGuid(),
                    TimingPlanLogicFlagStates = new List<TimingPlanLogicFlagState>
                    {
                        new TimingPlanLogicFlagState { TimingPlan = 1, LogicFlag = 1, LogicFlagState = 0 },
                        new TimingPlanLogicFlagState { TimingPlan = 2, LogicFlag = 1, LogicFlagState = 0 },
                        new TimingPlanLogicFlagState { TimingPlan = 3, LogicFlag = 1, LogicFlagState = 0 },
                        new TimingPlanLogicFlagState { TimingPlan = 4, LogicFlag = 1, LogicFlagState = 0 }
                    }
                };
            }
            return Ok(globalConfig);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get the Weather Responsive Global Configuration");
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to get the Weather Responsive Global Configuration");
        }
    }

    /// <summary>
    /// Update the Weather Responsive Global Configuration
    /// </summary>
    /// <param name="globalConfig"></param>
    /// <returns></returns>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [AuthorizeOde(MoundRoadRole.Contributor)]
    public async Task<IActionResult> Update([FromBody] WeatherResponsiveGlobalConfig globalConfig)
    {
        try
        {
            var scope = _auditCrudScopeFactory.CreateUpdateAsync(_auditEventType, () => globalConfig);
            await using (await scope)
            {
                var gc = await _globalConfigRepository.GetGlobalConfig();
                if (gc == null)
                {
                    await _globalConfigRepository.CreateGlobalConfig(globalConfig);
                }
                else
                {
                    await _globalConfigRepository.UpdateGlobalConfig(globalConfig);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update the Weather Responsive Global Configuration");
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to update the Weather Responsive Global Configuration");
        }

        return Ok(globalConfig);
    }

    /// <summary>
    /// Create the Weather Responsive Global Configuration
    /// </summary>
    /// <param name="globalConfig"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [AuthorizeOde(MoundRoadRole.Contributor)]
    public async Task<IActionResult> Create([FromBody] WeatherResponsiveGlobalConfig globalConfig)
    {
        try
        {
            var scope = _auditCrudScopeFactory.CreateAddAsync(_auditEventType, () => globalConfig);
            await using (await scope)
            {
                var gc = await _globalConfigRepository.GetGlobalConfig();
                if (gc == null)
                {
                    await _globalConfigRepository.CreateGlobalConfig(globalConfig);
                }
                else
                {
                    await _globalConfigRepository.UpdateGlobalConfig(globalConfig);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create the Weather Responsive Global Configuration");
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create the Weather Responsive Global Configuration");
        }

        return Ok(globalConfig);
    }

    /// <summary>
    /// Delete the Weather Responsive Global Configuration
    /// </summary>
    /// <param name="globalConfigId"></param>
    /// <returns></returns>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [AuthorizeOde(MoundRoadRole.Administrator)]
    public async Task<IActionResult> Delete(WeatherResponsiveGlobalConfig globalConfig)
    {
        try
        {
            var scope = _auditCrudScopeFactory.CreateDeleteAsync(_auditEventType, () => globalConfig);
            await using (await scope)
            {
                await _globalConfigRepository.DeleteGlobalConfig(globalConfig);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete the Weather Responsive Global Configuration");
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to delete the Weather Responsive Global Configuration");
        }

        return Ok(globalConfig);
    }
}
