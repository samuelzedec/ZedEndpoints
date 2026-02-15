namespace ZedEndpoints.Attributes;

/// <summary>
/// Represents an attribute that indicates a class should not have
/// a global prefix applied to its associated endpoints or routes.
/// </summary>
/// <remarks>
/// This attribute is used to explicitly opt-out of any global routing
/// prefixes that may be defined and applied within the application configuration.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class NoGlobalPrefixAttribute : Attribute;