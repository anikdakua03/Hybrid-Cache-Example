var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.HybridCacheExample>("hybrid-cache-example");

builder.Build().Run();