// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Authorization;
using Econolite.Ode.Repository.WeatherResponsive;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using Econolite.Ode.Repository.Entities;
using Econolite.Ode.Auditing.Extensions;
using Econolite.Ode.Auditing;

namespace Econolite.Ode.Services.WeatherResponsive;

[ApiController]
[Route("weather-responsive-timing-plans")]
[AuthorizeOde(MoundRoadRole.ReadOnly)]
public class WeatherResponsiveTimingPlanController : ControllerBase
{
    private readonly IWeatherResponsiveTimingPlanRepository _timingPlanRepository;
    private readonly IEntityRepository _entityRepository;
    private readonly ILogger _logger;
    private readonly IAuditCrudScopeFactory _auditCrudScopeFactory;
    private readonly string _auditEventType;

    public WeatherResponsiveTimingPlanController(IWeatherResponsiveTimingPlanRepository timingPlanRepository, IEntityRepository entityRepository, IAuditCrudScopeFactory auditCrudScopeFactory, ILogger<WeatherResponsiveTimingPlanController> logger)
    {
        _logger = logger;
        _timingPlanRepository = timingPlanRepository;
        _entityRepository = entityRepository;
        _auditCrudScopeFactory = auditCrudScopeFactory;
        _auditEventType = SupportedAuditEventTypes.AuditEventTypes[AuditEventType.WeatherResponsive].Event;
    }
        
    // POST: external-device-control/set-timing-plan/00001111-2222-3333-4444-555566667777?timingPlan=1&logicFlag=51&logicFlagState=2
    [HttpPost("set-timing-plan/{corridorId}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult> SetTimingPlan(Guid corridorId, [FromQuery] int timingPlan, [FromQuery] int logicFlag, [FromQuery] int logicFlagState)
    {
        try
        {
            var scope = _auditCrudScopeFactory.CreateUpdateAsync(_auditEventType, () => new { corridorId, timingPlan, logicFlag, logicFlagState });
            await using (await scope)
            {
                var auth = Request.Headers.Authorization[0]!.Split(" ");
                var corridor = await _entityRepository.GetByIdAsync(corridorId);
                var corridorNumber = long.Parse(corridor!.ExternalId!);
                await _timingPlanRepository.SetTimingPlan(auth[0], auth[1], corridorNumber, timingPlan, logicFlag, logicFlagState);

                return Ok();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending out command to set timing plan.");
            return Problem();
        }
    }
}
