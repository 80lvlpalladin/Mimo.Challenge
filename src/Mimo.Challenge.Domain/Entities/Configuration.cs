// ReSharper disable once CheckNamespace
namespace Mimo.Challenge.Domain.Entities.Configuration;

public class CacheExpirationHoursOptions
{
    public ushort AcademicUnit { get; init; } = 24;
    public ushort AchievementProgress { get; init; } = 24;
}
