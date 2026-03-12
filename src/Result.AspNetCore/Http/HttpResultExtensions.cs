using System.Net.ServerSentEvents;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace CoreMesh.Result.Http;

/// <summary>
/// Provides extension methods for converting <see cref="Result"/> and <see cref="Result{T}"/> instances
/// into ASP.NET Core <see cref="IResult"/> HTTP responses.
/// </summary>
public static class HttpResultExtensions
{
    extension(Result result)
    {
        /// <summary>
        /// Converts a non-generic <see cref="Result"/> to the corresponding <see cref="IResult"/> HTTP response.
        /// </summary>
        /// <remarks>
        /// Status mapping:
        /// <list type="bullet">
        ///   <item><see cref="ResultStatus.Ok"/> → 200 OK</item>
        ///   <item><see cref="ResultStatus.Created"/> → 201 Created</item>
        ///   <item><see cref="ResultStatus.NoContent"/> → 204 No Content</item>
        ///   <item><see cref="ResultStatus.BadRequest"/> → 400 Bad Request</item>
        ///   <item><see cref="ResultStatus.NotFound"/> → 404 Not Found</item>
        ///   <item><see cref="ResultStatus.Forbidden"/> → 403 Forbidden</item>
        ///   <item><see cref="ResultStatus.Invalid"/> → 422 Unprocessable Entity</item>
        ///   <item>Any other status → 500 Internal Server Error</item>
        /// </list>
        /// </remarks>
        /// <returns>The <see cref="IResult"/> that matches the result's status.</returns>
        public IResult ToHttpResult() => result.Status switch
        {
            ResultStatus.Ok        => TypedResults.Ok(ApiResponse.OnSuccess()),
            ResultStatus.Created   => TypedResults.Created((string?)null, ApiResponse.OnSuccess(nameof(ResultStatus.Created))),
            ResultStatus.NoContent => TypedResults.NoContent(),
            ResultStatus.BadRequest => TypedResults.BadRequest(ApiResponse.OnFailure(result.ToProblem(400), result.Error.Code)),
            ResultStatus.NotFound   => TypedResults.NotFound(ApiResponse.OnFailure(result.ToProblem(404), result.Error.Code)),
            ResultStatus.Forbidden  => TypedResults.Json(ApiResponse.OnFailure(result.ToProblem(403), result.Error.Code), statusCode: 403),
            ResultStatus.Invalid    => result.ValidationErrors.Count > 0
                ? TypedResults.UnprocessableEntity(ApiResponse.OnFailure(result.ToValidationProblem(), result.Error.Code))
                : TypedResults.UnprocessableEntity(ApiResponse.OnFailure(result.ToProblem(422), result.Error.Code)),
            _ => TypedResults.Json(ApiResponse.OnFailure(result.ToProblem(500), result.Error.Code), statusCode: 500)
        };

        /// <summary>
        /// Converts a successful <see cref="Result"/> into a <see cref="SignInHttpResult"/> that signs in the given principal.
        /// </summary>
        /// <param name="principal">The <see cref="ClaimsPrincipal"/> to sign in.</param>
        /// <param name="properties">Optional authentication properties for the sign-in operation.</param>
        /// <param name="authenticationScheme">The authentication scheme to use. When <see langword="null"/> the default scheme is used.</param>
        /// <returns>A <see cref="SignInHttpResult"/> that performs the sign-in.</returns>
        public SignInHttpResult ToSignIn(
            ClaimsPrincipal principal,
            AuthenticationProperties? properties = null,
            string? authenticationScheme = null) =>
            TypedResults.SignIn(principal, properties, authenticationScheme);

        /// <summary>
        /// Converts a successful <see cref="Result"/> into a <see cref="SignOutHttpResult"/> that signs out the current user.
        /// </summary>
        /// <param name="properties">Optional authentication properties for the sign-out operation.</param>
        /// <param name="authenticationSchemes">The authentication schemes to sign out of. When <see langword="null"/> the default scheme is used.</param>
        /// <returns>A <see cref="SignOutHttpResult"/> that performs the sign-out.</returns>
        public SignOutHttpResult ToSignOut(
            AuthenticationProperties? properties = null,
            IList<string>? authenticationSchemes = null) =>
            TypedResults.SignOut(properties, authenticationSchemes);

        internal ProblemDetails ToProblem(int statusCode) => new()
        {
            Status = statusCode,
            Title = result.Error.Code,
            Detail = result.Error.Description
        };

        internal ValidationProblemDetails ToValidationProblem() => new(result.ValidationErrors.ToDictionary(k => k.Key, v => v.Value.ToArray()))
        {
            Status = 422,
            Title = result.Error.Code,
            Detail = result.Error.Description
        };
    }

    extension<T>(Result<T> result)
    {
        /// <summary>
        /// Converts a <see cref="Result{T}"/> to a <see cref="ResultHttpResult{T}"/> that implements
        /// <c>IEndpointMetadataProvider</c>, allowing ASP.NET Core to surface all possible
        /// response types to OpenAPI automatically without manual <c>.Produces&lt;T&gt;()</c> calls.
        /// </summary>
        /// <remarks>
        /// Status mapping:
        /// <list type="bullet">
        ///   <item><see cref="ResultStatus.Ok"/> → 200 OK with <see cref="ApiResponse{T}"/> body</item>
        ///   <item><see cref="ResultStatus.Created"/> → 201 Created with <see cref="ApiResponse{T}"/> body</item>
        ///   <item><see cref="ResultStatus.NoContent"/> → 204 No Content</item>
        ///   <item><see cref="ResultStatus.BadRequest"/> → 400 Bad Request</item>
        ///   <item><see cref="ResultStatus.NotFound"/> → 404 Not Found</item>
        ///   <item><see cref="ResultStatus.Forbidden"/> → 403 Forbidden</item>
        ///   <item><see cref="ResultStatus.Invalid"/> → 422 Unprocessable Entity</item>
        ///   <item>Any other status → 500 Internal Server Error</item>
        /// </list>
        /// </remarks>
        /// <returns>A <see cref="ResultHttpResult{T}"/> that carries full OpenAPI metadata.</returns>
        public ResultHttpResult<T> ToHttpResult() => new(result);

        /// <summary>
        /// Resolves the concrete <see cref="IResult"/> for execution. Called internally by
        /// <see cref="ResultHttpResult{T}.ExecuteAsync"/>.
        /// </summary>
        internal IResult ToInnerHttpResult() => result.Status switch
        {
            ResultStatus.Ok        => TypedResults.Ok(ApiResponse<T>.OnSuccess(result.Data!)),
            ResultStatus.Created   => TypedResults.Created((string?)null, ApiResponse<T>.OnSuccess(result.Data!, "created")),
            ResultStatus.NoContent => TypedResults.NoContent(),
            ResultStatus.BadRequest => TypedResults.BadRequest(ApiResponse.OnFailure(result.ToProblem(400), result.Error.Code)),
            ResultStatus.NotFound   => TypedResults.NotFound(ApiResponse.OnFailure(result.ToProblem(404), result.Error.Code)),
            ResultStatus.Forbidden  => TypedResults.Json(ApiResponse.OnFailure(result.ToProblem(403), result.Error.Code), statusCode: 403),
            ResultStatus.Invalid    => result.ValidationErrors.Count > 0
                ? TypedResults.UnprocessableEntity(ApiResponse.OnFailure(result.ToValidationProblem(), result.Error.Code))
                : TypedResults.UnprocessableEntity(ApiResponse.OnFailure(result.ToProblem(422), result.Error.Code)),
            _ => TypedResults.Json(ApiResponse.OnFailure(result.ToProblem(500), result.Error.Code), statusCode: 500)
        };

        /// <summary>
        /// Converts a successful <see cref="Result{T}"/> to a <see cref="JsonHttpResult{T}"/> response
        /// wrapping the data payload in an <see cref="ApiResponse{T}"/> envelope.
        /// </summary>
        /// <param name="options">Optional JSON serializer options. When <see langword="null"/> the default options are used.</param>
        /// <param name="contentType">Optional content-type header value. When <see langword="null"/> defaults to <c>application/json</c>.</param>
        /// <param name="statusCode">Optional HTTP status code override. When <see langword="null"/> defaults to 200.</param>
        /// <returns>A <see cref="JsonHttpResult{T}"/> containing the success envelope.</returns>
        public JsonHttpResult<ApiResponse<T>> ToJson(
            JsonSerializerOptions? options = null,
            string? contentType = null,
            int? statusCode = null) =>
            TypedResults.Json(ApiResponse<T>.OnSuccess(result.Data!), options, contentType, statusCode);

        internal ProblemDetails ToProblem(int statusCode) => new()
        {
            Status = statusCode,
            Title = result.Error.Code,
            Detail = result.Error.Description
        };

        internal ValidationProblemDetails ToValidationProblem() => new(result.ValidationErrors.ToDictionary(k => k.Key, v => v.Value.ToArray()))
        {
            Status = 422,
            Title = result.Error.Code,
            Detail = result.Error.Description
        };
    }

    extension<T>(Result<IAsyncEnumerable<T>> result)
    {
        /// <summary>
        /// Converts a <see cref="Result{T}"/> whose data is an <see cref="IAsyncEnumerable{T}"/> to a
        /// Server-Sent Events response. Returns a 500 error response if the result is a failure.
        /// </summary>
        /// <param name="eventType">
        /// The SSE event type name sent to the client. When <see langword="null"/> the default event type is used.
        /// </param>
        /// <returns>
        /// A Server-Sent Events <see cref="IResult"/> on success, or a 500 JSON error response on failure.
        /// </returns>
        public IResult ToServerSentEvents(string? eventType = null) =>
            result.IsSuccess
                ? TypedResults.ServerSentEvents(result.Data!, eventType)
                : TypedResults.Json(ApiResponse.OnFailure(result.ToProblem(500), result.Error.Code), statusCode: 500);

        /// <summary>
        /// Converts a <see cref="Result{T}"/> whose data is an <see cref="IAsyncEnumerable{T}"/> to a
        /// Server-Sent Events response using a custom <see cref="SseItem{T}"/> factory.
        /// Returns a 500 error response if the result is a failure.
        /// </summary>
        /// <param name="sseItemsFactory">
        /// A factory function that produces the sequence of <see cref="SseItem{T}"/> events to stream.
        /// </param>
        /// <returns>
        /// A Server-Sent Events <see cref="IResult"/> on success, or a 500 JSON error response on failure.
        /// </returns>
        public IResult ToServerSentEvents(Func<IAsyncEnumerable<SseItem<T>>> sseItemsFactory) =>
            result.IsSuccess
                ? TypedResults.ServerSentEvents(sseItemsFactory())
                : TypedResults.Json(ApiResponse.OnFailure(result.ToProblem(500), result.Error.Code), statusCode: 500);

        internal ProblemDetails ToProblem(int statusCode) => new()
        {
            Status = statusCode,
            Title = result.Error.Code,
            Detail = result.Error.Description
        };
    }

    extension(Result<byte[]> result)
    {
        /// <summary>
        /// Converts a <see cref="Result{T}"/> whose data is a byte array to a file download response.
        /// Returns a 500 error response if the result is a failure.
        /// </summary>
        /// <param name="contentType">The MIME type of the file. When <see langword="null"/> defaults to <c>application/octet-stream</c>.</param>
        /// <param name="fileDownloadName">The suggested file name for the download. When <see langword="null"/> no <c>Content-Disposition</c> header is set.</param>
        /// <param name="enableRangeProcessing">When <see langword="true"/>, enables range request processing (HTTP 206 Partial Content).</param>
        /// <param name="lastModified">The last-modified date for the file, used for conditional requests.</param>
        /// <param name="entityTag">The entity tag for the file, used for conditional requests.</param>
        /// <returns>
        /// A file <see cref="IResult"/> on success, or a 500 JSON error response on failure.
        /// </returns>
        public IResult ToFile(
            string? contentType = null,
            string? fileDownloadName = null,
            bool enableRangeProcessing = false,
            DateTimeOffset? lastModified = null,
            EntityTagHeaderValue? entityTag = null) =>
            result.IsSuccess
                ? TypedResults.File(result.Data!, contentType, fileDownloadName, enableRangeProcessing, lastModified, entityTag)
                : TypedResults.Json(ApiResponse.OnFailure(result.ToProblem(500), result.Error.Code), statusCode: 500);

        internal ProblemDetails ToProblem(int statusCode) => new()
        {
            Status = statusCode,
            Title = result.Error.Code,
            Detail = result.Error.Description
        };
    }

    extension(Result<Stream> result)
    {
        /// <summary>
        /// Converts a <see cref="Result{T}"/> whose data is a <see cref="Stream"/> to a file download response.
        /// Returns a 500 error response if the result is a failure.
        /// </summary>
        /// <remarks>
        /// Note: the parameter order differs from the <c>byte[]</c> overload — <paramref name="lastModified"/>
        /// and <paramref name="entityTag"/> appear before <paramref name="enableRangeProcessing"/> to match
        /// the underlying <c>TypedResults.Stream</c> signature.
        /// </remarks>
        /// <param name="contentType">The MIME type of the file. When <see langword="null"/> defaults to <c>application/octet-stream</c>.</param>
        /// <param name="fileDownloadName">The suggested file name for the download. When <see langword="null"/> no <c>Content-Disposition</c> header is set.</param>
        /// <param name="lastModified">The last-modified date for the file, used for conditional requests.</param>
        /// <param name="entityTag">The entity tag for the file, used for conditional requests.</param>
        /// <param name="enableRangeProcessing">When <see langword="true"/>, enables range request processing (HTTP 206 Partial Content).</param>
        /// <returns>
        /// A file <see cref="IResult"/> streaming the <see cref="Stream"/> on success, or a 500 JSON error response on failure.
        /// </returns>
        public IResult ToFile(
            string? contentType = null,
            string? fileDownloadName = null,
            DateTimeOffset? lastModified = null,
            EntityTagHeaderValue? entityTag = null,
            bool enableRangeProcessing = false) =>
            result.IsSuccess
                ? TypedResults.Stream(result.Data!, contentType, fileDownloadName, lastModified, entityTag, enableRangeProcessing)
                : TypedResults.Json(ApiResponse.OnFailure(result.ToProblem(500), result.Error.Code), statusCode: 500);

        internal ProblemDetails ToProblem(int statusCode) => new()
        {
            Status = statusCode,
            Title = result.Error.Code,
            Detail = result.Error.Description
        };
    }
}
