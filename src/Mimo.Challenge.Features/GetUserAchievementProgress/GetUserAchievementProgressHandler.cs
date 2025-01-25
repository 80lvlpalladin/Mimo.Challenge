using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using Mimo.Challenge.Domain;
using Mimo.Challenge.Domain.Abstractions;

namespace Mimo.Challenge.Features.GetUserAchievementProgress;


public record GetUserAchievementProgressRequest(uint UserId)
    : IRequest<ErrorOr<GetUserAchievementProgressResponse>>;
public record GetUserAchievementProgressResponse
    (IDictionary<uint, ushort> AchievementIdToProgressPercentageMap);

public class GetUserAchievementProgressHandler(
    IAchievementProgressRepository achievementProgressRepository, 
    ILogger<GetUserAchievementProgressHandler> logger) : 
    IRequestHandler<GetUserAchievementProgressRequest, ErrorOr<GetUserAchievementProgressResponse>>
{
    public async Task<ErrorOr<GetUserAchievementProgressResponse>> 
        Handle(GetUserAchievementProgressRequest request, CancellationToken cancellationToken)
    {
        var userProgress = await achievementProgressRepository.GetUserAchievementProgressAsync(
            request.UserId, cancellationToken);

        if (userProgress.Count == 0)
        {
            GlobalLogger.LogWarningForUser
                (logger, $"User does not have any progress in achievements", request.UserId);
            return Error.NotFound();
        }
        
        return new GetUserAchievementProgressResponse(userProgress);
    }
}