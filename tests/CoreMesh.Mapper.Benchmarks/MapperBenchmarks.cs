using AutoMapper;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging.Abstractions;

namespace CoreMesh.Mapper.Benchmarks;

[MemoryDiagnoser]
public class MapperBenchmarks
{
    private Mapper _mapper = default!;
    private AutoMapper.IMapper _autoMapper = default!;

    private User _user = default!;
    private UserProfile _profile = default!;
    private UserStats _stats = default!;
    private List<User> _users = default!;

    [GlobalSetup]
    public void Setup()
    {
        _mapper = new Mapper();
        _mapper.RegisterMapper(typeof(MapperBenchmarks).Assembly);

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>()
                .ConstructUsing(src => new UserDto($"{src.FirstName} {src.LastName}", src.Email));

            cfg.CreateMap<(User User, UserProfile Profile), UserAggregateDto>()
                .ConstructUsing(src => new UserAggregateDto(
                    $"{src.User.FirstName} {src.User.LastName}",
                    src.User.Email,
                    src.Profile.City,
                    src.Profile.Country));

            cfg.CreateMap<(User User, UserProfile Profile, UserStats Stats), UserSummary3Dto>()
                .ConstructUsing(src => new UserSummary3Dto(
                    $"{src.User.FirstName} {src.User.LastName}",
                    src.User.Email,
                    src.Profile.City,
                    src.Stats.OrderCount));
        }, NullLoggerFactory.Instance);

        _autoMapper = config.CreateMapper();

        _user = new User
        {
            Id = "1",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Password = "secret"
        };

        _profile = new UserProfile
        {
            City = "Taipei",
            Country = "Taiwan"
        };

        _stats = new UserStats
        {
            OrderCount = 42
        };

        _users = Enumerable.Range(0, 100)
            .Select(i => new User
            {
                Id = i.ToString(),
                FirstName = $"F{i}",
                LastName = $"L{i}",
                Email = $"user{i}@test.com",
                Password = $"pwd{i}"
            })
            .ToList();
    }

    [Benchmark]
    public UserDto Map_Single_Source()
        => _mapper.Map<User, UserDto>(_user);

    [Benchmark]
    public UserDto AutoMapper_Map_Single_Source()
        => _autoMapper.Map<UserDto>(_user);

    [Benchmark]
    public UserAggregateDto Map_Two_Sources()
        => _mapper.Map<User, UserProfile, UserAggregateDto>(_user, _profile);

    [Benchmark]
    public UserAggregateDto AutoMapper_Map_Two_Sources()
        => _autoMapper.Map<UserAggregateDto>((_user, _profile));

    [Benchmark]
    public UserSummary3Dto Map_Three_Sources()
        => _mapper.Map<User, UserProfile, UserStats, UserSummary3Dto>(_user, _profile, _stats);

    [Benchmark]
    public UserSummary3Dto AutoMapper_Map_Three_Sources()
        => _autoMapper.Map<UserSummary3Dto>((_user, _profile, _stats));

    [Benchmark]
    public List<UserDto> Map_Collection_100_ToList()
        => _mapper.Map<User, UserDto>(_users).ToList();

    [Benchmark]
    public List<UserDto> AutoMapper_Map_Collection_100_ToList()
        => _autoMapper.Map<List<UserDto>>(_users);

    public sealed class User
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public sealed class UserProfile
    {
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }

    public sealed class UserStats
    {
        public int OrderCount { get; set; }
    }

    public sealed record UserDto(string FullName, string Email)
        : IMapFrom<User, UserDto>
    {
        public UserDto() : this(string.Empty, string.Empty)
        {
        }

        public UserDto MapFrom(User source)
            => new($"{source.FirstName} {source.LastName}", source.Email);
    }

    public sealed record UserAggregateDto(string FullName, string Email, string City, string Country)
        : IMapFrom<User, UserProfile, UserAggregateDto>
    {
        public UserAggregateDto() : this(string.Empty, string.Empty, string.Empty, string.Empty)
        {
        }

        public UserAggregateDto MapFrom(User user, UserProfile profile)
            => new($"{user.FirstName} {user.LastName}", user.Email, profile.City, profile.Country);
    }

    public sealed record UserSummary3Dto(string FullName, string Email, string City, int OrderCount)
        : IMapFrom<User, UserProfile, UserStats, UserSummary3Dto>
    {
        public UserSummary3Dto() : this(string.Empty, string.Empty, string.Empty, 0)
        {
        }

        public UserSummary3Dto MapFrom(User user, UserProfile profile, UserStats stats)
            => new($"{user.FirstName} {user.LastName}", user.Email, profile.City, stats.OrderCount);
    }
}
