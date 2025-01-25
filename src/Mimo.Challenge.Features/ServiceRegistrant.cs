using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Mimo.Challenge.Features;

public static class ServiceRegistrant
{
    public static IServiceCollection AddFeaturesServices(this IServiceCollection services)
    {
        var mediatrHandlersAssembly = Assembly.GetExecutingAssembly();
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(mediatrHandlersAssembly));
        return services;
    }
}