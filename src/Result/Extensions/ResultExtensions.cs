namespace CoreMesh.Result.Extensions;

/// <summary>
/// Provides factory extension methods for creating <see cref="Result"/> and <see cref="Result{T}"/> instances
/// with specific outcome statuses.
/// </summary>
public static class ResultExtensions
{
    extension(Result)
    {
        /// <summary>
        /// Creates a successful <see cref="Result"/> with status <see cref="ResultStatus.Ok"/>.
        /// </summary>
        /// <returns>A successful <see cref="Result"/>.</returns>
        public static Result Ok() => Result.Create(ResultStatus.Ok, Error.None);

        /// <summary>
        /// Creates a successful <see cref="Result"/> with status <see cref="ResultStatus.Created"/>.
        /// </summary>
        /// <returns>A successful <see cref="Result"/> indicating a resource was created.</returns>
        public static Result Created() => Result.Create(ResultStatus.Created, Error.None);

        /// <summary>
        /// Creates a successful <see cref="Result"/> with status <see cref="ResultStatus.NoContent"/>.
        /// </summary>
        /// <returns>A successful <see cref="Result"/> with no content payload.</returns>
        public static Result NoContent() => Result.Create(ResultStatus.NoContent, Error.None);

        /// <summary>
        /// Creates a failed <see cref="Result"/> with status <see cref="ResultStatus.BadRequest"/>.
        /// </summary>
        /// <param name="error">The error describing the bad request.</param>
        /// <returns>A failed <see cref="Result"/>.</returns>
        public static Result BadRequest(Error error) => Result.Create(ResultStatus.BadRequest, error);

        /// <summary>
        /// Creates a failed <see cref="Result"/> with status <see cref="ResultStatus.NotFound"/>.
        /// </summary>
        /// <param name="error">The error describing the missing resource.</param>
        /// <returns>A failed <see cref="Result"/>.</returns>
        public static Result NotFound(Error error) => Result.Create(ResultStatus.NotFound, error);

        /// <summary>
        /// Creates a failed <see cref="Result"/> with status <see cref="ResultStatus.Forbidden"/>.
        /// </summary>
        /// <param name="error">The error describing the authorization failure.</param>
        /// <returns>A failed <see cref="Result"/>.</returns>
        public static Result Forbidden(Error error) => Result.Create(ResultStatus.Forbidden, error);

        /// <summary>
        /// Creates a failed <see cref="Result"/> with status <see cref="ResultStatus.Invalid"/> from a single error.
        /// </summary>
        /// <param name="error">The validation error.</param>
        /// <returns>A failed <see cref="Result"/>.</returns>
        public static Result Invalid(Error error) => Result.Create(ResultStatus.Invalid, error);

        /// <summary>
        /// Creates a failed <see cref="Result"/> with status <see cref="ResultStatus.Invalid"/> from a dictionary of field-level validation errors.
        /// </summary>
        /// <param name="errors">
        /// A dictionary mapping field names to their associated error messages.
        /// </param>
        /// <returns>A failed <see cref="Result"/> whose <see cref="Result.ValidationErrors"/> is populated.</returns>
        public static Result Invalid(IDictionary<string, IReadOnlyList<string>> errors) => Result.Create(errors);

        /// <summary>
        /// Creates a failed <see cref="Result"/> with status <see cref="ResultStatus.Invalid"/> from a dictionary
        /// whose values implement <see cref="IReadOnlyList{T}"/>.
        /// </summary>
        /// <typeparam name="TList">The list type of the error values.</typeparam>
        /// <param name="errors">A dictionary mapping field names to their associated error messages.</param>
        /// <returns>A failed <see cref="Result"/> whose <see cref="Result.ValidationErrors"/> is populated.</returns>
        public static Result Invalid<TList>(IDictionary<string, TList> errors) where TList : IReadOnlyList<string>
            => Result.Create(errors.ToDictionary(k => k.Key, v => (IReadOnlyList<string>)v.Value));
    }

    extension<T>(Result<T>)
    {
        /// <summary>
        /// Creates a successful <see cref="Result{T}"/> with the given value and status <see cref="ResultStatus.Ok"/>.
        /// </summary>
        /// <param name="value">The data payload.</param>
        /// <returns>A successful <see cref="Result{T}"/>.</returns>
        public static Result<T> Ok(T value) => Result<T>.Create(value, ResultStatus.Ok);

        /// <summary>
        /// Creates a successful <see cref="Result{T}"/> with the given value and status <see cref="ResultStatus.Created"/>.
        /// </summary>
        /// <param name="value">The data payload of the newly created resource.</param>
        /// <returns>A successful <see cref="Result{T}"/> indicating a resource was created.</returns>
        public static Result<T> Created(T value) => Result<T>.Create(value, ResultStatus.Created);

        /// <summary>
        /// Creates a failed <see cref="Result{T}"/> with status <see cref="ResultStatus.BadRequest"/>.
        /// </summary>
        /// <param name="error">The error describing the bad request.</param>
        /// <returns>A failed <see cref="Result{T}"/>.</returns>
        public static Result<T> BadRequest(Error error) => Result<T>.Create(error, ResultStatus.BadRequest);

        /// <summary>
        /// Creates a failed <see cref="Result{T}"/> with status <see cref="ResultStatus.NotFound"/>.
        /// </summary>
        /// <param name="error">The error describing the missing resource.</param>
        /// <returns>A failed <see cref="Result{T}"/>.</returns>
        public static Result<T> NotFound(Error error) => Result<T>.Create(error, ResultStatus.NotFound);

        /// <summary>
        /// Creates a failed <see cref="Result{T}"/> with status <see cref="ResultStatus.Forbidden"/>.
        /// </summary>
        /// <param name="error">The error describing the authorization failure.</param>
        /// <returns>A failed <see cref="Result{T}"/>.</returns>
        public static Result<T> Forbidden(Error error) => Result<T>.Create(error, ResultStatus.Forbidden);

        /// <summary>
        /// Creates a failed <see cref="Result{T}"/> with status <see cref="ResultStatus.Invalid"/> from a single error.
        /// </summary>
        /// <param name="error">The validation error.</param>
        /// <returns>A failed <see cref="Result{T}"/>.</returns>
        public static Result<T> Invalid(Error error) => Result<T>.Create(error, ResultStatus.Invalid);

        /// <summary>
        /// Creates a failed <see cref="Result{T}"/> with status <see cref="ResultStatus.Invalid"/> from a dictionary of field-level validation errors.
        /// </summary>
        /// <param name="errors">
        /// A dictionary mapping field names to their associated error messages.
        /// </param>
        /// <returns>A failed <see cref="Result{T}"/> whose <see cref="Result.ValidationErrors"/> is populated.</returns>
        public static Result<T> Invalid(IDictionary<string, IReadOnlyList<string>> errors) => Result<T>.Create(errors);

        /// <summary>
        /// Creates a failed <see cref="Result{T}"/> with status <see cref="ResultStatus.Invalid"/> from a dictionary
        /// whose values implement <see cref="IReadOnlyList{T}"/>.
        /// </summary>
        /// <typeparam name="TList">The list type of the error values.</typeparam>
        /// <param name="errors">A dictionary mapping field names to their associated error messages.</param>
        /// <returns>A failed <see cref="Result{T}"/> whose <see cref="Result.ValidationErrors"/> is populated.</returns>
        public static Result<T> Invalid<TList>(IDictionary<string, TList> errors) where TList : IReadOnlyList<string>
            => Result<T>.Create(errors.ToDictionary(k => k.Key, v => (IReadOnlyList<string>)v.Value));
    }
}
