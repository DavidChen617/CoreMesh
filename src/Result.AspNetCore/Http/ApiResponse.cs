using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace CoreMesh.Result.Http;

/// <summary>
/// Represents the non-generic HTTP API response envelope returned by all endpoints.
/// </summary>
public class ApiResponse
{
    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    [JsonPropertyName("isSuccess")]
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Gets the problem details payload describing the failure. Omitted from serialization when <see langword="null"/>.
    /// </summary>
    [JsonPropertyName("problem")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ProblemDetails? Problem { get; init; }

    /// <summary>
    /// Gets the application-specific status code for the response (e.g., the error code or <c>"ok"</c>).
    /// </summary>
    [JsonPropertyName("code")]
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Creates a failure response envelope with the given problem details and error code.
    /// </summary>
    /// <param name="problem">The problem details payload describing the failure.</param>
    /// <param name="code">The application-specific error code.</param>
    /// <returns>A failure <see cref="ApiResponse"/> envelope.</returns>
    public static ApiResponse OnFailure(ProblemDetails problem, string code)
        => new()
        {
            IsSuccess = false,
            Problem = problem,
            Code = code
        };

    /// <summary>
    /// Creates a success response envelope with no data payload.
    /// </summary>
    /// <param name="code">The application-specific success code. Defaults to <c>"ok"</c>.</param>
    /// <returns>A success <see cref="ApiResponse"/> envelope.</returns>
    public static ApiResponse OnSuccess(string code = "ok")
        => new()
        {
            IsSuccess = true,
            Code = code
        };
}

/// <summary>
/// Represents the generic HTTP API response envelope that includes a typed data payload.
/// </summary>
/// <typeparam name="T">The type of the data returned by the operation.</typeparam>
public sealed class ApiResponse<T> : ApiResponse
{
    /// <summary>
    /// Gets the typed data payload returned by the operation. Omitted from serialization when <see langword="null"/>.
    /// </summary>
    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; init; }

    /// <summary>
    /// Creates a success response envelope that wraps the given data payload.
    /// </summary>
    /// <param name="data">The data payload to include in the response.</param>
    /// <param name="code">The application-specific success code. Defaults to <c>"ok"</c>.</param>
    /// <returns>A success <see cref="ApiResponse{T}"/> envelope containing <paramref name="data"/>.</returns>
    public static ApiResponse<T> OnSuccess(T data, string code = "ok")
        => new()
        {
            IsSuccess = true,
            Data = data,
            Code = code
        };
}
