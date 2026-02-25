namespace CoreMesh.Mapper.Tests;

public sealed class MapperTests
{
    [Fact]
    public void RegisterMapper_Should_Map_IMapFrom()
    {
        var mapper = new Mapper();
        InvokePrivate(mapper, "RegisterIMapFrom", typeof(UserFromDto));

        var source = new SourceUser
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        var dto = mapper.Map<SourceUser, UserFromDto>(source);

        Assert.Equal("John Doe", dto.FullName);
        Assert.Equal("john@example.com", dto.Email);
    }

    [Fact]
    public void RegisterMapper_Should_Map_IMapWith_From_And_To()
    {
        var mapper = new Mapper();
        InvokePrivate(mapper, "RegisterIMapWith", typeof(UserWithDto));

        var entity = new SourceUser
        {
            Id = "1",
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@example.com",
            Password = "secret"
        };

        var dto = mapper.Map<SourceUser, UserWithDto>(entity);
        var mappedBack = mapper.Map<UserWithDto, SourceUser>(dto);

        Assert.Equal("Jane Doe", dto.FullName);
        Assert.Equal(entity.Email, dto.Email);
        Assert.Equal("Jane", mappedBack.FirstName);
        Assert.Equal("Doe", mappedBack.LastName);
        Assert.Equal(entity.Email, mappedBack.Email);
    }

    [Fact]
    public void RegisterMapper_Should_Map_IMapFrom_With_Two_Sources()
    {
        var mapper = new global::CoreMesh.Mapper.Mapper();
        InvokePrivate(mapper, "RegisterIMapFrom2", typeof(UserAggregateDto));

        var user = new SourceUser
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };
        var profile = new UserProfile
        {
            City = "Taipei",
            Country = "Taiwan"
        };

        var dto = mapper.Map<SourceUser, UserProfile, UserAggregateDto>(user, profile);

        Assert.Equal("John Doe", dto.FullName);
        Assert.Equal("john@example.com", dto.Email);
        Assert.Equal("Taipei", dto.City);
        Assert.Equal("Taiwan", dto.Country);
    }

    [Fact]
    public void RegisterMapper_Should_Map_IMapFrom_With_Three_Sources()
    {
        var mapper = new global::CoreMesh.Mapper.Mapper();
        InvokePrivate(mapper, "RegisterIMapFrom3", typeof(UserSummary3Dto));

        var user = new SourceUser
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@example.com"
        };
        var profile = new UserProfile
        {
            City = "Tokyo",
            Country = "Japan"
        };
        var stats = new UserStats
        {
            OrderCount = 12
        };

        var dto = mapper.Map<SourceUser, UserProfile, UserStats, UserSummary3Dto>(user, profile, stats);

        Assert.Equal("Jane Doe", dto.FullName);
        Assert.Equal("jane@example.com", dto.Email);
        Assert.Equal("Tokyo", dto.City);
        Assert.Equal(12, dto.OrderCount);
    }

    [Fact]
    public void Map_Should_Throw_When_Mapping_Not_Registered()
    {
        var mapper = new Mapper();

        var ex = Assert.Throws<KeyNotFoundException>(() =>
            mapper.Map<SourceUser, UnknownDestination>(new SourceUser()));

        Assert.Contains("was not found", ex.Message);
    }

    [Fact]
    public void Map_With_Two_Sources_Should_Throw_When_Mapping_Not_Registered()
    {
        var mapper = new global::CoreMesh.Mapper.Mapper();

        var ex = Assert.Throws<KeyNotFoundException>(() =>
            mapper.Map<SourceUser, UserProfile, UserAggregateDto>(new SourceUser(), new UserProfile()));

        Assert.Contains("was not found", ex.Message);
    }

    [Fact]
    public void Map_With_Three_Sources_Should_Throw_When_Mapping_Not_Registered()
    {
        var mapper = new global::CoreMesh.Mapper.Mapper();

        var ex = Assert.Throws<KeyNotFoundException>(() =>
            mapper.Map<SourceUser, UserProfile, UserStats, UserSummary3Dto>(
                new SourceUser(),
                new UserProfile(),
                new UserStats()));

        Assert.Contains("was not found", ex.Message);
    }

    [Fact]
    public void Map_Enumerable_Should_Map_All_Items()
    {
        var mapper = new Mapper();
        InvokePrivate(mapper, "RegisterIMapFrom", typeof(UserFromDto));

        var sources = new[]
        {
            new SourceUser { FirstName = "A", LastName = "One", Email = "a@test.com" },
            new SourceUser { FirstName = "B", LastName = "Two", Email = "b@test.com" }
        };

        var result = mapper.Map<SourceUser, UserFromDto>(sources).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("A One", result[0].FullName);
        Assert.Equal("B Two", result[1].FullName);
    }

    [Fact]
    public void RegisterMapper_Should_Throw_When_IMapWith_Has_No_Public_Parameterless_Ctor()
    {
        var mapper = new Mapper();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            mapper.RegisterMapper(typeof(NoDefaultCtorMapWithDto).Assembly));

        Assert.Contains("public parameterless constructor", ex.Message);
    }

    private static void InvokePrivate(Mapper mapper, string methodName, Type targetType)
    {
        var method = typeof(Mapper).GetMethod(
            methodName,
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        Assert.NotNull(method);
        method!.Invoke(mapper, [targetType]);
    }

    private sealed class SourceUser
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    private sealed class UserProfile
    {
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }

    private sealed class UserStats
    {
        public int OrderCount { get; set; }
    }

    private sealed record UserFromDto(string FullName, string Email) : IMapFrom<SourceUser, UserFromDto>
    {
        public UserFromDto() : this(string.Empty, string.Empty)
        {
        }

        UserFromDto IMapFrom<SourceUser, UserFromDto>.MapFrom(SourceUser source)
            => new($"{source.FirstName} {source.LastName}", source.Email);
    }

    private sealed record UserWithDto(string FullName, string Email) : IMapWith<SourceUser, UserWithDto>
    {
        public UserWithDto() : this(string.Empty, string.Empty)
        {
        }

        UserWithDto IMapWith<SourceUser, UserWithDto>.MapFrom(SourceUser entity)
            => new($"{entity.FirstName} {entity.LastName}", entity.Email);

        SourceUser IMapWith<SourceUser, UserWithDto>.MapTo()
        {
            var parts = FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return new SourceUser
            {
                Id = string.Empty,
                FirstName = parts.Length > 0 ? parts[0] : string.Empty,
                LastName = parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : string.Empty,
                Email = Email,
                Password = string.Empty
            };
        }
    }

    private sealed record UserAggregateDto(string FullName, string Email, string City, string Country)
        : IMapFrom<SourceUser, UserProfile, UserAggregateDto>
    {
        public UserAggregateDto() : this(string.Empty, string.Empty, string.Empty, string.Empty)
        {
        }

        UserAggregateDto IMapFrom<SourceUser, UserProfile, UserAggregateDto>.MapFrom(SourceUser source1, UserProfile source2)
            => new(
                $"{source1.FirstName} {source1.LastName}",
                source1.Email,
                source2.City,
                source2.Country);
    }

    private sealed record UserSummary3Dto(string FullName, string Email, string City, int OrderCount)
        : IMapFrom<SourceUser, UserProfile, UserStats, UserSummary3Dto>
    {
        public UserSummary3Dto() : this(string.Empty, string.Empty, string.Empty, 0)
        {
        }

        UserSummary3Dto IMapFrom<SourceUser, UserProfile, UserStats, UserSummary3Dto>.MapFrom(
            SourceUser source1,
            UserProfile source2,
            UserStats source3)
            => new(
                $"{source1.FirstName} {source1.LastName}",
                source1.Email,
                source2.City,
                source3.OrderCount);
    }

    private sealed record NoDefaultCtorMapWithDto(string Name) : IMapWith<SourceUser, NoDefaultCtorMapWithDto>
    {
        public NoDefaultCtorMapWithDto MapFrom(SourceUser entity) => new(entity.FirstName);

        public SourceUser MapTo() => new() { FirstName = Name };
    }

    private sealed class UnknownDestination;
}
