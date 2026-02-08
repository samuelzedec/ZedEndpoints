using Microsoft.AspNetCore.Routing;
using ZedEndpoints.Abstractions;

namespace ZedEndpoints.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IEndpointRouteBuilder"/> to register endpoints.
/// </summary>
public static class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// Registers an endpoint that implements <see cref="IEndpoint"/> to the route builder.
    /// </summary>
    /// <typeparam name="TEndpoint">
    /// The endpoint type to register. Must implement <see cref="IEndpoint"/>.
    /// </typeparam>
    /// <param name="app">The endpoint route builder to register the endpoint with.</param>
    /// <returns>
    /// The same <see cref="IEndpointRouteBuilder"/> instance for method chaining.
    /// </returns>
    /// <remarks>
    /// This method invokes the static <see cref="IEndpoint.Map"/> method on the specified endpoint type,
    /// allowing compile-time endpoint registration without instance allocation.
    /// <para>
    /// Example:
    /// <code>
    /// app.MapGroup("api/users")
    ///    .MapEndpoint&lt;CreateUserEndpoint&gt;()
    ///    .MapEndpoint&lt;GetUserEndpoint&gt;()
    ///    .MapEndpoint&lt;DeleteUserEndpoint&gt;();
    /// </code>
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <typeparamref name="TEndpoint"/> does not properly implement <see cref="IEndpoint"/>.
    /// </exception>
    public static IEndpointRouteBuilder MapEndpoint<TEndpoint>(this IEndpointRouteBuilder app)
        where TEndpoint : IEndpoint
    {
        TEndpoint.Map(app);
        return app;
    }
}