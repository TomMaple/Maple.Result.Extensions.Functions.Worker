using Azure.Core.Serialization;
using Maple.Result.Extensions.Functions.Worker.Tests.Functional.TestingInfrastructure.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.Json;

namespace Maple.Result.Extensions.Functions.Worker.Tests.Functional.TestingInfrastructure.Application;

/// <summary>
///     Builds the service provider an Azure Functions isolated worker would expose through the
///     <see cref="FunctionContext.InstanceServices" /> property, and creates the requests the functions
///     covered by the functional tests are invoked with.
/// </summary>
/// <remarks>
///     The isolated worker model has no in-memory test host: driving a real HTTP trigger requires the
///     Azure Functions host communicating with the worker over gRPC. The functions are therefore invoked
///     directly, with a request backed by the same worker services and the same serializer the host
///     would have configured.
/// </remarks>
internal sealed class TestWorkerFactory : IDisposable
{
    #region read-only fields

    private readonly ServiceProvider _services;

    #endregion

    #region constructors

    private TestWorkerFactory(ServiceProvider services)
    {
        _services = services;
    }

    #endregion

    internal static TestWorkerFactory Create(Action<IServiceCollection>? configureServices = null)
    {
        var services = new ServiceCollection();

        services.AddOptions();

        // The same serializer the ConfigureFunctionsWorkerDefaults method registers.
        services.Configure<WorkerOptions>(options =>
            options.Serializer = new JsonObjectSerializer(new JsonSerializerOptions(JsonSerializerDefaults.Web)));

        configureServices?.Invoke(services);

        return new TestWorkerFactory(services.BuildServiceProvider());
    }

    internal HttpRequestData CreateRequest()
    {
        return TestHttpRequestData.Create(_services);
    }

    public void Dispose()
    {
        _services.Dispose();
    }
}
