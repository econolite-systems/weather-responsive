// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Persistence.Mongo.Context;
using Econolite.Ode.Persistence.Mongo.Repository;
using Econolite.Ode.Repository.Entities;
using Econolite.Ode.Repository.WeatherResponsive.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using NLog.Filters;

namespace Econolite.Ode.Repository.WeatherResponsive;

// This is simply a set of stubs that can be used to develop the Monitoring Service.
// There will need to some effort put into collecting the needed configuration for
// the monitoring service run with "real data"
public class WeatherResponsiveConfigRepository : DocumentRepositoryBase<WeatherResponsiveConfiguration, Guid>, IWeatherResponsiveConfigRepository
{
    private readonly IEntityRepository _entityRepo;
    
    public WeatherResponsiveConfigRepository(IMongoContext context, IEntityRepository entityRepo, ILogger<WeatherResponsiveConfigRepository> logger) : base(context, logger)
    {
        _entityRepo = entityRepo;
    }

    public async Task<(double Latitude, double Longitude)> GetCorridorLocationAsync(Guid corridorId, CancellationToken cancellationToken = default)
    {
        var corridorEntry = await _entityRepo.GetByIdAsync(corridorId);
        var childrenIds = corridorEntry?.Children.Select(c => c.Id).Distinct().ToArray() ?? Array.Empty<Guid>();
        var children = await _entityRepo.GetByIdsAsync(childrenIds);
        // Coordinates[1] = lat
        // Coordinates[0] = lon
        return children.Any() ? (children.Average(c => c.Geometry?.Point?.Coordinates?[1]) ?? 0, children.Average(c => c.Geometry?.Point?.Coordinates?[0] ?? 0)) : (0, 0);
    }

    public async Task<Guid[]> GetCorridorEssAsync(Guid corridorId, CancellationToken cancellationToken = default)
    {
        return (await _entityRepo.GetNodesByTypeAsync("Corridor")).Select(n => n.Id).ToArray();
    }

    public async Task<Guid[]> GetEnabledWeatherResponsiveCorridorIdsAsync(bool enabledOnly = true, CancellationToken cancellationToken = default)
    {
        return (await ExecuteDbSetFuncAsync(collection => collection.Find(x => x.IsEnabled || !enabledOnly).SortBy(c => c.Priority).Project(c => c.CorridorId).ToListAsync(cancellationToken))).ToArray();
    }

    public async Task<ICollection<WeatherResponsiveConfiguration>> GetConfigurations(Guid corridorId, params Guid[] ids)
    {
        FilterDefinition<WeatherResponsiveConfiguration> filter;
        if (ids?.Any() ?? false)
        {
            filter = Builders<WeatherResponsiveConfiguration>.Filter.Where(x => x.CorridorId == corridorId && ids.Contains(x.Id));
        }
        else
        {
            filter = Builders<WeatherResponsiveConfiguration>.Filter.Where(x => x.CorridorId == corridorId);
        }
        ICollection<WeatherResponsiveConfiguration> results = (await ExecuteDbSetFuncAsync(collection => collection.FindAsync(filter))).ToList();
        return results.OrderBy(c => c.Priority).ToList();
    }

    public async Task UpdateConfiguration(WeatherResponsiveConfiguration model)
    {
        if (model.Id == Guid.Empty)
        {
            model.Id = Guid.NewGuid();
            var others = await GetConfigurations(model.CorridorId);
            model.Priority = others.Any() ? others.Max(o => o.Priority) + 1 : 0;
            Add(model);
        }
        else
        {
            Update(model);
        }

        var (success, error) = await DbContext.SaveChangesAsync();
        if (!success)
        {
            _logger.LogError("Did not update configuration: {configId}, error: {error}", model?.Id, error);
        }
    }

    public async Task DeleteConfiguration(Guid configurationId)
    {
        Remove(configurationId);

        var (success, error) = await DbContext.SaveChangesAsync();
        if (!success)
        {
            _logger.LogError("Did not update configuration: {configId}, error: {error}", configurationId, error);
        }
    }

    public async Task UpdateConfigurationOrder(WeatherResponsiveConfigurationOrder model)
    {
        var filter = Builders<WeatherResponsiveConfiguration>.Filter.Where(x => x.CorridorId == model.CorridorId);
        var results = (await ExecuteDbSetFuncAsync(collection => collection.FindAsync(filter))).ToList();
        var idx = 0;
        foreach (var configId in model.ConfigurationOrder)
        {
            var match = results.FirstOrDefault(x => x.Id == configId);
            if (match != null)
            {
                match.Priority = idx;
                Update(match);
            }
            else
            {
                throw new IndexOutOfRangeException("Invalid config ID sent: " + configId);
            }

            idx++;
        }

        var (success, error) = await DbContext.SaveChangesAsync();
        if (!success)
        {
            _logger.LogError("Did not update orders for corridor: {corridor}, error: {error}", model?.CorridorId, error);
        }
    }

    public async Task<Dictionary<Guid, WeatherResponsiveConfiguration[]>> GetWeatherResponsiveCorridorConfigsAsync(CancellationToken cancellationToken = default)
    {
        var result = await ExecuteDbSetFuncAsync<List<WeatherResponsiveConfiguration>>(collection => collection.Find(x => x.IsEnabled).ToListAsync(cancellationToken));
        return result.GroupBy(c => c.CorridorId).ToDictionary(c => c.Key, c => c.OrderBy(_ => _.Priority).ToArray());
    }
}
