
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Mimo.Challenge.DAL.Entities;


/// <param name="HierarchyPath">
/// Path for navigating the hierarchy tree in the database.
/// Also determines academic unit type.
/// Examples: course path - null, chapter path - 'courseId', lesson path - 'courseId.chapterId'
/// </param>
/// <param name="Order">Order of display within type</param>
public record AcademicUnit(
    uint Id,
    string? HierarchyPath,
    string Title, 
    string Content, 
    int Order = 0);

[PrimaryKey(nameof(UserId), nameof(AcademicUnitId))]
public record CompletedLesson(
    uint UserId,
    uint AcademicUnitId,
    uint StartedTimestamp,
    uint CompletedTimestamp);

[PrimaryKey(nameof(UserId), nameof(AcademicUnitId))]
public record CompletedChapter(
    uint UserId,
    uint AcademicUnitId,
    uint StartedTimestamp,
    uint CompletedTimestamp);

[PrimaryKey(nameof(UserId), nameof(AcademicUnitId))]
public record CompletedCourse(
    uint UserId,
    uint AcademicUnitId,
    uint StartedTimestamp,
    uint CompletedTimestamp);

public record User(
    uint Id,
    string Email);

[PrimaryKey(nameof(UserId), nameof(AchievementId))]
public class AchievementProgress(
    uint userId,
    uint achievementId,
    ushort percentCompleted,
    uint lastUpdatedTimestamp)
{
    public uint UserId { get; init; } = userId;
    public uint AchievementId { get; init; } = achievementId;
    public ushort PercentCompleted { get; set; } = percentCompleted;
    public uint LastUpdatedTimestamp { get; set; } = lastUpdatedTimestamp;
    
}
