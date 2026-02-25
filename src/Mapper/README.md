# CoreMesh.Mapper

`CoreMesh.Mapper` is a lightweight convention-based object mapper for application-level DTO and entity mapping.

It discovers mappings from contracts implemented by your types and compiles delegates for runtime execution.

## Features

- Convention-based registration via assembly scanning
- Strongly typed mapping APIs
- One-way mapping contract: `IMapFrom<TSource, TDestination>`
- Two-way mapping contract: `IMapWith<TEntity, TSelf>`
- DI registration extension: `AddCoreMeshMapper(...)`

## Mapping Contracts

### `IMapFrom<TSource, TDestination>`

Use this contract for one-way mapping.

```csharp
using CoreMesh.Mapper;

public sealed record UserDto(string FullName, string Email)
    : IMapFrom<User, UserDto>
{
    public UserDto MapFrom(User source)
        => new($"{source.FirstName} {source.LastName}", source.Email);
}
```

### `IMapWith<TEntity, TSelf>`

Use this contract for two-way mapping (entity <-> dto).

```csharp
using CoreMesh.Mapper;

public sealed record UserDto(string FullName, string Email)
    : IMapWith<User, UserDto>
{
    public UserDto() : this(string.Empty, string.Empty) { }

    public UserDto MapFrom(User entity)
        => new($"{entity.FirstName} {entity.LastName}".Trim(), entity.Email);

    public User MapTo()
    {
        var parts = FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return new User
        {
            Id = string.Empty,
            FirstName = parts.Length > 0 ? parts[0] : string.Empty,
            LastName = parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : string.Empty,
            Email = Email,
            Password = string.Empty
        };
    }
}
```

### Multi-source one-way mapping (`IMapFrom<...>`)

For composite DTO scenarios, `CoreMesh.Mapper` supports one-way mapping from up to three source objects.

#### Two sources

```csharp
using CoreMesh.Mapper;

public sealed record UserAggregateDto(string FullName, string Email, string City)
    : IMapFrom<User, UserProfile, UserAggregateDto>
{
    public UserAggregateDto() : this(string.Empty, string.Empty, string.Empty) { }

    public UserAggregateDto MapFrom(User user, UserProfile profile)
        => new($"{user.FirstName} {user.LastName}", user.Email, profile.City);
}
```

#### Three sources

```csharp
using CoreMesh.Mapper;

public sealed record UserSummaryDto(string FullName, string Email, string City, int OrderCount)
    : IMapFrom<User, UserProfile, UserStats, UserSummaryDto>
{
    public UserSummaryDto() : this(string.Empty, string.Empty, string.Empty, 0) { }

    public UserSummaryDto MapFrom(User user, UserProfile profile, UserStats stats)
        => new($"{user.FirstName} {user.LastName}", user.Email, profile.City, stats.OrderCount);
}
```

## Usage (Without DI)

```csharp
using CoreMesh.Mapper;

var mapper = new Mapper();
mapper.RegisterMapper(typeof(Program).Assembly);

var dto = mapper.Map<User, UserDto>(user);
var users = mapper.Map<User, UserDto>(userList).ToList();

var aggregate = mapper.Map<User, UserProfile, UserAggregateDto>(user, profile);
var summary = mapper.Map<User, UserProfile, UserStats, UserSummaryDto>(user, profile, stats);
```

## Usage (With DI)

```csharp
using CoreMesh.Mapper;
using CoreMesh.Mapper.Extensions;

builder.Services.AddCoreMeshMapper(typeof(Program).Assembly);
```

Resolve and use:

```csharp
var mapper = serviceProvider.GetRequiredService<IMapper>();
var dto = mapper.Map<User, UserDto>(user);
```

## Public API

- `IMapFrom<TSource, TDestination>`
- `IMapFrom<TSource1, TSource2, TDestination>`
- `IMapFrom<TSource1, TSource2, TSource3, TDestination>`
- `IMapWith<TEntity, TSelf>`
- `IMapper`
- `Mapper`

## Notes

- Mapping contract implementations (`IMapFrom<...>` and `IMapWith<...>`) require a **public parameterless constructor** for registration.
- Mapping registration currently ignores duplicate keys by using `TryAdd`. If multiple mappings target the same `(source, destination)` pair, only the first registration is kept.
- Collection mapping returns a lazily evaluated sequence.

## Current Scope

This module intentionally focuses on:

- Strongly typed object mapping
- Strongly typed multi-source one-way mapping (up to 3 sources)
- Assembly scanning registration
- Convention-based delegate compilation

It does not currently include:

- Weakly typed runtime mapping APIs
- Automatic nested object graph mapping
- Property-by-property reflection mapping
- Profile/configuration DSL similar to AutoMapper
