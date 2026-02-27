namespace CoreMesh.Interception;

[AttributeUsage(AttributeTargets.Interface,  AllowMultiple = true)]
public class InterceptedByAttribute<T> : Attribute where T : IInterceptorBase;
