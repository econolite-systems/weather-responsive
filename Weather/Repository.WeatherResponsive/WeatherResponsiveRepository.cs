// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Models.Entities;
using Econolite.Ode.Persistence.Mongo.Context;
using Econolite.Ode.Persistence.Mongo.Repository;
using Econolite.Ode.Repository.Entities;
using Econolite.Ode.Repository.WeatherResponsive.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Econolite.Ode.Repository.WeatherResponsive;

public class WeatherResponsiveRepository : DocumentRepositoryBase<WeatherResponsiveResult, Guid>, IWeatherResponsiveRepository
{
    private readonly IEntityRepository _entityRepository;
    private readonly IWeatherResponsiveEdaptiveRepository _edaptiveRepository;
    private readonly IWeatherResponsiveConfigRepository _weatherResponsiveConfigRepository;

    public WeatherResponsiveRepository(IMongoContext context, IEntityRepository entityRepository, IWeatherResponsiveConfigRepository weatherResponsiveConfigRepository, IWeatherResponsiveEdaptiveRepository edaptiveRepository, ILogger<WeatherResponsiveRepository> logger) : base(context, logger)
    {
        _entityRepository = entityRepository;
        _weatherResponsiveConfigRepository = weatherResponsiveConfigRepository;
        _edaptiveRepository = edaptiveRepository;
    }

    public async Task<WeatherResponsiveResult?> GetLastResponse(Guid corridorId, CancellationToken cancellationToken)
    {
        return await ExecuteDbSetFuncAsync(collection => collection.Find(r => r.CorridorId == corridorId).SortByDescending(r => r.Timestamp).FirstOrDefaultAsync(cancellationToken));
    }

    public async Task SaveResponse(WeatherResponsiveResult result, CancellationToken cancellationToken)
    {
        result.Id = Guid.NewGuid();
        Add(result);

        var (success, error) = await DbContext.SaveChangesAsync(cancellationToken);
        if (!success)
        {
            _logger.LogError("Did not insert result for corridor: {corridor}, error: {error}", result.CorridorId, error);
        }
    }

    public async Task<ICollection<WeatherResponsiveResultModel>> Find(string authScheme, string authToken, List<Guid> corridorIds, DateTime startDate, DateTime? endDate)
    {
        var end = endDate ?? startDate.AddDays(1);
        var result = corridorIds.Any() ? await ExecuteDbSetFuncAsync(collection => collection.Find(w => corridorIds.Contains(w.CorridorId) && w.Timestamp >= startDate && w.Timestamp < end).SortByDescending(w => w.Timestamp).ToListAsync()) :
            await ExecuteDbSetFuncAsync(collection => collection.Find(w => w.Timestamp >= startDate && w.Timestamp < end).SortByDescending(w => w.Timestamp).ToListAsync());
        var foundCorridorIds = result.Select(r => r.CorridorId).Distinct().ToArray();
        var corridors = foundCorridorIds.Length > 0 ? await _entityRepository.GetByIdsAsync(foundCorridorIds) : Array.Empty<EntityNode>();
        var configurationIds = result.Where(r => r.EdaptiveConfigurationId.HasValue).Select(r => r.EdaptiveConfigurationId!.Value).Distinct().ToArray();
        var edaptiveConfigs = new Dictionary<int, string>();
        try
        {
            edaptiveConfigs = configurationIds.Length > 0 ? await _edaptiveRepository.GetConfigurationNames(authScheme, authToken, configurationIds) : new Dictionary<int, string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to pull Edaptive configuration map.");
        }
        return result?.Select(r => r.ToModel(corridors, edaptiveConfigs)).ToArray() ?? Array.Empty<WeatherResponsiveResultModel>();
    }

    public async Task<ICollection<WeatherResponsiveResultModel>> FindAllLatest(string authScheme, string authToken)
    {
        var enabledCorridors = await _weatherResponsiveConfigRepository.GetEnabledWeatherResponsiveCorridorIdsAsync(false);
        var corridors = await _entityRepository.GetByIdsAsync(enabledCorridors);
        var result = await ExecuteDbSetFuncAsync(collection => collection.Aggregate().Group(w => w.CorridorId, w => w.OrderByDescending(_ => _.Timestamp).First()).ToListAsync());
        var configurationIds = result.Where(r => r!.EdaptiveConfigurationId.HasValue).Select(r => r!.EdaptiveConfigurationId!.Value).Distinct().ToArray();
        var edaptiveConfigs = new Dictionary<int, string>();
        try
        {
            edaptiveConfigs = configurationIds.Length > 0 ? await _edaptiveRepository.GetConfigurationNames(authScheme, authToken, configurationIds) : new Dictionary<int, string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to pull Edaptive configuration map.");
        }
        var results = result?.Select(r => r.ToModel(corridors, edaptiveConfigs)).ToList() ?? new List<WeatherResponsiveResultModel>();
        foreach (var corridor in corridors)
        {
            if (!results.Any(r => r.CorridorId == corridor.Id))
            {
                results.Add(new WeatherResponsiveResultModel
                {
                    // Need unique IDs for table to render properly in UI
                    Id = Guid.NewGuid(),
                    CorridorId = corridor.Id,
                    CorridorName = corridor.Name,
                    Errors = "No data",
                });
            }
        }
        return results.OrderBy(r => r.CorridorName).ToArray();
    }
}
