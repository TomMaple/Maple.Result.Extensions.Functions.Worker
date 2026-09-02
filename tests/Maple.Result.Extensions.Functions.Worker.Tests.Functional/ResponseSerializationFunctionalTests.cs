using Maple.Result.Extensions.Functions.Worker.Tests.Functional.TestingInfrastructure.Application;
using Maple.Result.Extensions.Functions.Worker.Tests.Functional.TestingInfrastructure.Fixtures;
using Maple.Result.Extensions.Functions.Worker.Tests.Functional.TestingInfrastructure.Http;

namespace Maple.Result.Extensions.Functions.Worker.Tests.Functional;

/// <summary>
///     Asserts that the bodies are written with the serializer configured for the worker, so that a response
///     produced by this library is serialized exactly like one written by the <c>WriteAsJsonAsync</c> method
///     of the worker SDK in the same application.
/// </summary>
public class ResponseSerializationFunctionalTests : IClassFixture<CustomSerializerWorkerFixture>
{
    #region consts

    // The line ending both the response body and the expected JSON literal are normalized to,
    // because the indenting serializer writes the line ending of the platform it runs on.
    private const string LineFeed = "\n";

    #endregion

    #region read-only fields

    private readonly CustomSerializerWorkerFixture _fixture;

    #endregion

    #region constructors

    public ResponseSerializationFunctionalTests(CustomSerializerWorkerFixture fixture)
    {
        _fixture = fixture;
    }

    #endregion

    #region tests

    [Fact]
    public void ToHttpResponseData_SuccessfulResultWithValue_WritesTheValueWithTheWorkerSerializer()
    {
        // Arrange
        var request = _fixture.CreateRequest();

        // Act
        var response = TestFunctions.SuccessValue(request);

        // Assert
        const string ExpectedJson =
            """
            {
              "id": 13,
              "name": "Test value"
            }
            """;

        response.ReadBodyAsJson().ReplaceLineEndings(LineFeed)
            .ShouldBe(ExpectedJson.ReplaceLineEndings(LineFeed));
    }

    [Fact]
    public void ToHttpResponseData_Error_WritesTheProblemDetailsWithTheWorkerSerializer()
    {
        // Arrange
        var request = _fixture.CreateRequest();

        // Act
        var response = TestFunctions.FailureError(request);

        // Assert
        const string ExpectedJson =
            """
            {
              "type": "tag:test.com,2026:failure",
              "title": "Failure title",
              "status": 422,
              "detail": "Failure detail.",
              "instance": "https://test.com/instances/failure"
            }
            """;

        response.ReadBodyAsJson().ReplaceLineEndings(LineFeed)
            .ShouldBe(ExpectedJson.ReplaceLineEndings(LineFeed));
    }

    #endregion
}
