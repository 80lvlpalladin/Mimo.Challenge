using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Mimo.Challenge.Domain.Abstractions;
using Mimo.Challenge.Domain.Entities.Messages;

namespace Mimo.Challenge.Domain.Services;

public class ReportLessonProgressConsumerService(
    Channel<ReportLessonProgressMessage> channel, 
    ICompletedAcademicUnitRepository completedAcademicUnitRepository, 
    IEnumerable<IAchievementDeclaration> achievementDeclarations,
    IDatabasePersistor databasePersistor)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var reader = channel.Reader;
        
        while (!reader.Completion.IsCompleted && await reader.WaitToReadAsync(stoppingToken))
        {
            if (reader.TryRead(out var msg))
            {
                var userProgressTracker = await completedAcademicUnitRepository
                    .RegisterCompletedLessonAsync(
                        msg.UserId, 
                        msg.LessonId, 
                        msg.StartedTimestamp, 
                        msg.CompletedTimestamp, 
                        stoppingToken);

                //first, we save progress to the database, then we update achievements
                //in case of an exception in achievements progress update, we will not lose user progress
                await databasePersistor.SaveChangesAsync(stoppingToken); 

                foreach (var achievementDeclaration in achievementDeclarations)
                {
                    await achievementDeclaration.UpdateProgressAsync(userProgressTracker, stoppingToken);
                }
                
                await databasePersistor.SaveChangesAsync(stoppingToken); 
            }
        }
    }
}