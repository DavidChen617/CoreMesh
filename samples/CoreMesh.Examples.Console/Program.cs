using System.Text.Json;
using CoreMesh.Examples.Console;
using CoreMesh.Mapper;
using CoreMesh.Mapper.Extensions;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddCoreMeshMapper([typeof(Program).Assembly]);
using var app = builder.Build();
var mapper = app.Services.GetRequiredService<IMapper>();

var singleUser = new User()
{
    Id = "1",
    FirstName = "John",
    LastName = "Doe",
    Email = "example@test.com",
    Password = "password"
};

var singleUserDto = mapper.Map<User, UserDto>(singleUser);
Console.WriteLine(JsonSerializer.Serialize(singleUserDto));

var users =  new List<User>();

for (int i = 0; i < 10; i++)
{
    users.Add(new User()
    {
        Id = i.ToString(),
        FirstName = "T" + i,
        LastName = "S" + i,
        Email = "test" + i + "@test.com",
        Password = "password" + i
    });
}

var dtos = mapper.Map<User, UserDto>(users);
Console.WriteLine($"Dtos: {JsonSerializer.Serialize(dtos)}");

var revertToUser  = mapper.Map<UserDto, User>(singleUserDto);
Console.WriteLine($" revertToUser {JsonSerializer.Serialize(revertToUser)}");

var revertToUsers = mapper.Map<UserDto, User>(dtos);
Console.WriteLine($" revertToUsers {JsonSerializer.Serialize(revertToUsers)}");
