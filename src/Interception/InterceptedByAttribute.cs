namespace CoreMesh.Interception;

/// <summary>
/// Specifies an interceptor type to be applied to the decorated interface.
/// Multiple attributes can be applied to use multiple interceptors.
/// </summary>
/// <typeparam name="T">The interceptor type that implements <see cref="IInterceptorBase"/>.</typeparam>
/// <example>
/// <code>
/// [InterceptedBy&lt;LoggingInterceptor&gt;]
/// [InterceptedBy&lt;CacheInterceptor&gt;]
/// public interface IMyService
/// {
///     Task&lt;string&gt; GetDataAsync();
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Interface, AllowMultiple = true)]
public class InterceptedByAttribute<T> : Attribute where T : IInterceptorBase;
