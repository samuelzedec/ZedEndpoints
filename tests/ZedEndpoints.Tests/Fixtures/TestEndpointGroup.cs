using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ZedEndpoints.Abstractions;
using ZedEndpoints.Extensions;

namespace ZedEndpoints.Tests.Fixtures;

/// <summary>
/// Test endpoint group that maps multiple endpoints.
/// </summary>
public sealed class TestEndpointGroup : IEndpointGroup
{
    public void MapGroup(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/test")
            .WithTags("Test");

        group.MapEndpoint<TestEndpoint>()
            .MapEndpoint<AnotherTestEndpoint>();
    }
}