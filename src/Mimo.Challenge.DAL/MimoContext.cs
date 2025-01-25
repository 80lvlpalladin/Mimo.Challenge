using Microsoft.EntityFrameworkCore;
using Mimo.Challenge.DAL.Entities;

namespace Mimo.Challenge.DAL;

public class MimoContext(DbContextOptions<MimoContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<AchievementProgress> AchievementProgress { get; set; }
    public DbSet<AcademicUnit> AcademicUnits { get; set; }
    public DbSet<CompletedLesson> CompletedLessons { get; set; }
    public DbSet<CompletedChapter> CompletedChapters { get; set; }
    public DbSet<CompletedCourse> CompletedCourses { get; set; }
    
}