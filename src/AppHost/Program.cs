var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.ApiService>("apiservice");

builder.AddProject<Projects.DslCopilot_Web>("webfrontend")
    .WithReference(apiService);

builder.Build().Run();
