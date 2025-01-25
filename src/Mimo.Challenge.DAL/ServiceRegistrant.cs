using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mimo.Challenge.DAL.Repositories;
using Mimo.Challenge.Domain.Abstractions;

namespace Mimo.Challenge.DAL;

public static class ServiceRegistrant
{
    public static IServiceCollection AddDataAccessLayerServices
        (this IServiceCollection services, IConfiguration configuration)
    {
        
        
        return services
            .AddDbContext<MimoContext>(optionsBuilder =>
            {
                var aspireDbResourseName = "Database";
                var dbConnectionString = 
                    configuration.GetConnectionString(aspireDbResourseName);
                ArgumentException.ThrowIfNullOrWhiteSpace(dbConnectionString);
                optionsBuilder.UseSqlite(dbConnectionString);
            })
            .AddScoped<IAchievementProgressRepository, AchievementProgressRepository>()
            .AddScoped<IAcademicUnitRepository, AcademicUnitRepository>()
            .AddScoped<ICompletedAcademicUnitRepository, CompletedAcademicUnitRepository>()
            .AddScoped<IDatabasePersistor, DatabasePersistor>();
    }
}