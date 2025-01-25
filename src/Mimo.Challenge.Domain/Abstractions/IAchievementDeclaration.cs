
using Mimo.Challenge.Domain.Entities;

namespace Mimo.Challenge.Domain.Abstractions;

public interface IAchievementDeclaration
{
    public uint AchievementId { get; }
    
    public string Description { get; }
    
    public Task UpdateProgressAsync(
        UserProgressTracker userProgressTracker, 
        CancellationToken cancellationToken = default);
}