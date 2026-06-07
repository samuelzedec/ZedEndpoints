# ZedEndpoints

[![NuGet](https://img.shields.io/nuget/v/ZedEndpoints.svg)](https://www.nuget.org/packages/ZedEndpoints/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/ZedEndpoints.svg)](https://www.nuget.org/packages/ZedEndpoints/)

A minimal library for automatic endpoint discovery and organization in ASP.NET Core Minimal APIs.

## Why ZedEndpoints

Carter and similar libraries rely on reflection-based discovery that requires
public classes. This prevents using C#'s `file` and `internal` modifiers,
which are essential for proper encapsulation in feature-based architectures.

ZedEndpoints solves this by combining explicit registration within groups
and automatic group discovery — giving you encapsulation without sacrificing
convenience.

```
// Carter — requires public
public sealed class CreateUserEndpoint : ICarterModule { ... }

// ZedEndpoints — file modifier works
file sealed class CreateUserEndpoint : IEndpoint { ... }
```

## Features

- Automatic endpoint discovery via reflection
- Group-based endpoint organization with shared configuration
- Standalone endpoint discovery — register `IEndpoint` implementations without an `IEndpointGroup`
- Optional global route prefix for all endpoint groups and standalone endpoints
- Per-group/per-endpoint prefix override via `[NoGlobalPrefix]` attribute
- Type-safe interfaces and generics
- Fluent API with method chaining support
- Idempotent operations (safe to call multiple times)
- Thread-safe implementation

## Installation
```bash
dotnet add package ZedEndpoints
```

## Quick Start

### Define an Endpoint
```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ZedEndpoints.Abstractions;

public class GetProductEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/{id}", (int id) => 
        {
            return Results.Ok(new { Id = id, Name = "Product" });
        })
        .WithName("GetProduct")
        .WithTags("Products");
    }
}
```

### Create an Endpoint Group
```csharp
using Microsoft.AspNetCore.Routing;
using ZedEndpoints.Abstractions;
using ZedEndpoints.Extensions;

public class ProductEndpoints : IEndpointGroup
{
    public void MapGroup(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/products")
            .WithTags("Products")
            .RequireAuthorization();

        group.MapEndpoint<GetProductEndpoint>()
             .MapEndpoint<CreateProductEndpoint>()
             .MapEndpoint<UpdateProductEndpoint>()
             .MapEndpoint<DeleteProductEndpoint>();
    }
}
```

### Register in Program.cs
```csharp
using ZedEndpoints.Extensions;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Automatic discovery and registration
app.MapEndpointGroups();

app.Run();
```

## Usage Patterns

### Manual Endpoint Registration
```csharp
app.MapEndpoint<GetProductEndpoint>();
```

### Manual Group Registration
```csharp
var productGroup = new ProductEndpoints();
productGroup.MapGroup(app);
```

### Working with Route Groups
```csharp
var apiV1 = app.MapGroup("/api/v1");
apiV1.MapEndpoint<GetProductEndpoint>();
```

### Standalone Endpoints (Without a Group)
Any class implementing `IEndpoint` that is not registered through an `IEndpointGroup` is automatically discovered and registered by `MapEndpointGroups`. No group is required:
```csharp
// No IEndpointGroup needed — discovered automatically
public class HealthCheckEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Results.Ok())
            .WithName("HealthCheck");
    }
}
```

Endpoints already registered through a group are tracked and skipped during standalone discovery, so there is no risk of double registration.

### Scanning Specific Assembly
```csharp
app.MapEndpointGroups(typeof(ProductEndpoints).Assembly);
```

### Global Route Prefix
Apply a common prefix to all discovered endpoint groups at once:
```csharp
// All routes will be prefixed with /api/v1
app.MapEndpointGroups(globalPrefix: "api/v1");

// Combined with a specific assembly
app.MapEndpointGroups(typeof(ProductEndpoints).Assembly, globalPrefix: "api/v1");
```

This is useful for applying a consistent API versioning prefix without modifying each group individually. For example, a group that maps `/products` will automatically become `/api/v1/products`.

### Opting Out of the Global Prefix
Use the `[NoGlobalPrefix]` attribute on a group or standalone endpoint to bypass the global prefix:
```csharp
using ZedEndpoints.Attributes;

// On a group
[NoGlobalPrefix]
public class HealthEndpoints : IEndpointGroup
{
    public void MapGroup(IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Results.Ok())
            .WithName("HealthCheck");
    }
}

// On a standalone endpoint
[NoGlobalPrefix]
public class ReadinessEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/ready", () => Results.Ok())
            .WithName("Readiness");
    }
}
```

With a global prefix of `api/v1`, all other groups and standalone endpoints will be prefixed normally while the ones decorated with `[NoGlobalPrefix]` will remain at their original routes.

## Architecture

### IEndpoint Interface

Defines a single endpoint with a static mapping method.
```csharp
public interface IEndpoint
{
    static abstract void Map(IEndpointRouteBuilder app);
}
```

### IEndpointGroup Interface

Defines a group of related endpoints with shared configuration.
```csharp
public interface IEndpointGroup
{
    void MapGroup(IEndpointRouteBuilder app);
}
```

### NoGlobalPrefix Attribute

Marks an endpoint group to be excluded from the global route prefix.
```csharp
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class NoGlobalPrefixAttribute : Attribute;
```

## Best Practices

### Endpoint Organization

Organize endpoints by feature or domain area. Use groups for related endpoints that share configuration (auth, tags, rate limiting), and standalone endpoints for isolated cases like health checks or webhooks:
```
Features/
├── Products/
│   ├── GetProduct.cs
│   ├── CreateProduct.cs
│   └── ProductEndpoints.cs      ← IEndpointGroup with shared auth/tags
├── Orders/
│   ├── GetOrder.cs
│   ├── CreateOrder.cs
│   └── OrderEndpoints.cs        ← IEndpointGroup with shared auth/tags
├── HealthCheck.cs               ← standalone IEndpoint, no group needed
└── Readiness.cs                 ← standalone IEndpoint, no group needed
```

### Naming Conventions

- Endpoints: `{Verb}{Resource}Endpoint` (e.g., `GetProductEndpoint`)
- Groups: `{Resource}Endpoints` (e.g., `ProductEndpoints`)

### Shared Configuration

Apply common settings at the group level:
```csharp
public class ProductEndpoints : IEndpointGroup
{
    public void MapGroup(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/products")
            .WithTags("Products")
            .RequireAuthorization()
            .RequireRateLimiting("fixed")
            .WithOpenApi();

        group.MapEndpoint<GetProductEndpoint>()
             .MapEndpoint<CreateProductEndpoint>();
    }
}
```

### API Versioning with Global Prefix

Use the global prefix for versioning instead of repeating it in every group:
```csharp
// Instead of hardcoding /api/v1 in every IEndpointGroup
app.MapEndpointGroups(globalPrefix: "api/v1");
```

For groups that should not follow the versioning scheme, such as health checks or webhooks, use `[NoGlobalPrefix]`:
```csharp
[NoGlobalPrefix]
public class HealthEndpoints : IEndpointGroup { ... }

[NoGlobalPrefix]
public class WebhookEndpoints : IEndpointGroup { ... }
```

## How It Works

### Automatic Discovery

The `MapEndpointGroups()` extension method:

1. Scans the specified assembly (or entry assembly by default)
2. Finds all concrete classes implementing `IEndpointGroup`
3. Creates instances using parameterless constructors
4. Checks for the `[NoGlobalPrefix]` attribute on each group
5. Invokes their `MapGroup()` methods with the appropriate route builder
6. Finds all concrete classes implementing `IEndpoint` that were not already registered through a group
7. Registers each standalone endpoint directly, respecting `[NoGlobalPrefix]` if present

### Global Prefix

When a `globalPrefix` is provided, a root `RouteGroupBuilder` is created and passed to all endpoint groups instead of the `WebApplication` directly. Both implement `IEndpointRouteBuilder`, so existing group implementations require no changes. Groups decorated with `[NoGlobalPrefix]` always receive the `WebApplication` directly, bypassing the prefix entirely.

### Idempotency

The library prevents duplicate registrations:
```csharp
app.MapEndpointGroups(); // Registers all endpoint groups
app.MapEndpointGroups(); // Safe: does nothing (already registered)
```

This is achieved through assembly tracking:
- Each assembly is processed only once
- Thread-safe using lock mechanism
- Prevents accidental duplicate endpoint registration

### Integration Testing with WebApplicationFactory

When using `WebApplicationFactory<T>` for integration or acceptance tests, the default assembly resolution via `Assembly.GetEntryAssembly()` may return the test runner assembly instead of your application assembly. This means no endpoints will be discovered.

**This is handled automatically since v1.1.1** — the library now uses `app.Environment.ApplicationName` to resolve the correct assembly, which works seamlessly with `WebApplicationFactory`.

If you're on an older version, pass the assembly explicitly:
```csharp
app.MapEndpointGroups(typeof(Program).Assembly, globalPrefix: "api/v1");
```

## Requirements

- .NET 8.0 or higher
- ASP.NET Core

## License

MIT

## Contributing

Contributions are welcome. Please open an issue or submit a pull request.

## Support

For issues and questions, please use the GitHub issue tracker.