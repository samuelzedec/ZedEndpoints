using Microsoft.AspNetCore.Routing;

namespace ZedEndpoints.Extensions;

internal static class EndpointBuilderContext
{
    internal static readonly AsyncLocal<IEndpointRouteBuilder?> RootBuilder = new();
    internal static readonly AsyncLocal<HashSet<Type>?> RegisteredEndpoints = new();
}
