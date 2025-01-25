using Microsoft.Data.Sqlite;
using SQLitePCL;

namespace Mimo.Challenge.AppHost;

public class SqliteDatabaseFactory
{
    /// <returns>Connection string</returns>
    public static string CreateDatabaseFile(string directoryPath, string fileName)
    {
        var databaseFilePath = Path.Combine(directoryPath, fileName);
        var connectionString = new SqliteResource("connection-string-creator", directoryPath, fileName)
            .ConnectionStringExpression.ValueExpression;

        if (File.Exists(databaseFilePath))
        {
            return connectionString;
        }

        Directory.CreateDirectory(directoryPath);
        File.Create(databaseFilePath).Dispose();

        if (!OperatingSystem.IsWindows())
        {
            // Change permissions for non-root accounts (container user account)
            const UnixFileMode ownershipPermissions =
                UnixFileMode.UserRead | UnixFileMode.UserWrite |
                UnixFileMode.GroupRead | UnixFileMode.GroupWrite |
                UnixFileMode.OtherRead | UnixFileMode.OtherWrite;

            File.SetUnixFileMode(databaseFilePath, ownershipPermissions);
        }
        
        Batteries.Init();
        
        using var sqliteConnection = new SqliteConnection(connectionString);

        sqliteConnection.Open();

        using var command = sqliteConnection.CreateCommand();
        command.CommandText = CreateTables;
        command.ExecuteNonQuery();
        command.CommandText = PopulateTables;
        command.ExecuteNonQuery();

        return connectionString;
    }

    private const string CreateTables = """
                                        CREATE TABLE IF NOT EXISTS AcademicUnits
                                        (
                                            Id INTEGER PRIMARY KEY,
                                            HierarchyPath TEXT,
                                            Title TEXT NOT NULL,
                                            Content TEXT NOT NULL,
                                            [Order] INTEGER NOT NULL DEFAULT 0
                                        );

                                        CREATE TABLE IF NOT EXISTS Users 
                                        (
                                            Id INTEGER PRIMARY KEY,
                                            Email TEXT NOT NULL
                                        ); 

                                        CREATE TABLE IF NOT EXISTS CompletedLessons 
                                        (
                                            UserId INTEGER NOT NULL,
                                            AcademicUnitId INTEGER NOT NULL,
                                            StartedTimestamp INTEGER NOT NULL,
                                            CompletedTimestamp INTEGER NOT NULL,
                                            PRIMARY KEY (UserId, AcademicUnitId),
                                            FOREIGN KEY (UserId) REFERENCES Users(Id),
                                            FOREIGN KEY (AcademicUnitId) REFERENCES AcademicUnits(Id)
                                        );  

                                        CREATE TABLE IF NOT EXISTS CompletedChapters 
                                        (
                                            UserId INTEGER NOT NULL,
                                            AcademicUnitId INTEGER NOT NULL,
                                            StartedTimestamp INTEGER NOT NULL,
                                            CompletedTimestamp INTEGER NOT NULL,
                                            PRIMARY KEY (UserId, AcademicUnitId),
                                            FOREIGN KEY (UserId) REFERENCES Users(Id),
                                            FOREIGN KEY (AcademicUnitId) REFERENCES AcademicUnits(Id)
                                        );

                                        CREATE TABLE IF NOT EXISTS CompletedCourses 
                                        (
                                            UserId INTEGER NOT NULL,
                                            AcademicUnitId INTEGER NOT NULL,
                                            StartedTimestamp INTEGER NOT NULL,
                                            CompletedTimestamp INTEGER NOT NULL,
                                            PRIMARY KEY (UserId, AcademicUnitId),
                                            FOREIGN KEY (UserId) REFERENCES Users(Id),
                                            FOREIGN KEY (AcademicUnitId) REFERENCES AcademicUnits(Id)
                                        );

                                        CREATE TABLE IF NOT EXISTS AchievementProgress 
                                        (
                                            UserId INTEGER NOT NULL,
                                            AchievementId INTEGER NOT NULL,
                                            PercentCompleted INTEGER NOT NULL,
                                            LastUpdatedTimestamp INTEGER NOT NULL,
                                            PRIMARY KEY (UserId, AchievementId),
                                            FOREIGN KEY (UserId) REFERENCES Users(Id)
                                        );
                                        """;

    private const string PopulateTables = """
                                          INSERT OR IGNORE INTO AcademicUnits (Id, HierarchyPath, Title, Content, [Order]) VALUES 
                                             (1, NULL, 'Swift course', 'Swift course content', 0),
                                             (2, '1', 'Swift chapter 1', 'Swift chapter 1 content', 0),
                                             (3, '1.2', 'Swift chapter 1 lesson 1', 'Swift chapter 1 lesson 1 content',0),
                                             (4, '1.2', 'Swift chapter 1 lesson 2', 'Swift chapter 1 lesson 2 content',10),
                                             (5, '1.2', 'Swift chapter 1 lesson 3', 'Swift chapter 1 lesson 3 content',20),
                                             (6, '1', 'Swift chapter 2', 'Swift chapter 2content', 10),
                                             (7, '1.6', 'Swift chapter 2 lesson 1', 'Swift chapter 2 lesson 1 content', 30),
                                             (8, NULL, 'C# course', 'C# course content', 10),
                                             (9, NULL, 'Javascript course', 'Java course content', 20),
                                             (10, '9', 'Javascript chapter 1', 'Java chapter 1 content', 0),
                                             (11, '9.10', 'Javascript chapter 1 lesson 1', 'Java chapter 1 lesson 1 content', 0),
                                             (12, '9.10', 'Javascript chapter 1 lesson 2', 'Java chapter 1 lesson 2 content', 10),
                                             (13, '9.10', 'Javascript chapter 1 lesson 3', 'Java chapter 1 lesson 3 content', 20),
                                             (14, '8', 'C# chapter 1', 'C# chapter 1 content', 0),
                                             (15, '8.14', 'C# chapter 1 lesson 1', 'C# chapter 1 lesson 1 content', 0),
                                             (16, '8.14', 'C# chapter 1 lesson 2', 'C# chapter 1 lesson 2 content', 10),
                                             (17, '8.14', 'C# chapter 1 lesson 3', 'C# chapter 1 lesson 3 content', 20),
                                             (18, '8', 'C# chapter 2', 'C# chapter 2 content', 10),
                                             (19, '8.18', 'C# chapter 2 lesson 1', 'C# chapter 2 lesson 1 content', 30);
                                          
                                             INSERT OR IGNORE INTO Users (Id, Email) VALUES 
                                                 (1, 'test.user1@gmail.com'),
                                                 (2, 'test.user2@gmail.com'),
                                                 (3, 'test.user3@gmail.com');
                                          """;
}