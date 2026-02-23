using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CoreMesh.Endpoints;

/// <summary>
/// Defines an endpoint that can register routes on the provided route builder.
/// </summary>
public interface IEndpoint
{
    /// <summary>
    /// Registers the endpoint route(s).
    /// </summary>
    /// <param name="app">The route builder.</param>
    void AddRoute(IEndpointRouteBuilder app);
}

/// <summary>
/// Defines an endpoint with a request payload that returns an <see cref="IResult"/>.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
public interface IEndpoint<TRequest> : IEndpoint
{
    /// <summary>
    /// Handles the endpoint request.
    /// </summary>
    /// <param name="request">The request payload.</param>
    /// <returns>The HTTP result.</returns>
    Task<IResult> HandleAsync(TRequest request);
}

/// <summary>
/// Defines an endpoint with a request payload that returns a response payload.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public interface IEndpoint<TRequest, TResponse> : IEndpoint
{
    /// <summary>
    /// Handles the endpoint request.
    /// </summary>
    /// <param name="request">The request payload.</param>
    /// <returns>The response payload.</returns>
    Task<TResponse> HandleAsync(TRequest request);
}

/// <summary>
/// Defines a route group endpoint that configures a shared route group.
/// </summary>
public interface IGroupEndpoint
{
    /// <summary>
    /// Gets the route prefix for the group.
    /// </summary>
    string GroupPrefix { get; }

    /// <summary>
    /// Configures the route group.
    /// </summary>
    /// <param name="group">The route group builder.</param>
    void Configure(RouteGroupBuilder group);
}

/// <summary>
/// Defines an endpoint that is mapped under a route group.
/// </summary>
public interface IGroupedEndpoint
{
    /// <summary>
    /// Gets the concrete group endpoint type this endpoint belongs to.
    /// </summary>
    Type GroupType { get; }

    /// <summary>
    /// Registers the endpoint route(s) under the route group.
    /// </summary>
    /// <param name="group">The route group builder.</param>
    void AddRoute(RouteGroupBuilder group);
}

/// <summary>
/// Defines a grouped endpoint associated with a specific group type.
/// </summary>
/// <typeparam name="TGroup">The group endpoint type.</typeparam>
public interface IGroupedEndpoint<TGroup> : IGroupedEndpoint where TGroup : IGroupEndpoint
{
    Type IGroupedEndpoint.GroupType => typeof(TGroup);
}

/// <summary>
/// Defines a grouped endpoint with a request payload that returns an <see cref="IResult"/>.
/// </summary>
/// <typeparam name="TGroup">The group endpoint type.</typeparam>
/// <typeparam name="TRequest">The request type.</typeparam>
public interface IGroupedEndpoint<TGroup, TRequest> : IGroupedEndpoint where TGroup : IGroupEndpoint
{
    Type IGroupedEndpoint.GroupType => typeof(TGroup);

    /// <summary>
    /// Handles the endpoint request.
    /// </summary>
    /// <param name="request">The request payload.</param>
    /// <returns>The HTTP result.</returns>
    Task<IResult> HandleAsync(TRequest request);
}

/// <summary>
/// Defines a grouped endpoint with a request payload that returns a response payload.
/// </summary>
/// <typeparam name="TGroup">The group endpoint type.</typeparam>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public interface IGroupedEndpoint<TGroup, TRequest, TResponse> : IGroupedEndpoint where TGroup : IGroupEndpoint
{
    Type IGroupedEndpoint.GroupType => typeof(TGroup);

    /// <summary>
    /// Handles the endpoint request.
    /// </summary>
    /// <param name="request">The request payload.</param>
    /// <returns>The response payload.</returns>
    Task<TResponse> HandleAsync(TRequest request);
}
