using Microsoft.AspNetCore.Routing;
using ZedEndpoints.Extensions;

namespace ZedEndpoints.Abstractions;

/// <summary>
/// Defines a contract for registering individual endpoints in an ASP.NET Core application.
/// </summary>
/// <remarks>
/// Implementations of this interface use static abstract members to define endpoint mappings
/// without requiring instance creation. This enables compile-time registration and zero-allocation
/// endpoint discovery.
/// <para>
/// Example usage:
/// <code>
/// public class CreateUserEndpoint : IEndpoint
/// {
///     public static void Map(IEndpointRouteBuilder app)
///     {
///         app.MapPost("/users", async (CreateUserRequest request) =>
///         {
///             // Handler logic
///             return Results.Created($"/users/{id}", user);
///         })
///         .WithName("CreateUser")
///         .WithTags("Users");
///     }
/// }
/// </code>
/// </para>
/// </remarks>
public interface IEndpoint
{
    /// <summary>
    /// Maps and registers the endpoint to the specified route builder.
    /// </summary>
    /// <param name="app">The endpoint route builder used to register HTTP endpoints.</param>
    /// <remarks>
    /// This method is called automatically during application startup by the
    /// <see cref="WebApplicationExtensions.MapEndpointGroups"/> extension method.
    /// </remarks>
    static abstract void Map(IEndpointRouteBuilder app);
}