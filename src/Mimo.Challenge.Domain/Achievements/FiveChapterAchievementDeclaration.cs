using Mimo.Challenge.Domain.Abstractions;
using Mimo.Challenge.Domain.Entities;

namespace Mimo.Challenge.Domain.Achievements;

public class FiveChapterAchievementDeclaration(
    IAchievementProgressRepository achievementProgressRepository) : IAchievementDeclaration
{
    private readonly ushort _achievementChaptersCount = 5;
    public uint AchievementId => 5;

    public string Description => "Complete 5 chapters";

    public async Task UpdateProgressAsync
        (UserProgressTracker userProgressTracker, CancellationToken ct = default)
    {
        var userId = userProgressTracker.UserId;
        
        var alreadyHasAchievement = await achievementProgressRepository
            .HasUserCompletedAchievementAsync(userId, AchievementId, ct);

        if (alreadyHasAchievement)
            return;
        
        float completedChaptersCount = userProgressTracker.TotalChaptersCompletedCount;
        
        var progressPercentage = 
            Convert.ToUInt16(completedChaptersCount / _achievementChaptersCount * 100);
        
        if(progressPercentage == 0 || userProgressTracker.LastCompletedChapterTimestamp is null)
            return;

        await achievementProgressRepository.UpdateAchievementProgressAsync(
            userId, 
            AchievementId, 
            userProgressTracker.LastCompletedChapterTimestamp.Value,
            progressPercentage > 100 ? (ushort) 100 : progressPercentage,
            ct);
    }
}