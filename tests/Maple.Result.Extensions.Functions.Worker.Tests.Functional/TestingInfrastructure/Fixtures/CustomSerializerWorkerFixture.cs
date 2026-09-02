using Azure.Core.Serialization;
using Maple.Result.Extensions.Functions.Worker.Tests.Functional.TestingInfrastructure.Application;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.Json;

namespace Maple.Result.Extensions.Functions.Worker.Tests.Functional.TestingInfrastructure.Fixtures;

/// <summary>
///     Hosts the worker services with a serializer other than the one registered by
///     the <c>ConfigureFunctionsWorkerDefaults</c> method, so that it can be asserted that the mapping
///     writes its bodies with the serializer configured for the worker rather than with one of its own.
/// </summary>
public sealed class CustomSerializerWorkerFixture : IDisposable
{
    #region read-only fields

    private readonly TestWorkerFactory _worker;

    #endregion

    #region constructors

    public CustomSerializerWorkerFixture()
    {
        _worker = TestWorkerFactory.Create(ConfigureServices);
    }

    #endregion

    internal HttpRequestData CreateRequest()
    {
        return _worker.CreateRequest();
    }

    public void Dispose()
    {
        _worker.Dispose();
    }

    #region helper methods

    private static void ConfigureServices(IServiceCollection services)
    {
        // Writes the members that would otherwise be omitted, which no default of this library produces,
        // so the serializer cannot be mistaken for the built-in fallback.
        var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = true
        };

        services.Configure<WorkerOptions>(options => options.Serializer = new JsonObjectSerializer(serializerOptions));
    }

    #endregion
}
