using Mimo.Challenge.Domain.Abstractions;
using Mimo.Challenge.Domain.Entities;

namespace Mimo.Challenge.Domain.Achievements;

public class CSharpCourseAchievementDeclaration(
    IAchievementProgressRepository achievementProgressRepository) : IAchievementDeclaration
{
    private readonly uint _courseId = 8;
    public uint AchievementId => 6;
    public string Description => "Complete the C# course";

    public async Task UpdateProgressAsync
        (UserProgressTracker userProgressTracker,  CancellationToken ct = default)
    {
        var userId = userProgressTracker.UserId;
        
        var alreadyHasAchievement = await achievementProgressRepository
            .HasUserCompletedAchievementAsync(userId, AchievementId, ct);

        if (alreadyHasAchievement)
            return;

        var courseProgressTracker = userProgressTracker[_courseId];
        
        var progress = courseProgressTracker.PercentageCompleted;
        
        if (progress == 0 || courseProgressTracker.LastCompletedChapterTimestamp is null)
            return;

        await achievementProgressRepository.UpdateAchievementProgressAsync(
            userId, 
            AchievementId, 
            courseProgressTracker.LastCompletedChapterTimestamp.Value, 
            progress, 
            ct);
    }
}