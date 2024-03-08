// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Authorization;
using Econolite.Ode.Repository.WeatherResponsive;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using Econolite.Ode.Auditing;
using Econolite.Ode.Repository.WeatherResponsive.Models;
using Econolite.Ode.Auditing.Extensions;

namespace Econolite.Ode.Services.WeatherResponsive;

[ApiController]
[Route("weather-responsive-edaptive")]
[AuthorizeOde(MoundRoadRole.ReadOnly)]
public class WeatherResponsiveEdaptiveController : ControllerBase
{
    private readonly IWeatherResponsiveEdaptiveRepository _edaptiveRepository;
    private readonly ILogger _logger;
    private readonly IAuditCrudScopeFactory _auditCrudScopeFactory;
    private readonly string _auditEventType;

    public WeatherResponsiveEdaptiveController(IWeatherResponsiveEdaptiveRepository edaptiveRepository, IAuditCrudScopeFactory auditCrudScopeFactory, ILogger<WeatherResponsiveEdaptiveController> logger)
    {
        _logger = logger;
        _edaptiveRepository = edaptiveRepository;
        _auditCrudScopeFactory = auditCrudScopeFactory;
        _auditEventType = SupportedAuditEventTypes.AuditEventTypes[AuditEventType.WeatherResponsive].Event;
    }
        
    // GET: edaptive-control
    [HttpGet("")]
    [ProducesResponseType(typeof(AlgorithmResultsSummaryModel[]), (int)HttpStatusCode.OK)]
    public async Task<ActionResult> GetResultsSummary()
    {
        // Essentially a copy from the EdaptiveRoutesController, but split in case want to modify the model at all such as simplifying it.
        try
        {
            var auth = Request.Headers.Authorization[0]!.Split(" ");
            var result = await _edaptiveRepository.GetResultsSummaryAsync(auth[0], auth[1]);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Edaptive results summary");
            return StatusCode(500);
        }
    }

    // GET: edaptive-control/5/2023-05-25
    [HttpGet("{algorithmId}/{date}")]
    [ProducesResponseType(typeof(AlgorithmResultsModel), (int)HttpStatusCode.OK)]
    public async Task<ActionResult> Results(int algorithmId, DateTime? date, int severity = 7, int pageIndex = 0, int pageSize = 25)
    {
        // Essentially a copy from the EdaptiveRoutesController, but split in case want to modify the model at all such as simplifying it.
        try
        {
            var auth = Request.Headers.Authorization[0]!.Split(" ");
            var result = await _edaptiveRepository.GetResultsAsync(auth[0], auth[1], algorithmId, date, severity, pageIndex, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Edaptive results for algorithm: {algorithm}, date: {date}", algorithmId, date);
            return StatusCode(500);
        }
    }

    // GET: edaptive-control/5
    [HttpGet("configurations/{corridorId}")]
    [ProducesResponseType(typeof(AlgorithmConfigurationSlimModel[]), (int)HttpStatusCode.OK)]
    public async Task<ActionResult> GetConfigurationsPerCorridor(Guid corridorId)
    {
        try
        {
            var auth = Request.Headers.Authorization[0]!.Split(" ");
            var result = await _edaptiveRepository.GetConfigurationsPerCorridorAsync(auth[0], auth[1], corridorId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Edaptive configuration per corridor");
            return StatusCode(500);
        }
    }

    // GET: edaptive-control/algorithm-for-configuration/5
    [HttpGet("algorithm-for-configuration/{configurationId}")]
    [ProducesResponseType(typeof(AlgorithmConfigurationSlimModel[]), (int)HttpStatusCode.OK)]
    public async Task<ActionResult> GetConfigurationsPerCorridor(int configurationId)
    {
        try
        {
            var auth = Request.Headers.Authorization[0]!.Split(" ");
            var result = await _edaptiveRepository.GetAlgorithmIdFromConfigurationIdAsync(auth[0], auth[1], configurationId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Edaptive configuration per corridor");
            return StatusCode(500);
        }
    }

    // POST: edaptive-control/start/5
    [HttpPost("start/{configurationId}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult> StartEdaptive(int configurationId)
    {
        try
        {
            var scope = _auditCrudScopeFactory.CreateUpdateAsync(_auditEventType, () => configurationId);
            await using (await scope)
            {
                var auth = Request.Headers.Authorization[0]!.Split(" ");
                await _edaptiveRepository.StartEdaptive(auth[0], auth[1], configurationId);
                return Ok();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting Edaptive configuration {configuration}", configurationId);
            return StatusCode(500);
        }
    }

    // POST: edaptive-control/stop/5
    [HttpPost("stop/{configurationId}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult> StopEdaptive(int configurationId)
    {
        try
        {
            var scope = _auditCrudScopeFactory.CreateUpdateAsync(_auditEventType, () => configurationId);
            await using (await scope)
            {
                var auth = Request.Headers.Authorization[0]!.Split(" ");
                await _edaptiveRepository.StopEdaptive(auth[0], auth[1], configurationId);
                return Ok();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping Edaptive configuration {configuration}", configurationId);
            return StatusCode(500);
        }
    }
}
