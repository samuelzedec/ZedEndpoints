using Microsoft.AspNetCore.Routing;
using ZedEndpoints.Abstractions;

namespace ZedEndpoints.Tests.Fixtures;

/// <summary>
/// Abstract endpoint group that should NOT be discovered.
/// </summary>
public abstract class AbstractTestEndpointGroup : IEndpointGroup
{
    public abstract void MapGroup(IEndpointRouteBuilder app);
}