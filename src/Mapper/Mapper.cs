using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace CoreMesh.Mapper;

/// <summary>
/// Provides runtime registration and execution of object mappings discovered from mapping contracts.
/// </summary>
public class Mapper : IMapper
{
    private readonly ConcurrentDictionary<(Type Source, Type Destination), object> _mappers = new();
    private readonly ConcurrentDictionary<(Type Source1, Type Source2, Type Destination), object> _mappers2 = new();
    private readonly ConcurrentDictionary<(Type Source1, Type Source2, Type Source3, Type Destination), object> _mappers3 = new();

    /// <summary>
    /// Scans the specified assembly for mapping contracts and registers all discovered mappings.
    /// </summary>
    /// <param name="assembly">The assembly to scan.</param>
    public void RegisterMapper(Assembly assembly)
    {
        var types = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType);

        foreach (var type in types)
        {
            RegisterIMapFrom(type);
            RegisterIMapFrom2(type);
            RegisterIMapFrom3(type);
            RegisterIMapWith(type);
        }
    }

    private void RegisterIMapFrom(Type type)
    {
        IEnumerable<Type> interfaces = GetInterfaces(type, typeof(IMapFrom<,>));

        foreach (var @interface in interfaces)
        {
            var args = @interface.GetGenericArguments();
            var sourceType = args[0];
            var destType = args[1];

            var mapper = CompileMapper(
                sourceType,
                destType,
                type,
                @interface,
                nameof(IMapFrom<,>.MapFrom));

            _mappers.TryAdd((sourceType, destType), mapper);
        }
    }

    private void RegisterIMapFrom2(Type type)
    {
        IEnumerable<Type> interfaces = GetInterfaces(type, typeof(IMapFrom<,,>));

        foreach (var @interface in interfaces)
        {
            var args = @interface.GetGenericArguments();
            var sourceType1 = args[0];
            var sourceType2 = args[1];
            var destType = args[2];

            var mapper = CompileMapper2(
                sourceType1,
                sourceType2,
                destType,
                type,
                @interface,
                nameof(IMapFrom<object, object, object>.MapFrom));

            _mappers2.TryAdd((sourceType1, sourceType2, destType), mapper);
        }
    }

    private void RegisterIMapFrom3(Type type)
    {
        IEnumerable<Type> interfaces = GetInterfaces(type, typeof(IMapFrom<,,,>));

        foreach (var @interface in interfaces)
        {
            var args = @interface.GetGenericArguments();
            var sourceType1 = args[0];
            var sourceType2 = args[1];
            var sourceType3 = args[2];
            var destType = args[3];

            var mapper = CompileMapper3(
                sourceType1,
                sourceType2,
                sourceType3,
                destType,
                type,
                @interface,
                nameof(IMapFrom<,,,>.MapFrom));

            _mappers3.TryAdd((sourceType1, sourceType2, sourceType3, destType), mapper);
        }
    }

    private void RegisterIMapWith(Type type)
    {
        IEnumerable<Type> interfaces = GetInterfaces(type, typeof(IMapWith<,>));

        foreach (var @interface in interfaces)
        {
            var args = @interface.GetGenericArguments();
            var sourceType = args[0];
            var destinationType = args[1];

            // Source -> Destination
            var mapFromMapper = CompileMapper(
                sourceType,
                destinationType,
                type,
                @interface,
                nameof(IMapWith<,>.MapFrom));

            _mappers.TryAdd((sourceType, destinationType), mapFromMapper);

            // Destination -> Source
            var destParam = Expression.Parameter(destinationType, "destination");
            var mapToMethod = type.GetInterfaceMap(@interface)
                .TargetMethods
                .First(m => m.Name.EndsWith(nameof(IMapWith<,>.MapTo)));

            var mapToCall = Expression.Call(destParam, mapToMethod);
            var mapToLambdaType = typeof(Func<,>).MakeGenericType(destinationType, sourceType);
            var mapToMapper = Expression.Lambda(mapToLambdaType, mapToCall, destParam).Compile();

            _mappers.TryAdd((destinationType, sourceType), mapToMapper);
        }
    }


    private static IEnumerable<Type> GetInterfaces(Type type, Type interfaceType)
    {
        var interfaces = type.GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType);
        return interfaces;
    }

    private Delegate CompileMapper(
        Type sourceType,
        Type destType,
        Type mapperType,
        Type targetInterface,
        string methodName)
    {
        var sourceParam = Expression.Parameter(sourceType, "source");

        var defaultCtor = mapperType.GetConstructor(Type.EmptyTypes);
        if (defaultCtor is null)
        {
            throw new InvalidOperationException(
                $"Mapper type '{mapperType.FullName}' must define a public parameterless constructor.");
        }

        var mapperInstance = Expression.New(defaultCtor);

        var method = mapperType.GetInterfaceMap(targetInterface)
            .TargetMethods
            .First(m => m.Name.EndsWith(methodName));

        var callMethod = Expression.Call(mapperInstance, method, sourceParam);
        var lambdaType = typeof(Func<,>).MakeGenericType(sourceType, destType);

        return Expression.Lambda(lambdaType, callMethod, sourceParam).Compile();
    }

    private Delegate CompileMapper2(
        Type sourceType1,
        Type sourceType2,
        Type destType,
        Type mapperType,
        Type targetInterface,
        string methodName)
    {
        var sourceParam1 = Expression.Parameter(sourceType1, "source1");
        var sourceParam2 = Expression.Parameter(sourceType2, "source2");

        var defaultCtor = mapperType.GetConstructor(Type.EmptyTypes);
        if (defaultCtor is null)
        {
            throw new InvalidOperationException(
                $"Mapper type '{mapperType.FullName}' must define a public parameterless constructor.");
        }

        var mapperInstance = Expression.New(defaultCtor);
        var method = mapperType.GetInterfaceMap(targetInterface)
            .TargetMethods
            .First(m => m.Name.EndsWith(methodName));

        var callMethod = Expression.Call(mapperInstance, method, sourceParam1, sourceParam2);
        var lambdaType = typeof(Func<,,>).MakeGenericType(sourceType1, sourceType2, destType);

        return Expression.Lambda(lambdaType, callMethod, sourceParam1, sourceParam2).Compile();
    }

    private Delegate CompileMapper3(
        Type sourceType1,
        Type sourceType2,
        Type sourceType3,
        Type destType,
        Type mapperType,
        Type targetInterface,
        string methodName)
    {
        var sourceParam1 = Expression.Parameter(sourceType1, "source1");
        var sourceParam2 = Expression.Parameter(sourceType2, "source2");
        var sourceParam3 = Expression.Parameter(sourceType3, "source3");

        var defaultCtor = mapperType.GetConstructor(Type.EmptyTypes);
        if (defaultCtor is null)
        {
            throw new InvalidOperationException(
                $"Mapper type '{mapperType.FullName}' must define a public parameterless constructor.");
        }

        var mapperInstance = Expression.New(defaultCtor);
        var method = mapperType.GetInterfaceMap(targetInterface)
            .TargetMethods
            .First(m => m.Name.EndsWith(methodName));

        var callMethod = Expression.Call(mapperInstance, method, sourceParam1, sourceParam2, sourceParam3);
        var lambdaType = typeof(Func<,,,>).MakeGenericType(sourceType1, sourceType2, sourceType3, destType);

        return Expression.Lambda(lambdaType, callMethod, sourceParam1, sourceParam2, sourceParam3).Compile();
    }

    /// <summary>
    /// Maps a source instance to the destination type.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TDestination">The destination type.</typeparam>
    /// <param name="source">The source instance to map.</param>
    /// <returns>The mapped destination instance.</returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no mapping has been registered for the specified source and destination types.
    /// </exception>
    public TDestination Map<TSource, TDestination>(TSource source)
    {
        var key = (typeof(TSource), typeof(TDestination));

        if (_mappers.TryGetValue(key, out var mapper))
        {
            var typedMapper = (Func<TSource, TDestination>)mapper;
            return typedMapper(source);
        }

        throw new KeyNotFoundException($"The key {key} was not found in the mappers");
    }

    /// <summary>
    /// Maps a sequence of source instances to destination instances using deferred execution.
    /// </summary>
    /// <typeparam name="TSource">The source element type.</typeparam>
    /// <typeparam name="TDestination">The destination element type.</typeparam>
    /// <param name="sources">The source sequence to map.</param>
    /// <returns>A lazily evaluated sequence of mapped destination instances.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="sources"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no mapping has been registered for the specified source and destination types.
    /// </exception>
    public IEnumerable<TDestination> Map<TSource, TDestination>(IEnumerable<TSource> sources)
    {
        ArgumentNullException.ThrowIfNull(sources);

        var key = (typeof(TSource), typeof(TDestination));

        if (!_mappers.TryGetValue(key, out var mapper))
        {
            throw new KeyNotFoundException($"The key {key} was not found in the mappers");
        }

        var typedMapper = (Func<TSource, TDestination>)mapper;

        foreach (var source in sources)
        {
            yield return typedMapper(source);
        }
    }

    /// <summary>
    /// Maps two source instances to the destination type.
    /// </summary>
    /// <typeparam name="TSource1">The first source type.</typeparam>
    /// <typeparam name="TSource2">The second source type.</typeparam>
    /// <typeparam name="TDestination">The destination type.</typeparam>
    /// <param name="source1">The first source instance.</param>
    /// <param name="source2">The second source instance.</param>
    /// <returns>The mapped destination instance.</returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no mapping has been registered for the specified source and destination types.
    /// </exception>
    public TDestination Map<TSource1, TSource2, TDestination>(TSource1 source1, TSource2 source2)
    {
        var key = (typeof(TSource1), typeof(TSource2), typeof(TDestination));

        if (_mappers2.TryGetValue(key, out var mapper))
        {
            var typedMapper = (Func<TSource1, TSource2, TDestination>)mapper;
            return typedMapper(source1, source2);
        }

        throw new KeyNotFoundException($"The key {key} was not found in the mappers");
    }

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
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no mapping has been registered for the specified source and destination types.
    /// </exception>
    public TDestination Map<TSource1, TSource2, TSource3, TDestination>(TSource1 source1, TSource2 source2, TSource3 source3)
    {
        var key = (typeof(TSource1), typeof(TSource2), typeof(TSource3), typeof(TDestination));

        if (_mappers3.TryGetValue(key, out var mapper))
        {
            var typedMapper = (Func<TSource1, TSource2, TSource3, TDestination>)mapper;
            return typedMapper(source1, source2, source3);
        }

        throw new KeyNotFoundException($"The key {key} was not found in the mappers");
    }
}
