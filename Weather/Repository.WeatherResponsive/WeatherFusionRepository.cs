// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Models.Entities;
using Econolite.Ode.Persistence.Mongo.Context;
using Econolite.Ode.Persistence.Mongo.Repository;
using Econolite.Ode.Repository.Entities;
using Econolite.Ode.Repository.WeatherResponsive.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Weather.Fusion;

namespace Econolite.Ode.Repository.WeatherResponsive;

public class WeatherFusionRepository : DocumentRepositoryBase<WeatherFusionDocument, Guid>, IWeatherFusionRepository
{
    private readonly IEntityRepository _entityRepository;

    public WeatherFusionRepository(IMongoContext mongoContext, IEntityRepository entityRepository, ILogger<WeatherFusionRepository> logger) : base(mongoContext, logger)
    {
        _entityRepository = entityRepository;
    }

    public async Task PutCorridorWeatherAsync(Guid corridorId, CorridorStatus corridorStatus, CancellationToken cancellationToken = default)
    {
        var dbModel = WeatherFusionDocument.ToDb(corridorStatus, corridorId);
        Add(dbModel);

        var (success, error) = await DbContext.SaveChangesAsync(cancellationToken);
        if (!success)
        {
            _logger.LogError("Did not save status for corridor: {corridorId}, status: {@status}, error: {error}", corridorId, dbModel, error);
        }
    }

    public async Task<WeatherFusionDocument?> GetCorridorWeatherAsync(Guid corridorId, CancellationToken cancellationToken = default)
    {
        var result = await ExecuteDbSetFuncAsync(collection => collection.Find(w => w.CorridorId == corridorId).SortByDescending(w => w.Timestamp).FirstOrDefaultAsync(cancellationToken));
        return result;
    }

    public async Task<WeatherFusionResultModel[]> Find(List<Guid> corridorIds, DateTime startDate, DateTime? endDate)
    {
        var end = endDate ?? startDate.AddDays(1);
        var result = corridorIds.Any() ? await ExecuteDbSetFuncAsync(collection => collection.Find(w => corridorIds.Contains(w.CorridorId) && w.Timestamp >= startDate && w.Timestamp < end).SortByDescending(w => w.Timestamp).ToListAsync()) :
            await ExecuteDbSetFuncAsync(collection => collection.Find(w => w.Timestamp >= startDate && w.Timestamp < end).SortByDescending(w => w.Timestamp).ToListAsync());
        var foundCorridorIds = result.Select(r => r.CorridorId).Distinct().ToArray();
        var corridors = Array.Empty<EntityNode>();
        try
        {
            corridors = foundCorridorIds.Length > 0 ? (await _entityRepository.GetByIdsAsync(foundCorridorIds)).ToArray() : Array.Empty<EntityNode>();
            if (!corridors.Any())
                _logger.LogWarning("No corridors found from entities.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to retrieve corridors.");
        }
        return result?.Select(r => r.ToModel(corridors)).ToArray() ?? Array.Empty<WeatherFusionResultModel>();
    }
}
