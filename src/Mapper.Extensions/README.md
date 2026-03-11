English | [繁體中文](README.zh-TW.md)

# CoreMesh.Mapper.Extensions

DI registration extension for `CoreMesh.Mapper`.

## Usage

```csharp
using CoreMesh.Mapper.Extensions;

builder.Services.AddCoreMeshMapper(typeof(Program).Assembly);
```

Registers `IMapper` as a singleton and scans the given assemblies for mapping contracts (`IMapFrom<...>`, `IMapWith<...>`). Multiple assemblies can be passed in a single call. Calling it more than once is a no-op (`TryAddSingleton` semantics).

See `CoreMesh.Mapper` for defining mapping contracts and using `IMapper`.
