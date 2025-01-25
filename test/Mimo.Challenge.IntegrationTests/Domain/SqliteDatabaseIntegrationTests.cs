using Microsoft.EntityFrameworkCore;
using Mimo.Challenge.AppHost;
using Mimo.Challenge.DAL;

namespace Mimo.Challenge.IntegrationTests.Domain;

public abstract class SqliteDatabaseIntegrationTests
{
    protected MimoContext CreateDbContext()
    {
        DeleteDatabaseFileIfExists();
        
        var connectionString = 
            SqliteDatabaseFactory.CreateDatabaseFile(DatabaseFolderPath, DatabaseFileName);
        
        var dbcontextOptions = new DbContextOptionsBuilder<MimoContext>()
            .UseSqlite(connectionString).Options;
        
        return new MimoContext(dbcontextOptions);
    }


    [After(Test)]
    public void DeleteDatabaseFileIfExists()
    {
        if(File.Exists(DatabaseFilePath))
            File.Delete(DatabaseFilePath);
    }
    
    
    private const string DatabaseFileName = "test.db";
    private static string DatabaseFolderPath => Directory.GetCurrentDirectory();
    private static string DatabaseFilePath => Path.Combine(DatabaseFolderPath, DatabaseFileName);
}