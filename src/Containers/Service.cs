using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using dotnet_sample_action.Helpers;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using MongoDB.Driver;

namespace dotnet_sample_action.Containers;

public static class Service
{
    private static readonly INetwork _Network;
    public static readonly IContainer _Container;
    public static readonly IContainer _mongoContainer;
    private static readonly HttpClient _httpClient = new();
    public static IOutputConsumer mongoConsumer = Consume.RedirectStdoutAndStderrToStream(new MemoryStream(), new MemoryStream());
    public static IOutputConsumer Consumer = Consume.RedirectStdoutAndStderrToStream(new MemoryStream(), new MemoryStream());

    static Service()
    {
        var mongoContainerName = RandomString.Generate(16);
        // var mongoConsumer = Consume.RedirectStdoutAndStderrToStream(new MemoryStream(), new MemoryStream());
        // var Consumer = Consume.RedirectStdoutAndStderrToStream(new MemoryStream(), new MemoryStream());
        
        _Network = new NetworkBuilder().WithName(Guid.NewGuid().ToString("D")).Build();
        
        _mongoContainer = new ContainerBuilder()
            .WithImage("mongo:latest")
            .WithName(mongoContainerName)
            .WithPortBinding(27017, true)
            .WithNetwork(_Network)
            .WithOutputConsumer(mongoConsumer)
            .WithEnvironment("MONGO_INITDB_ROOT_USERNAME", "root")
            .WithEnvironment("MONGO_INITDB_ROOT_PASSWORD", "root")
            .WithEnvironment("MONGO_INITDB_DATABASE", "local")
            // .WithWaitStrategy(Wait.ForUnixContainer().UntilContainerIsHealthy())
            .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged(mongoConsumer.Stdout, "Waiting for connections"))
            .Build();

        _Container = new ContainerBuilder()
            .WithImage("hello-world:latest")
            // .WithPortBinding(6001, true)
            .WithNetwork(_Network)
            // .WithEnvironment("ASPNETCORE_URLS", "http://+:6001")
            // .WithEnvironment("MongoDatabaseSettings__DatabaseName", "local")
            // .WithEnvironment("ConnectionStrings__MongoDatabase", $"mongodb://root:root@{mongoContainerName}:27017/")
            // .WithEnvironment("ServiceBusNamespace", "test")
            .WithOutputConsumer(Consumer)
            // .WithWaitStrategy(Wait.ForUnixContainer().UntilContainerIsHealthy())
            // .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged(ocpiConsumer.Stdout, "Now listening on"))
            .Build();
    }

    public static HttpClient GetHttpClient()
    {
        _httpClient.BaseAddress ??= new Uri($"http://localhost:{_Container.GetMappedPublicPort(6001).ToString()}");
        return _httpClient;
    }

    public static IMongoDatabase GetMongoDb()
    {
        var client = new MongoClient($"mongodb://root:root@localhost:{_mongoContainer.GetMappedPublicPort(27017).ToString()}/");
        return client.GetDatabase("local");
    }

    public static async Task StartAsync()
    {
        await _Network.CreateAsync();
        await _mongoContainer.StartAsync();
        await _Container.StartAsync();
    }

    public static async Task StopAsync()
    {
        await _Container.StopAsync();
        await _mongoContainer.StopAsync();
        await _Network.DeleteAsync();
    }
}
