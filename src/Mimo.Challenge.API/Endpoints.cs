using MediatR;
using Microsoft.AspNetCore.Mvc;
using Mimo.Challenge.API.Extensions;
using Mimo.Challenge.Features.GetUserAchievementProgress;
using Mimo.Challenge.Features.ReportLessonProgress;
using Mimo.Challenge.Features.ResetCache;

namespace Mimo.Challenge.API;

public static class Endpoints
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapPost("api/lesson-progress", ReportLessonProgressAsync);
        routeBuilder.MapGet("api/achievement-progress/user/{userId}", GetUserAchievementProgressAsync);
        routeBuilder.MapGet("api/reset-cache", ResetCacheAsync);

        return routeBuilder;
    }
    
    private static Task<IResult> ReportLessonProgressAsync(
        IMediator mediator,
        [FromBody] ReportLessonProgressRequest request,
        CancellationToken cancellationToken) =>
        mediator.SendAndReturnResultAsync(request, cancellationToken);
    
    private static Task<IResult> GetUserAchievementProgressAsync(
        IMediator mediator,
        [FromQuery] uint userId,
        CancellationToken cancellationToken)
    {
        var request = new GetUserAchievementProgressRequest(userId);
        return mediator.SendAndReturnResultAsync(request, cancellationToken);
    }
    
    private static Task<IResult> ResetCacheAsync(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var request = new ResetCacheRequest();
        return mediator.SendAndReturnResultAsync(request, cancellationToken);
    }
}