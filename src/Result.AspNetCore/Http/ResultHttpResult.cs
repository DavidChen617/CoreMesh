using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;

namespace CoreMesh.Result.Http;

/// <summary>
/// An <see cref="IResult"/> wrapper for <see cref="Result{T}"/> that also implements
/// <see cref="IEndpointMetadataProvider"/> so that ASP.NET Core can surface all possible
/// response types to OpenAPI without any manual <c>.Produces&lt;T&gt;()</c> calls.
/// </summary>
/// <typeparam name="T">The data type carried by the underlying <see cref="Result{T}"/>.</typeparam>
public sealed class ResultHttpResult<T> : IResult, IEndpointMetadataProvider
{
    private readonly Result<T> _result;

    internal ResultHttpResult(Result<T> result) => _result = result;

    /// <inheritdoc/>
    public Task ExecuteAsync(HttpContext httpContext) =>
        _result.ToInnerHttpResult().ExecuteAsync(httpContext);

    /// <summary>
    /// Populates the endpoint metadata with all response types that
    /// <see cref="ResultHttpResult{T}"/> can produce, enabling OpenAPI to display
    /// strongly-typed response schemas without manual <c>.Produces()</c> annotations.
    /// </summary>
    /// <remarks>
    /// Response types declared:
    /// <list type="bullet">
    ///   <item>200 OK → <see cref="ApiResponse{T}"/></item>
    ///   <item>201 Created → <see cref="ApiResponse{T}"/></item>
    ///   <item>204 No Content → (no body)</item>
    ///   <item>400 Bad Request → <see cref="ApiResponse"/></item>
    ///   <item>403 Forbidden → <see cref="ApiResponse"/></item>
    ///   <item>404 Not Found → <see cref="ApiResponse"/></item>
    ///   <item>422 Unprocessable Entity → <see cref="ApiResponse"/></item>
    ///   <item>500 Internal Server Error → <see cref="ApiResponse"/></item>
    /// </list>
    /// </remarks>
    public static void PopulateMetadata(MethodInfo method, EndpointBuilder builder)
    {
        builder.Metadata.Add(new ProducesResponseTypeMetadata(statusCode: 200, type: typeof(ApiResponse<T>), contentTypes: ["application/json"]));
        builder.Metadata.Add(new ProducesResponseTypeMetadata(statusCode: 201, type: typeof(ApiResponse<T>), contentTypes: ["application/json"]));
        builder.Metadata.Add(new ProducesResponseTypeMetadata(statusCode: 204));
        builder.Metadata.Add(new ProducesResponseTypeMetadata(statusCode: 400, type: typeof(ApiResponse), contentTypes: ["application/json"]));
        builder.Metadata.Add(new ProducesResponseTypeMetadata(statusCode: 403, type: typeof(ApiResponse), contentTypes: ["application/json"]));
        builder.Metadata.Add(new ProducesResponseTypeMetadata(statusCode: 404, type: typeof(ApiResponse), contentTypes: ["application/json"]));
        builder.Metadata.Add(new ProducesResponseTypeMetadata(statusCode: 422, type: typeof(ApiResponse), contentTypes: ["application/json"]));
        builder.Metadata.Add(new ProducesResponseTypeMetadata(statusCode: 500, type: typeof(ApiResponse), contentTypes: ["application/json"]));
    }
}
