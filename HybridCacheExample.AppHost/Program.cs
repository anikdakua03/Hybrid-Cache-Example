IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

var redisCache = builder.AddRedis("redis");

builder.AddProject<Projects.HybridCacheExample>("hybrid-cache-example")
        .WithReference(redisCache)
        .WaitFor(redisCache);

builder.Build().Run();