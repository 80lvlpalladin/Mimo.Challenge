using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Mimo.Challenge.DAL.Repositories;
using Mimo.Challenge.Domain.Entities;
using Mimo.Challenge.Domain.Entities.Configuration;
using Mimo.Challenge.Domain.Services;

namespace Mimo.Challenge.IntegrationTests.Domain.Services;

public class UserProgressTrackerFactoryTests : SqliteDatabaseIntegrationTests
{
    [Test]
    [NotInParallel]
    public async Task BuildAsync_ProducesValidTracker()
    {
        //Arrange
        var cacheExpirationOptions = Options.Create(new CacheExpirationHoursOptions());
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var context = CreateDbContext();
        var academicUnitRepository = new AcademicUnitRepository(context, memoryCache, cacheExpirationOptions);
        var factory = new UserProgressTrackerFactory(academicUnitRepository, memoryCache, cacheExpirationOptions);
        const uint userId = 1;
        var expectedTracker = CreateExpectedTracker(userId);
        
        //Act
        var actualTracker = await factory.BuildAsync(userId);
        
        //Assert
        await Assert.That(actualTracker.UserId).IsEqualTo(userId);

        foreach (var courseId in actualTracker.Keys)
        {
            foreach (var chapterId in actualTracker[courseId].Keys)
            {
                await Assert
                    .That(actualTracker[courseId][chapterId])
                    .IsEquivalentTo(expectedTracker[courseId][chapterId]);
            }
        }
    }

    //creates UserProgressTracker according to Sql command in SqliteDatabaseFactory
    private UserProgressTracker CreateExpectedTracker(uint userId)
    {
        return new UserProgressTracker(userId)
        {
            [1] = new CourseProgressTracker
            {
                [2] = new ChapterProgressTracker([3, 4, 5]),
                [6] = new ChapterProgressTracker([7])
            },
            [8] = new CourseProgressTracker
            {
                [14] = new ChapterProgressTracker([15, 16, 17]),
                [18] = new ChapterProgressTracker([19])
            },
            [9] = new CourseProgressTracker
            {
                [10] = new ChapterProgressTracker([11, 12, 13])
            }
        };
    } 
    
}