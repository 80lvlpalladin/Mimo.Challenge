using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Mimo.Challenge.Domain.Abstractions;
using Mimo.Challenge.Domain.Entities.Configuration;

namespace Mimo.Challenge.DAL.Repositories;

public sealed class AcademicUnitRepository : IAcademicUnitRepository
{
    private readonly MimoContext _context;
    private readonly TimeSpan _cacheExpiration;
    private readonly MemoryCache _memoryCache;

    // ReSharper disable once ConvertToPrimaryConstructor
    public AcademicUnitRepository(
        MimoContext context,
        MemoryCache memoryCache,
        IOptions<CacheExpirationHoursOptions> cacheExpirationOptions) 
    {
        _context = context;
        _cacheExpiration = TimeSpan.FromHours(cacheExpirationOptions.Value.AcademicUnit);
        _memoryCache = memoryCache;
    }

    private const string TopLevelUnitIdsCacheKey = "top_level_unit_ids";
    private static string CreateUnitChildrenIdsCacheKey(uint unitId) => $"unit_{unitId}_children";
    private static string CreateUnitPathCacheKey(uint unitId) => $"unit_{unitId}_path";
    
    public async ValueTask<string?> GetPathForUnitAsync(uint unitId, CancellationToken ct = default)
    {
        if (_memoryCache.TryGetValue(TopLevelUnitIdsCacheKey, out string? cachedPath))
            return cachedPath;
        
        var dbPath = await _context.AcademicUnits
            .Where(au => au.Id == unitId)
            .Select(au => au.HierarchyPath)
            .SingleAsync(ct);
        
        _memoryCache.Set(CreateUnitPathCacheKey(unitId), dbPath, _cacheExpiration);

        return dbPath;
    }
    
    
    public async ValueTask<uint[]> GetChildrenIdsAsync(uint? unitId = null, CancellationToken ct = default)
    {
        //top level unit = course
        var isTopLevelUnit = unitId is null;

        if (isTopLevelUnit)
        {
            if (_memoryCache.TryGetValue(TopLevelUnitIdsCacheKey, out uint[]? cachedTopLevelUnitIds) && 
                cachedTopLevelUnitIds != null)
                return cachedTopLevelUnitIds;

            var dbTopLevelUnitIds = await _context.AcademicUnits
                .Where(au => au.HierarchyPath == null)
                .Select(au => au.Id)
                .ToArrayAsync(ct);
            
            _memoryCache.Set(TopLevelUnitIdsCacheKey, dbTopLevelUnitIds, _cacheExpiration);

            return dbTopLevelUnitIds;
        }

        var unitChildrenIdsCacheKey = CreateUnitChildrenIdsCacheKey(unitId!.Value);
            
        if (_memoryCache.TryGetValue(unitChildrenIdsCacheKey, out uint[]? unitChildrenIds) && 
            unitChildrenIds != null)
            return unitChildrenIds;
            
        var unitPath = await _context.AcademicUnits
            .Where(au => au.Id == unitId)
            .Select(au => au.HierarchyPath)
            .SingleAsync(ct);

        var expectedChildrenPath = unitPath is null ? 
            unitId.ToString() : 
            unitPath + "." + unitId;

        var dbUnitChildrenIds =
            await _context.AcademicUnits
                .Where(au => au.HierarchyPath == expectedChildrenPath)
                .Select(au => au.Id)
                .ToArrayAsync(ct);
        
        if(dbUnitChildrenIds.Length == 0)
            return dbUnitChildrenIds;
            
        _memoryCache.Set(unitChildrenIdsCacheKey, dbUnitChildrenIds, _cacheExpiration);

        return dbUnitChildrenIds;
    }

    public async ValueTask<bool> CheckIfUnitExistsAsync(uint unitId, CancellationToken cancellationToken)
    {
        if (_memoryCache.TryGetValue(CreateUnitPathCacheKey(unitId), out string? _))
            return true;
        
        return await _context.AcademicUnits
            .AnyAsync(u => u.Id == unitId, cancellationToken);
    }
}