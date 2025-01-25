using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Mimo.Challenge.DAL.Entities;
using Mimo.Challenge.DAL.Repositories;
using Mimo.Challenge.Domain.Entities.Configuration;
using Mimo.Challenge.Domain.Services;
using Mimo.Challenge.IntegrationTests.Domain;

namespace Mimo.Challenge.IntegrationTests.DAL.Repositories;

public class CompletedAcademicUnitRepositoryTests : SqliteDatabaseIntegrationTests
{
    [Test]
    [NotInParallel]
    public async Task WhenAllChaptersFromCourseAreRegistered_AllCompletedAcademicUnitsTableAreUpdated()
    {
        // Arrange
        const uint userId = 3;
        
        var expectedCompletedCourse = 
            new CompletedCourse(userId, 1, 1, 8);
        
        var expectectedCompletedChapters = new[]
        {
            new CompletedChapter(userId, 2, 1, 6),
            new CompletedChapter(userId, 6, 7, 8)
        };
        
        var expectedCompletedLessons = new[]
        {
            new CompletedLesson(userId, 3, 1, 2),
            new CompletedLesson(userId, 4, 3, 4),
            new CompletedLesson(userId, 5, 5, 6),
            new CompletedLesson(userId, 7, 7, 8),
        };
        
        var cacheExpirationOptions = Options.Create(new CacheExpirationHoursOptions());
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var context = CreateDbContext();
        var academicUnitRepository = new AcademicUnitRepository(context, memoryCache, cacheExpirationOptions);
        var userProgressTrackerfactory =
            new UserProgressTrackerFactory(academicUnitRepository, memoryCache, cacheExpirationOptions);
        var completedAcademicUnitRepository =
            new CompletedAcademicUnitRepository(context, academicUnitRepository, userProgressTrackerfactory);
        
        //Act-assert
        for (var index = 1; index <= expectedCompletedLessons.Length; index++)
        {
            var lesson = expectedCompletedLessons[index - 1];
            
            var userProgressTracker = await completedAcademicUnitRepository.RegisterCompletedLessonAsync(
                lesson.UserId,
                lesson.AcademicUnitId,
                lesson.StartedTimestamp,
                lesson.CompletedTimestamp);

            await context.SaveChangesAsync();

            var courseProgressTracker =
                userProgressTracker[expectedCompletedCourse.AcademicUnitId];

            await Assert.That(courseProgressTracker.TotalLessonsCompletedCount).IsEqualTo(index);
        }
        
        var actualCompletedLessons = await context.CompletedLessons
            .Where(cl => cl.UserId == userId)
            .ToArrayAsync();
        
        var actualCompletedChapters = await context.CompletedChapters
            .Where(cl => cl.UserId == userId)
            .ToArrayAsync();
        
        await Assert.That(actualCompletedLessons).IsEquivalentTo(expectedCompletedLessons);
        await Assert.That(actualCompletedChapters).IsEquivalentTo(expectectedCompletedChapters);
        
        await Assert.That(context.CompletedCourses.Single(chapter => chapter.UserId == userId))
            .IsEqualTo(expectedCompletedCourse);
    }
    
    [Test]
    [NotInParallel]
    public async Task WhenFirstLessonFromChapterIsRegistered_OnlyCompletedLessonsTableIsUpdated()
    {
        // Arrange
        const uint chapterId = 2;
        const uint courseId = 1;
        const uint userId = 1;
        var expectedCompletedLesson =
            new CompletedLesson(userId, 3, 1, 2);
        var cacheExpirationOptions = Options.Create(new CacheExpirationHoursOptions());
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var context = CreateDbContext();
        var academicUnitRepository = new AcademicUnitRepository(context, memoryCache, cacheExpirationOptions);
        var userProgressTrackerfactory =
            new UserProgressTrackerFactory(academicUnitRepository, memoryCache, cacheExpirationOptions);
        var completedAcademicUnitRepository =
            new CompletedAcademicUnitRepository(context, academicUnitRepository, userProgressTrackerfactory);
        
        //Act
        var userProgressTracker = await completedAcademicUnitRepository.RegisterCompletedLessonAsync(
            expectedCompletedLesson.UserId, 
            expectedCompletedLesson.AcademicUnitId, 
            expectedCompletedLesson.StartedTimestamp, 
            expectedCompletedLesson.CompletedTimestamp);
        
        await context.SaveChangesAsync();
        
        await Assert.That(userProgressTracker[courseId][chapterId].LessonsCompletedCount).IsEqualTo(1);
        await Assert.That(context.CompletedLessons.Single(lesson => lesson.UserId == userId)).IsEqualTo(expectedCompletedLesson);
    }

    [Test]
    [NotInParallel]
    public async Task WhenAllLessonsFromChapterAreRegistered_CompletedLessonsAndCompletedChapterTablesAreUpdated()
    {
        // Arrange
        const uint courseId = 1;
        const uint userId = 2;
        CompletedLesson[] expectedCompletedLessons = 
        [
            new(userId, 3, 1, 2),
            new(userId, 4, 3, 4),
            new(userId, 5, 5, 6)
        ];

        var expectedCompletedChapter = 
            new CompletedChapter(userId, 2, 1, 6);
        var cacheExpirationOptions = Options.Create(new CacheExpirationHoursOptions());
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var context = CreateDbContext();
        var academicUnitRepository = new AcademicUnitRepository(context, memoryCache, cacheExpirationOptions);
        var userProgressTrackerfactory =
            new UserProgressTrackerFactory(academicUnitRepository, memoryCache, cacheExpirationOptions);
        var completedAcademicUnitRepository =
            new CompletedAcademicUnitRepository(context, academicUnitRepository, userProgressTrackerfactory);

        //Act-assert
        for (var index = 1; index <= expectedCompletedLessons.Length; index++)
        {
            var lesson = expectedCompletedLessons[index - 1];
            
            var userProgressTracker = await completedAcademicUnitRepository.RegisterCompletedLessonAsync(
                lesson.UserId,
                lesson.AcademicUnitId,
                lesson.StartedTimestamp,
                lesson.CompletedTimestamp);

            await context.SaveChangesAsync();
            

            var chapterProgressTracker =
                userProgressTracker[courseId][expectedCompletedChapter.AcademicUnitId];

            await Assert.That(chapterProgressTracker.LessonsCompletedCount).IsEqualTo(index);

            if (index != 3) 
                continue;
            
            var courseProjectTracker = userProgressTracker[courseId];
            await Assert.That(courseProjectTracker.CompletedChaptersCount).IsEqualTo(1);
            await Assert.That(courseProjectTracker.TotalLessonsCompletedCount).IsEqualTo(3);
        }
        
        var actualCompletedLessons = await context.CompletedLessons
            .Where(cl => cl.UserId == userId)
            .ToArrayAsync();

        await Assert.That(actualCompletedLessons)
            .IsEquivalentTo(expectedCompletedLessons);
        
    }

}