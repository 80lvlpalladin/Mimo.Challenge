using Microsoft.EntityFrameworkCore;
using Mimo.Challenge.DAL.Entities;
using Mimo.Challenge.Domain.Abstractions;
using Mimo.Challenge.Domain.Entities;
using Mimo.Challenge.Domain.Services;

namespace Mimo.Challenge.DAL.Repositories;

public class CompletedAcademicUnitRepository(
    MimoContext context,
    IAcademicUnitRepository academicUnitRepository,
    UserProgressTrackerFactory userProgressTrackerFactory) : ICompletedAcademicUnitRepository
{
    public async Task<UserProgressTracker> RegisterCompletedLessonAsync(
        uint userId, 
        uint lessonId,
        uint startedTimestamp, 
        uint completedTimestamp, 
        CancellationToken ct = default)
    {
        var userExists = await context.Users.AnyAsync(user => user.Id == userId, ct);
                
        if(!userExists)
            throw new Exception($"User {userId} does not exist");
                
        var unitExists = 
            await academicUnitRepository.CheckIfUnitExistsAsync(lessonId, ct);
        
        if(!unitExists)
            throw new Exception($"Unit {lessonId} does not exist");

        var userProgressTracker = await UpdateCompletedAcademicUnitsTablesAsync(
            new CompletedLesson(
                userId,
                lessonId,
                startedTimestamp,
                completedTimestamp),
            ct);

        return userProgressTracker;
    }

    public Task<int> GetCompletedLessonsCountAsync(uint userId, CancellationToken ct = default) =>
        context.CompletedLessons.CountAsync(cl => cl.UserId == userId, ct);

    public Task<int> GetCompletedChaptersCountAsync(uint userId, CancellationToken ct = default) =>
        context.CompletedChapters.CountAsync(cc => cc.UserId == userId, ct);

    public Task<bool> HasUserCompletedCourseAsync(uint userId, uint courseId, CancellationToken ct = default) => 
        context.CompletedCourses.AnyAsync(cc => cc.UserId == userId && cc.AcademicUnitId == courseId, ct);

    private async Task<UserProgressTracker> UpdateCompletedAcademicUnitsTablesAsync
        (CompletedLesson newCompletedLesson, CancellationToken ct)
    {
        var userId = newCompletedLesson.UserId;
        var alreadyCompletedLessons = await context.CompletedLessons
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .ToArrayAsync(ct);
        
        var userProgressTracker = await userProgressTrackerFactory.BuildAsync(userId, ct);

        foreach (var completedLesson in alreadyCompletedLessons.Append(newCompletedLesson))
        {
            var lessonPath = await academicUnitRepository.GetPathForUnitAsync(completedLesson.AcademicUnitId, ct);

            if (!TryGetChapterAndCourseIdFromPath(lessonPath, out var chapterId, out var courseId))
                throw new Exception($"Academic unit {completedLesson.AcademicUnitId} is not a lesson");

            _ = userProgressTracker.TryMarkLessonCompleted(
                courseId, 
                chapterId, 
                completedLesson.AcademicUnitId, 
                completedLesson.StartedTimestamp, 
                completedLesson.CompletedTimestamp);
        }

        await context.CompletedLessons.AddAsync(newCompletedLesson, ct);

        var completedCourseIds = await context.CompletedCourses
            .Where(cc => cc.UserId == userId)
            .Select(cc => cc.AcademicUnitId)
            .ToArrayAsync(ct);
        
        var completedChapterIds = await context.CompletedChapters
            .Where(cc => cc.UserId == userId)
            .Select(cc => cc.AcademicUnitId)
            .ToArrayAsync(ct);
        
        foreach (var (courseId, courseProgressTracker) in userProgressTracker)
        {
            foreach (var (chapterId, chapterProgressTracker) in courseProgressTracker)
            {
                if (completedChapterIds.Contains(chapterId) || chapterProgressTracker.PercentageCompleted != 100)
                    continue;
                
                await context.CompletedChapters.AddAsync(
                    new CompletedChapter(
                        userId, 
                        chapterId, 
                        chapterProgressTracker.FirstStartedLessonTimestamp ??
                        throw new Exception($"Invalid data in ChapterProgressTracker for course {courseId} and chapter {chapterId}"),
                        chapterProgressTracker.LastCompletedLessonTimestamp ??
                        throw new Exception($"Invalid data in ChapterProgressTracker for course {courseId} and chapter {chapterId}")),
                    ct);
            }
            
            if (completedCourseIds.Contains(courseId))
                continue;
            
            if (courseProgressTracker.PercentageCompleted == 100)
            {
                
                await context.CompletedCourses.AddAsync(
                    new CompletedCourse(
                        userId, 
                        courseId, 
                        courseProgressTracker.FirstStartedLessonTimestamp ?? 
                            throw new Exception($"Invalid data in CourseProgressTracker for course {courseId}"), 
                        courseProgressTracker.LastCompletedLessonTimestamp ??
                            throw new Exception($"Invalid data in CourseProgressTracker for course {courseId}")),
                        ct);

                continue;
            }

            
        }

        return userProgressTracker;
    }

    private bool TryGetChapterAndCourseIdFromPath(
        string? lessonPath, 
        out uint chapterId, 
        out uint courseId)
    {
        if(lessonPath is null)
        {
            chapterId = 0;
            courseId = 0;
            return false;
        }
        
        var lessonPathSegments = lessonPath.Split('.');
        
        if(lessonPathSegments.Length != 2)
        {
            chapterId = 0;
            courseId = 0;
            return false;
        }
        
        chapterId = uint.Parse(lessonPathSegments[1]);
        courseId = uint.Parse(lessonPathSegments[0]);
        
        return true;
    }
}


