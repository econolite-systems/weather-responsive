// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Authorization;
using Econolite.Ode.Repository.WeatherResponsive;
using Econolite.Ode.Repository.WeatherResponsive.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Econolite.Ode.Services.WeatherResponsive;

[ApiController]
[Route("weather-responsive-status")]
[AuthorizeOde(MoundRoadRole.ReadOnly)]
public class WeatherResponsiveStatusController : ControllerBase
{
    private readonly IWeatherResponsiveRepository _weatherResponsiveService;
    private readonly IWeatherFusionRepository _weatherFusionRepository;

    /// <summary>
    /// Constructs a Weather Responsive Status controller
    /// </summary>
    public WeatherResponsiveStatusController(IWeatherResponsiveRepository weatherResponsiveService, IWeatherFusionRepository weatherFusionRepository)
    {
        _weatherResponsiveService = weatherResponsiveService;
        _weatherFusionRepository = weatherFusionRepository;
    }

    /// <summary>
    /// Finds corridors with weather responsive statuses matching the given query parameters
    /// </summary>
    /// <remarks>
    /// The start date is mandatory, but the end date is optional. If no end date is given, all status entries with a
    /// timestamp from the start date up to the latest will be returned. If an end date is provided, only status entries
    /// within the date range will be returned.
    ///
    /// If no corridor ID parameters are given, then the query will *not* filter on any corridor IDs, so statuses for
    /// any corridor will be returned. The corridor ID parameter may be provided multiple times to filter on multiple
    /// corridor IDs. If any corridor IDs are given, only statuses for the given corridor IDs will be returned.
    /// </remarks>
    /// <param name="corridorIds">Optional corridor IDs to filter on</param>
    /// <param name="startDate">Required start date</param>
    /// <param name="endDate">Optional end date</param>
    /// <response code="200">Returns a list of corridor status entries matching the given query parameters</response>
    [HttpGet("find")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<WeatherResponsiveResultModel>))]
    public async Task<IActionResult> Find([FromQuery] IEnumerable<Guid>? corridorIds, [BindRequired] DateTime startDate, DateTime? endDate)
    {
        var auth = Request.Headers.Authorization[0]!.Split(" ");
        return Ok(await _weatherResponsiveService.Find(auth[0], auth[1], corridorIds?.ToList() ?? new List<Guid>(), startDate, endDate));
    }

    /// <summary>
    /// Returns the latest weather responsive status entries for all corridors
    /// </summary>
    /// <response code="200">Returns a list of the latest weather responsive statuses for all corridors</response>
    [HttpGet("latest/all")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<WeatherResponsiveResultModel>))]
    public async Task<IActionResult> FindAllLatest()
    {
        var auth = Request.Headers.Authorization[0]!.Split(" ");
        return Ok(await _weatherResponsiveService.FindAllLatest(auth[0], auth[1]));
    }

    /// <summary>
    /// Finds corridors with weather responsive fusion statuses matching the given query parameters
    /// </summary>
    /// <remarks>
    /// The start date is mandatory, but the end date is optional. If no end date is given, all status entries with a
    /// timestamp from the start date up to the latest will be returned. If an end date is provided, only status entries
    /// within the date range will be returned.
    ///
    /// If no corridor ID parameters are given, then the query will *not* filter on any corridor IDs, so statuses for
    /// any corridor will be returned. The corridor ID parameter may be provided multiple times to filter on multiple
    /// corridor IDs. If any corridor IDs are given, only statuses for the given corridor IDs will be returned.
    /// </remarks>
    /// <param name="corridorIds">Optional corridor IDs to filter on</param>
    /// <param name="startDate">Required start date</param>
    /// <param name="endDate">Optional end date</param>
    /// <response code="200">Returns a list of corridor status entries matching the given query parameters</response>
    [HttpGet("fusion")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WeatherFusionResultModel[]))]
    public async Task<IActionResult> Fusion([FromQuery] IEnumerable<Guid>? corridorIds, [BindRequired] DateTime startDate, DateTime? endDate)
    {
        return Ok(await _weatherFusionRepository.Find(corridorIds?.ToList() ?? new List<Guid>(), startDate, endDate));
    }
}
