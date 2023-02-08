using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using dotnet_sample_action.Helpers;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using DotnetActionsToolkit;
using MongoDB.Driver;

namespace dotnet_sample_action.Containers;

public static class OcpiService
{
    private static readonly INetwork _ocpiNetwork;
    public static readonly IContainer _ocpiContainer;
    public static readonly IContainer _mongoContainer;
    private static readonly HttpClient _httpClient = new();
    public static IOutputConsumer mongoConsumer = Consume.RedirectStdoutAndStderrToStream(new MemoryStream(), new MemoryStream());
    public static IOutputConsumer ocpiConsumer = Consume.RedirectStdoutAndStderrToStream(new MemoryStream(), new MemoryStream());

    static OcpiService()
    {
        var mongoContainerName = RandomString.Generate(16);
        // var mongoConsumer = Consume.RedirectStdoutAndStderrToStream(new MemoryStream(), new MemoryStream());
        // var ocpiConsumer = Consume.RedirectStdoutAndStderrToStream(new MemoryStream(), new MemoryStream());
        
        _ocpiNetwork = new NetworkBuilder().WithName(Guid.NewGuid().ToString("D")).Build();
        
        _mongoContainer = new ContainerBuilder()
            .WithImage("mongo:latest")
            .WithName(mongoContainerName)
            .WithPortBinding(27017, true)
            .WithNetwork(_ocpiNetwork)
            .WithOutputConsumer(mongoConsumer)
            .WithEnvironment("MONGO_INITDB_ROOT_USERNAME", "root")
            .WithEnvironment("MONGO_INITDB_ROOT_PASSWORD", "root")
            .WithEnvironment("MONGO_INITDB_DATABASE", "cpo-ocpi-local")
            // .WithWaitStrategy(Wait.ForUnixContainer().UntilContainerIsHealthy())
            .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged(mongoConsumer.Stdout, "Waiting for connections"))
            .Build();

        _ocpiContainer = new ContainerBuilder()
            .WithImage("hello-world:latest")
            // .WithPortBinding(6001, true)
            .WithNetwork(_ocpiNetwork)
            // .WithEnvironment("ASPNETCORE_URLS", "http://+:6001")
            // .WithEnvironment("MongoDatabaseSettings__DatabaseName", "cpo-ocpi-local")
            // .WithEnvironment("ConnectionStrings__MongoDatabase", $"mongodb://root:root@{mongoContainerName}:27017/")
            // .WithEnvironment("ServiceBusNamespace", "test")
            .WithOutputConsumer(ocpiConsumer)
            // .WithWaitStrategy(Wait.ForUnixContainer().UntilContainerIsHealthy())
            // .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged(ocpiConsumer.Stdout, "Now listening on"))
            .Build();
    }

    public static HttpClient GetHttpClient()
    {
        _httpClient.BaseAddress ??= new Uri($"http://localhost:{_ocpiContainer.GetMappedPublicPort(6001).ToString()}");
        return _httpClient;
    }

    public static IMongoDatabase GetMongoDb()
    {
        var client = new MongoClient($"mongodb://root:root@localhost:{_mongoContainer.GetMappedPublicPort(27017).ToString()}/");
        return client.GetDatabase("cpo-ocpi-local");
    }

    public static async Task StartAsync()
    {
        await _ocpiNetwork.CreateAsync();
        await _mongoContainer.StartAsync();
        await _ocpiContainer.StartAsync();
    }

    public static async Task StopAsync()
    {
        await _ocpiContainer.StopAsync();
        await _mongoContainer.StopAsync();
        await _ocpiNetwork.DeleteAsync();
    }
}
