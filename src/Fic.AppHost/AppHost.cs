using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var useRedis = builder.Configuration.GetValue("Features:UseRedis", false);
var useExternalEvents = builder.Configuration.GetValue("Features:UseExternalEvents", false);

var web = builder.AddProject<Projects.Fic_Platform_Web>("web")
    .WithExternalHttpEndpoints()
    .WithEnvironment("Features__UseRedis", useRedis ? "true" : "false")
    .WithEnvironment("Features__UseExternalEvents", useExternalEvents ? "true" : "false");

var workers = builder.AddProject<Projects.Fic_Platform_Workers>("workers")
    .WithEnvironment("Features__UseRedis", useRedis ? "true" : "false")
    .WithEnvironment("Features__UseExternalEvents", useExternalEvents ? "true" : "false");

if (useRedis)
{
    var cache = builder.AddRedis("cache");
    web.WithReference(cache);
    workers.WithReference(cache);
}

builder.Build().Run();
