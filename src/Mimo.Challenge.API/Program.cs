using System.Threading.Channels;
using Microsoft.Extensions.Caching.Memory;
using Mimo.Challenge.API;
using Mimo.Challenge.API.Extensions;
using Mimo.Challenge.DAL;
using Mimo.Challenge.Domain;
using Mimo.Challenge.Features;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables();

builder.AddTelemetry();

builder.Services
    .AddOpenApi()
    .AddExceptionHandler<GlobalExceptionHandler>()
    .AddConcurrencyRateLimiter(builder.Configuration)
    .AddSingleton<MemoryCache>()
    .AddChannels(builder.Configuration)
    .AddDataAccessLayerServices(builder.Configuration)
    .AddDomainServices()
    .AddFeaturesServices();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();
app.MapEndpoints();

app.Run();