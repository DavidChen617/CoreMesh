namespace CoreMesh.Dispatching.Abstractions;

/// <summary>
/// Represents a void type, since <see cref="System.Void"/> is not a valid return type in C#.
/// </summary>
public readonly struct Unit
{
    /// <summary>
    /// Gets the singleton value of <see cref="Unit"/>.
    /// </summary>
    public static readonly Unit Value = default;

    /// <summary>
    /// Gets a completed task that returns <see cref="Value"/>.
    /// </summary>
    public static readonly Task<Unit> Task = System.Threading.Tasks.Task.FromResult(Value);
}
