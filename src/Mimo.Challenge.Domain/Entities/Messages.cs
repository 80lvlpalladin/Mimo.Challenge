// ReSharper disable once CheckNamespace
namespace Mimo.Challenge.Domain.Entities.Messages;

public record ReportLessonProgressMessage
    (uint UserId, uint LessonId, uint StartedTimestamp, uint CompletedTimestamp);