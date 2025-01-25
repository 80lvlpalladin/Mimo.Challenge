using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Mimo.Challenge.DAL.Repositories;
using Mimo.Challenge.Domain.Entities.Configuration;
using Mimo.Challenge.Domain.Services;

namespace Mimo.Challenge.IntegrationTests.Domain.Entities;

public class UserProgressTrackerTests : SqliteDatabaseIntegrationTests
{
    [Test]
    [NotInParallel]
    public async Task WhenAllChaptersInCourseMarkedCompleted_WholeUserProgressTree_ShouldChangeAccordingly()
    {
        // Arrange
        const uint userId = 1;
        const uint courseId = 1;
        
        var lessonsCompleted = 
            new List<(uint lessonId, uint chapterId, uint startedTimestamp, uint completedTimestamp)>
            {
                (3, 2, 1, 2),
                (4, 2, 3, 4),
                (5, 2, 5, 6),
                (7, 6, 7, 8)
            };
        var cacheExpirationOptions = Options.Create(new CacheExpirationHoursOptions());
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var context = CreateDbContext();
        var academicUnitRepository = new AcademicUnitRepository(context, memoryCache, cacheExpirationOptions);
        var factory = new UserProgressTrackerFactory(academicUnitRepository, memoryCache, cacheExpirationOptions);
        var userProgressTracker = await factory.BuildAsync(userId);
        
        //Act-assert
        foreach (var lessonCompleted in lessonsCompleted)
        {
            var lessonCompletedResult = userProgressTracker.TryMarkLessonCompleted(
                courseId, 
                lessonCompleted.chapterId, 
                lessonCompleted.lessonId, 
                lessonCompleted.startedTimestamp, 
                lessonCompleted.completedTimestamp);

            await Assert.That(lessonCompletedResult).IsTrue();
        }
        
        await Assert.That(userProgressTracker.FirstStartedLessonTimestamp).IsEqualTo(lessonsCompleted.First().startedTimestamp);
        await Assert.That(userProgressTracker.LastCompletedLessonTimestamp).IsEqualTo(lessonsCompleted.Last().completedTimestamp);
        await Assert.That(userProgressTracker.UserId).IsEqualTo(userId);
        await Assert.That(userProgressTracker.TotalChaptersCompletedCount).IsEqualTo(2);
        await Assert.That(userProgressTracker.TotalLessonsCompletedCount).IsEqualTo(lessonsCompleted.Count);
        await Assert.That(userProgressTracker.FirstStartedChapterTimestamp).IsEqualTo(lessonsCompleted.First().startedTimestamp);
        await Assert.That(userProgressTracker.LastCompletedChapterTimestamp).IsEqualTo(lessonsCompleted.Last().completedTimestamp);
        
        var courseProgressTracker = userProgressTracker[courseId];
        
        await Assert.That(courseProgressTracker.FirstStartedLessonTimestamp).IsEqualTo(lessonsCompleted.First().startedTimestamp);
        await Assert.That(courseProgressTracker.LastCompletedLessonTimestamp).IsEqualTo(lessonsCompleted.Last().completedTimestamp);
        await Assert.That(courseProgressTracker.FirstStartedChapterTimestamp).IsEqualTo(lessonsCompleted.First().startedTimestamp);
        await Assert.That(courseProgressTracker.LastCompletedChapterTimestamp).IsEqualTo(lessonsCompleted.Last().completedTimestamp);
        await Assert.That(courseProgressTracker.TotalLessonsCompletedCount).IsEqualTo(lessonsCompleted.Count);
        await Assert.That(courseProgressTracker.CompletedChaptersCount).IsEqualTo(lessonsCompleted.GroupBy(l => l.chapterId).Count());
        await Assert.That(courseProgressTracker.PercentageCompleted).IsEqualTo((ushort) 100);

        foreach (var chapterLessonsGroup in 
                 lessonsCompleted.GroupBy(l => l.chapterId))
        {
            var chapterId = chapterLessonsGroup.Key;
            var chapterProgressTracker = courseProgressTracker[chapterId];
        
            await Assert.That(chapterProgressTracker.FirstStartedLessonTimestamp).IsEqualTo(chapterLessonsGroup.First().startedTimestamp);
            await Assert.That(chapterProgressTracker.LastCompletedLessonTimestamp).IsEqualTo(chapterLessonsGroup.Last().completedTimestamp);
            await Assert.That(chapterProgressTracker.PercentageCompleted).IsEqualTo((ushort) 100);
            await Assert.That(chapterProgressTracker.LessonsCompletedCount).IsEqualTo(chapterLessonsGroup.Count());
        }
    }
    
    [Test]
    [NotInParallel]
    public async Task WhenAllLessonInChapterMarkedCompleted_WholeUserProgressTree_ShouldChangeAccordingly()
    {
        // Arrange
        const uint userId = 1;
        const uint chapterId = 2;
        const uint courseId = 1;
        var lessonsCompleted = 
            new List<(uint lessonId, uint startedTimestamp, uint completedTimestamp)>
        {
            (3, 1, 2),
            (4, 3, 4),
            (5, 5, 6)
        };
        var cacheExpirationOptions = Options.Create(new CacheExpirationHoursOptions());
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var context = CreateDbContext();
        var academicUnitRepository = new AcademicUnitRepository(context, memoryCache, cacheExpirationOptions);
        var factory = new UserProgressTrackerFactory(academicUnitRepository, memoryCache, cacheExpirationOptions);
        var userProgressTracker = await factory.BuildAsync(userId);
        
        //Act-assert
        foreach (var lessonCompleted in lessonsCompleted)
        {
            var lessonCompletedResult = userProgressTracker.TryMarkLessonCompleted(
                courseId, 
                chapterId, 
                lessonCompleted.lessonId, 
                lessonCompleted.startedTimestamp, 
                lessonCompleted.completedTimestamp);

            await Assert.That(lessonCompletedResult).IsTrue();
        }
        
        await Assert.That(userProgressTracker.FirstStartedLessonTimestamp).IsEqualTo(lessonsCompleted.First().startedTimestamp);
        await Assert.That(userProgressTracker.LastCompletedLessonTimestamp).IsEqualTo(lessonsCompleted.Last().completedTimestamp);
        await Assert.That(userProgressTracker.UserId).IsEqualTo(userId);
        await Assert.That(userProgressTracker.TotalChaptersCompletedCount).IsEqualTo(1);
        await Assert.That(userProgressTracker.TotalLessonsCompletedCount).IsEqualTo(lessonsCompleted.Count);
        await Assert.That(userProgressTracker.FirstStartedChapterTimestamp).IsEqualTo(lessonsCompleted.First().startedTimestamp);
        await Assert.That(userProgressTracker.LastCompletedChapterTimestamp).IsEqualTo(lessonsCompleted.Last().completedTimestamp);
        
        var courseProgressTracker = userProgressTracker[courseId];
        
        await Assert.That(courseProgressTracker.FirstStartedLessonTimestamp).IsEqualTo(lessonsCompleted.First().startedTimestamp);
        await Assert.That(courseProgressTracker.LastCompletedLessonTimestamp).IsEqualTo(lessonsCompleted.Last().completedTimestamp);
        await Assert.That(courseProgressTracker.FirstStartedChapterTimestamp).IsEqualTo(lessonsCompleted.First().startedTimestamp);
        await Assert.That(courseProgressTracker.LastCompletedChapterTimestamp).IsEqualTo(lessonsCompleted.Last().completedTimestamp);
        await Assert.That(courseProgressTracker.TotalLessonsCompletedCount).IsEqualTo(lessonsCompleted.Count);
        await Assert.That(courseProgressTracker.CompletedChaptersCount).IsEqualTo(1);
        await Assert.That(courseProgressTracker.PercentageCompleted).IsEqualTo((ushort) 50);
        
        var chapterProgressTracker = courseProgressTracker[chapterId];
        
        await Assert.That(chapterProgressTracker.FirstStartedLessonTimestamp).IsEqualTo(lessonsCompleted.First().startedTimestamp);
        await Assert.That(chapterProgressTracker.LastCompletedLessonTimestamp).IsEqualTo(lessonsCompleted.Last().completedTimestamp);
        await Assert.That(chapterProgressTracker.PercentageCompleted).IsEqualTo((ushort) 100);
        await Assert.That(chapterProgressTracker.LessonsCompletedCount).IsEqualTo(3);
        
    }
    
    [Test]
    [NotInParallel]
    public async Task WhenFirstLessonInChapterMarkedCompleted_WholeUserProgressTree_ShouldChangeAccordingly()
    {
        // Arrange
        const uint userId = 1;
        const uint lessonId = 3;
        const uint chapterId = 2;
        const uint courseId = 1;
        const uint lessonStartedTimestamp = 1;
        const uint lessonCompletedTimestamp = 2;
        var cacheExpirationOptions = Options.Create(new CacheExpirationHoursOptions());
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var context = CreateDbContext();
        var academicUnitRepository = new AcademicUnitRepository(context, memoryCache, cacheExpirationOptions);
        var factory = new UserProgressTrackerFactory(academicUnitRepository, memoryCache, cacheExpirationOptions);
        var userProgressTracker = await factory.BuildAsync(userId);

        // Act
        var result = userProgressTracker.TryMarkLessonCompleted
            (courseId, chapterId, lessonId, lessonStartedTimestamp, lessonCompletedTimestamp);

        // Assert
        await Assert.That(result).IsTrue();
        
        await Assert.That(userProgressTracker.FirstStartedLessonTimestamp).IsEqualTo(lessonStartedTimestamp);
        await Assert.That(userProgressTracker.LastCompletedLessonTimestamp).IsEqualTo(lessonCompletedTimestamp);
        await Assert.That(userProgressTracker.UserId).IsEqualTo(userId);
        await Assert.That(userProgressTracker.TotalChaptersCompletedCount).IsEqualTo(0);
        await Assert.That(userProgressTracker.TotalLessonsCompletedCount).IsEqualTo(1);
        await Assert.That(userProgressTracker.FirstStartedChapterTimestamp).IsEqualTo(lessonStartedTimestamp);
        await Assert.That(userProgressTracker.LastCompletedChapterTimestamp).IsNull();
        
        var courseProgressTracker = userProgressTracker[courseId];
        
        await Assert.That(courseProgressTracker.FirstStartedLessonTimestamp).IsEqualTo(lessonStartedTimestamp);
        await Assert.That(courseProgressTracker.LastCompletedLessonTimestamp).IsEqualTo(lessonCompletedTimestamp);
        await Assert.That(courseProgressTracker.FirstStartedChapterTimestamp).IsEqualTo(lessonStartedTimestamp);
        await Assert.That(courseProgressTracker.LastCompletedChapterTimestamp).IsEqualTo(null);
        await Assert.That(courseProgressTracker.TotalLessonsCompletedCount).IsEqualTo(1);
        await Assert.That(courseProgressTracker.CompletedChaptersCount).IsEqualTo(0);
        await Assert.That(courseProgressTracker.PercentageCompleted).IsEqualTo((ushort) 0);
        
        var chapterProgressTracker = courseProgressTracker[chapterId];
        
        await Assert.That(chapterProgressTracker.FirstStartedLessonTimestamp).IsEqualTo(lessonStartedTimestamp);
        await Assert.That(chapterProgressTracker.LastCompletedLessonTimestamp).IsEqualTo(lessonCompletedTimestamp);
        await Assert.That(chapterProgressTracker.PercentageCompleted).IsEqualTo((ushort) 33);
        await Assert.That(chapterProgressTracker.LessonsCompletedCount).IsEqualTo(1);
        
    }
}