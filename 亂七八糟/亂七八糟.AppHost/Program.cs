IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<RedisResource> cache = builder.AddRedis("cache");

IResourceBuilder<ProjectResource> apiService = builder.AddProject<Projects.亂七八糟_ApiService>("apiservice");

builder.AddProject<Projects.亂七八糟_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
