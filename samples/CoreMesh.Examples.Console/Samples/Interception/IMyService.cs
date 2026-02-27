using CoreMesh.Examples.Console.Samples.Interception.Interceptors;
using CoreMesh.Interception;

namespace CoreMesh.Examples.Console.Samples.Interception;

public interface IMyService: IInterceptable
{
    Task<List<int>> Add(int a, int b);
}

[InterceptedBy<LoggingInterceptor>]
[InterceptedBy<CacheAsyncInterceptor>]
public interface IInterceptable;
