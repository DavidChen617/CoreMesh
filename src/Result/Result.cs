namespace CoreMesh.Result;

/// <summary>
/// Represents a non-generic operation result that carries a status and an optional error.
/// </summary>
public record Result
{
    private static readonly HashSet<ResultStatus> SuccessStatuses =
        [ResultStatus.Ok, ResultStatus.Created, ResultStatus.NoContent];

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> record with the specified status and error.
    /// </summary>
    /// <param name="status">The outcome status of the operation.</param>
    /// <param name="error">
    /// The error associated with the result. Must be <see cref="Error.None"/> for success statuses,
    /// and must not be <see cref="Error.None"/> for failure statuses.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when a success status is paired with a non-empty error, or a failure status is paired with <see cref="Error.None"/>.
    /// </exception>
    protected Result(ResultStatus status, Error error)
    {
        if (SuccessStatuses.Contains(status) && error != Error.None ||
            !SuccessStatuses.Contains(status) && error == Error.None)
            throw new ArgumentException("Invalid error", nameof(error));

        Status = status;
        IsSuccess = SuccessStatuses.Contains(status);
        Error = error;
    }

    internal static Result Create(ResultStatus status, Error error) => new(status, error);

    internal static Result Create(IDictionary<string, string[]> validationErrors) =>
        new(ResultStatus.Invalid, new Error("VALIDATION", "One or more validation errors occurred"))
        {
            ValidationErrors = new Dictionary<string, string[]>(validationErrors)
        };

    /// <summary>
    /// Gets the outcome status of the operation.
    /// </summary>
    public ResultStatus Status { get; }

    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error associated with the result. Returns <see cref="Error.None"/> for successful results.
    /// </summary>
    public Error Error { get; }

    /// <summary>
    /// Gets the validation errors produced when the result status is <see cref="ResultStatus.Invalid"/>.
    /// Returns an empty dictionary when there are no validation errors.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> ValidationErrors { get; init; } =
        new Dictionary<string, string[]>();

    /// <summary>
    /// Implicitly converts an <see cref="Error"/> to a failed <see cref="Result"/> with status <see cref="ResultStatus.BadRequest"/>.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    /// <returns>A failed <see cref="Result"/> carrying the given error.</returns>
    public static implicit operator Result(Error error) =>
        Result.Create(ResultStatus.BadRequest, error);
}

/// <summary>
/// Represents a generic operation result that carries a status, an optional error, and an optional data payload.
/// </summary>
/// <typeparam name="T">The type of the data returned by the operation.</typeparam>
public record Result<T> : Result
{
    /// <summary>
    /// Gets the data payload returned by a successful operation. Returns <see langword="null"/> for failure results.
    /// </summary>
    public T? Data { get; }

    /// <summary>
    /// Initializes a new successful instance of <see cref="Result{T}"/> with the specified data and status.
    /// </summary>
    /// <param name="data">The data payload.</param>
    /// <param name="status">The success status of the operation.</param>
    protected Result(T data, ResultStatus status) : base(status, Error.None) =>
        Data = data;

    /// <summary>
    /// Initializes a new failed instance of <see cref="Result{T}"/> with the specified error and status.
    /// </summary>
    /// <param name="error">The error describing the failure.</param>
    /// <param name="status">The failure status of the operation.</param>
    protected Result(Error error, ResultStatus status) : base(status, error)
    {
    }

    internal static Result<T> Create(T value, ResultStatus status) => new(value, status);
    internal static Result<T> Create(Error error, ResultStatus status) => new(error, status);

    internal new static Result<T> Create(IDictionary<string, string[]> validationErrors) =>
        new(new Error("VALIDATION", "One or more validation errors occurred"), ResultStatus.Invalid)
        {
            ValidationErrors = new Dictionary<string, string[]>(validationErrors)
        };

    /// <summary>
    /// Implicitly converts a value of type <typeparamref name="T"/> to a successful <see cref="Result{T}"/>
    /// with status <see cref="ResultStatus.Ok"/>.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A successful <see cref="Result{T}"/> carrying the given value.</returns>
    public static implicit operator Result<T>(T value) =>
        Create(value, ResultStatus.Ok);

    /// <summary>
    /// Implicitly converts an <see cref="Error"/> to a failed <see cref="Result{T}"/>
    /// with status <see cref="ResultStatus.BadRequest"/>.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    /// <returns>A failed <see cref="Result{T}"/> carrying the given error.</returns>
    public static implicit operator Result<T>(Error error) =>
        Create(error, ResultStatus.BadRequest);
}

/// <summary>
/// Represents an application error with a machine-readable code and a human-readable description.
/// </summary>
/// <param name="Code">A short, machine-readable identifier for the error (e.g., <c>"NOT_FOUND"</c>).</param>
/// <param name="Description">A human-readable message describing what went wrong.</param>
public sealed record Error(string Code, string Description)
{
    /// <summary>
    /// Represents the absence of an error. Used to indicate a successful result.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty);
}
