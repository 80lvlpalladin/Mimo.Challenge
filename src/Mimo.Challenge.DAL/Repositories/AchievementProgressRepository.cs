using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Mimo.Challenge.DAL.Entities;
using Mimo.Challenge.Domain.Abstractions;
using Mimo.Challenge.Domain.Entities.Configuration;

namespace Mimo.Challenge.DAL.Repositories;

public class AchievementProgressRepository : IAchievementProgressRepository
{
    private readonly MimoContext _context;
    private readonly TimeSpan _cacheExpiration;
    private readonly MemoryCache _memoryCache;
    private string CreateUserAchievementCacheKey(uint userId, uint achievementId) => 
        $"{userId}_{achievementId}";

    // ReSharper disable once ConvertToPrimaryConstructor
    public AchievementProgressRepository(
        MimoContext context,
        MemoryCache memoryCache,
        IOptions<CacheExpirationHoursOptions> cacheExpirationHoursOptions) 
    {
        _context = context;
        _cacheExpiration = TimeSpan.FromHours(cacheExpirationHoursOptions.Value.AchievementProgress);
        _memoryCache = memoryCache;
    }

    public async Task UpdateAchievementProgressAsync(
        uint userId, 
        uint achievementId, 
        uint timeStamp, 
        ushort percentage,
        CancellationToken ct = default)
    {
        var achievementProgress = 
            await _context.AchievementProgress.FindAsync([userId, achievementId], ct);
        
        if (achievementProgress is null)
        {
            await _context.AchievementProgress.AddAsync(
                new AchievementProgress
                    (userId, achievementId, percentage, timeStamp), 
                ct);

            return;
        }

        if (achievementProgress.PercentCompleted < percentage)
        {
            achievementProgress.PercentCompleted = percentage;
            achievementProgress.LastUpdatedTimestamp = timeStamp;
        }
    }

    public async ValueTask<bool> HasUserCompletedAchievementAsync
        (uint userId, uint achievementId, CancellationToken ct = default)
    {
        var cacheKey = CreateUserAchievementCacheKey(userId, achievementId);
        
        if(_memoryCache.TryGetValue(cacheKey, out _))
        {
            return true;
        }
        
        var dbResult = await _context.AchievementProgress.AnyAsync(
            x => 
                x.UserId == userId && 
                x.AchievementId == achievementId &&
                x.PercentCompleted == 100, ct);
        
        if (dbResult)
            _memoryCache.Set(cacheKey, true, _cacheExpiration);

        return dbResult;
    }

    public Task<Dictionary<uint, ushort>> GetUserAchievementProgressAsync
        (uint userId, CancellationToken ct = default)
    {
        return _context.AchievementProgress
            .Where(x => x.UserId == userId)
            .ToDictionaryAsync(x => x.AchievementId, x => x.PercentCompleted, ct);
    }
}