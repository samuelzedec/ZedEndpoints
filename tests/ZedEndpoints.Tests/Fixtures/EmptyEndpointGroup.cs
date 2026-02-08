using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using ZedEndpoints.Abstractions;

namespace ZedEndpoints.Tests.Fixtures;

/// <summary>
/// Empty endpoint group for testing groups without endpoints.
/// </summary>
public sealed class EmptyEndpointGroup : IEndpointGroup
{
    public void MapGroup(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/empty");
    }
}