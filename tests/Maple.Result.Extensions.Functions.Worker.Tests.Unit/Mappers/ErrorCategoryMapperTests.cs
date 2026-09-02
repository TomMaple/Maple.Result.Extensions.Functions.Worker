using System;
using System.Linq;
using System.Net;
using Sut = Maple.Result.Extensions.Functions.Worker.Mappers.ErrorCategoryMapper;

namespace Maple.Result.Extensions.Functions.Worker.Tests.Unit.Mappers;

public class ErrorCategoryMapperTests
{
    [Theory]
    [InlineData(ErrorCategory.Validation, HttpStatusCode.BadRequest)]
    [InlineData(ErrorCategory.Unauthenticated, HttpStatusCode.Unauthorized)]
    [InlineData(ErrorCategory.Unauthorized, HttpStatusCode.Forbidden)]
    [InlineData(ErrorCategory.NotFound, HttpStatusCode.NotFound)]
    [InlineData(ErrorCategory.Timeout, HttpStatusCode.RequestTimeout)]
    [InlineData(ErrorCategory.Conflict, HttpStatusCode.Conflict)]
    [InlineData(ErrorCategory.Failure, HttpStatusCode.UnprocessableEntity)]
    [InlineData(ErrorCategory.Critical, HttpStatusCode.InternalServerError)]
    [InlineData(ErrorCategory.NotImplemented, HttpStatusCode.NotImplemented)]
    [InlineData(ErrorCategory.Unavailable, HttpStatusCode.ServiceUnavailable)]
    public void GetStatusCode_KnownCategory_ReturnsMatchingStatusCode(ErrorCategory category,
        HttpStatusCode expectedStatusCode)
    {
        // Act
        var result = Sut.GetStatusCode(category);

        // Assert
        result.ShouldBe(expectedStatusCode);
    }

    /// <summary>
    ///     Guards against a new <see cref="ErrorCategory" /> member being added by the Maple.Result package
    ///     without a matching arm being added to the mapper, which would otherwise surface only at run time,
    ///     as an unhandled exception thrown from the error handling path itself.
    /// </summary>
    [Fact]
    public void GetStatusCode_EveryDeclaredCategory_IsMapped()
    {
        // Arrange
        var categories = Enum.GetValues<ErrorCategory>();

        // Act
        var unmappedCategories = categories
            .Where(category => Record.Exception(() => Sut.GetStatusCode(category)) is not null)
            .ToArray();

        // Assert
        unmappedCategories.ShouldBeEmpty(
            $"Every declared {nameof(ErrorCategory)} value must be mapped to a status code, "
            + $"but the mapper throws for: {string.Join(", ", unmappedCategories)}.");
    }

    [Fact]
    public void GetStatusCode_UndeclaredCategory_ThrowsNotImplementedException()
    {
        // Arrange
        const ErrorCategory UndeclaredCategory = (ErrorCategory)int.MaxValue;

        // Act & Assert
        Should.Throw<NotImplementedException>(() => Sut.GetStatusCode(UndeclaredCategory));
    }
}
