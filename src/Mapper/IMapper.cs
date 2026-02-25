namespace CoreMesh.Mapper;

/// <summary>
/// Defines a one-way mapping capability from a source type to a destination type.
/// </summary>
/// <typeparam name="TSource">The source type.</typeparam>
/// <typeparam name="TDestination">The destination type.</typeparam>
public interface IMapFrom<in TSource, out TDestination>
{
    /// <summary>
    /// Maps the specified source instance to the destination type.
    /// </summary>
    /// <param name="source">The source instance to map.</param>
    /// <returns>The mapped destination instance.</returns>
    TDestination MapFrom(TSource source);
}

/// <summary>
/// Defines a one-way mapping capability from two source types to a destination type.
/// </summary>
/// <typeparam name="TSource1">The first source type.</typeparam>
/// <typeparam name="TSource2">The second source type.</typeparam>
/// <typeparam name="TDestination">The destination type.</typeparam>
public interface IMapFrom<in TSource1, in TSource2, out TDestination>
{
    /// <summary>
    /// Maps the specified source instances to the destination type.
    /// </summary>
    /// <param name="source1">The first source instance.</param>
    /// <param name="source2">The second source instance.</param>
    /// <returns>The mapped destination instance.</returns>
    TDestination MapFrom(TSource1 source1, TSource2 source2);
}

/// <summary>
/// Defines a one-way mapping capability from three source types to a destination type.
/// </summary>
/// <typeparam name="TSource1">The first source type.</typeparam>
/// <typeparam name="TSource2">The second source type.</typeparam>
/// <typeparam name="TSource3">The third source type.</typeparam>
/// <typeparam name="TDestination">The destination type.</typeparam>
public interface IMapFrom<in TSource1, in TSource2, in TSource3, out TDestination>
{
    /// <summary>
    /// Maps the specified source instances to the destination type.
    /// </summary>
    /// <param name="source1">The first source instance.</param>
    /// <param name="source2">The second source instance.</param>
    /// <param name="source3">The third source instance.</param>
    /// <returns>The mapped destination instance.</returns>
    TDestination MapFrom(TSource1 source1, TSource2 source2, TSource3 source3);
}

/// <summary>
/// Defines a two-way mapping contract between an entity type and the implementing type.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <typeparam name="TSelf">The implementing type (typically a DTO).</typeparam>
public interface IMapWith<TEntity, TSelf>
    where TSelf : IMapWith<TEntity, TSelf>
{
    /// <summary>
    /// Maps the specified entity instance to the implementing type.
    /// </summary>
    /// <param name="entity">The entity instance to map.</param>
    /// <returns>The mapped <typeparamref name="TSelf"/> instance.</returns>
    TSelf MapFrom(TEntity entity);

    /// <summary>
    /// Maps the current instance back to the entity type.
    /// </summary>
    /// <returns>The mapped entity instance.</returns>
    TEntity MapTo();
}

/// <summary>
/// Provides object mapping capabilities between registered source and destination types.
/// </summary>
public interface IMapper
{
    /// <summary>
    /// Maps a source instance to the destination type.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TDestination">The destination type.</typeparam>
    /// <param name="source">The source instance to map.</param>
    /// <returns>The mapped destination instance.</returns>
    TDestination Map<TSource, TDestination>(TSource source);

    /// <summary>
    /// Maps a sequence of source instances to destination instances.
    /// </summary>
    /// <typeparam name="TSource">The source element type.</typeparam>
    /// <typeparam name="TDestination">The destination element type.</typeparam>
    /// <param name="sources">The source sequence to map.</param>
    /// <returns>A lazily evaluated sequence of mapped destination instances.</returns>
    IEnumerable<TDestination> Map<TSource, TDestination>(IEnumerable<TSource> sources);

    /// <summary>
    /// Maps two source instances to the destination type.
    /// </summary>
    /// <typeparam name="TSource1">The first source type.</typeparam>
    /// <typeparam name="TSource2">The second source type.</typeparam>
    /// <typeparam name="TDestination">The destination type.</typeparam>
    /// <param name="source1">The first source instance.</param>
    /// <param name="source2">The second source instance.</param>
    /// <returns>The mapped destination instance.</returns>
    TDestination Map<TSource1, TSource2, TDestination>(TSource1 source1, TSource2 source2);

    /// <summary>
    /// Maps three source instances to the destination type.
    /// </summary>
    /// <typeparam name="TSource1">The first source type.</typeparam>
    /// <typeparam name="TSource2">The second source type.</typeparam>
    /// <typeparam name="TSource3">The third source type.</typeparam>
    /// <typeparam name="TDestination">The destination type.</typeparam>
    /// <param name="source1">The first source instance.</param>
    /// <param name="source2">The second source instance.</param>
    /// <param name="source3">The third source instance.</param>
    /// <returns>The mapped destination instance.</returns>
    TDestination Map<TSource1, TSource2, TSource3, TDestination>(TSource1 source1, TSource2 source2, TSource3 source3);
}
