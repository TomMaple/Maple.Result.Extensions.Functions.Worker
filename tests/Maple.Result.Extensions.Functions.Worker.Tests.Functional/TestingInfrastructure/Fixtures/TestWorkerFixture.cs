using Maple.Result.Extensions.Functions.Worker.Configuration;
using Maple.Result.Extensions.Functions.Worker.Tests.Functional.TestingInfrastructure.Application;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Maple.Result.Extensions.Functions.Worker.Tests.Functional.TestingInfrastructure.Fixtures;

/// <summary>
///     Hosts the worker services the functions are invoked with. The derived fixtures register
///     the custom result mappings by overriding the <see cref="ConfigureResultMapping" /> property.
/// </summary>
public class TestWorkerFixture : IDisposable
{
    #region read-only fields

    private readonly TestWorkerFactory _worker;

    #endregion

    #region constructors

    public TestWorkerFixture()
    {
        _worker = TestWorkerFactory.Create(ConfigureServices);
    }

    #endregion

    private protected virtual Action<ResultMappingOptions>? ConfigureResultMapping => null;

    /// <summary>
    ///     Creates a request backed by the worker services of this fixture.
    /// </summary>
    internal HttpRequestData CreateRequest()
    {
        return _worker.CreateRequest();
    }

    public void Dispose()
    {
        _worker.Dispose();

        GC.SuppressFinalize(this);
    }

    #region helper methods

    private void ConfigureServices(IServiceCollection services)
    {
        if (ConfigureResultMapping is not null)
            services.ConfigureResultMapping(ConfigureResultMapping);
    }

    #endregion
}
