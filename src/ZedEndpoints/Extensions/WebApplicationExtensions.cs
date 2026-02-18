using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using ZedEndpoints.Abstractions;
using ZedEndpoints.Attributes;

namespace ZedEndpoints.Extensions;

/// <summary>
/// Provides extension methods for <see cref="WebApplication"/> to enable automatic endpoint discovery.
/// </summary>
public static class WebApplicationExtensions
{
    private static readonly ConditionalWeakTable<WebApplication, HashSet<Assembly>> ProcessedAssembliesPerApp = new();

    /// <summary>
    /// Automatically discovers and registers all endpoint groups in the application.
    /// </summary>
    /// <param name="app">The web application to register endpoint groups with.</param>
    /// <param name="assembly">
    /// Optional assembly to scan for endpoint groups. 
    /// If null, uses the entry assembly of the application.
    /// </param>
    /// <param name="globalPrefix">
    /// Optional global route prefix to prepend to all endpoint groups.
    /// For example, passing <c>"api/v1"</c> will prefix all routes with <c>/api/v1</c>.
    /// If null, no prefix is applied.
    /// </param>
    /// <returns>
    /// The same <see cref="WebApplication"/> instance for method chaining.
    /// </returns>
    /// <remarks>
    /// This method scans the specified assembly (or the entry assembly if not provided) 
    /// for all concrete classes that implement <see cref="IEndpointGroup"/>, creates instances 
    /// of them, and invokes their <see cref="IEndpointGroup.MapGroup"/> methods to register endpoints.
    /// <para>
    /// Abstract classes and interfaces are automatically excluded from discovery.
    /// </para>
    /// <para>
    /// This method should be called once during application startup, typically in Program.cs:
    /// <code>
    /// var app = builder.Build();
    /// app.MapEndpointGroups();  // Auto-discovers endpoint groups from entry assembly
    /// // or
    /// app.MapEndpointGroups("api/v1");  // All routes prefixed with /api/v1
    /// // or
    /// app.MapEndpointGroups("api/v1", typeof(MyEndpointGroup).Assembly);  // Prefix + specific assembly
    /// app.Run();
    /// </code>
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the entry assembly cannot be determined when <paramref name="assembly"/> is null,
    /// or if an endpoint group cannot be instantiated (e.g., missing parameterless constructor).
    /// </exception>
    public static WebApplication MapEndpointGroups(
        this WebApplication app,
        Assembly? assembly = null,
        string? globalPrefix = null)
    {
        assembly ??= Assembly.Load(app.Environment.ApplicationName);

        var processedAssemblies = ProcessedAssembliesPerApp.GetOrCreateValue(app);
        lock (processedAssemblies)
        {
            if (!processedAssemblies.Add(assembly)) return app;
        }

        var groupTypes = assembly
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsAssignableTo(typeof(IEndpointGroup)));

        IEndpointRouteBuilder builder = globalPrefix is not null
            ? app.MapGroup(globalPrefix)
            : app;

        foreach (var groupType in groupTypes)
        {
            var instance = Activator.CreateInstance(groupType) as IEndpointGroup;
            var hasNoPrefix = groupType.IsDefined(typeof(NoGlobalPrefixAttribute), inherit: false);

            var targetBuilder = hasNoPrefix ? app : builder;
            instance?.MapGroup(targetBuilder);
        }

        return app;
    }
}