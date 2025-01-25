using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Mimo.Challenge.Domain.Abstractions;
using Mimo.Challenge.Domain.Entities;
using Mimo.Challenge.Domain.Entities.Configuration;

namespace Mimo.Challenge.Domain.Services;

public class UserProgressTrackerFactory
{
    private readonly IAcademicUnitRepository _academicUnitRepository;
    private readonly TimeSpan _cacheExpiration;
    private readonly IMemoryCache _memoryCache;

    // ReSharper disable once ConvertToPrimaryConstructor
    public UserProgressTrackerFactory(
        IAcademicUnitRepository academicUnitRepository,
        MemoryCache memoryCache,
        IOptions<CacheExpirationHoursOptions> cacheExpirationHoursOptions) 
    {
        _academicUnitRepository = academicUnitRepository;
        _cacheExpiration = TimeSpan.FromHours(cacheExpirationHoursOptions.Value.AcademicUnit);
        _memoryCache = memoryCache;
    }

    private const string ProgressTrackerCacheKey = "progress_tracker";

    public async ValueTask<UserProgressTracker> BuildAsync(uint userId, CancellationToken ct = default)
    {
        if(_memoryCache.TryGetValue(ProgressTrackerCacheKey, out UserProgressTracker? cachedProgressTracker) && 
           cachedProgressTracker != null)
            return cachedProgressTracker;
        
        var allCourseIds =
            await _academicUnitRepository.GetChildrenIdsAsync(null, ct);
        
        var allProgressTracker = new UserProgressTracker(userId);
        
        foreach (var courseId in allCourseIds)
        {
            var chapterIds = await _academicUnitRepository.GetChildrenIdsAsync(courseId, ct);
            
            if(chapterIds.Length == 0)
                throw new Exception($"Course {courseId} has no chapters");
            
            var courseProgressTracker = new CourseProgressTracker();
            foreach (var chapterId in chapterIds)
            {
                var lessonIds = await _academicUnitRepository.GetChildrenIdsAsync(chapterId, ct);
                
                if(lessonIds.Length == 0)
                    throw new Exception($"Chapter {chapterId} has no lessons");

                var chapterProgressTracker = new ChapterProgressTracker(lessonIds);
                courseProgressTracker[chapterId] = chapterProgressTracker;
            }

            allProgressTracker[courseId] = courseProgressTracker;
        }
        
        _memoryCache.Set(ProgressTrackerCacheKey, allProgressTracker, _cacheExpiration);

        return allProgressTracker;
    }
}