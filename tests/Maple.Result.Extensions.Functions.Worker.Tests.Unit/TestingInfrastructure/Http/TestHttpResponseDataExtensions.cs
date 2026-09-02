using Microsoft.Azure.Functions.Worker.Http;

namespace Maple.Result.Extensions.Functions.Worker.Tests.Unit.TestingInfrastructure.Http;

/// <summary>
///     The convenience extension methods reading back a response produced by the mapping, which is always
///     a <see cref="TestHttpResponseData" /> because that is what the test request creates.
/// </summary>
internal static class TestHttpResponseDataExtensions
{
    /// <summary>
    ///     Reads the whole body written to the response as UTF-8 text.
    /// </summary>
    internal static string ReadBodyAsJson(this HttpResponseData response)
    {
        return ((TestHttpResponseData)response).ReadBody();
    }
}
