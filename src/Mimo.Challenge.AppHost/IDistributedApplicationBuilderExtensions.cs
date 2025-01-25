using Microsoft.Data.Sqlite;
using SQLitePCL;

namespace Mimo.Challenge.AppHost;

// ReSharper disable once InconsistentNaming
public static class IDistributedApplicationBuilderExtensions
{
    public static IResourceBuilder<SqliteResource> AddSeededSqlite(this IDistributedApplicationBuilder builder, [ResourceName] string name)
    {
        var databaseDirectoryPath = Directory.GetCurrentDirectory();
        const string databaseFileName = "mimo.db";

        SqliteDatabaseFactory.CreateDatabaseFile(databaseDirectoryPath, databaseFileName);

        return builder
            .AddSqlite(name, databaseDirectoryPath, databaseFileName);
    }
}