// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using Econolite.Ode.Repository.WeatherResponsive;
using Econolite.Ode.Repository.WeatherResponsive.Models;
using Econolite.Ode.Auditing.Extensions;
using Econolite.Ode.Auditing;

namespace Econolite.Ode.Services.WeatherResponsive;

[ApiController]
[Route("weather-responsive-speed")]
[AuthorizeOde(MoundRoadRole.ReadOnly)]
public class WeatherResponsiveSpeedController : ControllerBase
{
    private readonly IWeatherResponsiveSpeedRepository _speedRepository;
    private readonly ILogger _logger;
    private readonly IAuditCrudScopeFactory _auditCrudScopeFactory;
    private readonly string _auditEventType;

    public WeatherResponsiveSpeedController(IWeatherResponsiveSpeedRepository speedRepository, IAuditCrudScopeFactory auditCrudScopeFactory, ILogger<WeatherResponsiveSpeedController> logger)
    {
        _logger = logger;
        _speedRepository = speedRepository;
        _auditCrudScopeFactory = auditCrudScopeFactory;
        _auditEventType = SupportedAuditEventTypes.AuditEventTypes[AuditEventType.WeatherResponsive].Event;
    }

    // GET: edaptive-speed
    [HttpGet("", Name = "GetAll")]
    [ProducesResponseType(typeof(ICollection<CorridorSpeedOverride>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult> Home()
    {
        try
        {
            var auth = Request.Headers.Authorization[0]!.Split(" ");
            return Ok(await _speedRepository.GetAllCorridorSpeedOverrides(auth[0], auth[1]));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all corridor speed overrides");
            return Problem();
        }
    }

    // GET: edaptive-speed/5
    [HttpGet("{corridorId}", Name = "GetOne")]
    [ProducesResponseType(typeof(ICollection<CorridorSpeedOverride>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult> Get(Guid corridorId, [FromQuery] DateTime? date)
    {
        try
        {
            var auth = Request.Headers.Authorization[0]!.Split(" ");
            return Ok(await _speedRepository.GetCorridorSpeedOverridesForDate(auth[0], auth[1], corridorId, date));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting speed overrides for corridor {corridor}, date {date}", corridorId, date);
            return Problem();
        }
    }

    // POST: edaptive-speed/5
    [HttpPost("{corridorId}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult> Edit(long corridorId, [FromQuery] double speedAdjustment, [FromQuery] CorridorSpeedOverrideType speedType)
    {
        try
        {
            var scope = _auditCrudScopeFactory.CreateUpdateAsync(_auditEventType, () => new { corridorId, speedAdjustment, speedType });
            await using (await scope)
            {
                var auth = Request.Headers.Authorization[0]!.Split(" ");
                await _speedRepository.SetCorridorSpeedOverride(auth[0], auth[1], corridorId, speedAdjustment, speedType);
                return Ok();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving speed override for corridor {corridor}, speed {speed}, type {type}", corridorId, speedAdjustment, speedType);
            return Problem();
        }
    }

    // DELETE: edaptive-speed/5
    [HttpDelete("{corridorId}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult> Delete(long corridorId)
    {
        try
        {
            var scope = _auditCrudScopeFactory.CreateDeleteAsync(_auditEventType, () => corridorId);
            await using (await scope)
            {
                var auth = Request.Headers.Authorization[0]!.Split(" ");
                await _speedRepository.RemoveCorridorSpeedOverride(auth[0], auth[1], corridorId);
                return Ok();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing speed override for corridor {corridor}", corridorId);
            return Problem();
        }
    }
}
