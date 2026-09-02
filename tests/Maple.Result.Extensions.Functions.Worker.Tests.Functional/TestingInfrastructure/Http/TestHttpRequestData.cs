using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Claims;

namespace Maple.Result.Extensions.Functions.Worker.Tests.Functional.TestingInfrastructure.Http;

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
