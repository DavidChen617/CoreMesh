using CoreMesh.Mapper;

namespace CoreMesh.Examples.Console;

public class User
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } =  string.Empty;
}

public record UserDto(string FullName, string Email) : IMapWith<User, UserDto>
{
    public UserDto() : this(string.Empty, string.Empty)
    {
    }
    
    UserDto IMapWith<User, UserDto>.MapFrom(User entity) => new(entity.FirstName + " " + entity.LastName, entity.Email);

    User IMapWith<User, UserDto>.MapTo()
    {
        var firstNameAndLastName = FullName.Split(' ');

        return new User
        {
            Id = "", FirstName = firstNameAndLastName[0], LastName = firstNameAndLastName[1], Email = this.Email,
        };
    }
}
