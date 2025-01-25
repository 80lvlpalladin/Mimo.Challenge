using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Mimo.Challenge.Domain.Abstractions;
using Mimo.Challenge.Domain.Achievements;
using Mimo.Challenge.Domain.Services;

namespace Mimo.Challenge.Domain;

public static class ServiceRegistrant
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        return services
            .AddAchievementDeclarations()
            .AddScoped<UserProgressTrackerFactory>()
            .AddHostedService<ReportLessonProgressConsumerService>();
    }

    private static IServiceCollection AddAchievementDeclarations(this IServiceCollection services)
    {
        var achievementDeclarationTypes = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(type => 
                type.IsAssignableTo(typeof(IAchievementDeclaration)) && !type.IsInterface);
        
        foreach (var type in achievementDeclarationTypes)
        {
            services.AddSingleton(typeof(IAchievementDeclaration), type);
        }

        return services;
    }
}