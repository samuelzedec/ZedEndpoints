using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ZedEndpoints.Abstractions;
using ZedEndpoints.Attributes;

namespace ZedEndpoints.Tests.Fixtures;

/// <summary>
/// Endpoint group that should NOT receive the global prefix.
/// </summary>
[NoGlobalPrefix]
public sealed class NoGlobalPrefixEndpointGroup : IEndpointGroup
{
    public void MapGroup(IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Results.Ok())
            .WithName("HealthEndpoint");
    }
}