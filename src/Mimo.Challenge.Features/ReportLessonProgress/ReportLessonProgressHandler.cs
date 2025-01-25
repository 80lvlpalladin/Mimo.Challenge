using System.Threading.Channels;
using ErrorOr;
using MediatR;
using Mimo.Challenge.Domain.Entities.Messages;

namespace Mimo.Challenge.Features.ReportLessonProgress;

public record ReportLessonProgressRequest
    (uint UserId, uint LessonId, uint StartedTimestamp, uint CompletedTimestamp)
    : IRequest<ErrorOr<Success>>;

public class ReportLessonProgressHandler(
    Channel<ReportLessonProgressMessage> channel) : 
    IRequestHandler<ReportLessonProgressRequest, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(ReportLessonProgressRequest request, CancellationToken cancellationToken)
    {
        var domainMessage = new ReportLessonProgressMessage(
            request.UserId, request.LessonId, request.StartedTimestamp, request.CompletedTimestamp);
        await channel.Writer.WriteAsync(domainMessage, cancellationToken);
        return Result.Success;
    }
}