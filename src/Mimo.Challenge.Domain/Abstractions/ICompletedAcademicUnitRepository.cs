using Mimo.Challenge.Domain.Entities;

namespace Mimo.Challenge.Domain.Abstractions;

public interface ICompletedAcademicUnitRepository
{
    /// <returns>Full map of user's progress in object hierarchy</returns>
    public Task<UserProgressTracker> RegisterCompletedLessonAsync(
        uint userId,
        uint lessonId,
        uint startedTimestamp,
        uint completedTimestamp,
        CancellationToken ct = default);
    
    public Task<int> GetCompletedLessonsCountAsync(uint userId, CancellationToken ct = default);
    
    public Task<int> GetCompletedChaptersCountAsync(uint userId, CancellationToken ct = default);
    
    public Task<bool> HasUserCompletedCourseAsync(uint userId, uint courseId, CancellationToken ct = default);
}