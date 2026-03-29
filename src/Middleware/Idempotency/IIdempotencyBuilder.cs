namespace CoreMesh.Middleware.Idempotency;

public interface IIdempotencyBuilder
{
    /// <summary>
    /// 配置 Idempotency 選項
    /// </summary>
    IIdempotencyBuilder Configure(Action<IdempotencyOptions> configure);

    /// <summary>
    /// 設置自定義 Idempotency Handler
    /// </summary>
    IIdempotencyBuilder WithHandler<THandler>()
        where THandler : class, IIdempotencyHandler;

    /// <summary>
    /// 設置自定義 Idempotency Handler（實例）
    /// </summary>
    IIdempotencyBuilder WithHandler(IIdempotencyHandler handler);
}
