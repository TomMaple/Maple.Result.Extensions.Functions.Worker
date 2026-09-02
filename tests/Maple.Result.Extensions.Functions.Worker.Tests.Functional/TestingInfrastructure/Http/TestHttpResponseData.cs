using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace Maple.Result.Extensions.Functions.Worker.Tests.Functional.TestingInfrastructure.Http;

/// <summary>
///     An in-memory <see cref="HttpResponseData" /> that keeps the written body in a
///     <see cref="MemoryStream" />, so that it can be read back and asserted on.
/// </summary>
internal sealed class TestHttpResponseData : HttpResponseData
{
    #region constructors

    internal TestHttpResponseData(FunctionContext functionContext, HttpStatusCode statusCode)
        : base(functionContext)
    {
        StatusCode = statusCode;
    }

    #endregion

    public override HttpStatusCode StatusCode { get; set; }

    public override HttpHeadersCollection Headers { get; set; } = [];

    public override Stream Body { get; set; } = new MemoryStream();

    public override HttpCookies Cookies => throw new NotSupportedException();

    /// <summary>
    ///     Reads the whole body written to the response as UTF-8 text.
    /// </summary>
    internal string ReadBody()
    {
        Body.Position = 0;

        using var reader = new StreamReader(Body, Encoding.UTF8, leaveOpen: true);

        return reader.ReadToEnd();
    }

    /// <summary>
    ///     Returns the single value of the <c>Content-Type</c> header, or <see langword="null" /> when
    ///     the header is not present.
    /// </summary>
    internal string? GetContentType()
    {
        return Headers.TryGetValues("Content-Type", out var values)
            ? string.Join(", ", values)
            : null;
    }
}
