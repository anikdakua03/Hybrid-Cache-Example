using HybridCacheExample.AppHost;
using Microsoft.Extensions.Hosting;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

var redisCache = builder.AddRedis("redis");

var weatherApi = builder.AddProject<Projects.HybridCacheExample>("hybrid-cache-example")
        .WithReference(redisCache)
        .WaitFor(redisCache)
        .WithUrl("http://localhost:5210", "API")
        .WithSwaggerUI()
        .WithScalar();

var functionAPI = builder.AddAzureFunctionsProject<Projects.HybridCacheFunction>("hybridcachefunction")
        ;

var frontend = builder.AddNpmApp("angular-with-material", "../angular-with-material")
    .WithReference(weatherApi)
    .WithHttpEndpoint(env: "PORT")
    .WithUrl("http://localhost:4200", "Angular UI")
    //.WithEndpoint(port:4200, scheme: "http", env: "PORT")
    .WithExternalHttpEndpoints();

var launchProfile = builder.Configuration["DOTNET_LAUNCH_PROFILE"];

if (builder.Environment.IsDevelopment() && launchProfile == "https")
{
    //frontend.RunWithHttpsDevCertificate("HTTPS_CERT_FILE", "HTTPS_CERT_KEY_FILE");
}

builder.Build().Run();