using System.Text.Json;
using CoreMesh.Examples.Console;
using CoreMesh.Examples.Console.Samples.Interception;
using CoreMesh.Interception.Extensions;
using CoreMesh.Mapper;
using CoreMesh.Mapper.Extensions;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddScoped<IMyService, MyService>();

builder.Services.AddCoreMeshMapper([typeof(Program).Assembly]);
builder.Services.AddInterceptor([typeof(Program).Assembly]);

var app = builder.Build();
var service = app.Services.GetService<IMyService>();

var s = await service!.Add(1, 2);
Console.WriteLine(s[0]);

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
// Console.WriteLine(JsonSerializer.Serialize(singleUserDto));

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
// Console.WriteLine($"Dtos: {JsonSerializer.Serialize(dtos)}");

var revertToUser  = mapper.Map<UserDto, User>(singleUserDto);
// Console.WriteLine($" revertToUser {JsonSerializer.Serialize(revertToUser)}");

var revertToUsers = mapper.Map<UserDto, User>(dtos);
// Console.WriteLine($" revertToUsers {JsonSerializer.Serialize(revertToUsers)}");
