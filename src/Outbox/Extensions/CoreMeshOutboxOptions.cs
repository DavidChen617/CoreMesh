using CoreMesh.Outbox.Abstractions;
using CoreMesh.Outbox.InMemory;
using Microsoft.Extensions.DependencyInjection;

namespace CoreMesh.Outbox.Extensions;

/// <summary>
/// Configuration options for CoreMesh Outbox. Passed as a delegate to
/// <see cref="CoreMeshOutboxExtensions.AddCoreMeshOutbox"/>.
/// </summary>
public sealed class CoreMeshOutboxOptions(IServiceCollection services)
{
    /// <summary>
    /// Registers in-memory implementations of <see cref="IOutboxStore"/> and <see cref="IOutboxWriter"/>
    /// backed by a thread-safe in-process dictionary.
    /// </summary>
    public CoreMeshOutboxOptions UseInMemoryStore()
    {
        services.AddSingleton<IOutboxStore, InMemoryOutboxStore>();
        services.AddSingleton<IOutboxWriter>(sp => (IOutboxWriter)sp.GetRequiredService<IOutboxStore>());
        return this;
    }

    /// <summary>
    /// Registers a custom <see cref="IOutboxStore"/> implementation. Registered as <c>Scoped</c>.
    /// </summary>
    public CoreMeshOutboxOptions AddOutboxStore<TStore>()
        where TStore : class, IOutboxStore
    {
        services.AddScoped<IOutboxStore, TStore>();
        return this;
    }

    /// <summary>
    /// Registers a custom <see cref="IOutboxWriter"/> implementation. Registered as <c>Scoped</c>.
    /// </summary>
    public CoreMeshOutboxOptions AddOutboxWriter<TWriter>()
        where TWriter : class, IOutboxWriter
    {
        services.AddScoped<IOutboxWriter, TWriter>();
        return this;
    }

    /// <summary>
    /// Registers in-memory implementations of <see cref="IEventPublisher"/> and <see cref="IMessageSubscriber"/>
    /// backed by a .NET <see cref="System.Threading.Channels.Channel{T}"/>.
    /// </summary>
    public CoreMeshOutboxOptions UseInMemoryChannel()
    {
        services.AddSingleton<InMemoryMessageChannel>();
        services.AddSingleton<IEventPublisher, InMemoryEventPublisher>();
        services.AddSingleton<IMessageSubscriber, InMemoryMessageSubscriber>();
        return this;
    }

    /// <summary>
    /// Registers custom implementations for <see cref="IEventPublisher"/> and <see cref="IMessageSubscriber"/>.
    /// Registered as <c>Singleton</c>.
    /// </summary>
    public CoreMeshOutboxOptions AddMessageQueue<TPublisher, TConsumer>()
        where TPublisher : class, IEventPublisher
        where TConsumer : class, IMessageSubscriber
    {
        services.AddSingleton<IEventPublisher, TPublisher>();
        services.AddSingleton<IMessageSubscriber, TConsumer>();
        return this;
    }

    /// <summary>
    /// Registers <see cref="EventConsumer"/> as a hosted background service.
    /// Call this only when the application acts as a message consumer.
    /// Requires <see cref="IMessageSubscriber"/> to be registered (via <see cref="UseInMemoryChannel"/>
    /// or <see cref="AddMessageQueue{TPublisher,TConsumer}"/>).
    /// </summary>
    public CoreMeshOutboxOptions WithConsumer()
    {
        services.AddHostedService<EventConsumer>();
        return this;
    }
}
