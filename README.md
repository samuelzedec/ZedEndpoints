# ZedEndpoints

[![NuGet](https://img.shields.io/nuget/v/ZedEndpoints.svg)](https://www.nuget.org/packages/ZedEndpoints/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/ZedEndpoints.svg)](https://www.nuget.org/packages/ZedEndpoints/)

A minimal library for automatic endpoint discovery and organization in ASP.NET Core Minimal APIs.

## Features

- Automatic endpoint discovery via reflection
- Group-based endpoint organization with shared configuration
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
        var group = app.MapGroup("/api/v1/products")
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

### Scanning Specific Assembly
```csharp
app.MapEndpointGroups(typeof(ProductEndpoints).Assembly);
```

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

## Best Practices

### Endpoint Organization

Organize endpoints by feature or domain area:
```
Features/
├── Products/
│   ├── GetProduct.cs
│   ├── CreateProduct.cs
│   └── ProductEndpoints.cs
├── Orders/
│   ├── GetOrder.cs
│   ├── CreateOrder.cs
│   └── OrderEndpoints.cs
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
        var group = app.MapGroup("/api/v1/products")
            .WithTags("Products")
            .RequireAuthorization()
            .RequireRateLimiting("fixed")
            .WithOpenApi();

        group.MapEndpoint<GetProductEndpoint>()
             .MapEndpoint<CreateProductEndpoint>();
    }
}
```

## How It Works

### Automatic Discovery

The `MapEndpointGroups()` extension method:

1. Scans the specified assembly (or entry assembly by default)
2. Finds all concrete classes implementing `IEndpointGroup`
3. Creates instances using parameterless constructors
4. Invokes their `MapGroup()` methods

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

## Requirements

- .NET 8.0 or higher
- ASP.NET Core

## License

MIT

## Contributing

Contributions are welcome. Please open an issue or submit a pull request.

## Support

For issues and questions, please use the GitHub issue tracker.