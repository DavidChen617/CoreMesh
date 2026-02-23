using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace CoreMesh.Http.Responses;

/// <summary>
/// Represents the non-generic API response envelope.
/// </summary>
public class ApiResponse
{
    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    [JsonPropertyName("isSuccess")] 
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Gets the problem details payload for failed operations.
    /// </summary>
    [JsonPropertyName("problem")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ProblemDetails? Problem { get; init; }

    /// <summary>
    /// Gets the application-specific response code.
    /// </summary>
    [JsonPropertyName("code")] 
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Creates a failure response envelope.
    /// </summary>
    /// <param name="problem">The problem details payload.</param>
    /// <param name="code">The application-specific error code.</param>
    /// <returns>A failure response envelope.</returns>
    public static ApiResponse OnFailure(ProblemDetails problem, string code)
        => new()
        {
            IsSuccess = false, 
            Problem = problem, 
            Code = code
        };
    
    /// <summary>
    /// Creates a success response envelope.
    /// </summary>
    /// <param name="code">The application-specific code.</param>
    /// <returns>A success response envelope.</returns>
    public static ApiResponse OnSuccess(string code = "ok")
        => new()
        {
            IsSuccess = true,
            Code = code
        };
}

/// <summary>
/// Represents a typed API response envelope.
/// </summary>
/// <typeparam name="T">The data payload type.</typeparam>
public sealed class ApiResponse<T> : ApiResponse
{
    /// <summary>
    /// Gets the success payload.
    /// </summary>
    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; init; }

    /// <summary>
    /// Creates a success response envelope.
    /// </summary>
    /// <param name="data">The success payload.</param>
    /// <param name="code">The application-specific success code.</param>
    /// <returns>A success response envelope.</returns>
    public static ApiResponse<T> OnSuccess(T data, string code = "ok")
        => new()
        {
            IsSuccess = true,
            Data = data,
            Problem = null,
            Code = code
        };
}
