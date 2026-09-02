using Azure.Core.Serialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace Maple.Result.Extensions.Functions.Worker.Tests.Unit.TestingInfrastructure.Http;

/// <summary>
///     An in-memory <see cref="HttpRequestData" /> creating <see cref="TestHttpResponseData" /> instances,
///     which stands in for the request the Azure Functions host would otherwise provide.
/// </summary>
internal sealed class TestHttpRequestData : HttpRequestData
{
    #region constructors

    private TestHttpRequestData(FunctionContext functionContext)
        : base(functionContext)
    {
    }

    #endregion

    public override Stream Body => new MemoryStream();

    public override HttpHeadersCollection Headers { get; } = [];

    public override IReadOnlyCollection<IHttpCookie> Cookies => [];

    public override Uri Url { get; } = new("https://test.com/api/test");

    public override IEnumerable<ClaimsIdentity> Identities => [];

    public override string Method => "GET";

    /// <summary>
    ///     Creates a request backed by a service provider built from the given service registrations,
    ///     on top of the worker services the <c>ConfigureFunctionsWorkerDefaults</c> method registers.
    /// </summary>
    internal static TestHttpRequestData Create(Action<IServiceCollection>? configureServices = null)
    {
        return CreateWithoutWorkerDefaults(services =>
        {
            // The mapping writes its bodies with the WriteAsJsonAsync method of the worker SDK, which
            // requires the serializer the ConfigureFunctionsWorkerDefaults method would have registered.
            services.Configure<WorkerOptions>(options =>
                options.Serializer = new JsonObjectSerializer(new JsonSerializerOptions(JsonSerializerDefaults.Web)));

            configureServices?.Invoke(services);
        });
    }

    /// <summary>
    ///     Creates a request backed by a service provider that has no worker serializer registered, so that
    ///     the behaviour of a misconfigured worker can be asserted.
    /// </summary>
    internal static TestHttpRequestData CreateWithoutWorkerDefaults(Action<IServiceCollection>? configureServices = null)
    {
        var services = new ServiceCollection();
        services.AddOptions();
        configureServices?.Invoke(services);

        return Create(services.BuildServiceProvider());
    }

    /// <summary>
    ///     Creates a request backed by the given service provider.
    /// </summary>
    internal static TestHttpRequestData Create(IServiceProvider instanceServices)
    {
        return new TestHttpRequestData(new TestFunctionContext(instanceServices));
    }

    public override HttpResponseData CreateResponse()
    {
        return new TestHttpResponseData(FunctionContext, HttpStatusCode.OK);
    }
}
