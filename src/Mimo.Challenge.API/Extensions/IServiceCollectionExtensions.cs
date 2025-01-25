using System.Net;
using System.Threading.Channels;
using Microsoft.AspNetCore.RateLimiting;
using Mimo.Challenge.Domain.Entities.Messages;

namespace Mimo.Challenge.API.Extensions;

// ReSharper disable once InconsistentNaming
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddConcurrencyRateLimiter
        (this IServiceCollection services, IConfiguration configuration)
    {
        var configurationSectionName = "ConcurrentRequestsLimit";
        var concurrentRequestsLimit =
            configuration.GetSection(configurationSectionName).Get<uint>();

        if (concurrentRequestsLimit == 0)
            throw new InvalidOperationException($"{configurationSectionName} must be set in configuration.");
        
        services.AddRateLimiter(rateLimiterOptions =>
        {
            rateLimiterOptions.RejectionStatusCode = (int)HttpStatusCode.TooManyRequests;
            rateLimiterOptions.AddConcurrencyLimiter("concurrency", concurrencyLimiter =>
            {
                concurrencyLimiter.PermitLimit = (int) concurrentRequestsLimit;
            });
        });

        return services;
    }

    public static IServiceCollection AddChannels
        (this IServiceCollection services, IConfiguration configuration)
    {
        var configurationSectionName = "ChannelCapacity:ReportLessonProgressMessage";
        
        var reportLessonProgressMessageChannelCapacity =
            configuration.GetSection(configurationSectionName).Get<uint>();

        if (reportLessonProgressMessageChannelCapacity == 0)
            throw new InvalidOperationException($"{configurationSectionName} must be set in configuration.");


        var channel = Channel.CreateBounded<ReportLessonProgressMessage>
            ((int)reportLessonProgressMessageChannelCapacity);

        return services.AddSingleton(channel);
    }
}