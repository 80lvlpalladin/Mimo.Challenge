using Mimo.Challenge.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var databaseResourceBuilder = builder
    .AddSeededSqlite("Database")
    .WithSqliteWeb();

builder
    .AddProject<Projects.Mimo_Challenge_API>("API")
    .WithReference(databaseResourceBuilder)
    .WaitFor(databaseResourceBuilder);

builder.Build().Run();