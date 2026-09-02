using Microsoft.Azure.Functions.Worker;
using System;
using System.Collections.Generic;

namespace Maple.Result.Extensions.Functions.Worker.Tests.Functional.TestingInfrastructure.Http;

/// <summary>
///     A minimal <see cref="FunctionContext" /> exposing a real service provider, which is the only member
///     the mapping relies on. Every other member is unused by the mapping and throws when it is touched,
///     so that an accidental dependency on it does not pass unnoticed.
/// </summary>
internal sealed class TestFunctionContext : FunctionContext
{
    #region constructors

    internal TestFunctionContext(IServiceProvider instanceServices)
    {
        InstanceServices = instanceServices;
    }

    #endregion

    public override IServiceProvider InstanceServices { get; set; }

    public override IDictionary<object, object> Items { get; set; } = new Dictionary<object, object>();

    public override string InvocationId => "test-invocation";

    public override string FunctionId => "test-function";

    public override TraceContext TraceContext => throw new NotSupportedException();

    public override BindingContext BindingContext => throw new NotSupportedException();

    public override RetryContext RetryContext => throw new NotSupportedException();

    public override FunctionDefinition FunctionDefinition => throw new NotSupportedException();

    public override IInvocationFeatures Features => throw new NotSupportedException();
}
