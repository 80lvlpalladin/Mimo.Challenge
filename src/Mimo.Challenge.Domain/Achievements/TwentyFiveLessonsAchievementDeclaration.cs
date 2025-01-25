using Mimo.Challenge.Domain.Abstractions;
using Mimo.Challenge.Domain.Entities;

namespace Mimo.Challenge.Domain.Achievements;

public class TwentyFiveLessonsAchievementDeclaration(
    IAchievementProgressRepository achievementProgressRepository) : IAchievementDeclaration
{
    private readonly int _achievementLessonsCount = 25;
    public uint AchievementId => 2;

    public string Description => "Complete 25 lessons";

    public async Task UpdateProgressAsync
        (UserProgressTracker userProgressTracker, CancellationToken ct = default)
    {
        var userId = userProgressTracker.UserId;
        
        var alreadyCompletedAchievement = 
            await achievementProgressRepository.HasUserCompletedAchievementAsync(userId, AchievementId, ct);

        if (alreadyCompletedAchievement)
            return;
        
        float userCompletedLessonsCount = userProgressTracker.TotalLessonsCompletedCount;
        
        var progressPercentage = 
            Convert.ToUInt16(userCompletedLessonsCount / _achievementLessonsCount * 100);
        
        if(progressPercentage == 0 || userProgressTracker.LastCompletedLessonTimestamp is null)
            return;

        await achievementProgressRepository.UpdateAchievementProgressAsync(
            userId, 
            AchievementId, 
            userProgressTracker.LastCompletedLessonTimestamp.Value,
            progressPercentage > 100 ? (ushort) 100 : progressPercentage,
            ct);
    }
}