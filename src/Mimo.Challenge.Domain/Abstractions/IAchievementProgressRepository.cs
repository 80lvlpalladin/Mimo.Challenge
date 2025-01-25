namespace Mimo.Challenge.Domain.Abstractions;

public interface IAchievementProgressRepository
{
    public Task UpdateAchievementProgressAsync(
        uint userId, 
        uint achievementId, 
        uint timeStamp, 
        ushort percentage, 
        CancellationToken ct = default);

    public ValueTask<bool> HasUserCompletedAchievementAsync
        (uint userId, uint achievementId, CancellationToken ct = default);
    
    public Task<Dictionary<uint, ushort>> GetUserAchievementProgressAsync
        (uint userId, CancellationToken ct = default);
}