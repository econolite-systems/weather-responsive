// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Persistence.Mongo.Context;
using Econolite.Ode.Persistence.Mongo.Repository;
using Econolite.Ode.Repository.WeatherResponsive.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Econolite.Ode.Repository.WeatherResponsive;

/// <summary>
/// WeatherResponsiveGlobalConfigRepository
/// </summary>
public class WeatherResponsiveGlobalConfigRepository : DocumentRepositoryBase<WeatherResponsiveGlobalConfig, Guid>, IWeatherResponsiveGlobalConfigRepository
{
    /// <summary>
    /// WeatherResponsiveGlobalConfigRepository
    /// </summary>
    /// <param name="context"></param>
    /// <param name="logger"></param>
    public WeatherResponsiveGlobalConfigRepository(IMongoContext context, ILogger<WeatherResponsiveGlobalConfigRepository> logger) : base(context, logger)
    {
    }

    /// <summary>
    /// GetGlobalConfig
    /// </summary>
    /// <returns></returns>
    public async Task<WeatherResponsiveGlobalConfig?> GetGlobalConfig()
    {
        try
        {
            return (await GetAllAsync()).SingleOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get the Weather Responsive Global Configuration");
            throw;
        }
    }

    /// <summary>
    /// UpdateGlobalConfig
    /// </summary>
    /// <param name="globalConfig"></param>
    public async Task UpdateGlobalConfig(WeatherResponsiveGlobalConfig globalConfig)
    {
        try
        {
            Update(globalConfig);

            await DbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update the Weather Responsive Global Configuration");
            throw;
        }
    }

    /// <summary>
    /// CreateGlobalConfig
    /// </summary>
    /// <param name="globalConfig"></param>
    /// <returns></returns>
    public async Task CreateGlobalConfig(WeatherResponsiveGlobalConfig globalConfig)
    {
        try
        {
            Add(globalConfig);

            await DbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create the Weather Responsive Global Configuration");
            throw;
        }
    }

    /// <summary>
    /// DeleteGlobalConfig
    /// </summary>
    /// <param name="globalConfig"></param>
    public async Task DeleteGlobalConfig(WeatherResponsiveGlobalConfig globalConfig)
    {
        try
        {
            Remove(globalConfig.Id);

            await DbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete the Weather Responsive Global Configuration");
            throw;
        }
    }
}
