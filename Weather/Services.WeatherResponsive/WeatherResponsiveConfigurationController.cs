// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Auditing;
using Econolite.Ode.Auditing.Extensions;
using Econolite.Ode.Authorization;
using Econolite.Ode.Repository.WeatherResponsive;
using Econolite.Ode.Repository.WeatherResponsive.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Econolite.Ode.Services.WeatherResponsive;

[ApiController]
[Route("weather-responsive-config")]
[AuthorizeOde(MoundRoadRole.ReadOnly)]
public class WeatherResponsiveConfigurationController : ControllerBase
{
    private readonly IWeatherResponsiveConfigRepository _configRepository;
    private readonly ILogger _logger;
    private readonly IAuditCrudScopeFactory _auditCrudScopeFactory;
    private readonly string _auditEventType;

    public WeatherResponsiveConfigurationController(IWeatherResponsiveConfigRepository configRepository, IAuditCrudScopeFactory auditCrudScopeFactory, ILogger<WeatherResponsiveConfigurationController> logger)
    {
        _configRepository = configRepository;
        _logger = logger;
        _auditCrudScopeFactory = auditCrudScopeFactory;
        _auditEventType = SupportedAuditEventTypes.AuditEventTypes[AuditEventType.WeatherResponsive].Event;
    }

    // GET: weather-responsive-config/{corridorId}
    [HttpGet("{corridorId}")]
    [ProducesResponseType(typeof(ICollection<WeatherResponsiveConfiguration>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult> Get(Guid corridorId)
    {
        try
        {
            return Ok(await _configRepository.GetConfigurations(corridorId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all weather responsive configurations for corridor {corridorId}", corridorId);
            return Problem();
        }
    }

    // GET: weather-responsive-config/{corridorId}/{configId}
    [HttpGet("{corridorId}/{configId}")]
    [ProducesResponseType(typeof(WeatherResponsiveConfiguration), (int)HttpStatusCode.OK)]
    public async Task<ActionResult> Get(Guid corridorId, Guid configId)
    {
        try
        {
            return Ok((await _configRepository.GetConfigurations(corridorId, configId)).FirstOrDefault());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting weather responsive config for corridor: {corridorId}, config: {configId}", corridorId, configId);
            return Problem();
        }
    }

    // PUT: weather-responsive-config
    [HttpPut]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [AuthorizeOde(MoundRoadRole.Contributor)]
    public async Task<ActionResult> Update([FromBody] WeatherResponsiveConfiguration model)
    {
        try
        {
            var scope = model.Id == Guid.Empty ? _auditCrudScopeFactory.CreateAddAsync(_auditEventType, () => model) : _auditCrudScopeFactory.CreateUpdateAsync(_auditEventType, () => model);
            await using (await scope)
            {
                await _configRepository.UpdateConfiguration(model);
                return Ok();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating weather responsive config {@model}", model);
            return Problem();
        }
    }

    // POST: weather-responsive-config
    [HttpPost]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [AuthorizeOde(MoundRoadRole.Contributor)]
    public async Task<ActionResult> Create([FromBody] WeatherResponsiveConfiguration model)
    {
        // Audit handled in Update()
        model.Id = Guid.Empty;
        return await Update(model);
    }

    // Delete: weather-responsive-config/{configId}
    [HttpDelete("{configId}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [AuthorizeOde(MoundRoadRole.Administrator)]
    public async Task<ActionResult> Delete(Guid configId)
    {
        try
        {
            var scope = _auditCrudScopeFactory.CreateDeleteAsync(_auditEventType, configId.ToString);
            await using (await scope)
            {
                await _configRepository.DeleteConfiguration(configId);
                return Ok();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting weather responsive config {configId}", configId);
            return Problem();
        }
    }

    // PUT: weather-responsive-config/order
    [HttpPut("order")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [AuthorizeOde(MoundRoadRole.Contributor)]
    public async Task<ActionResult> UpdateOrder([FromBody] WeatherResponsiveConfigurationOrder model)
    {
        try
        {
            var scope = _auditCrudScopeFactory.CreateUpdateAsync(_auditEventType, () => model);
            await using (await scope)
            {
                await _configRepository.UpdateConfigurationOrder(model);
                return Ok();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating weather responsive config {@model}", model);
            return Problem();
        }
    }
}
