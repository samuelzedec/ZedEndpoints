using Microsoft.AspNetCore.Routing;

namespace ZedEndpoints.Abstractions;

/// <summary>
/// Defines a contract for grouping and organizing related endpoints with shared configuration.
/// </summary>
/// <remarks>
/// Implementations of this interface are used to organize endpoints by feature or domain area,
/// applying common settings such as route prefixes, authorization requirements, tags, and CORS policies.
/// <para>
/// Endpoint groups are automatically discovered and registered during application startup
/// by the <see cref="WebApplicationExtensions.MapEndpointGroups"/> extension method.
/// </para>
/// <para>
/// Example usage:
/// <code>
/// public class ProductEndpoints : IEndpointGroup
/// {
///     public void MapGroup(IEndpointRouteBuilder app)
///     {
///         var group = app.MapGroup("api/v1/products")
///             .WithTags("Products")
///             .RequireAuthorization();
///
///         group.MapEndpoint&lt;CreateProductEndpoint&gt;()
///              .MapEndpoint&lt;GetProductEndpoint&gt;()
///              .MapEndpoint&lt;UpdateProductEndpoint&gt;();
///     }
/// }
/// </code>
/// </para>
/// </remarks>
public interface IEndpointGroup
{
    /// <summary>
    /// Configures and registers a group of related endpoints with shared settings.
    /// </summary>
    /// <param name="app">The endpoint route builder used to create route groups and register endpoints.</param>
    /// <remarks>
    /// This method is called automatically during application startup. Use it to:
    /// <list type="bullet">
    ///     <item><description>Create route groups with <see cref="EndpointRouteBuilderExtensions.MapGroup"/></description></item>
    ///     <item><description>Apply shared configuration (tags, authorization, rate limiting, etc.)</description></item>
    ///     <item><description>Register individual endpoints using <see cref="EndpointRouteBuilderExtensions.MapEndpoint&lt;TEndpoint&gt;"/></description></item>
    /// </list>
    /// </remarks>
    void MapGroup(IEndpointRouteBuilder app);
}